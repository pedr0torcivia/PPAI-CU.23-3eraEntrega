// Modelos/OrigenDeGeneracion.cs
using System;

namespace PPAI_Revisiones.Modelos
{
    public class OrigenDeGeneracion
    {
        // === Atributos de dominio ===
        public string Nombre { get; private set; }
        public string Descripcion { get; private set; }

        // === Comportamiento del dominio ===
        public string GetNombreOrigen() => Nombre;
        public override string ToString() => Nombre;

        // === Constructores ===
        public OrigenDeGeneracion(string nombre, string descripcion)
        {
            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            Descripcion = descripcion ?? string.Empty;
        }

        protected OrigenDeGeneracion() { }
    }
}
