// Infra/Data/EFModels/AlcanceSismoEF.cs
using System;

namespace PPAI_2.Infra.Data.EFModels
{
    public class AlcanceSismoEF
    {
        // === Clave primaria técnica ===
        public Guid Id { get; set; }

        // === Atributos persistidos ===
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}
