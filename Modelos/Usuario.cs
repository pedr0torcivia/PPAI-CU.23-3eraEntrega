using System;

namespace PPAI_Revisiones.Modelos
{
    public class Usuario
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string NombreUsuario { get; set; }
        public string Contraseña { get; set; }

    }
}
