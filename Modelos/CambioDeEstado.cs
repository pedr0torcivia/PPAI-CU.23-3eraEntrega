// Modelos/CambioDeEstado.cs
using System;
using PPAI_Revisiones.Modelos.Estados;

namespace PPAI_Revisiones.Modelos
{
    public class CambioDeEstado
    {
        // === Atributos de dominio (según tu lista) ===
        public DateTime? FechaHoraInicio { get; set; }
        public DateTime? FechaHoraFin { get; set; }
        public Estado EstadoActual { get; set; }              // (1.)
        public Empleado ResponsableInspeccion { get; set; }   // (1.) hacia Empleado

        // === Comportamiento de dominio ===
        public bool EsEstadoActual() => !FechaHoraFin.HasValue;
        public string GetEstadoNombre() => EstadoActual?.Nombre ?? throw new InvalidOperationException("EstadoActual no puede ser nulo.");

        public void SetFechaHoraFin(DateTime fin)
        {
            if (fin < FechaHoraInicio)
                throw new ArgumentException("La fecha de fin no puede ser anterior al inicio.", nameof(fin));
            FechaHoraFin = fin;
        }

        // Fábrica de creación coherente con el dominio
        public static CambioDeEstado Crear(DateTime inicio, Estado estado, Empleado responsable)
        {
            if (estado == null) throw new ArgumentNullException(nameof(estado));
            if (responsable == null) throw new ArgumentNullException(nameof(responsable));

            return new CambioDeEstado
            {
                FechaHoraInicio = inicio,
                FechaHoraFin = null,
                EstadoActual = estado,
                ResponsableInspeccion = responsable
            };
        }

        // Constructor protegido para frameworks/serialización si hiciera falta
        protected CambioDeEstado() { }
    }
}
