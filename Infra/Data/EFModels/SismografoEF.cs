// Infra/Data/EFModels/SismografoEF.cs
using System;

namespace PPAI_2.Infra.Data.EFModels
{
    public class SismografoEF
    {
        // === Clave primaria técnica ===
        public Guid Id { get; set; }

        // === Atributos persistidos ===
        public string IdentificadorSismografo { get; set; } = string.Empty;
        public string NroSerie { get; set; } = string.Empty;
        public DateTime? FechaAdquisicion { get; set; }

        // === Relaciones ===
        // Clave Foránea (FK)
        public Guid EstacionId { get; set; }

        // Propiedad de Navegación
        public EstacionSismologicaEF Estacion { get; set; }
    }
}
