using System;

namespace PPAI_Revisiones.Modelos
{
    public class EstacionSismologica
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public int Id_Estacion { get; set; }
        public string CodigoEstacion { get; set; }
        public string DocumentoCertificacionAdq { get; set; }
        public DateTime FechaSolicitudCertificacion { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string Nombre { get; set; }
        public string NroCertificacionAdquisicion { get; set; }

        
        public string GetNombreEstacion() => Nombre;
    }
}
