// Infra/Data/EFModels/UsuarioEF.cs
using System;

namespace PPAI_2.Infra.Data.EFModels
{
    public class UsuarioEF
    {
        // === Clave primaria técnica ===
        public Guid Id { get; set; }

        // === Atributos persistidos ===
        public string NombreUsuario { get; set; } = string.Empty;
        public string Contrasenia { get; set; } = string.Empty;

        // === Relación 1:1 con Empleado ===
        public Guid EmpleadoId { get; set; }
        public EmpleadoEF Empleado { get; set; } = null!;
    }
}
