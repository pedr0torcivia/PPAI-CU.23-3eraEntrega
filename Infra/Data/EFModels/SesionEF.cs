// Infra/Data/EFModels/SesionEF.cs
using PPAI_Revisiones.Modelos;
using System;

namespace PPAI_2.Infra.Data.EFModels
{
    public class SesionEF
    {
        // === Clave primaria técnica ===
        public Guid Id { get; set; }

        // === Atributos persistidos ===
        public DateTime FechaHoraInicio { get; set; }
        public DateTime? FechaHoraFin { get; set; }

        // === Relación con Usuario ===
        public Guid UsuarioId { get; set; }
        public UsuarioEF Usuario { get; set; } = null!;
    }
}
