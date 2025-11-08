// Modelos/SerieTemporal.cs
using System;
using System.Collections.Generic;
using System.Text;

namespace PPAI_Revisiones.Modelos
{
    public class SerieTemporal
    {
        // === Atributos de dominio (según tu lista) ===
        public bool CondicionAlarma { get; private set; }
        public DateTime FechaHoraInicioRegistroMuestras { get; private set; }
        public DateTime FechaHoraRegistro { get; private set; }
        public double FrecuenciaMuestreo { get; private set; }
        public List<MuestraSismica> Muestras { get; private set; } = new();

        // === Comportamiento de dominio (sin dependencias técnicas) ===
        public string GetSeries()
        {
            var sb = new StringBuilder();

            var ordenadas = (Muestras ?? new List<MuestraSismica>());
            ordenadas.Sort((a, b) => a.FechaHoraMuestra.CompareTo(b.FechaHoraMuestra));

            sb.AppendLine($"[Serie] Frecuencia: {FrecuenciaMuestreo} Hz | " +
                          $"Desde: {FechaHoraInicioRegistroMuestras:yyyy-MM-dd HH:mm:ss} | " +
                          $"Ult. Registro: {FechaHoraRegistro:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Muestras: {ordenadas.Count}");

            if (ordenadas.Count == 0)
            {
                sb.AppendLine("  (sin muestras)");
                return sb.ToString();
            }

            int i = 1;
            foreach (var muestra in ordenadas)
            {
                sb.AppendLine($"\n  • Muestra #{i++}");
                sb.Append(muestra.GetDatos());
            }

            return sb.ToString();
        }

        // === Constructores ===
        public SerieTemporal(
            bool condicionAlarma,
            DateTime fechaHoraInicioRegistroMuestras,
            DateTime fechaHoraRegistro,
            double frecuenciaMuestreo,
            List<MuestraSismica> muestras)
        {
            CondicionAlarma = condicionAlarma;
            FechaHoraInicioRegistroMuestras = fechaHoraInicioRegistroMuestras;
            FechaHoraRegistro = fechaHoraRegistro;
            FrecuenciaMuestreo = frecuenciaMuestreo;
            Muestras = muestras ?? new List<MuestraSismica>();
        }

        protected SerieTemporal() { }
    }
}
