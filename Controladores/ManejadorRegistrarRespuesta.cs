using PPAI;
using PPAI_Revisiones.Boundary;
using PPAI_Revisiones.Modelos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PPAI_Revisiones.Controladores
{
    public class ManejadorRegistrarRespuesta
    {
        private List<EventoSismico> eventosAutodetectadosNoRevisados;
        private EventoSismico eventoSeleccionado;
        private EventoSismico eventoBloqueadoTemporal;
        private DateTime fechaHoraActual;
        private Empleado usuarioLogueado;
        private string detallesEvento;

        public List<object> RegistrarNuevaRevision(PantallaNuevaRevision pantalla)
        {
            eventosAutodetectadosNoRevisados = BuscarEventosAutoDetecNoRev();
            OrdenarEventos();

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

        private List<EventoSismico> BuscarEventosAutoDetecNoRev()
        {
            List<EventoSismico> lista = DatosMock.Eventos.Where(e =>
            {
                var estado = e.getEstado();
                return estado != null && (estado.EsNoRevisado() || estado.EsAutodetectado());
            }).ToList();

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

        public void TomarSeleccionEvento(int indice, PantallaNuevaRevision pantalla)
        {
            var nuevoEvento = eventosAutodetectadosNoRevisados[indice];

            if (eventoBloqueadoTemporal != null && eventoBloqueadoTemporal != nuevoEvento)
                RevertirBloqueo(eventoBloqueadoTemporal);

            eventoSeleccionado = nuevoEvento;
            eventoBloqueadoTemporal = nuevoEvento;

            ActualizarEventoBloqueado();
            pantalla.MostrarMensaje("El evento ha sido BLOQUEADO para su revisión.");

            BuscarDetallesEventoSismico();
            string sismograma = GenerarSismograma();
            pantalla.MostrarDetalleEventoSismico(detallesEvento);
            pantalla.MostrarSismograma(sismograma);
            HabilitarOpcionVisualizarMapa(pantalla);
        }

        private void ActualizarEventoBloqueado()
        {
            Estado estadoBloqueado = BuscarEstadoBloqueado();
            usuarioLogueado = BuscarUsuarioLogueado();
            fechaHoraActual = GetFechaHora();

            eventoSeleccionado.RegistrarEstadoBloqueado(estadoBloqueado, usuarioLogueado.Nombre, fechaHoraActual);

        }

        private Estado BuscarEstadoBloqueado()
        {
            return DatosMock.Estados.FirstOrDefault(e => e.EsAmbitoEvento() && e.EsBloqueado());
        }

        private Empleado BuscarUsuarioLogueado()
        {
            var usuario = DatosMock.SesionActual.GetUsuario();
            return DatosMock.Empleados.FirstOrDefault(e => e.EsTuUsuario(usuario.NombreUsuario));
        }

        private DateTime GetFechaHora()
        {
            return DateTime.Now;
        }

        private void BuscarDetallesEventoSismico()
        {
            detallesEvento = eventoSeleccionado?.GetDetalleEventoSismico() ?? "(Evento nulo)";
        }

        private string GenerarSismograma()
        {
            Console.WriteLine("[Manejador] → GenerarSismograma() ejecutado (Extensión CU)");

            // <<include>> al caso de uso extendido
            var extensionCU = new CU_GenerarSismograma();
            return extensionCU.Ejecutar(); // delega la generación del sismograma
        }

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

        public void HabilitarModificacionAlcance(PantallaNuevaRevision pantalla)
        {
            pantalla.OpcionModificacionAlcance();
        }

        public void TomarOpcionModificacionAlcance(bool modificar, PantallaNuevaRevision pantalla)
        {
            if (!modificar)
                Console.WriteLine("Actor eligió NO modificar Alcance.");
        }

        public void HabilitarModificacionMagnitud(PantallaNuevaRevision pantalla)
        {
            pantalla.OpcionModificacionMagnitud();
        }

        public void TomarOpcionModificacionMagnitud(bool modificar, PantallaNuevaRevision pantalla)
        {
            if (!modificar)
                Console.WriteLine("Actor eligió NO modificar Magnitud.");
        }

        public void HabilitarModificacionOrigen(PantallaNuevaRevision pantalla)
        {
            pantalla.OpcionModificacionOrigen();
        }

        public void TomarOpcionModificacionOrigen(bool modificar, PantallaNuevaRevision pantalla)
        {
            if (!modificar)
                Console.WriteLine("Actor eligió NO modificar Origen.");
        }

        public void TomarOpcionAccion(int opcion, PantallaNuevaRevision pantalla)
        {
            if (!ValidarAccion(opcion, pantalla))
            {
                pantalla.MostrarMensaje("Faltan datos obligatorios o acción inválida.");
                return;
            }

            switch (opcion)
            {
                case 1:
                    pantalla.MostrarMensaje("Evento confirmado correctamente.");
                    break;
                case 2:
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
                        var resp = cambio.Responsable ?? "(desconocido)";
                        mensaje += $"- {nombre}: {inicio} → {fin} | Responsable: {resp}\n";
                    }
                    pantalla.MostrarMensaje(mensaje);
                    break;
                case 3:
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

        private void ActualizarEstadoRechazado(PantallaNuevaRevision pantalla)
        {
            if (eventoSeleccionado == null)
            {
                pantalla.MostrarMensaje("Error interno: no hay evento seleccionado.");
                return;
            }

            Estado estadoRechazado = DatosMock.Estados
                .First(e => e.EsAmbitoEvento() && e.EsRechazado());

            fechaHoraActual = GetFechaHora();

            eventoSeleccionado.Rechazar(
                estadoRechazado,
                usuarioLogueado?.Nombre ?? "(desconocido)",
                fechaHoraActual
            );

            // REGENERA LISTA VISUAL PARA LA PANTALLA
            eventosAutodetectadosNoRevisados.Remove(eventoSeleccionado);

            var listaVisual = eventosAutodetectadosNoRevisados.Select(e => new
            {
                Fecha = e.FechaHoraInicio,
                LatEpicentro = e.GetLatitudEpicentro(),
                LongEpicentro = e.GetLongitudEpicentro(),
                LatHipocentro = e.GetLatitudHipocentro(),
                LongHipocentro = e.GetLongitudHipocentro(),
                Magnitud = e.GetMagnitud(),
            }).Cast<object>().ToList();
            // ACTUALIZAR GRILLA EN LA PANTALLA
            pantalla.SolicitarSeleccionEvento(listaVisual);
            // LIMPIAR CONTROLES
            pantalla.RestaurarEstadoInicial();
        }

        private void RevertirBloqueo(EventoSismico evento)
        {
            var ultimo = evento.CambiosDeEstado.LastOrDefault();
            if (ultimo != null && ultimo.EstadoActual.EsBloqueado())
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

            pantalla.RestaurarEstadoInicial(); //PARA LOS ELEMENTOS VISUALES
            pantalla.SolicitarSeleccionEvento(listaVisual);
        }
    }
}
