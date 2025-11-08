using System;
using System.Collections.Generic;
using System.Linq;

namespace PPAI_Revisiones.Dominio
{
    public class SerieTemporal
    {
        public bool CondicionAlarma { get; set; }
        public DateTime FechaHoraInicioRegistroMuestras { get; set; }
        public DateTime FechaHoraRegistro { get; set; }
        public double FrecuenciaMuestreo { get; set; }
        public List<MuestraSismica> MuestrasSismicas { get; set; } = new();
        public Sismografo Sismografo { get; set; }

        public string GetSeries()
        {
            var muestras = (MuestrasSismicas ?? new List<MuestraSismica>())
                           .OrderBy(x => x.FechaHoraMuestra)
                           .ToList();

            string nombreEstacion = Sismografo?.GetNombreEstacion() ?? "(sin estación)";
            var sb = new System.Text.StringBuilder();

            sb.AppendLine($"Estación: {nombreEstacion}");
            sb.AppendLine($"  Frecuencia muestreo: {FrecuenciaMuestreo:0.##} Hz");
            sb.AppendLine($"  Inicio registro     : {FechaHoraInicioRegistroMuestras:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine($"  Último registro     : {FechaHoraRegistro:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine($"  Condición de alarma : {(CondicionAlarma ? "Sí" : "No")}");

            if (muestras.Count == 0)
            {
                sb.AppendLine("  (sin muestras)");
                sb.AppendLine();
                return sb.ToString();
            }

            int n = 1;
            foreach (var m in muestras)
            {
                sb.AppendLine($"  • Muestra #{n++}  ({m.FechaHoraMuestra:dd/MM/yyyy HH:mm:ss})");
                sb.Append(m.GetDatos());
            }
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
