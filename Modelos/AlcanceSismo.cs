// Modelos/AlcanceSismo.cs
using System;

namespace PPAI_Revisiones.Modelos
{
    public class AlcanceSismo
    {
        // === Atributos de dominio ===
        public string Nombre { get; private set; }
        public string Descripcion { get; private set; }

        // === Comportamiento del dominio ===
        public string GetNombreAlcance() => Nombre;

        public override string ToString() => Nombre;

        // === Constructores ===
        public AlcanceSismo(string nombre, string descripcion)
        {
            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            Descripcion = descripcion ?? string.Empty;
        }

        protected AlcanceSismo() { }
    }
}
