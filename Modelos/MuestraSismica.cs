using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PPAI_Revisiones.Modelos
{
    public class MuestraSismica
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SerieTemporalId { get; set; }   // <-- NUEVO

        // Esta propiedad debe existir en la tabla MuestrasSismicas
        public DateTime FechaHoraMuestra { get; set; }

        public List<DetalleMuestra> DetalleMuestraSismica { get; set; } = new();

        public string GetDatos()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"    Fecha: {FechaHoraMuestra:yyyy-MM-dd HH:mm:ss}");

            var detalles = DetalleMuestraSismica ?? new List<DetalleMuestra>();
            Debug.WriteLine($"[Muestra] {Id} detalles={detalles.Count}");

            if (detalles.Count == 0)
            {
                sb.AppendLine("      (sin detalles)");
                return sb.ToString();
            }

            // Mantiene tu cadena: Muestra -> DetalleMuestra -> TipoDeDato.GetDatos()
            foreach (var d in detalles)
            {
                var tipo = d.TipoDeDato?.GetDatos() ?? "(tipo)"; // delega a TipoDeDato
                var um = d.TipoDeDato?.NombreUnidadMedida ?? "";
                sb.AppendLine($"      {tipo}: {d.Valor:0.######} {um}");
            }

            return sb.ToString();
        }
    }
}
