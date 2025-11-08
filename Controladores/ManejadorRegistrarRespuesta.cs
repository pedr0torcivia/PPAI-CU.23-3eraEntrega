using PPAI_2.Infra.Data;
using PPAI_Revisiones.Boundary;
using PPAI_Revisiones.Modelos;
using PPAI_Revisiones.Modelos.Estados;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;


namespace PPAI_Revisiones.Controladores
{
    public class ManejadorRegistrarRespuesta
    {
        // ==== Dependencias (solo a través del repo; nunca tocamos EF acá) ====
        private readonly RedSismicaContext _ctx;
        private readonly IEventoRepository _repo;

        // === Variables de control del CU ===
        private List<EventoSismico> eventosAutodetectadosNoRevisados;
        private EventoSismico eventoSeleccionado;
        private EventoSismico eventoBloqueadoTemporal;

        private DateTime fechaHoraActual;
        private Empleado responsable;
        private string detallesEvento;

        public ManejadorRegistrarRespuesta(RedSismicaContext ctx, IEventoRepository repo)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        // ================== FLUJO PRINCIPAL ==================
        public List<object> RegistrarNuevaRevision(PantallaNuevaRevision pantalla)
        {
            // 1) Buscar candidatos (Autodetectados / sin revisión)
            eventosAutodetectadosNoRevisados = BuscarEventosAutoDetecNoRev();

            // 2) Ordenar por fecha de ocurrencia (desc)
            OrdenarEventos();

            // 3) Proyección SOLO para la grilla
            var listaProyectada = eventosAutodetectadosNoRevisados
                .Select(e => new
                {
                    e.FechaHoraOcurrencia,
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
            var todos = _repo.GetEventosParaRevision().ToList();

            var resultado = new List<EventoSismico>();
            foreach (var e in todos)
            {
                // materializa estado actual + estados de cambios (dominio)
                e.MaterializarEstadoDesdeNombre();
                e.MaterializarEstadosDeCambios();

                if (e.sosAutodetectado() || e.sosEventoSinRevision())
                {
                    // Logging útil en consola
                    Console.WriteLine($"[EventoSismico] {e.GetDatosOcurrencia()}");
                    resultado.Add(e);
                }
            }
            return resultado;
        }

        private void OrdenarEventos()
        {
            eventosAutodetectadosNoRevisados = eventosAutodetectadosNoRevisados
                .OrderByDescending(e => e.FechaHoraOcurrencia)
                .ToList();
        }

        // ================== SELECCIÓN Y BLOQUEO ==================
        public void TomarSeleccionEvento(int indice, PantallaNuevaRevision pantalla)
        {
            var nuevoEvento = eventosAutodetectadosNoRevisados[indice];

            // Si había otro bloqueado temporal, revertir
            if (eventoBloqueadoTemporal != null && eventoBloqueadoTemporal != nuevoEvento)
                RevertirBloqueo(eventoBloqueadoTemporal);

            // Cargar el seleccionado con series y detalles (desde repo, por coincidencia de atributos)
            eventoSeleccionado = _repo.GetEventoConSeriesYDetalles(nuevoEvento);
            eventoBloqueadoTemporal = eventoSeleccionado;

            // Autodetectado → Bloqueado (State en dominio)
            ActualizarEventoBloqueado();

            // Refrescar grilla
            ReaplicarFiltroYPintar(pantalla);
            pantalla.MostrarMensaje("El evento ha sido BLOQUEADO para su revisión.");

            // Detalles y sismograma
            BuscarDetallesEventoSismico();
            var sismograma = GenerarSismograma();
            pantalla.MostrarDetalleEventoSismico(detallesEvento);
            pantalla.MostrarSismograma(sismograma);

            // UI siguiente paso
            HabilitarOpcionVisualizarMapa(pantalla);
        }

        private void ActualizarEventoBloqueado()
        {
            responsable = BuscarUsuarioLogueado();
            fechaHoraActual = GetFechaHora();

            // Delegación al dominio (State)
            eventoSeleccionado.RegistrarEstadoBloqueado(fechaHoraActual, responsable);

            // Persistencia delegada al repositorio
            _repo.GuardarCambiosDeEstado(eventoSeleccionado);
        }

        private Empleado BuscarUsuarioLogueado() => _repo.GetUsuarioLogueado();

        private DateTime GetFechaHora() => DateTime.Now;

        // ================== DETALLE Y SISMOGRAMA ==================
        private void BuscarDetallesEventoSismico()
        {
            detallesEvento = eventoSeleccionado?.GetDetalleEventoSismico() ?? "(Evento nulo)";
            MessageBox.Show(detallesEvento, "TRACE - Series y Muestras por estación",
                 MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        // ================== OPCIONES UI ==================
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
            if (!ValidarAccion(opcion))
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

        private bool ValidarAccion(int opcion)
        {
            if (opcion < 1 || opcion > 3) return false;
            if (eventoSeleccionado == null) return false;

            if (eventoSeleccionado.Alcance == null ||
                eventoSeleccionado.OrigenDeGeneracion == null ||
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
                responsable = BuscarUsuarioLogueado();

            // Bloqueado → Rechazado (State en dominio)
            eventoSeleccionado.Rechazar(fechaHoraActual, responsable);

            // Persistencia y refresco delegados al repo
            _repo.GuardarCambiosDeEstado(eventoSeleccionado);
            _repo.Refresh(eventoSeleccionado);

            // Rematerializar estados de dominio
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
            _repo.RevertirBloqueo(ev);
        }

        // ================== REINICIAR CU ==================
        private List<object> ProyectarParaGrilla(IEnumerable<EventoSismico> src) =>
            src.Select(e => new
            {
                e.FechaHoraOcurrencia,
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

                if (e.EstadoActual.Nombre == "Autodetectado" || e.EstadoActual.Nombre == "Bloqueado")
                    candidatos.Add(e);
            }

            eventosAutodetectadosNoRevisados = candidatos
                .OrderByDescending(x => x.FechaHoraOcurrencia)
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
                var nombre = c.EstadoActual?.Nombre ?? c.GetEstadoNombre() ?? "(sin estado)";
                var inicio = c.FechaHoraInicio?.ToString("g") ?? "(sin inicio)";
                var fin = c.FechaHoraFin?.ToString("g") ?? "(en curso)";
                var resp = c.ResponsableInspeccion?.Nombre ?? "(desconocido)";
                msg += $"- {nombre}: {inicio} → {fin} | Responsable: {resp}\n";
            }
            pantalla.MostrarMensaje(msg);
        }
    }
}
