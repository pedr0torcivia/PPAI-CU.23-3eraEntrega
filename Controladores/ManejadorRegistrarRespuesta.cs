using Microsoft.EntityFrameworkCore;
using PPAI_2.Infra.Data;
using PPAI_2.Infra.Repos;
using PPAI_Revisiones.Boundary;
using PPAI_Revisiones.Modelos;
using PPAI_Revisiones.Modelos.Estados;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PPAI_Revisiones.Controladores
{
    public class ManejadorRegistrarRespuesta : IDisposable
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
            // 1) Buscar y 2) Ordenar
            eventosAutodetectadosNoRevisados = BuscarEventosAutoDetecNoRev();
            OrdenarEventos();

            // 3) Proyectar SOLO las columnas que querés mostrar
            var lista = eventosAutodetectadosNoRevisados
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

            // Enviar a la pantalla la lista PROYECTADA (no la entidad completa)
            pantalla.SolicitarSeleccionEvento(lista);

            // Devolver lo mismo por consistencia (si lo usás en otro lado)
            return lista;
        }

        // ================== BÚSQUEDA Y ORDEN ==================
        private List<EventoSismico> BuscarEventosAutoDetecNoRev()
        {
            // Usa el repositorio EF (último cambio de estado abierto)
            var lista = _repo.GetEventosAutoDetectadosNoRevisados().ToList();

            // Log opcional
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

            eventoSeleccionado = _repo.GetEventoConSeriesYDetalles(nuevoEvento.Id);
            eventoBloqueadoTemporal = eventoSeleccionado;

            // Autodetectado → Bloqueado (delegado al estado del evento)
            ActualizarEventoBloqueado();
            _repo.Guardar(); // persistir

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

            // Delego en el Evento → Estado Autodetectado maneja la transición
            eventoSeleccionado.RegistrarEstadoBloqueado(fechaHoraActual, responsable);
        }

        private Empleado BuscarUsuarioLogueado()
        {
            // Sin DatosMock: tomamos el primer Empleado disponible (o null si no hay)
            // Si querés algo más específico, filtrá por Usuario.NombreUsuario
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
            ruta = ruta?.Trim().Trim('"'); // por si vienen comillas/espacios

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
                    _repo.Guardar(); // persistir
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
            eventoSeleccionado.Rechazar(fechaHoraActual, responsable);

            // Quitar evento rechazado de la lista en memoria
            eventosAutodetectadosNoRevisados.Remove(eventoSeleccionado);

            // Regenerar grilla con los eventos restantes
            var lista = eventosAutodetectadosNoRevisados.Cast<object>().ToList();
            pantalla.SolicitarSeleccionEvento(lista);
            pantalla.RestaurarEstadoInicial();

            // Mensaje con historial
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
        }

        // ================== REVERSIÓN DE BLOQUEO TEMPORAL ==================
        private void RevertirBloqueo(EventoSismico evento)
        {
            // 1) Volver a cargar el evento TRACKED (grafo completo) por Id
            var ev = _repo.GetEventoConSeriesYDetalles(evento.Id);
            if (ev == null) return;

            // 2) Tomar el último cambio y el anterior (orden por fecha)
            var ordenados = ev.CambiosDeEstado
                .OrderByDescending(c => c.FechaHoraInicio ?? DateTime.MinValue)
                .ToList();

            var ultimo = ordenados.FirstOrDefault();
            var anterior = ordenados.Skip(1).FirstOrDefault();

            // 3) Si el último es Bloqueado → eliminarlo FÍSICAMENTE
            if (ultimo != null && string.Equals(ultimo.EstadoNombre, "Bloqueado", StringComparison.OrdinalIgnoreCase))
            {
                _ctx.CambiosDeEstado.Remove(ultimo);
            }

            // 4) Reabrir el anterior SOLO si estaba cerrado
            if (anterior != null && anterior.FechaHoraFin.HasValue)
            {
                anterior.FechaHoraFin = null;
                ev.EstadoActualNombre = anterior.EstadoNombre; // reflejar estado actual persistido
                ev.MaterializarEstadoDesdeNombre();            // reconstruir objeto EstadoActual
            }

            // (Opcional) Diagnóstico
            // var estados = string.Join("\n", _ctx.ChangeTracker.Entries().Select(e => $"{e.Entity.GetType().Name} -> {e.State}"));
            // MessageBox.Show(estados, "TRACKER EF antes de Guardar");

            _repo.Guardar();
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

            var lista = eventosAutodetectadosNoRevisados.Cast<object>().ToList();
            pantalla.RestaurarEstadoInicial();
            pantalla.SolicitarSeleccionEvento(lista);
        }

        public void Dispose()
        {
            _ctx?.Dispose();
        }
    }
}
