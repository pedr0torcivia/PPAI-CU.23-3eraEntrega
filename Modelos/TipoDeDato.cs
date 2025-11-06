// Modelos/TipoDeDato.cs
using System;

namespace PPAI_Revisiones.Modelos
{
    public class TipoDeDato
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Denominacion { get; set; }
        public string NombreUnidadMedida { get; set; }
        public double ValorUmbral { get; set; }

        public string GetDatos() => GetDenominacion();
        public string GetDenominacion() => Denominacion;
    }
}
