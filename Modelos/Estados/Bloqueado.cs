using System;
using System.Collections.Generic;

namespace PPAI_Revisiones.Modelos.Estados
{
    public sealed class Bloqueado : Estado
    {
        public override string Nombre => "Bloqueado";

        public override void rechazar(
            List<CambioDeEstado> cambiosEstado,   // ✅ mismo orden que en Estado
            EventoSismico ctx,
            DateTime fechaHoraActual,
            Empleado responsable)
        {
            var abierto = BuscarCambioAbierto(cambiosEstado);
            if (abierto != null && !abierto.FechaHoraFin.HasValue)
                abierto.SetFechaHoraFin(fechaHoraActual);

            var ce = new CambioDeEstado
            {
                Id = Guid.NewGuid(),
                EventoSismicoId = ctx.Id,
                EstadoNombre = "Rechazado",
                EstadoActual = new Rechazado(),
                FechaHoraInicio = fechaHoraActual,
                FechaHoraFin = null,
                ResponsableId = responsable?.Id,
                Responsable = responsable
            };
            cambiosEstado.Add(ce);

            ctx.SetEstado(new Rechazado());
        }

        private static CambioDeEstado BuscarCambioAbierto(List<CambioDeEstado> cambios)
            => cambios?.Find(c => c.EsEstadoActual());
    }

}
