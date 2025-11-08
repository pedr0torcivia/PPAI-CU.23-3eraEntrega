// Modelos/TipoDeDato.cs
using System;

namespace PPAI_Revisiones.Modelos
{
    public class TipoDeDato
    {
        // === Atributos de dominio ===
        public string Denominacion { get; private set; }
        public string NombreUnidadMedida { get; private set; }
        public double ValorUmbral { get; private set; }

        // === Comportamiento del dominio ===
        public string GetDatos() => GetDenominacion();
        public string GetDenominacion() => Denominacion;

        public override string ToString() =>
            $"{Denominacion} ({NombreUnidadMedida}) — Umbral: {ValorUmbral}";

        // === Constructores ===
        public TipoDeDato(string denominacion, string nombreUnidadMedida, double valorUmbral)
        {
            Denominacion = denominacion ?? throw new ArgumentNullException(nameof(denominacion));
            NombreUnidadMedida = nombreUnidadMedida ?? string.Empty;
            ValorUmbral = valorUmbral;
        }

        protected TipoDeDato() { }
    }
}
