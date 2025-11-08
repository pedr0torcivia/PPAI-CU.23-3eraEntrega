// Infra/Data/EFModels/SerieTemporalEF.cs
using PPAI_Revisiones.Dominio;
using System;
using System.Collections.Generic;

namespace PPAI_2.Infra.Data.EFModels
{
    public class SerieTemporalEF
    {
        public Guid Id { get; set; }
        public Guid? SismografoId { get; set; }
        public Guid? EventoSismicoId { get; set; }

        public bool CondicionAlarma { get; set; }
        public DateTime FechaHoraInicioRegistroMuestras { get; set; }
        public DateTime FechaHoraRegistro { get; set; }
        public double FrecuenciaMuestreo { get; set; }

        public SismografoEF Sismografo { get; set; }
        public EventoSismicoEF Evento { get; set; }
        public List<MuestraSismicaEF> Muestras { get; set; } = new();
    }
}
