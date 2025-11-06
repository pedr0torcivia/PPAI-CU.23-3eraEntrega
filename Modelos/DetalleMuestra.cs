using System;

namespace PPAI_Revisiones.Modelos
{
    public class DetalleMuestra
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid MuestraSismicaId { get; set; }
        public Guid TipoDeDatoId { get; set; }

        public double Valor { get; set; }
        public TipoDeDato TipoDeDato { get; set; }

        public string GetDatos()
        {
            if (TipoDeDato == null)
                return "(tipo de dato desconocido)";

            string nombreTipo = TipoDeDato.GetDatos(); // → GetDenominacion()
            string unidad = TipoDeDato.NombreUnidadMedida ?? "(sin unidad)";
            double valor = Math.Round(Valor, 2);

            return $"{nombreTipo}: {valor} {unidad}";
        }
    }
}
