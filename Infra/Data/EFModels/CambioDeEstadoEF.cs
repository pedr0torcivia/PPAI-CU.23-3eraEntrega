// Infra/Data/EFModels/CambioDeEstadoEF.cs
using System;

namespace PPAI_2.Infra.Data.EFModels
{
    public class CambioDeEstadoEF
    {
        public Guid Id { get; set; }
        public Guid EventoSismicoId { get; set; }
        public Guid? ResponsableId { get; set; }
        public string EstadoNombre { get; set; }
        public DateTime? FechaHoraInicio { get; set; }
        public DateTime? FechaHoraFin { get; set; }

        public EmpleadoEF Responsable { get; set; }
        public EventoSismicoEF Evento { get; set; }
    }
}
