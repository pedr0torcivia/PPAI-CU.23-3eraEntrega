// Infra/Data/EFModels/MuestraSismicaEF.cs
using System;
using System.Collections.Generic;

namespace PPAI_2.Infra.Data.EFModels
{
    public class MuestraSismicaEF
    {
        public Guid Id { get; set; }
        public Guid SerieTemporalId { get; set; }
        public DateTime FechaHoraMuestra { get; set; }

        public SerieTemporalEF SerieTemporal { get; set; }
        public List<DetalleMuestraEF> Detalles { get; set; } = new();
    }
}
