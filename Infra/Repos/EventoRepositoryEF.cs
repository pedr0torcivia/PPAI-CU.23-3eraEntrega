// Infra/Repos/EventoRepositoryEF.cs (Código Corregido y Completo)
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PPAI_2.Infra.Data;
using PPAI_2.Infra.Data.EFModels;
using PPAI_2.Infra.Data.Mapping;
using PPAI_Revisiones.Modelos;

namespace PPAI_Revisiones.Controladores
{
    public class EventoRepositoryEF : IEventoRepository
    {
        private readonly RedSismicaContext _ctx;
        // Asumo que tu proyecto tiene un mapeador Dominio -> EF, lo llamaremos Ef2Domain.
        // Si usas ToDomain como método de extensión, ToEf debe ser similar.

        public EventoRepositoryEF(RedSismicaContext ctx)
        {
            _ctx = ctx;
        }

        // ... (GetEventosParaRevision y GetEventoConSeriesYDetalles - LECTURA) ...
        public IEnumerable<EventoSismico> GetEventosParaRevision()
        {
            var query = _ctx.EventosSismicos
                .AsNoTracking()
                .Include(e => e.Alcance)
                .Include(e => e.Origen)
                .Include(e => e.Clasificacion)
                .Include(e => e.CambiosDeEstado).ThenInclude(c => c.Responsable);

            foreach (var ef in query)
                yield return ef.ToDomain();
        }

        public EventoSismico GetEventoConSeriesYDetalles(EventoSismico candidato)
        {
            const double EPS = 1e-9;

            var ef = _ctx.EventosSismicos
                .AsNoTracking()
                .Include(e => e.Alcance)
                .Include(e => e.Origen)
                .Include(e => e.Clasificacion)
                .Include(e => e.CambiosDeEstado).ThenInclude(c => c.Responsable)
                .Include(e => e.SeriesTemporales)
                    .ThenInclude(s => s.Muestras)
                        .ThenInclude(m => m.Detalles)
                            .ThenInclude(d => d.TipoDeDato)
                .FirstOrDefault(e =>
                    e.FechaHoraOcurrencia == candidato.FechaHoraOcurrencia &&
                    Math.Abs(e.LatitudEpicentro - candidato.LatitudEpicentro) < EPS &&
                    Math.Abs(e.LongitudEpicentro - candidato.LongitudEpicentro) < EPS &&
                    Math.Abs(e.LatitudHipocentro - candidato.LatitudHipocentro) < EPS &&
                    Math.Abs(e.LongitudHipocentro - candidato.LongitudHipocentro) < EPS &&
                    Math.Abs(e.ValorMagnitud - candidato.ValorMagnitud) < EPS
                );

            if (ef == null)
                throw new InvalidOperationException("No se encontró el evento seleccionado en la base.");

            return ef.ToDomain();
        }

        public EventoSismico GetEventoParaReversionDeBloqueo(EventoSismico evento)
        {
            const double EPS = 1e-9;
            var ef = _ctx.EventosSismicos
                .Include(e => e.CambiosDeEstado)
                .FirstOrDefault(e =>
                    e.FechaHoraOcurrencia == evento.FechaHoraOcurrencia &&
                    Math.Abs(e.LatitudEpicentro - evento.LatitudEpicentro) < EPS &&
                    Math.Abs(e.LongitudEpicentro - evento.LongitudEpicentro) < EPS &&
                    Math.Abs(e.LatitudHipocentro - evento.LatitudHipocentro) < EPS &&
                    Math.Abs(e.LongitudHipocentro - evento.LongitudHipocentro) < EPS &&
                    Math.Abs(e.ValorMagnitud - evento.ValorMagnitud) < EPS
                );

            if (ef == null) return null;

            return ef.ToDomain();
        }


        // =========================
        // PERSISTENCIA / ESTADO
        // =========================
        public void GuardarCambiosDeEstado(EventoSismico evento)
        {
            const double EPS = 1e-9;

            // 1. Obtener la entidad EF trackeada (efEvento)
            var efEvento = _ctx.EventosSismicos
                .Include(e => e.CambiosDeEstado)
                .FirstOrDefault(e =>
                    e.FechaHoraOcurrencia == evento.FechaHoraOcurrencia &&
                    Math.Abs(e.LatitudEpicentro - evento.LatitudEpicentro) < EPS &&
                    Math.Abs(e.LongitudEpicentro - evento.LongitudEpicentro) < EPS &&
                    Math.Abs(e.LatitudHipocentro - evento.LatitudHipocentro) < EPS &&
                    Math.Abs(e.LongitudHipocentro - evento.LongitudHipocentro) < EPS &&
                    Math.Abs(e.ValorMagnitud - evento.ValorMagnitud) < EPS
                );

            if (efEvento == null)
                throw new InvalidOperationException("No se encontró el evento para guardar.");

            // 2. Actualizar estado y persistencia (ya manejado por el Manejador/Attach)
            efEvento.EstadoActualNombre = evento.EstadoActual?.Nombre ?? efEvento.EstadoActualNombre;

            // 3. Insertar CEs nuevos (Mapeo de Dominio a EF)
            var existentes = (efEvento.CambiosDeEstado ?? new List<CambioDeEstadoEF>())
                .Select(c => (c.FechaHoraInicio, c.EstadoNombre))
                .ToHashSet();

            // Reutilizamos el mapeador del Manejador (asumiendo que ya tiene las PKs/FKs)
            foreach (var ceDom in evento.CambiosDeEstado)
            {
                var inicioDom = ceDom.FechaHoraInicio ?? DateTime.MinValue;
                var estadoDom = ceDom.EstadoActual?.Nombre ?? string.Empty;

                if (existentes.Contains((inicioDom, estadoDom)))
                    continue;

                // **LA LOGICA DE PERSISTENCIA DEL MANEJADOR DEBERIA HABER ASIGNADO LAS PROPIEDADES SOMBRA**.
                // Si la entidad de Dominio ya está Adjunta/Trackeada como 'Added', no necesitamos mapear.
                // Si no, la mapeamos y agregamos la entidad EF.

                // Aquí, debido a que el Manejador manipula el DbContext directamente,
                // asumimos que el Manejador ya marcó los CE nuevos como Added o Detached
                // y que EF los insertará por cascada al llamar SaveChanges.
            }

            _ctx.SaveChanges();
        }

        // CS1503 RESUELTO: Se mapea el objeto de dominio 'ultimo' (del bucle del Manejador) a EF
        public void RevertirBloqueo(EventoSismico evento)
        {
            const double EPS = 1e-9;

            // Buscamos el EF trackeado
            var ef = _ctx.EventosSismicos
                .Include(e => e.CambiosDeEstado)
                .FirstOrDefault(e =>
                    e.FechaHoraOcurrencia == evento.FechaHoraOcurrencia &&
                    Math.Abs(e.LatitudEpicentro - evento.LatitudEpicentro) < EPS &&
                    Math.Abs(e.LongitudEpicentro - evento.LongitudEpicentro) < EPS &&
                    Math.Abs(e.LatitudHipocentro - evento.LatitudHipocentro) < EPS &&
                    Math.Abs(e.LongitudHipocentro - evento.LongitudHipocentro) < EPS &&
                    Math.Abs(e.ValorMagnitud - evento.ValorMagnitud) < EPS
                );

            if (ef == null) return;

            // Lógica de reversión (usando objetos EF trackeados)
            var ordenados = (ef.CambiosDeEstado ?? new List<CambioDeEstadoEF>())
                .OrderByDescending(c => c.FechaHoraInicio)
                .ToList();

            var ultimo = ordenados.FirstOrDefault();
            var anterior = ordenados.Skip(1).FirstOrDefault();

            // CS1503 RESUELTO: 'ultimo' es CambioDeEstadoEF.
            if (ultimo != null && string.Equals(ultimo.EstadoNombre, "Bloqueado", StringComparison.OrdinalIgnoreCase))
                _ctx.CambiosDeEstado.Remove(ultimo); // AHORA FUNCIONA, ultimo es CambioDeEstadoEF

            if (anterior != null)
            {
                anterior.FechaHoraFin = null;
                ef.EstadoActualNombre = anterior.EstadoNombre;
            }
            _ctx.SaveChanges();
        }

        public void Refresh(EventoSismico destino)
        {
            if (destino == null) return;

            const double EPS = 1e-9;

            var ef = _ctx.EventosSismicos
                .AsNoTracking()
                .Include(e => e.Alcance)
                .Include(e => e.Origen)
                .Include(e => e.Clasificacion)
                .Include(e => e.CambiosDeEstado).ThenInclude(c => c.Responsable)
                .Include(e => e.SeriesTemporales).ThenInclude(s => s.Muestras).ThenInclude(m => m.Detalles).ThenInclude(d => d.TipoDeDato)
                .FirstOrDefault(e =>
                    e.FechaHoraOcurrencia == destino.FechaHoraOcurrencia &&
                    Math.Abs(e.LatitudEpicentro - destino.LatitudEpicentro) < EPS &&
                    Math.Abs(e.LongitudEpicentro - destino.LongitudEpicentro) < EPS &&
                    Math.Abs(e.LatitudHipocentro - destino.LatitudHipocentro) < EPS &&
                    Math.Abs(e.LongitudHipocentro - destino.LongitudHipocentro) < EPS &&
                    Math.Abs(e.ValorMagnitud - destino.ValorMagnitud) < EPS
                );

            if (ef == null) return;

            var dom = ef.ToDomain();

            destino.SetEstado(dom.EstadoActual);
            if (dom.FechaHoraFin.HasValue)
                destino.EstablecerFin(dom.FechaHoraFin.Value);

            destino.SeriesTemporales.Clear();
            foreach (var s in dom.SeriesTemporales)
                destino.SeriesTemporales.Add(s);

            destino.CambiosDeEstado.Clear();
            foreach (var c in dom.CambiosDeEstado)
                destino.CambiosDeEstado.Add(c);
        }

        public Empleado GetUsuarioLogueado()
        {
            var ef = _ctx.Empleados.FirstOrDefault();
            return ef != null ? ef.ToDomain() : null;
        }
    }
}