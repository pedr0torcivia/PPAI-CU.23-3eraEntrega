// Infra/Data/EFModels/OrigenDeGeneracionEF.cs
using System;

namespace PPAI_2.Infra.Data.EFModels
{
    public class OrigenDeGeneracionEF
    {
        // === Clave primaria técnica ===
        public Guid Id { get; set; }

        // === Atributos persistidos ===
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}
