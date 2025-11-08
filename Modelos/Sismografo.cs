// Modelos/Sismografo.cs
using System;

namespace PPAI_Revisiones.Modelos
{
    public class Sismografo
    {
        // === Atributos de dominio ===
        public string IdentificadorSismografo { get; private set; }
        public string NroSerie { get; private set; }
        public DateTime? FechaAdquisicion { get; private set; }
        public EstacionSismologica Estacion { get; private set; }

        // === Comportamiento de dominio ===
        public string GetNombreEstacion() => Estacion?.GetNombreEstacion() ?? "(sin estación asignada)";

        public override string ToString()
        {
            var fecha = FechaAdquisicion?.ToString("yyyy-MM-dd") ?? "N/D";
            return $"{IdentificadorSismografo} - Serie {NroSerie} (Adq: {fecha})";
        }

        // === Constructores ===
        public Sismografo(
            string identificadorSismografo,
            string nroSerie,
            DateTime? fechaAdquisicion,
            EstacionSismologica estacion)
        {
            IdentificadorSismografo = identificadorSismografo ?? throw new ArgumentNullException(nameof(identificadorSismografo));
            NroSerie = nroSerie ?? string.Empty;
            FechaAdquisicion = fechaAdquisicion;
            Estacion = estacion ?? throw new ArgumentNullException(nameof(estacion));
        }

        protected Sismografo() { }
    }
}
