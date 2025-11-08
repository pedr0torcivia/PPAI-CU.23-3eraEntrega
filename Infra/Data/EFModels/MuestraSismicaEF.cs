// Infra/Data/EFModels/MuestraSismicaEF.cs
using PPAI_Revisiones.Modelos;
using System;
using System.Collections.Generic;

namespace PPAI_2.Infra.Data.EFModels
{
    public class MuestraSismicaEF
    {
        // === Clave primaria técnica ===
        public Guid Id { get; set; }

        // === Atributos persistidos ===
        public DateTime FechaHoraMuestra { get; set; }

        // === Relaciones ===
        public Guid SerieTemporalId { get; set; }
        public SerieTemporalEF Serie { get; set; } = null!;

        public List<DetalleMuestraSismicaEF> Detalles { get; set; } = new();
    }
}
