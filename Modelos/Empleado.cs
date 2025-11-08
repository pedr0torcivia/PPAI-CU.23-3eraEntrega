// Modelos/Empleado.cs
using System;

namespace PPAI_Revisiones.Modelos
{
    public class Empleado
    {
        // === Atributos de dominio (sin PK/FK técnicas) ===
        public string Apellido { get; private set; }
        public string Nombre { get; private set; }
        public string Mail { get; private set; }
        public string Telefono { get; private set; }
        public string Rol { get; private set; }

        // === Comportamiento mínimo útil ===
        public string NombreCompleto() => $"{Apellido}, {Nombre}";
        public override string ToString() => $"{NombreCompleto()} ({Rol})";

        // === Constructores ===
        public Empleado(string apellido, string nombre, string mail, string telefono, string rol)
        {
            Apellido = apellido ?? throw new ArgumentNullException(nameof(apellido));
            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            Mail = mail ?? string.Empty;
            Telefono = telefono ?? string.Empty;
            Rol = rol ?? string.Empty;
        }

        protected Empleado() { }
    }
}
