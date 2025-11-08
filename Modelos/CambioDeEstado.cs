using PPAI_Revisiones.Dominio;
using PPAI_Revisiones.Modelos.Estados;
using System;

namespace PPAI_Revisiones.Modelos
{
    public class CambioDeEstado
    {
        // === Atributos del dominio que pediste ===
        public DateTime? FechaHoraFin { get; set; }
        public DateTime? FechaHoraInicio { get; set; }
        public Estado EstadoActual { get; set; }            // 1..1 (State en memoria)
        public Empleado Responsable { get; set; }           // ← el CU usa .Responsable

        // === Shims de compatibilidad usados por el CU ===
        public string EstadoNombre => EstadoActual?.Nombre; // el CU lo lee para mensajes

        // === Métodos existentes (no se tocan) ===
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
