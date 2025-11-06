using System;
using System.Collections.Generic;

namespace PPAI_Revisiones.Modelos.Estados
{
    public sealed class Autodetectado : Estado
    {
        public override string Nombre => "Autodetectado";
        public override bool EsAutodetectado => true;

        // registrarEstadoBloqueado(ctx, cambiosEstado, fechaHoraActual, responsable)
        public override void registrarEstadoBloqueado(
            EventoSismico ctx,
            List<CambioDeEstado> cambiosEstado,
            DateTime fechaHoraActual,
            Empleado responsable)
        {
            // 1️ Buscar y cerrar cambio abierto (si existe)
            var abierto = BuscarCambioAbierto(cambiosEstado);
            if (abierto != null && !abierto.FechaHoraFin.HasValue)   // ← solo si está abierto
                abierto.SetFechaHoraFin(fechaHoraActual);

            // 2️ Crear nuevo estado Bloqueado
            var nuevoEstado = new Bloqueado();

            // 3️ Crear CE y agregarlo
            var ce = new CambioDeEstado
            {
                EstadoActual = nuevoEstado,
                FechaHoraInicio = fechaHoraActual,
                FechaHoraFin = null,
                Responsable = responsable
            };
            ce.EstadoNombre = "Bloqueado";   // ← **AGREGAR ESTA LÍNEA**
            ce.ResponsableId = responsable?.Id ?? Guid.Empty;
            cambiosEstado.Add(ce);

            // 4️⃣ Actualizar estado del evento
            ctx.SetEstado(nuevoEstado);
        }

        // Helper local
        private static CambioDeEstado BuscarCambioAbierto(List<CambioDeEstado> cambios)
            => cambios?.Find(c => c.EsEstadoActual());
    }
}
