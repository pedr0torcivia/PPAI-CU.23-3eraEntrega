// Modelos/OrigenDeGeneracion.cs
using System;

namespace PPAI_Revisiones.Modelos
{
    public class OrigenDeGeneracion
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        public string GetNombreOrigen() => Nombre;

        public override string ToString()
        {
            return Nombre;
        }

    }
}
