// Infra/Data/EFModels/DetalleMuestraEF.cs
using System;

namespace PPAI_2.Infra.Data.EFModels
{
    public class DetalleMuestraEF
    {
        public Guid Id { get; set; }
        public Guid MuestraSismicaId { get; set; }
        public Guid TipoDeDatoId { get; set; }
        public double Valor { get; set; }

        public MuestraSismicaEF Muestra { get; set; }
        public TipoDeDatoEF TipoDeDato { get; set; }
    }
}
