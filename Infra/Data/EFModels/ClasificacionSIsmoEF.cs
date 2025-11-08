// Infra/Data/EFModels/ClasificacionSismoEF.cs
using System;

namespace PPAI_2.Infra.Data.EFModels
{
    public class ClasificacionSismoEF
    {
        // === Clave primaria técnica ===
        public Guid Id { get; set; }

        // === Atributos persistidos ===
        public string Nombre { get; set; } = string.Empty;
        public double KmProfundidadDesde { get; set; }
        public double KmProfundidadHasta { get; set; }
    }
}
