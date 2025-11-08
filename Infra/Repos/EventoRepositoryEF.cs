// EventoRepositoryEF.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PPAI_2.Infra.Data;
using PPAI_2.Infra.Data.EFModels;
using PPAI_2.Infra.Data.Mapping;
using PPAI_Revisiones.Modelos;
using D = PPAI_Revisiones.Dominio;   // alias del dominio
using M = PPAI_Revisiones.Modelos;   // alias modelos/estado
using PPAI_2.Infra.Data.EFModels;



namespace PPAI_2.Infra.Repos
{
    public class EventoRepositoryEF : IEventoRepository
    {
        private readonly RedSismicaContext _ctx;
        public EventoRepositoryEF(RedSismicaContext ctx) => _ctx = ctx;

        // === Listado para CU (solo lectura, no trackeado) ===
        public IEnumerable<EventoSismico> GetEventosParaRevision()
        {
            var efList = _ctx.EventosSismicos
                .AsNoTracking()
                .Include(e => e.CambiosDeEstado).ThenInclude(c => c.Responsable)
                .Include(e => e.Alcance)
                .Include(e => e.Clasificacion)
                .Include(e => e.Origen)
                .Include(e => e.Responsable)
                .ToList();

            return efList.Select(e => e.ToDomain());
        }

        public IEnumerable<EventoSismico> GetEventosAutoDetectadosNoRevisados()
        {
            var efList = _ctx.EventosSismicos
                .AsNoTracking()
                .Include(e => e.CambiosDeEstado).ThenInclude(c => c.Responsable)
                .Include(e => e.Alcance)
                .Include(e => e.Clasificacion)
                .Include(e => e.Origen)
                .Include(e => e.Responsable)
                .Where(e => e.EstadoActualNombre == "Autodetectado"
                         || e.EstadoActualNombre == "Bloqueado")
                .ToList();

            var dom = efList.Select(e => e.ToDomain()).ToList();
            foreach (var d in dom)
            {
                d.MaterializarEstadoDesdeNombre();
                d.MaterializarEstadosDeCambios();
            }
            return dom;
        }

        // === Carga completa TRACKED para modificar desde el CU ===
        public EventoSismico GetEventoConSeriesYDetalles(Guid eventoId)
        {
            var ef = _ctx.EventosSismicos
                .AsNoTracking()
                .Include(e => e.Alcance)           // <<-- NECESARIO
                .Include(e => e.Clasificacion)     // <<-- NECESARIO
                .Include(e => e.Origen)            // <<-- NECESARIO
                .Include(e => e.CambiosDeEstado)
                    .ThenInclude(c => c.Responsable)   // <-- clave para que no salga "(desconocido)"
                .Include(e => e.SeriesTemporales)
                    .ThenInclude(st => st.Sismografo)
                        .ThenInclude(s => s.Estacion)
                .Include(e => e.SeriesTemporales)
                    .ThenInclude(st => st.Muestras)
                        .ThenInclude(m => m.Detalles)
                            .ThenInclude(d => d.TipoDeDato) // ← CLAVE
                .AsSplitQuery()
                .FirstOrDefault(e => e.Id == eventoId);

            return ef?.ToDomain();
        }

        public EventoSismico GetEventoParaReversionDeBloqueo(Guid eventoId)
        {
            var ef = _ctx.EventosSismicos
                .AsTracking()
                .Include(e => e.CambiosDeEstado).ThenInclude(c => c.Responsable)
                .FirstOrDefault(e => e.Id == eventoId);

            var dom = ef?.ToDomain();
            dom?.MaterializarEstadoDesdeNombre();
            dom?.MaterializarEstadosDeCambios();
            return dom;
        }


        // Guarda el evento que modificaste en el CU (bloqueo/rechazo) y
        // materializa CambiosDeEstado de dominio -> EF con PK/FK.
        public void Guardar(EventoSismico evDom, Guid? responsableIdEf)
        {
            var evEf = _ctx.EventosSismicos
                .Include(e => e.CambiosDeEstado)
                .FirstOrDefault(e => e.Id == evDom.Id)
                ?? throw new InvalidOperationException("Evento inexistente.");

            // Asegurar estado actual en EF usando el objeto Estado del dominio
            if (evDom.Estado != null)
                evEf.EstadoActualNombre = evDom.Estado.Nombre;

            // --- Sincronía de historial ---
            // Índice por (NombreEstado, Inicio) tomando el nombre desde el objeto Estado
            var dIndex = evDom.CambiosDeEstado
                .GroupBy(d => new { Nombre = d.EstadoActual?.Nombre ?? "Autodetectado", d.FechaHoraInicio })
                .ToDictionary(g => g.Key, g => g.First());

            // 1) Actualizar filas EF existentes
            foreach (var ef in evEf.CambiosDeEstado.ToList())
            {
                var key = new { Nombre = ef.EstadoNombre, ef.FechaHoraInicio };
                if (dIndex.TryGetValue(key, out var d))
                {
                    ef.FechaHoraFin = d.FechaHoraFin; // cerrar si en dominio está cerrada
                    if (ef.ResponsableId == null && responsableIdEf.HasValue)
                        ef.ResponsableId = responsableIdEf;
                }
            }

            // 2) Insertar las filas de dominio que no existen en EF
            var efKeys = evEf.CambiosDeEstado
                .Select(x => new { Nombre = x.EstadoNombre, x.FechaHoraInicio })
                .ToHashSet();

            foreach (var d in evDom.CambiosDeEstado)
            {
                var nombre = d.EstadoActual?.Nombre ?? "Autodetectado";
                var key = new { Nombre = nombre, d.FechaHoraInicio };

                if (!efKeys.Contains(key))
                {
                    evEf.CambiosDeEstado.Add(new E.CambioDeEstadoEF
                    {
                        Id = Guid.NewGuid(),
                        EventoSismicoId = evEf.Id,
                        EstadoNombre = nombre,                 // string solo en EF
                        FechaHoraInicio = d.FechaHoraInicio,
                        FechaHoraFin = d.FechaHoraFin,
                        ResponsableId = responsableIdEf
                    });
                }
            }

            // 3) Garantizar un solo "abierto" (FechaHoraFin == null)
            var abiertos = evEf.CambiosDeEstado
                .Where(c => c.FechaHoraFin == null)
                .OrderBy(c => c.FechaHoraInicio ?? DateTime.MinValue)
                .ToList();

            if (abiertos.Count > 1)
            {
                for (int i = 0; i < abiertos.Count - 1; i++)
                    abiertos[i].FechaHoraFin = abiertos[i + 1].FechaHoraInicio ?? DateTime.Now;
            }

            _ctx.SaveChanges();
        }

        // Detecta qué cambios del dominio aún no existen en EF y los agrega con GUID/FK
        private void SincronizarCambiosDeEstado(M.EventoSismico dom, EventoSismicoEF ef)
        {
            // Clave lógica para “igualdad” (no hay Id en dominio):
            // usamos (FechaInicio ISO8601 + NombreEstado)
            string Key(DateTime? inicio, string nombre)
                => $"{inicio?.ToString("o") ?? ""}|{nombre ?? ""}";

            var existentes = new HashSet<string>(
                ef.CambiosDeEstado.Select(c => Key(c.FechaHoraInicio, c.EstadoNombre)));

            foreach (var ceDom in dom.CambiosDeEstado ?? Enumerable.Empty<M.CambioDeEstado>())
            {
                var nombreEstado = ceDom.EstadoActual?.Nombre ?? ceDom.EstadoNombre;
                var k = Key(ceDom.FechaHoraInicio, nombreEstado);

                if (existentes.Contains(k))
                    continue; // ya está en EF, no lo duplico

                // Resolver responsable (opcional)
                Guid? responsableId = ResolverEmpleadoId(ceDom.Responsable);

                // Materializo un CE EF con PK/FKs
                var ceEf = new CambioDeEstadoEF
                {
                    Id = Guid.NewGuid(),
                    EventoSismicoId = ef.Id,
                    ResponsableId = responsableId,
                    EstadoNombre = nombreEstado,
                    FechaHoraInicio = ceDom.FechaHoraInicio,
                    FechaHoraFin = ceDom.FechaHoraFin
                };

                ef.CambiosDeEstado.Add(ceEf);
                existentes.Add(k);
            }
        }

        private Guid? ResolverEmpleadoId(D.Empleado resp)
        {
            if (resp == null) return null;

            // 1) Preferimos Mail si está
            var q = _ctx.Empleados.AsNoTracking().FirstOrDefault(e =>
                (!string.IsNullOrWhiteSpace(resp.Mail) && e.Mail == resp.Mail) ||
                (e.Nombre == resp.Nombre && e.Apellido == resp.Apellido));

            return q?.Id;
        }


        public void Guardar()
        {
            var saved = false;
            while (!saved)
            {
                try
                {
                    _ctx.SaveChanges();
                    saved = true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    foreach (var entry in ex.Entries) entry.Reload();
                }
            }
        }

    }
}
