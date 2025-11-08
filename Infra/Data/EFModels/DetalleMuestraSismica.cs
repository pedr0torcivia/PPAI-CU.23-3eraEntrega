// Infra/Data/EFModels/DetalleMuestraSismicaEF.cs
using PPAI_Revisiones.Modelos;
using System;

namespace PPAI_2.Infra.Data.EFModels
{
    public class DetalleMuestraSismicaEF
    {
        // === Clave primaria técnica ===
        public Guid Id { get; set; }

        // === Atributos persistidos ===
        public double Valor { get; set; }

        // === Relaciones ===
        public Guid TipoDeDatoId { get; set; }
        public TipoDeDatoEF TipoDeDato { get; set; } = null!;

        public Guid MuestraSismicaId { get; set; }
        public MuestraSismicaEF Muestra { get; set; } = null!;
    }
}
