// EventoRepositoryEF.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PPAI_2.Infra.Data;
using PPAI_Revisiones.Modelos;

namespace PPAI_2.Infra.Repos
{
    public class EventoRepositoryEF : IEventoRepository
    {
        private readonly RedSismicaContext _ctx;
        public EventoRepositoryEF(RedSismicaContext ctx) => _ctx = ctx;

        // EventoRepositoryEF.cs

        public IEnumerable<EventoSismico> GetEventosParaRevision()
        {
            return _ctx.EventosSismicos
                .AsNoTracking() // Correcto para el CU
                .Include(e => e.CambiosDeEstado).ThenInclude(c => c.Responsable)
                .Include(e => e.Alcance)     // <— AÑADIDO: Carga la referencia Alcance
                .Include(e => e.Clasificacion) // <— AÑADIDO: Carga la referencia Clasificacion
                .Include(e => e.Origen)      // <— AÑADIDO: Carga la referencia Origen
                .Include(e => e.Responsable) // Responsable del Evento
                .ToList();
        }

        public IEnumerable<EventoSismico> GetEventosAutoDetectadosNoRevisados()
        {
            var lista = _ctx.EventosSismicos
                .Include(e => e.CambiosDeEstado).ThenInclude(c => c.Responsable)
                .Include(e => e.Alcance)
                .Include(e => e.Clasificacion)
                .Include(e => e.Origen)
                .Include(e => e.Responsable)
                .AsNoTracking()
                .Where(e => e.EstadoActualNombre == "Autodetectado"
                         || e.EstadoActualNombre == "Bloqueado") // ← clave
                .ToList();

            foreach (var e in lista)
            {
                e.MaterializarEstadoDesdeNombre();
                e.MaterializarEstadosDeCambios();
            }
            return lista;
        }



        // Para modificar: CARGAR TRACKED (clave)
        public EventoSismico GetEventoConSeriesYDetalles(Guid eventoId)
        {
            var e = _ctx.EventosSismicos
                .AsTracking()
                .Include(ev => ev.CambiosDeEstado).ThenInclude(c => c.Responsable)
                // Carga de Estación/Sismógrafo (Necesario para ordenar y mostrar el nombre)
                .Include(ev => ev.SeriesTemporales)
                    .ThenInclude(st => st.Sismografo)
                        .ThenInclude(sis => sis.Estacion) // <-- CRÍTICO: Carga Estacion
                                                          // Carga de Muestras/Detalles/TipoDeDato (Necesario para el detalle de mediciones)
                .Include(ev => ev.SeriesTemporales)
                    .ThenInclude(st => st.Muestras)
                        .ThenInclude(m => m.DetalleMuestraSismica)
                            .ThenInclude(d => d.TipoDeDato) // <-- CRÍTICO: Carga TipoDeDato
                                                            // Catálogos principales (para evitar NullReferenceException)
                .Include(ev => ev.Alcance)
                .Include(ev => ev.Clasificacion)
                .Include(ev => ev.Origen)
                .Include(ev => ev.Responsable)
                .FirstOrDefault(ev => ev.Id == eventoId);

            e?.MaterializarEstadoDesdeNombre();
            return e;
        }

        public EventoSismico GetEventoParaReversionDeBloqueo(Guid eventoId)
        {
            return _ctx.EventosSismicos
                .AsTracking()
                .Include(e => e.CambiosDeEstado)
                    .ThenInclude(c => c.Responsable)
                .FirstOrDefault(e => e.Id == eventoId);
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
                catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
                {
                    // Recargo lo que entró en conflicto y reintento
                    foreach (var entry in ex.Entries)
                    {
                        entry.Reload();
                    }
                    // vuelve al while y reintenta el SaveChanges()
                }
            }
        }

    }
}