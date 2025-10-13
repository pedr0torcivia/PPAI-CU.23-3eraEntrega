// Modelos/MuestraSismica.cs
using System;
using System.Collections.Generic;

namespace PPAI_Revisiones.Modelos
{
    public class MuestraSismica
    {
        public List<DetalleMuestra> Detalles { get; set; } = new();

        public string GetDatos()
        {
            var sb = new System.Text.StringBuilder();

            foreach (var detalle in Detalles ?? new List<DetalleMuestra>())
            {
                Console.WriteLine("[DetalleMuestra] getDatos()");
                sb.AppendLine($"     - {detalle.GetDatos()}");
            }

            return sb.ToString();
        }
    }
}
