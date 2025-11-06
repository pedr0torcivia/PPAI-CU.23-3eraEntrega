using System;
using System.Collections.Generic;
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
            int numMuestra = 1;

            foreach (var muestra in Muestras ?? new List<MuestraSismica>())
            {
                Console.WriteLine("[SerieTemporal → Muestra] getDatos()");
                bloqueMuestras.AppendLine($"\n  • Muestra #{numMuestra++}");
                bloqueMuestras.Append(muestra.GetDatos()); // ← segundo loop vive adentro
            }

            Console.WriteLine("[SerieTemporal → Sismografo] getNombreEstacion()");
            var sismografo = Sismografo;

            Console.WriteLine("[Sismografo → EstacionSismologica] getNombreEstacion()");
            string nombreEstacion = sismografo?.GetNombreEstacion() ?? "(sin estación)";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"\nEstación: {nombreEstacion}");
            sb.Append(bloqueMuestras.ToString());
            return sb.ToString();
        }

    }
}
