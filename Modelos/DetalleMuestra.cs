using System;
using PPAI_Revisiones.Dominio;

namespace PPAI_Revisiones.Modelos
{
    public class DetalleMuestraSismica
    {
        public double Valor { get; set; }
        public TipoDeDato TipoDeDato { get; set; } // 1..1

        public string GetDatos()
        {
            if (TipoDeDato == null) return "(tipo de dato desconocido)";
            string nombreTipo = TipoDeDato.GetDatos(); // → GetDenominacion()
            string unidad = TipoDeDato.NombreUnidadMedida ?? "(sin unidad)";
            double valor = Math.Round(Valor, 2);
            return $"{nombreTipo}: {valor} {unidad}";
        }
    }
}
