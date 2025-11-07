using System;
using System.Collections.Generic;


namespace PPAI_Revisiones.Modelos
{
    public class MuestraSismica
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SerieTemporalId { get; set; }   

        // Esta propiedad debe existir en la tabla MuestrasSismicas
        public DateTime FechaHoraMuestra { get; set; }

        public List<DetalleMuestra> DetalleMuestraSismica { get; set; } = new();

        public string GetDatos()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"    Fecha: {FechaHoraMuestra:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"    [TRACE] Id Muestra: {Id}");

            var detalles = DetalleMuestraSismica ?? new List<DetalleMuestra>();
            sb.AppendLine($"    [TRACE] Detalles en esta muestra: {detalles.Count}");

            if (detalles.Count == 0)
            {
                sb.AppendLine("      (sin detalles)");
                return sb.ToString();
            }

            foreach (var d in detalles)
            {
                var tipo = d.TipoDeDato?.GetDatos() ?? "(tipo)";
                var um = d.TipoDeDato?.NombreUnidadMedida ?? "";
                sb.AppendLine($"      {tipo}: {d.Valor:0.######} {um}");
            }

            return sb.ToString();
        }
    }
}
