using System;

namespace PPAI_Revisiones.Modelos
{
    public class Empleado
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? UsuarioId { get; set; }   // FK al usuario (puede ser null)

        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Mail { get; set; }
        public string Telefono { get; set; }

        // Asociación 1 a 1 con Usuario
        public Usuario Usuario { get; set; }

        // Comparación con nombre de usuario asociado
        public bool EsTuUsuario(string nombreUsuario)
        {
            return Usuario != null && Usuario.NombreUsuario == nombreUsuario;
        }

    }
}

