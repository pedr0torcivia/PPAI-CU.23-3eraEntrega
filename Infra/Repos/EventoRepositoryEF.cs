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

        public IEnumerable<EventoSismico> GetEventosAutoDetectadosNoRevisados()
        {
            var lista = _ctx.EventosSismicos
                .Include(e => e.CambiosDeEstado).ThenInclude(c => c.Responsable)
                .Include(e => e.Alcance)
                .Include(e => e.Clasificacion)
                .Include(e => e.Origen)
                .Include(e => e.Responsable)
                .AsNoTracking()
                .Where(e =>
                    e.EstadoActualNombre == "Autodetectado" ||
                    e.EstadoActualNombre == "Evento sin revisión") // ← filtro real
                .ToList();
           
            foreach (var e in lista)
            {
                e.MaterializarEstadoDesdeNombre();
                e.MaterializarEstadosDeCambios(); // ← agrega esto
            }

            return lista;
        }


        // Para modificar: CARGAR TRACKED (clave)
        public EventoSismico GetEventoConSeriesYDetalles(Guid eventoId)
        {
            var e = _ctx.EventosSismicos
                .AsTracking() // ← clave
                .Include(ev => ev.CambiosDeEstado).ThenInclude(c => c.Responsable)
                .Include(ev => ev.SeriesTemporales).ThenInclude(st => st.Muestras)
                    .ThenInclude(m => m.DetalleMuestraSismica)
                .Include(ev => ev.Alcance)
                .Include(ev => ev.Clasificacion)
                .Include(ev => ev.Origen)
                .Include(ev => ev.Responsable)
                .FirstOrDefault(ev => ev.Id == eventoId);

            e?.MaterializarEstadoDesdeNombre();
            return e;

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