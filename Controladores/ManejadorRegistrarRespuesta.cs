using PPAI_2.Modelos;
using PPAI_Revisiones.Boundary;
using PPAI_Revisiones.Modelos;
using PPAI_Revisiones.Modelos.Estados;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PPAI_Revisiones.Controladores
{
    public class ManejadorRegistrarRespuesta
    {
        // === Variables de control del CU ===
        private List<EventoSismico> eventosAutodetectadosNoRevisados;
        private EventoSismico eventoSeleccionado;
        private EventoSismico eventoBloqueadoTemporal;

        private DateTime fechaHoraActual;
        private Empleado usuarioLogueado;

        private string detallesEvento;

        // ================== FLUJO PRINCIPAL ==================
        public List<object> RegistrarNuevaRevision(PantallaNuevaRevision pantalla)
        {
            // 1️⃣ Buscar eventos autodetectados o sin revisión
            eventosAutodetectadosNoRevisados = BuscarEventosAutoDetecNoRev();

            // 2️⃣ Ordenar por fecha
            OrdenarEventos();

            // 3️⃣ Mostrar lista visual
            var listaVisual = eventosAutodetectadosNoRevisados.Select(e => new
            {
                Fecha = e.FechaHoraInicio,
                LatEpicentro = e.GetLatitudEpicentro(),
                LongEpicentro = e.GetLongitudEpicentro(),
                LatHipocentro = e.GetLatitudHipocentro(),
                LongHipocentro = e.GetLongitudHipocentro(),
                Magnitud = e.GetMagnitud(),
            }).Cast<object>().ToList();

            pantalla.SolicitarSeleccionEvento(listaVisual);
            return listaVisual;
        }

        // ================== BÚSQUEDA Y ORDEN ==================
        private List<EventoSismico> BuscarEventosAutoDetecNoRev()
        {
            var lista = DatosMock.Eventos
                .Where(e => e.sosAutodetectado() || e.sosEventoSinRevision())
                .ToList();

            foreach (var evento in lista)
            {
                var datos = evento.GetDatosOcurrencia();
                Console.WriteLine($"[EventoSismico] getDatosOcurrencia(): {datos}");
            }

            return lista;
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

            // Si había otro bloqueado temporal, revertir
            if (eventoBloqueadoTemporal != null && eventoBloqueadoTemporal != nuevoEvento)
                RevertirBloqueo(eventoBloqueadoTemporal);

            eventoSeleccionado = nuevoEvento;
            eventoBloqueadoTemporal = nuevoEvento;

            // Autodetectado → Bloqueado
            ActualizarEventoBloqueado();

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
            usuarioLogueado = BuscarUsuarioLogueado();
            fechaHoraActual = GetFechaHora();

            // Delego en el Evento → Estado Autodetectado maneja la transición
            eventoSeleccionado.RegistrarEstadoBloqueado(fechaHoraActual, usuarioLogueado);
        }

        private Empleado BuscarUsuarioLogueado()
        {
            var usuario = DatosMock.SesionActual.GetUsuario();
            return DatosMock.Empleados.FirstOrDefault(e => e.EsTuUsuario(usuario.NombreUsuario));
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
            return extensionCU.Ejecutar();
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

                    string mensaje = "Evento rechazado correctamente.\n\n";
                    mensaje += eventoSeleccionado.GetDetalleEventoSismico() + "\n";
                    mensaje += "CAMBIOS DE ESTADO:\n";
                    foreach (var cambio in eventoSeleccionado.CambiosDeEstado)
                    {
                        var nombre = cambio.EstadoActual?.Nombre ?? "(sin estado)";
                        var inicio = cambio.FechaHoraInicio?.ToString("g") ?? "(sin inicio)";
                        var fin = cambio.FechaHoraFin?.ToString("g") ?? "(en curso)";
                        var resp = cambio.Responsable?.Nombre ?? "(desconocido)";

                        mensaje += $"- {nombre}: {inicio} → {fin} | Responsable: {resp}\n";
                    }
                    pantalla.MostrarMensaje(mensaje);
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

            bool modificarAlcance = pantalla.SeleccionoModificarAlcance;
            bool modificarMagnitud = pantalla.SeleccionoModificarMagnitud;
            bool modificarOrigen = pantalla.SeleccionoModificarOrigen;
            bool deseaMapa = pantalla.SeleccionoVisualizarMapa;

            if (modificarAlcance || modificarMagnitud || modificarOrigen || deseaMapa)
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

            // Delego en Evento → Estado Bloqueado maneja la transición
            eventoSeleccionado.Rechazar(fechaHoraActual, usuarioLogueado);

            // Quitar evento rechazado de la lista visual
            eventosAutodetectadosNoRevisados.Remove(eventoSeleccionado);

            // Regenerar grilla
            var listaVisual = eventosAutodetectadosNoRevisados.Select(e => new
            {
                Fecha = e.FechaHoraInicio,
                LatEpicentro = e.GetLatitudEpicentro(),
                LongEpicentro = e.GetLongitudEpicentro(),
                LatHipocentro = e.GetLatitudHipocentro(),
                LongHipocentro = e.GetLongitudHipocentro(),
                Magnitud = e.GetMagnitud(),
            }).Cast<object>().ToList();

            pantalla.SolicitarSeleccionEvento(listaVisual);
            pantalla.RestaurarEstadoInicial();
        }

        // ================== REVERSIÓN DE BLOQUEO TEMPORAL ==================
        private void RevertirBloqueo(EventoSismico evento)
        {
            var ultimo = evento.CambiosDeEstado.LastOrDefault();
            if (ultimo != null && ultimo.EstadoActual is Bloqueado)
            {
                evento.CambiosDeEstado.Remove(ultimo);

                var anterior = evento.CambiosDeEstado.LastOrDefault();
                if (anterior != null)
                {
                    anterior.FechaHoraFin = null;
                    evento.SetEstado(anterior.EstadoActual);
                }
            }
        }

        // ================== REINICIAR CU ==================
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

            var listaVisual = eventosAutodetectadosNoRevisados.Select(e => new
            {
                Fecha = e.FechaHoraInicio,
                Magnitud = e.GetMagnitud(),
                LatEpicentro = e.GetLatitudEpicentro(),
                LongEpicentro = e.GetLongitudEpicentro(),
                LatHipocentro = e.GetLatitudHipocentro(),
                LongHipocentro = e.GetLongitudHipocentro()
            }).Cast<object>().ToList();

            pantalla.RestaurarEstadoInicial();
            pantalla.SolicitarSeleccionEvento(listaVisual);
        }
    }
}
