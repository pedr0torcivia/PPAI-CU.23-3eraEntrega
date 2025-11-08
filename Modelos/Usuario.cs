// Modelos/Usuario.cs
using System;

namespace PPAI_Revisiones.Modelos
{
    public class Usuario
    {
        // === Atributos de dominio ===
        public string NombreUsuario { get; private set; }
        public string Contrasenia { get; private set; }
        public Empleado Empleado { get; private set; }

        // === Comportamiento del dominio ===
        public bool Autenticar(string contraseniaIngresada)
        {
            // En un sistema real se haría hash/salt, acá simple comparación
            return Contrasenia == contraseniaIngresada;
        }

        public override string ToString() => NombreUsuario;

        // === Constructores ===
        public Usuario(string nombreUsuario, string contrasenia, Empleado empleado)
        {
            NombreUsuario = nombreUsuario ?? throw new ArgumentNullException(nameof(nombreUsuario));
            Contrasenia = contrasenia ?? throw new ArgumentNullException(nameof(contrasenia));
            Empleado = empleado ?? throw new ArgumentNullException(nameof(empleado));
        }

        protected Usuario() { }
    }
}
