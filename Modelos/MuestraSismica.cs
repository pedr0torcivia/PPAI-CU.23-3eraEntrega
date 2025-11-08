// Modelos/MuestraSismica.cs
using System;
using System.Collections.Generic;
using System.Text;

namespace PPAI_Revisiones.Modelos
{
    public class MuestraSismica
    {
        // === Atributos de dominio ===
        public DateTime FechaHoraMuestra { get; private set; }
        public List<DetalleMuestraSismica> Detalles { get; private set; } = new();

        // === Comportamiento del dominio ===
        public string GetDatos()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"    Fecha: {FechaHoraMuestra:yyyy-MM-dd HH:mm:ss}");

            var detalles = Detalles ?? new List<DetalleMuestraSismica>();
            sb.AppendLine($"    Detalles en esta muestra: {detalles.Count}");

            if (detalles.Count == 0)
            {
                sb.AppendLine("      (sin detalles)");
                return sb.ToString();
            }

            foreach (var d in detalles)
            {
                var tipo = d.TipoDeDato?.GetDatos() ?? "(tipo desconocido)";
                var unidad = d.TipoDeDato?.NombreUnidadMedida ?? "";
                sb.AppendLine($"      {tipo}: {d.Valor:0.######} {unidad}");
            }

            return sb.ToString();
        }

        // === Constructores ===
        public MuestraSismica(DateTime fechaHoraMuestra, List<DetalleMuestraSismica> detalles)
        {
            FechaHoraMuestra = fechaHoraMuestra;
            Detalles = detalles ?? new List<DetalleMuestraSismica>();
        }

        protected MuestraSismica() { }
    }
}
