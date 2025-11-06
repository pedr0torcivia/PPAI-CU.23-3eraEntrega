using System;
using System.ComponentModel.DataAnnotations.Schema;
using PPAI_Revisiones.Modelos.Estados;

namespace PPAI_Revisiones.Modelos
{
    public class CambioDeEstado
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid EventoSismicoId { get; set; }   // <-- NUEVO (FK)
        public EventoSismico Evento { get; set; }   // <-- NUEVO (navegación)
        public Guid? ResponsableId { get; set; }    // <-- NUEVO (FK opcional, usado por el import)

        // Persistimos SOLO el nombre
        public string EstadoNombre { get; set; }

        [NotMapped]                      // <- no se mapea, igual que antes
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
                EstadoNombre = estado?.Nombre, // <- CLAVE
                Responsable = responsable,
                FechaHoraInicio = inicio,
                FechaHoraFin = null
            };
    }
}
