using System;
using System.Collections.Generic;
using System.Linq;

namespace PPAI_Revisiones.Modelos.Estados
{
    public abstract class Estado
    {
        public abstract string Nombre { get; }

        public virtual bool EsBloqueado => false;
        public virtual bool EsAutodetectado => false;
        public virtual bool EsEventoSinRevision => false;

        // Autodetectado -> Bloqueado
        public virtual void registrarEstadoBloqueado(
            EventoSismico ctx,
            List<CambioDeEstado> cambiosEstado,
            DateTime fechaHoraActual,
            Empleado responsable) =>
            LanzarInvalida(nameof(registrarEstadoBloqueado));

        // Bloqueado -> Rechazado  (AHORA con Empleado)
        public virtual void rechazar(
            List<CambioDeEstado> cambiosEstado,
            EventoSismico es,
            DateTime fechaHoraActual,
            Empleado responsable) =>
            LanzarInvalida(nameof(rechazar));

        public virtual CambioDeEstado buscarCambioEstadoAbierto(List<CambioDeEstado> cambiosEstado)
            => cambiosEstado?.FirstOrDefault(c => c.EsEstadoActual());

        public virtual Estado crearEstado() => this;

        public virtual CambioDeEstado crearCambioEstado(
            DateTime fechaHoraActual, Empleado responsable, Estado estado)
            => CambioDeEstado.Crear(fechaHoraActual, estado, responsable);

        protected static void LanzarInvalida(string transicion)
            => throw new InvalidOperationException($"Transición '{transicion}' no válida para el estado actual.");
    }
}
