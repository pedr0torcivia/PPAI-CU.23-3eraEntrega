// PPAI_Revisiones.Modelos.Estados/Bloqueado.cs
using System;
using System.Collections.Generic;

namespace PPAI_Revisiones.Modelos.Estados
{
    public sealed class Bloqueado : Estado
    {
        public override string Nombre => "Bloqueado";
        public override bool EsBloqueado => true;

        public override void rechazar(
            List<CambioDeEstado> cambiosEstado,
            EventoSismico ctx,
            DateTime fechaHoraActual,
            Empleado responsable)
        {
            // 1) Cerrar cambio abierto (si lo hay)
            var abierto = BuscarCambioAbierto(cambiosEstado);
            if (abierto != null && !abierto.FechaHoraFin.HasValue)
                abierto.SetFechaHoraFin(fechaHoraActual);

            // 2) Crear CE hacia Rechazado (dominio puro)
            var estadoDestino = new Rechazado();
            var ce = CambioDeEstado.Crear(fechaHoraActual, estadoDestino, responsable);
            cambiosEstado.Add(ce);

            // 3) Actualizar estado del evento
            ctx.SetEstado(estadoDestino);
        }

        private static CambioDeEstado BuscarCambioAbierto(List<CambioDeEstado> cambios)
            => cambios?.Find(c => c.EsEstadoActual());
    }
}
