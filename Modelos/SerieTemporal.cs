using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PPAI_Revisiones.Modelos
{
    public class SerieTemporal
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? SismografoId { get; set; }      // <-- NUEVO
        public Guid? EventoSismicoId { get; set; }   // <-- NUEVO

        public bool CondicionAlarma { get; set; }
        public DateTime FechaHoraInicioRegistroMuestras { get; set; }
        public DateTime FechaHoraRegistro { get; set; }

        public double FrecuenciaMuestreo { get; set; }
        public List<MuestraSismica> Muestras { get; set; } = new();
        public Sismografo Sismografo { get; set; }

        public string GetSeries()
        {
            var bloqueMuestras = new System.Text.StringBuilder();

            var muestras = (Muestras ?? new List<MuestraSismica>())
                           .OrderBy(x => x.FechaHoraMuestra)
                           .ToList();

            int numMuestra = 1;
            foreach (var muestra in muestras)
            {
                bloqueMuestras.AppendLine($"\n  • Muestra #{numMuestra++}");
                bloqueMuestras.Append(muestra.GetDatos());   // mantiene tu cadena de llamadas
            }

            // Encabezado con la estación (una sola vez por serie)
            string nombreEstacion = Sismografo?.GetNombreEstacion() ?? "(sin estación)";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"\nEstación: {nombreEstacion}");
            if (muestras.Count == 0)
                sb.AppendLine("    (sin muestras)");

            sb.Append(bloqueMuestras.ToString());
            return sb.ToString();
        }

    }
}
