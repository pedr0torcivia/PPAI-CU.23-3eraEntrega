using System;
using System.Collections.Generic;
using System.Text;
using PPAI_Revisiones.Modelos;

namespace PPAI_Revisiones.Dominio
{
    public class MuestraSismica
    {
        public DateTime FechaHoraMuestra { get; set; }
        public List<PPAI_Revisiones.Modelos.DetalleMuestraSismica> DetalleMuestraSismica { get; set; } = new();

        public string GetDatos()
        {
            var sb = new StringBuilder();
            var detalles = DetalleMuestraSismica ?? new List<PPAI_Revisiones.Modelos.DetalleMuestraSismica>();
            if (detalles.Count == 0)
            {
                sb.AppendLine("      (sin detalles)");
                return sb.ToString();
            }

            foreach (var d in detalles)
            {
                var nombreTipo = d.TipoDeDato?.GetDatos() ?? "(tipo)";
                var um = d.TipoDeDato?.NombreUnidadMedida ?? "";
                sb.AppendLine($"      {nombreTipo,-15}: {d.Valor:0.###} {um}");
            }
            return sb.ToString();
        }
    }
}
