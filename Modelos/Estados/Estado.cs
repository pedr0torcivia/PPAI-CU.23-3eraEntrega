// PPAI_Revisiones.Modelos.Estados/Estado.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

// Alias para referenciar bien los tipos donde realmente viven
using M = PPAI_Revisiones.Modelos;   // EventoSismico, CambioDeEstado
using D = PPAI_Revisiones.Dominio;   // Empleado

namespace PPAI_Revisiones.Modelos.Estados
{
    [NotMapped] // <- CLAVE: EF no intentará mapear esta jerarquía
    public abstract class Estado
    {
        public abstract string Nombre { get; }

        public virtual bool EsBloqueado => false;
        public virtual bool EsAutodetectado => false;
        public virtual bool EsEventoSinRevision => false;

        // === NO CAMBIO DE FIRMAS (solo califiqué los tipos con alias M/D) ===
        public virtual void registrarEstadoBloqueado(
            M.EventoSismico ctx,
            List<M.CambioDeEstado> cambiosEstado,
            DateTime fechaHoraActual,
            D.Empleado responsable)
            => LanzarInvalida(nameof(registrarEstadoBloqueado));

        public virtual void rechazar(
            List<M.CambioDeEstado> cambiosEstado,
            M.EventoSismico es,
            DateTime fechaHoraActual,
            D.Empleado responsable)
            => LanzarInvalida(nameof(rechazar));

        protected static void LanzarInvalida(string transicion)
            => throw new InvalidOperationException($"Transición '{transicion}' no válida para el estado actual.");

        public static Estado FromName(string nombre) => nombre switch
        {
            "Autodetectado" => new Autodetectado(),
            "Bloqueado" => new Bloqueado(),
            "Rechazado" => new Rechazado(),
            "Confirmado" => new Confirmado(),
            _ => null
        };
    }
}
