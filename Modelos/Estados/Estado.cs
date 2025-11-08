// PPAI_Revisiones.Modelos.Estados/Estado.cs
using System;
using System.Collections.Generic;
using PPAI_Revisiones.Modelos;

namespace PPAI_Revisiones.Modelos.Estados
{
    // Jerarquía de dominio puro (no es entidad EF, no se mapea)
    public abstract class Estado
    {
        public abstract string Nombre { get; }

        public virtual bool EsBloqueado => false;
        public virtual bool EsAutodetectado => false;
        public virtual bool EsEventoSinRevision => false;

        public virtual void registrarEstadoBloqueado(
            EventoSismico ctx, List<CambioDeEstado> cambiosEstado,
            DateTime fechaHoraActual, Empleado responsable)
            => LanzarInvalida(nameof(registrarEstadoBloqueado));

        public virtual void rechazar(
            List<CambioDeEstado> cambiosEstado, EventoSismico ctx,
            DateTime fechaHoraActual, Empleado responsable)
            => LanzarInvalida(nameof(rechazar));

        protected static void LanzarInvalida(string transicion)
            => throw new InvalidOperationException($"Transición '{transicion}' no válida para el estado actual.");

        // Materialización desde nombre persistido
        public static Estado FromName(string nombre) => nombre switch
        {
            "Autodetectado" => new Autodetectado(),
            "Bloqueado" => new Bloqueado(),
            "Rechazado" => new Rechazado(),
            "Confirmado" => new Confirmado(),
            "Autoconfirmado" => new Autoconfirmado(),
            "Evento sin revisión" => new EventoSinRevision(),
            "PendienteDeCierre" => new PendienteDeCierre(),
            "PendienteDeRevision" => new PendienteDeRevision(),
            "Cerrado" => new Cerrado(),
            "Derivado" => new Derivado(),
            _ => null
        };
    }
}
