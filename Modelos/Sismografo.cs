using PPAI_Revisiones.Modelos;
using System;

namespace PPAI_Revisiones.Dominio
{
    public class Sismografo
    {
        public DateTime? FechaAdquisicion { get; set; }
        public string IdentificadorSismografo { get; set; }
        public string NroSerie { get; set; }
        public EstacionSismologica Estacion { get; set; }

        public string GetNombreEstacion() => Estacion?.Nombre;
    }
}
