// Infra/Data/EFModels/EmpleadoEF.cs
using System;

namespace PPAI_2.Infra.Data.EFModels
{
    public class EmpleadoEF
    {
        // === Clave primaria técnica ===
        public Guid Id { get; set; }

        // === Atributos persistidos ===
        public string Apellido { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Mail { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;

        // Añadir la FK del Usuario si UsuarioEF usa EmpleadoId
        // Nota: Si la FK (EmpleadoId) está en UsuarioEF, EmpleadoEF necesita la navegación inversa.
        public UsuarioEF? Usuario { get; set; }
        // Opcionalmente, si la FK está en Empleado: public Guid? UsuarioId { get; set; }
        // Asumo la relación 1:1 donde Empleado tiene la FK del Usuario:
        public Guid? UsuarioId { get; set; } // <--- Agrega esta línea si no existe.
    }
}
