using Microsoft.EntityFrameworkCore;
using PPAI_2.Infra.Data;
using PPAI_2.Infra.Repos;
using PPAI_Revisiones.Boundary;
using PPAI_Revisiones.Modelos;
using PPAI_Revisiones.Modelos.Estados;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PPAI_Revisiones.Controladores
{
    public class ManejadorRegistrarRespuesta
    {
        // Infra (EF)
        private readonly RedSismicaContext _ctx;
        private readonly IEventoRepository _repo;

        // === Variables de control del CU ===
        private List<EventoSismico> eventosAutodetectadosNoRevisados;
        private EventoSismico eventoSeleccionado;
        private EventoSismico eventoBloqueadoTemporal;

        private DateTime fechaHoraActual;
        private Empleado responsable;

        private string detallesEvento;

        public ManejadorRegistrarRespuesta()
        {
            _ctx = new RedSismicaContext();
            _repo = new EventoRepositoryEF(_ctx);
        }

        // ================== FLUJO PRINCIPAL ==================
        public List<object> RegistrarNuevaRevision(PantallaNuevaRevision pantalla)
        {
            // 1) Cargar candidatos (no hace falta que estén trackeados todavía)
            eventosAutodetectadosNoRevisados = BuscarEventosAutoDetecNoRev();

            // 2) Ordenar
            OrdenarEventos();

            // 3) Proyección SOLO para la grilla
            var listaProyectada = eventosAutodetectadosNoRevisados
                .Select(e => new
                {
                    e.FechaHoraInicio,
                    e.LatitudEpicentro,
                    e.LongitudEpicentro,
                    e.LatitudHipocentro,
                    e.LongitudHipocentro,
                    e.ValorMagnitud
                })
                .Cast<object>()
                .ToList();

            pantalla.SolicitarSeleccionEvento(listaProyectada);
            return listaProyectada;
        }

        // ================== BÚSQUEDA Y ORDEN ==================
        private List<EventoSismico> BuscarEventosAutoDetecNoRev()
        {
            // Este método del repo puede devolver AsNoTracking; acá solo listamos/filtramos.
            var todos = _repo.GetEventosParaRevision().ToList();

            var resultado = new List<EventoSismico>();
            foreach (var e in todos)
            {
                e.MaterializarEstadoDesdeNombre();
                e.MaterializarEstadosDeCambios();

                if (e.sosAutodetectado() || e.sosEventoSinRevision())
                {
                    Console.WriteLine($"[EventoSismico] {e.GetDatosOcurrencia()}");
                    resultado.Add(e);
                }
            }
            return resultado;
        }

        private void OrdenarEventos()
        {
            eventosAutodetectadosNoRevisados = eventosAutodetectadosNoRevisados
              .OrderByDescending(e => e.FechaHoraInicio)
              .ToList();
        }

        // ================== SELECCIÓN Y BLOQUEO ==================
        public void TomarSeleccionEvento(int indice, PantallaNuevaRevision pantalla)
        {
            var nuevoEvento = eventosAutodetectadosNoRevisados[indice];

            // Si había otro bloqueado temporal, revertir (A) con un ciclo de persistencia separable
            if (eventoBloqueadoTemporal != null && eventoBloqueadoTemporal != nuevoEvento)
                RevertirBloqueo(eventoBloqueadoTemporal);

            // 🔧 FIX: limpiar tracker entre A y B para evitar “contaminación”
            _ctx.ChangeTracker.Clear();

            // Cargar B trackeado y con sus relaciones completas
            eventoSeleccionado = _repo.GetEventoConSeriesYDetalles(nuevoEvento.Id);
            eventoBloqueadoTemporal = eventoSeleccionado;

            // Autodetectado → Bloqueado
            ActualizarEventoBloqueado();

            // Refiltrar y repintar
            ReaplicarFiltroYPintar(pantalla);
            pantalla.MostrarMensaje("El evento ha sido BLOQUEADO para su revisión.");

            // Detalles y sismograma
            BuscarDetallesEventoSismico();
            var sismograma = GenerarSismograma();
            pantalla.MostrarDetalleEventoSismico(detallesEvento);
            pantalla.MostrarSismograma(sismograma);

            HabilitarOpcionVisualizarMapa(pantalla);
        }

        private void ActualizarEventoBloqueado()
        {
            responsable = BuscarUsuarioLogueado();
            fechaHoraActual = GetFechaHora();

            // Delego en el Evento (State): crea CE nuevo y setea EstadoActual/EstadoActualNombre
            eventoSeleccionado.RegistrarEstadoBloqueado(fechaHoraActual, responsable);

            // Adjuntar el evento si está detached
            if (_ctx.Entry(eventoSeleccionado).State == EntityState.Detached)
                _ctx.EventosSismicos.Attach(eventoSeleccionado);

            // Solo marcá modificado el nombre de estado del evento
            _ctx.Entry(eventoSeleccionado).Property(e => e.EstadoActualNombre).IsModified = true;

            // === NO hacer AddRange manual si EF ya los detectó ===
            // Si algún CE está completamente detached, lo preparo mínimamente y lo marco Added.
            // OJO: no agregues a DbSet algo que EF ya está trackeando como Added o Modified.
            foreach (var ce in eventoSeleccionado.CambiosDeEstado)
            {
                var entry = _ctx.Entry(ce);

                if (entry.State == EntityState.Detached)
                {
                    // FK asegurada por las dudas
                    ce.EventoSismicoId = eventoSeleccionado.Id;

                    // Que nunca vaya vacío
                    if (ce.Id == Guid.Empty) ce.Id = Guid.NewGuid();

                    // Marcamos Added (de lo contrario, al estar detached no entra por cascada)
                    _ctx.Entry(ce).State = EntityState.Added;
                }
                else if (entry.State == EntityState.Added)
                {
                    // Nada que hacer: ya está para insertarse una sola vez
                }
                else
                {
                    // Modified/Unchanged/Deleted: no toco nada
                }
            }

            // Persistir
            _repo.Guardar();
        }

        private Empleado BuscarUsuarioLogueado()
        {
            return _ctx.Empleados
                       .Include(e => e.Usuario)
                       .FirstOrDefault();
        }

        private DateTime GetFechaHora() => DateTime.Now;

        // ================== DETALLE Y SISMOGRAMA ==================
        private void BuscarDetallesEventoSismico()
        {
            detallesEvento = eventoSeleccionado?.GetDetalleEventoSismico() ?? "(Evento nulo)";
        }

        private string GenerarSismograma()
        {
            Console.WriteLine("[Manejador] → GenerarSismograma() ejecutado (Extensión CU)");
            var extensionCU = new CU_GenerarSismograma();

            var ruta = extensionCU.Ejecutar();
            ruta = ruta?.Trim().Trim('"');

            var exists = System.IO.File.Exists(ruta);
            Console.WriteLine($"[Manejador] Ruta devuelta por CU: '{ruta}' | Exists={exists}");

            return ruta;
        }

        // ================== OPCIONES UI (Mapa y Modificaciones) ==================
        public void HabilitarOpcionVisualizarMapa(PantallaNuevaRevision pantalla)
        {
            pantalla.opcionMostrarMapa();
            pantalla.MostrarBotonCancelar();
        }

        public void TomarDecisionVisualizarMapa(bool deseaVerMapa, PantallaNuevaRevision pantalla)
        {
            HabilitarModificacionAlcance(pantalla);
            HabilitarModificacionMagnitud(pantalla);
            HabilitarModificacionOrigen(pantalla);
            pantalla.SolicitarSeleccionAcciones();
        }

        public void HabilitarModificacionAlcance(PantallaNuevaRevision pantalla) => pantalla.OpcionModificacionAlcance();
        public void TomarOpcionModificacionAlcance(bool modificar, PantallaNuevaRevision pantalla)
        {
            if (!modificar) Console.WriteLine("Actor eligió NO modificar Alcance.");
        }

        public void HabilitarModificacionMagnitud(PantallaNuevaRevision pantalla) => pantalla.OpcionModificacionMagnitud();
        public void TomarOpcionModificacionMagnitud(bool modificar, PantallaNuevaRevision pantalla)
        {
            if (!modificar) Console.WriteLine("Actor eligió NO modificar Magnitud.");
        }

        public void HabilitarModificacionOrigen(PantallaNuevaRevision pantalla) => pantalla.OpcionModificacionOrigen();
        public void TomarOpcionModificacionOrigen(bool modificar, PantallaNuevaRevision pantalla)
        {
            if (!modificar) Console.WriteLine("Actor eligió NO modificar Origen.");
        }

        // ================== ACCIÓN FINAL (Confirmar / Rechazar / Derivar) ==================
        public void TomarOpcionAccion(int opcion, PantallaNuevaRevision pantalla)
        {
            if (!ValidarAccion(opcion, pantalla))
            {
                pantalla.MostrarMensaje("Faltan datos obligatorios o acción inválida.");
                return;
            }

            switch (opcion)
            {
                case 1: // Confirmar
                    pantalla.MostrarMensaje("Evento confirmado correctamente.");
                    break;

                case 2: // Rechazar (Bloqueado → Rechazado)
                    if (eventoSeleccionado == null)
                    {
                        pantalla.MostrarMensaje("Error: evento seleccionado nulo.");
                        return;
                    }

                    ActualizarEstadoRechazado(pantalla);
                    break;

                case 3: // Derivar
                    pantalla.MostrarMensaje("Evento derivado a experto.");
                    break;
            }

            Console.WriteLine("FinCU()");
        }

        private bool ValidarAccion(int opcion, PantallaNuevaRevision pantalla)
        {
            if (opcion < 1 || opcion > 3) return false;
            if (eventoSeleccionado == null) return false;

            if (eventoSeleccionado.Alcance == null ||
                eventoSeleccionado.Origen == null ||
                eventoSeleccionado.ValorMagnitud <= 0)
                return false;

            return true;
        }

        // ================== RECHAZO (Bloqueado → Rechazado) ==================
        private void ActualizarEstadoRechazado(PantallaNuevaRevision pantalla)
        {
            if (eventoSeleccionado == null)
            {
                pantalla.MostrarMensaje("Error interno: no hay evento seleccionado.");
                return;
            }

            fechaHoraActual = GetFechaHora();
            if (responsable == null)
                responsable = _ctx.Empleados.Include(e => e.Usuario).FirstOrDefault();

            // Bloqueado → Rechazado (misma instancia trackeada)
            eventoSeleccionado.Rechazar(fechaHoraActual, responsable);

            // 🔧 FIX: Adjuntar si hace falta y marcar solo lo necesario
            if (_ctx.Entry(eventoSeleccionado).State == EntityState.Detached)
                _ctx.EventosSismicos.Attach(eventoSeleccionado);

            _ctx.Entry(eventoSeleccionado).Property(e => e.EstadoActualNombre).IsModified = true;

            // 🔧 FIX: NO AddRange manual; setear Added solo si están detached
            foreach (var ce in eventoSeleccionado.CambiosDeEstado)
            {
                var entry = _ctx.Entry(ce);
                if (entry.State == EntityState.Detached)
                {
                    if (ce.Id == Guid.Empty) ce.Id = Guid.NewGuid();
                    ce.EventoSismicoId = eventoSeleccionado.Id;
                    _ctx.Entry(ce).State = EntityState.Added;
                }
            }

            _repo.Guardar();

            // Refrescar instancia
            _ctx.Entry(eventoSeleccionado).Reload();
            _ctx.Entry(eventoSeleccionado)
                .Collection(e => e.CambiosDeEstado)
                .Query()
                .Include(c => c.Responsable)
                .Load();

            eventoSeleccionado.MaterializarEstadoDesdeNombre();
            eventoSeleccionado.MaterializarEstadosDeCambios();

            MostrarMensajeCambios(pantalla, eventoSeleccionado);

            ReaplicarFiltroYPintar(pantalla);
            pantalla.RestaurarEstadoInicial();
        }

        // ================== REVERSIÓN DE BLOQUEO TEMPORAL ==================
        private void RevertirBloqueo(EventoSismico ev)
        {
            if (ev == null) return;

            // Detach si hay una copia local trackeada
            var currentTracking = _ctx.EventosSismicos.Local.FirstOrDefault(e => e.Id == ev.Id);
            if (currentTracking != null)
                _ctx.Entry(currentTracking).State = EntityState.Detached;

            // Nueva instancia limpia
            var evTracked = _repo.GetEventoParaReversionDeBloqueo(ev.Id);
            if (evTracked == null) return;

            // Eliminar último "Bloqueado" y reabrir anterior
            var ordenados = evTracked.CambiosDeEstado
                .OrderByDescending(c => c.FechaHoraInicio ?? DateTime.MinValue)
                .ToList();

            var ultimo = ordenados.FirstOrDefault();
            var anterior = ordenados.Skip(1).FirstOrDefault();

            if (ultimo != null && string.Equals(ultimo.EstadoNombre, "Bloqueado", StringComparison.OrdinalIgnoreCase))
            {
                _ctx.CambiosDeEstado.Remove(ultimo);
            }

            if (anterior != null && anterior.FechaHoraFin.HasValue)
            {
                anterior.FechaHoraFin = null;
                evTracked.EstadoActualNombre = anterior.EstadoNombre;
                evTracked.MaterializarEstadoDesdeNombre();
            }

            _repo.Guardar();

            // 🔧 FIX: cortar aquí el ciclo de tracking de A
            _ctx.ChangeTracker.Clear();
        }

        // ================== REINICIAR CU ==================
        private List<object> ProyectarParaGrilla(IEnumerable<EventoSismico> src) =>
            src.Select(e => new
            {
                e.FechaHoraInicio,
                e.LatitudEpicentro,
                e.LongitudEpicentro,
                e.LatitudHipocentro,
                e.LongitudHipocentro,
                e.ValorMagnitud
            })
            .Cast<object>()
            .ToList();

        public void ReiniciarCU(PantallaNuevaRevision pantalla)
        {
            if (eventoBloqueadoTemporal != null)
            {
                RevertirBloqueo(eventoBloqueadoTemporal);
                eventoBloqueadoTemporal = null;
            }

            eventoSeleccionado = null;

            eventosAutodetectadosNoRevisados = BuscarEventosAutoDetecNoRev();
            OrdenarEventos();

            var proyeccion = ProyectarParaGrilla(eventosAutodetectadosNoRevisados);
            pantalla.RestaurarEstadoInicial();
            pantalla.SolicitarSeleccionEvento(proyeccion);
        }

        // Recalcula la lista de candidatos
        private void ReaplicarFiltroYPintar(PantallaNuevaRevision pantalla)
        {
            var todos = _repo.GetEventosParaRevision().ToList();

            var candidatos = new List<EventoSismico>();
            foreach (var e in todos)
            {
                e.MaterializarEstadoDesdeNombre();
                e.MaterializarEstadosDeCambios();

                if (e.EstadoActualNombre == "Autodetectado" || e.EstadoActualNombre == "Bloqueado")
                    candidatos.Add(e);
            }

            eventosAutodetectadosNoRevisados = candidatos
              .OrderByDescending(x => x.FechaHoraInicio)
              .ToList();

            var proyeccion = ProyectarParaGrilla(eventosAutodetectadosNoRevisados);
            pantalla.SolicitarSeleccionEvento(proyeccion);
        }

        // HELPER PARA Msj
        private void MostrarMensajeCambios(PantallaNuevaRevision pantalla, EventoSismico ev)
        {
            var msg = "Evento rechazado correctamente.\n\n";
            msg += ev.GetDetalleEventoSismico() + "\n";
            msg += "CAMBIOS DE ESTADO:\n";

            foreach (var c in ev.CambiosDeEstado.OrderBy(x => x.FechaHoraInicio ?? DateTime.MinValue))
            {
                var nombre = c.EstadoActual?.Nombre ?? c.EstadoNombre ?? "(sin estado)";
                var inicio = c.FechaHoraInicio?.ToString("g") ?? "(sin inicio)";
                var fin = c.FechaHoraFin?.ToString("g") ?? "(en curso)";
                var resp = c.Responsable?.Nombre ?? "(desconocido)";
                msg += $"- {nombre}: {inicio} → {fin} | Responsable: {resp}\n";
            }
            pantalla.MostrarMensaje(msg);
        }
    }
}
