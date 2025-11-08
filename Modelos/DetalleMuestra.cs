// Modelos/DetalleMuestraSismica.cs
using System;

namespace PPAI_Revisiones.Modelos
{
    public class DetalleMuestraSismica
    {
        // === Atributos de dominio ===
        public double Valor { get; private set; }
        public TipoDeDato TipoDeDato { get; private set; }   // (1.)

        // === Comportamiento del dominio ===
        public string GetDatos()
        {
            if (TipoDeDato == null)
                return "(tipo de dato desconocido)";

            string nombreTipo = TipoDeDato.GetDatos(); // obtiene la denominación
            string unidad = TipoDeDato.NombreUnidadMedida ?? "(sin unidad)";
            double valorRedondeado = Math.Round(Valor, 2);

            return $"{nombreTipo}: {valorRedondeado} {unidad}";
        }

        // === Constructores ===
        public DetalleMuestraSismica(double valor, TipoDeDato tipoDeDato)
        {
            Valor = valor;
            TipoDeDato = tipoDeDato ?? throw new ArgumentNullException(nameof(tipoDeDato));
        }

        protected DetalleMuestraSismica() { }
    }
}
