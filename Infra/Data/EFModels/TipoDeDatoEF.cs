// Infra/Data/EFModels/TipoDeDatoEF.cs
using System;

namespace PPAI_2.Infra.Data.EFModels
{
    public class TipoDeDatoEF
    {
        public Guid Id { get; set; }
        public string Denominacion { get; set; }
        public string NombreUnidadMedida { get; set; }
        public double ValorUmbral { get; set; }
    }
}
