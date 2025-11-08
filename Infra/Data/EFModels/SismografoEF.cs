// Infra/Data/EFModels/SismografoEF.cs
using System;

namespace PPAI_2.Infra.Data.EFModels
{
    public class SismografoEF
    {
        public Guid Id { get; set; }
        public Guid? EstacionId { get; set; }
        public string IdentificadorSismografo { get; set; }
        public string NroSerie { get; set; }
        public DateTime? FechaAdquisicion { get; set; }

        public EstacionSismologicaEF Estacion { get; set; }
    }
}
