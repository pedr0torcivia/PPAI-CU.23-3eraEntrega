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

            string nombreEstacion = Sismografo?.GetNombreEstacion() ?? "(sin estación)";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"\nEstación: {nombreEstacion}");
            sb.AppendLine($"[TRACE] Serie {Id} → muestras: {muestras.Count}");

            if (muestras.Count == 0)
                sb.AppendLine("    (sin muestras)");

            int numMuestra = 1;
            foreach (var muestra in muestras)
            {
                sb.AppendLine($"\n  • Muestra #{numMuestra++}");
                sb.Append(muestra.GetDatos()); // mantiene tu cadena de llamadas
            }

            return sb.ToString();
        }


    }
}
