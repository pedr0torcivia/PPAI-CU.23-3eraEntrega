using System;
using System.Collections.Generic;

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
            var ts = $"    Fecha: {FechaHoraMuestra:yyyy-MM-dd HH:mm:ss}";
            foreach (var d in DetalleMuestraSismica ?? new())
                ts += $"\n      - {d?.TipoDeDato?.Denominacion}: {d?.Valor} {d?.TipoDeDato?.NombreUnidadMedida}";
            return ts + "\n";
        }
    }
}
