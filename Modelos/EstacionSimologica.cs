using System;

namespace PPAI_Revisiones.Modelos
{
    public class EstacionSismologica
    {
        public string CodigoEstacion { get; set; }
        public string DocumentoCertificacionAdq { get; set; }
        public DateTime FechaSolicitudCeritficacion { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string Nombre { get; set; }
        public string NroCertificadoAdquisicion { get; set; }

        public string GetNombreEstacion() => Nombre;
    }
}
