// Modelos/EstacionSismologica.cs
using System;

namespace PPAI_Revisiones.Modelos
{
    public class EstacionSismologica
    {
        // === Atributos de dominio ===
        public string CodigoEstacion { get; private set; }
        public string DocumentoCertificacionAdq { get; private set; }
        public DateTime FechaSolicitudCertificacion { get; private set; }
        public double Latitud { get; private set; }
        public double Longitud { get; private set; }
        public string Nombre { get; private set; }
        public string NroCertificacionAdquisicion { get; private set; }

        // === Comportamiento del dominio ===
        public string GetNombreEstacion() => Nombre;

        public override string ToString() => $"{CodigoEstacion} - {Nombre}";

        // === Constructor ===
        public EstacionSismologica(
            string codigoEstacion,
            string documentoCertificacionAdq,
            DateTime fechaSolicitudCertificacion,
            double latitud,
            double longitud,
            string nombre,
            string nroCertificacionAdquisicion)
        {
            CodigoEstacion = codigoEstacion ?? throw new ArgumentNullException(nameof(codigoEstacion));
            DocumentoCertificacionAdq = documentoCertificacionAdq ?? string.Empty;
            FechaSolicitudCertificacion = fechaSolicitudCertificacion;
            Latitud = latitud;
            Longitud = longitud;
            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            NroCertificacionAdquisicion = nroCertificacionAdquisicion ?? string.Empty;
        }

        protected EstacionSismologica() { }
    }
}
