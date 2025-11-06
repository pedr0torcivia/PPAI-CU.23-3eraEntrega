using System;
using System.Collections.Generic;

namespace PPAI_Revisiones.Modelos.Estados
{
    public sealed class Bloqueado : Estado
    {
        public override string Nombre => "Bloqueado";
        public override bool EsBloqueado => true;

        // rechazar(cambiosEstado, es, fechaHoraActual, responsable)
        public override void rechazar(
            List<CambioDeEstado> cambiosEstado,
            EventoSismico es,
            DateTime fechaHoraActual,
            Empleado responsable)
        {
            // 1️⃣ Buscar y cerrar cambio abierto
            var abierto = BuscarCambioAbierto(cambiosEstado);
            abierto?.SetFechaHoraFin(fechaHoraActual);

            // 2️⃣ Crear nuevo estado Rechazado
            var nuevoEstado = new Rechazado();

            // 3️⃣ Crear CE y agregarlo
            var ce = new CambioDeEstado
            {
                EstadoActual = nuevoEstado,
                FechaHoraInicio = fechaHoraActual,
                FechaHoraFin = null,
                Responsable = responsable
            };
            cambiosEstado.Add(ce);

            // 4️⃣ Actualizar estado del evento
            es.SetEstado(nuevoEstado);
        }

        private static CambioDeEstado BuscarCambioAbierto(List<CambioDeEstado> cambios)
            => cambios?.Find(c => c.EsEstadoActual());
    }
}
