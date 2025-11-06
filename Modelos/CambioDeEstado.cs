using System;
using PPAI_Revisiones.Modelos.Estados;

namespace PPAI_Revisiones.Modelos
{
    public class CambioDeEstado
    {
        public Estado EstadoActual { get; set; }
        public DateTime? FechaHoraInicio { get; set; }
        public DateTime? FechaHoraFin { get; set; }
        public Empleado Responsable { get; set; }

        public bool EsEstadoActual() => !FechaHoraFin.HasValue;
        public void SetFechaHoraFin(DateTime fecha) => FechaHoraFin = fecha;

        public static CambioDeEstado Crear(DateTime inicio, Estado estado, Empleado responsable)
            => new CambioDeEstado
            {
                EstadoActual = estado,
                Responsable = responsable,
                FechaHoraInicio = inicio,
                FechaHoraFin = null
            };
    }
}
