// Infra/Data/EFModels/EstacionSismologicaEF.cs
using System;
using System.Collections.Generic;

namespace PPAI_2.Infra.Data.EFModels
{
    public class EstacionSismologicaEF
    {
        // === Clave primaria técnica ===
        public Guid Id { get; set; }

        // === Atributos persistidos ===
        public string CodigoEstacion { get; set; } = string.Empty;
        public string DocumentoCertificacionAdq { get; set; } = string.Empty;
        public DateTime FechaSolicitudCertificacion { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string NroCertificacionAdquisicion { get; set; } = string.Empty;

        public ICollection<SismografoEF> Sismografos { get; set; } = new List<SismografoEF>();
    }
}
