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

// Alias dominio (Empleado vive en Dominio)
using D = PPAI_Revisiones.Dominio;

namespace PPAI_Revisiones.Controladores
{
    public class ManejadorRegistrarRespuesta
    {
        // Infra (EF)
        private readonly RedSismicaContext _ctx;
        private readonly IEventoRepository _repo;
        private Guid? _responsableIdEf;

        // === Variables de control del CU ===
        private List<EventoSismico> eventosAutodetectadosNoRevisados;
        private EventoSismico eventoSeleccionado;
        private EventoSismico eventoBloqueadoTemporal;

        private DateTime fechaHoraActual;
        private D.Empleado responsable;

        private string detallesEvento;

        public ManejadorRegistrarRespuesta()
        {
            _ctx = new RedSismicaContext();
            _repo = new EventoRepositoryEF(_ctx);
        }

        // ================== FLUJO PRINCIPAL ==================
        public List<object> RegistrarNuevaRevision(PantallaNuevaRevision pantalla)
        {
            eventosAutodetectadosNoRevisados = BuscarEventosAutoDetecNoRev();
            OrdenarEventos();

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
            var todos = _repo.GetEventosParaRevision().ToList();
            var resultado = new List<EventoSismico>();
            foreach (var e in todos)
            {
                e.MaterializarEstadoDesdeNombre();
                e.MaterializarEstadosDeCambios();
                if (e.sosAutodetectado() || e.sosEventoSinRevision())
                    resultado.Add(e);
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

            // Revertir bloqueo temporal del anterior (si existía)
            if (eventoBloqueadoTemporal != null && eventoBloqueadoTemporal != nuevoEvento)
                RevertirBloqueo(eventoBloqueadoTemporal);

            _ctx.ChangeTracker.Clear();

            // cargar completo y trackeado via repo (mapea a dominio)
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

            // State del dominio: agrega CE (sin PK/FK) y setea el nombre
            eventoSeleccionado.RegistrarEstadoBloqueado(fechaHoraActual, responsable);

            // Persistencia: que el repo materialice CE -> EF con PK/FK
            ((EventoRepositoryEF)_repo).Guardar(eventoSeleccionado, _responsableIdEf);

        }

        private D.Empleado BuscarUsuarioLogueado()
        {
            var ef = _ctx.Empleados
                         .AsNoTracking()
                         .Include(e => e.Usuario)
                         .FirstOrDefault();

            _responsableIdEf = ef?.Id; // <-- guardamos el Id EF para persistir FK
            if (ef == null) return null;

            return new D.Empleado
            {
                // NO agregamos Id al dominio
                Nombre = ef.Nombre,
                Apellido = ef.Apellido,
                Mail = ef.Mail,
                Telefono = ef.Telefono
            };
        }

        private DateTime GetFechaHora() => DateTime.Now;

        // ================== DETALLE Y SISMOGRAMA ==================
        private void BuscarDetallesEventoSismico()
        {
            // solo prepara el string para la pantalla; no mostrar MessageBox ni logs
            detallesEvento = eventoSeleccionado?.GetDetalleEventoSismico() ?? "(Evento nulo)";
        }

        private string GenerarSismograma()
        {
            var extensionCU = new CU_GenerarSismograma();
            var ruta = extensionCU.Ejecutar();
            return ruta?.Trim().Trim('"');
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
                case 1:
                    pantalla.MostrarMensaje("Evento confirmado correctamente.");
                    break;

                case 2: // Rechazar
                    if (eventoSeleccionado == null)
                    {
                        pantalla.MostrarMensaje("Error: evento seleccionado nulo.");
                        return;
                    }
                    ActualizarEstadoRechazado(pantalla);
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
            responsable ??= BuscarUsuarioLogueado();

            // State del dominio
            eventoSeleccionado.Rechazar(fechaHoraActual, responsable);

            // Persistencia vía repo (materializa CE → EF)
            ((EventoRepositoryEF)_repo).Guardar(eventoSeleccionado, _responsableIdEf);


            // Traer fresco para mostrar (dominio)
            eventoSeleccionado = _repo.GetEventoConSeriesYDetalles(eventoSeleccionado.Id);
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

            // Trabajamos con EF directo para revertir una fila reciente
            var evEf = _ctx.EventosSismicos
                           .Include(e => e.CambiosDeEstado)
                           .FirstOrDefault(e => e.Id == ev.Id);
            if (evEf == null) return;

            var ordenados = evEf.CambiosDeEstado
                .OrderByDescending(c => c.FechaHoraInicio ?? DateTime.MinValue)
                .ToList();

            var ultimo = ordenados.FirstOrDefault();
            var anterior = ordenados.Skip(1).FirstOrDefault();

            // Si el último fue "Bloqueado", lo quitamos
            if (ultimo != null && string.Equals(ultimo.EstadoNombre, "Bloqueado", StringComparison.OrdinalIgnoreCase))
            {
                _ctx.CambiosDeEstado.Remove(ultimo);
            }

            // Reabrimos el anterior (si estaba cerrado)
            if (anterior != null && anterior.FechaHoraFin.HasValue)
            {
                anterior.FechaHoraFin = null;
                evEf.EstadoActualNombre = anterior.EstadoNombre;
            }

            _ctx.SaveChanges();
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
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Evento rechazado correctamente.\n");
            sb.AppendLine(ev.GetDetalleEventoSismico());

            sb.AppendLine("CAMBIOS DE ESTADO:");
            foreach (var c in ev.CambiosDeEstado.OrderBy(x => x.FechaHoraInicio ?? DateTime.MinValue))
            {
                var nombre = c.EstadoActual?.Nombre ?? c.EstadoNombre ?? "(sin estado)";
                var inicio = c.FechaHoraInicio?.ToString("dd/MM/yyyy HH:mm") ?? "(sin inicio)";
                var fin = c.FechaHoraFin?.ToString("dd/MM/yyyy HH:mm") ?? "(en curso)";
                var resp = c.Responsable?.Nombre ?? "(desconocido)";
                sb.AppendLine($"- {nombre,-12}  {inicio} → {fin}  | Responsable: {resp}");
            }
            pantalla.MostrarMensaje(sb.ToString());
        }
    }
}
