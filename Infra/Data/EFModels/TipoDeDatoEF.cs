// Infra/Data/EFModels/TipoDeDatoEF.cs
using System;

namespace PPAI_2.Infra.Data.EFModels
{
    public class TipoDeDatoEF
    {
        // === Clave primaria técnica ===
        public Guid Id { get; set; }

        // === Atributos persistidos ===
        public string Denominacion { get; set; } = string.Empty;
        public string NombreUnidadMedida { get; set; } = string.Empty;
        public double ValorUmbral { get; set; }
    }
}
