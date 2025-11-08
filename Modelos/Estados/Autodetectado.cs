// PPAI_Revisiones.Modelos.Estados/Autodetectado.cs
using System;
using System.Collections.Generic;

namespace PPAI_Revisiones.Modelos.Estados
{
    public sealed class Autodetectado : Estado
    {
        public override string Nombre => "Autodetectado";
        public override bool EsAutodetectado => true;

        public override void registrarEstadoBloqueado(
            EventoSismico ctx,
            List<CambioDeEstado> cambiosEstado,
            DateTime fechaHoraActual,
            Empleado responsable)
        {
            // 1) Cerrar cambio abierto (si lo hay)
            var abierto = BuscarCambioAbierto(cambiosEstado);
            if (abierto != null && !abierto.FechaHoraFin.HasValue)
                abierto.SetFechaHoraFin(fechaHoraActual);

            // 2) Nuevo estado destino
            var nuevoEstado = new Bloqueado();

            // 3) Crear CE de dominio (sin IDs/FKs técnicas)
            var ce = CambioDeEstado.Crear(fechaHoraActual, nuevoEstado, responsable);
            cambiosEstado.Add(ce);

            // 4) Actualizar estado del evento (dominio persiste el nombre)
            ctx.SetEstado(nuevoEstado);
        }

        private static CambioDeEstado BuscarCambioAbierto(List<CambioDeEstado> cambios)
            => cambios?.Find(c => c.EsEstadoActual());
    }
}
