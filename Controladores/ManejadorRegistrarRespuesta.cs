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
            // 1) Cargar tracked + filtrar en dominio
            eventosAutodetectadosNoRevisados = BuscarEventosAutoDetecNoRev();

            // 2) Ordenar
            OrdenarEventos();

            // 3) Proyección SOLO para la grilla (guardamos la lista original trackeada)
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
            var todos = _repo.GetEventosParaRevision().ToList(); // ✅ TRACKED

            var resultado = new List<EventoSismico>();
            foreach (var e in todos)
            {
                // reconstruir objetos Estado para usar métodos del dominio
                e.MaterializarEstadoDesdeNombre();
                e.MaterializarEstadosDeCambios();

                if (e.sosAutodetectado() || e.sosEventoSinRevision())
                {
                    // (log del CU)
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

            // Si había otro bloqueado temporal, revertir
            if (eventoBloqueadoTemporal != null && eventoBloqueadoTemporal != nuevoEvento)
                RevertirBloqueo(eventoBloqueadoTemporal);

            eventoSeleccionado = eventosAutodetectadosNoRevisados[indice]; // ya está trackeado
            eventoBloqueadoTemporal = eventoSeleccionado;

            // Autodetectado → Bloqueado (delegado al estado del evento)
            ActualizarEventoBloqueado();
            _repo.Guardar(); // persistir
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

            // Delego en el Evento → Estado Autodetectado maneja la transición
            eventoSeleccionado.RegistrarEstadoBloqueado(fechaHoraActual, responsable);
            // ⬇️ Adjuntar los CE nuevos que EF aún no trackea
            var nuevos = eventoSeleccionado.CambiosDeEstado
                .Where(c => _ctx.Entry(c).State == EntityState.Detached)
                .ToList();

            if (nuevos.Count > 0)
                _ctx.CambiosDeEstado.AddRange(nuevos);

            // --- Forzar inserción de los CE nuevos ---
            foreach (var c in eventoSeleccionado.CambiosDeEstado)
            {
                var entry = _ctx.Entry(c);
                if (entry.State == EntityState.Modified || entry.State == EntityState.Unchanged)
                {
                    // si no tiene FechaHoraFin o EstadoNombre no existía antes, lo tratamos como nuevo
                    if (c.FechaHoraFin == null && c.Id != Guid.Empty)
                        entry.State = EntityState.Added;
                }
            }
            _repo.Guardar();
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
            if (responsable == null)
                responsable = _ctx.Empleados.Include(e => e.Usuario).FirstOrDefault();

            // Bloqueado → Rechazado (misma instancia trackeada)
            eventoSeleccionado.Rechazar(fechaHoraActual, responsable);

            // Persistir cambios (incluye el CE Bloqueado previo si no estaba)
            var nuevos = eventoSeleccionado.CambiosDeEstado
                .Where(c => _ctx.Entry(c).State == EntityState.Detached)
                .ToList();

            if (nuevos.Count > 0)
                _ctx.CambiosDeEstado.AddRange(nuevos);


// --- Forzar inserción de los CE nuevos ---
foreach (var c in eventoSeleccionado.CambiosDeEstado)
{
    var entry = _ctx.Entry(c);
    if (entry.State == EntityState.Modified || entry.State == EntityState.Unchanged)
    {
        // si no tiene FechaHoraFin o EstadoNombre no existía antes, lo tratamos como nuevo
        if (c.FechaHoraFin == null && c.Id != Guid.Empty)
            entry.State = EntityState.Added;
    }
}
            _repo.Guardar();

            _ctx.Entry(eventoSeleccionado).Reload();
            _ctx.Entry(eventoSeleccionado)
                .Collection(e => e.CambiosDeEstado)
                .Query()
                .Include(c => c.Responsable)
                .Load();

            eventoSeleccionado.MaterializarEstadoDesdeNombre();
            eventoSeleccionado.MaterializarEstadosDeCambios();

            // 2) 🚨 AHORA arma y muestra el mensaje
            MostrarMensajeCambios(pantalla, eventoSeleccionado);

            // 3) recién después repintá la grilla y reseteá UI
            ReaplicarFiltroYPintar(pantalla);
            pantalla.RestaurarEstadoInicial();
        }

        // ================== REVERSIÓN DE BLOQUEO TEMPORAL ==================
        private void RevertirBloqueo(EventoSismico ev)
        {
            if (ev == null) return;

            // trabajar sobre la colección trackeada actual
            var ordenados = ev.CambiosDeEstado
                .OrderByDescending(c => c.FechaHoraInicio ?? DateTime.MinValue)
                .ToList();

            var ultimo = ordenados.FirstOrDefault();
            var anterior = ordenados.Skip(1).FirstOrDefault();

            if (ultimo != null && string.Equals(ultimo.EstadoNombre, "Bloqueado", StringComparison.OrdinalIgnoreCase))
            {
                _ctx.CambiosDeEstado.Remove(ultimo); // ✅ lo sacamos del contexto actual
            }

            if (anterior != null && anterior.FechaHoraFin.HasValue)
            {
                anterior.FechaHoraFin = null;
                ev.EstadoActualNombre = anterior.EstadoNombre;
                ev.MaterializarEstadoDesdeNombre();
            }

            _repo.Guardar();
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

            var lista = eventosAutodetectadosNoRevisados.Cast<object>().ToList();
            pantalla.RestaurarEstadoInicial();
            pantalla.SolicitarSeleccionEvento(lista);
        }
        // Recalcula la lista de candidatos usando SOLO entidades trackeadas
        private void ReaplicarFiltroYPintar(PantallaNuevaRevision pantalla)
        {
            // Traigo TRACKED e incluyo cambios/responsable (ya lo hace el repo)
            var todos = _repo.GetEventosParaRevision().ToList();

            var candidatos = new List<EventoSismico>();
            foreach (var e in todos)
            {
                e.MaterializarEstadoDesdeNombre();
                e.MaterializarEstadosDeCambios();

                // Solo mostrar los “pendientes” (Autodetectado o Bloqueado)
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
