// PPAI_Revisiones.Modelos.Estados/Bloqueado.cs
using System;
using System.Collections.Generic;

// Alias
using M = PPAI_Revisiones.Modelos;
using D = PPAI_Revisiones.Dominio;

namespace PPAI_Revisiones.Modelos.Estados
{
    public sealed class Bloqueado : Estado
    {
        public override string Nombre => "Bloqueado";
        public override bool EsBloqueado => true;

        public override void rechazar(
            List<M.CambioDeEstado> cambiosEstado,
            M.EventoSismico ctx,
            DateTime fechaHoraActual,
            D.Empleado responsable)
        {
            var abierto = BuscarCambioAbierto(cambiosEstado);
            if (abierto != null && !abierto.FechaHoraFin.HasValue)
                abierto.SetFechaHoraFin(fechaHoraActual);

            var ce = new M.CambioDeEstado
            {
                EstadoActual = new Rechazado(),
                FechaHoraInicio = fechaHoraActual,
                FechaHoraFin = null,
                Responsable = responsable
            };
            cambiosEstado.Add(ce);

            ctx.SetEstado(new Rechazado());
        }

        private static M.CambioDeEstado BuscarCambioAbierto(List<M.CambioDeEstado> cambios)
            => cambios?.Find(c => c.EsEstadoActual());
    }
}
