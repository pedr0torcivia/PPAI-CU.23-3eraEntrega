// PPAI_Revisiones.Modelos.Estados/Autodetectado.cs
using System;
using System.Collections.Generic;

// Alias
using M = PPAI_Revisiones.Modelos;   // EventoSismico, CambioDeEstado
using D = PPAI_Revisiones.Dominio;   // Empleado

namespace PPAI_Revisiones.Modelos.Estados
{
    public sealed class Autodetectado : Estado
    {
        public override string Nombre => "Autodetectado";
        public override bool EsAutodetectado => true;

        public override void registrarEstadoBloqueado(
            M.EventoSismico ctx,
            List<M.CambioDeEstado> cambiosEstado,
            DateTime fechaHoraActual,
            D.Empleado responsable)
        {
            // 1️⃣ Buscar y cerrar cambio abierto (si existe)
            var abierto = BuscarCambioAbierto(cambiosEstado);
            if (abierto != null && !abierto.FechaHoraFin.HasValue)
                abierto.SetFechaHoraFin(fechaHoraActual);

            // 2️⃣ Crear nuevo estado Bloqueado
            var nuevoEstado = new Bloqueado();

            // 3️⃣ Crear CE y agregarlo
            var ce = new M.CambioDeEstado
            {
                EstadoActual = nuevoEstado,
                FechaHoraInicio = fechaHoraActual,
                FechaHoraFin = null,
                Responsable = responsable
            };
            cambiosEstado.Add(ce);

            // 4️⃣ Actualizar estado del evento
            ctx.SetEstado(nuevoEstado);
        }

        private static M.CambioDeEstado BuscarCambioAbierto(List<M.CambioDeEstado> cambios)
            => cambios?.Find(c => c.EsEstadoActual());
    }
}
