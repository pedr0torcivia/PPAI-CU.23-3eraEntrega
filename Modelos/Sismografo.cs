using System;

namespace PPAI_Revisiones.Modelos
{
    public class Sismografo
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? EstacionId { get; set; }        // <-- NUEVO


        public string IdentificadorSismografo { get; set; } = string.Empty;
        public string NroSerie { get; set; } = string.Empty;

        public DateTime? FechaAdquisicion { get; set; }   // columna agregada por parche en runtime
        public EstacionSismologica Estacion { get; set; }

        public string GetNombreEstacion() => Estacion?.GetNombreEstacion();
    }
}
