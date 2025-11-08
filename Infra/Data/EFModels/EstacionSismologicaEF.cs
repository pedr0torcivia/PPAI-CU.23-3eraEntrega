// Infra/Data/EFModels/EstacionSismologicaEF.cs
using System;

namespace PPAI_2.Infra.Data.EFModels
{
    public class EstacionSismologicaEF
    {
        public Guid Id { get; set; }
        public int Id_Estacion { get; set; }
        public string CodigoEstacion { get; set; }
        public string DocumentoCertificacionAdq { get; set; }
        public DateTime FechaSolicitudCertificacion { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string Nombre { get; set; }
        public string NroCertificacionAdquisicion { get; set; }
    }
}
