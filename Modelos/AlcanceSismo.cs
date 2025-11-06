// Modelos/AlcanceSismo.cs
using System;

namespace PPAI_Revisiones.Modelos
{
    public class AlcanceSismo
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        public string GetNombreAlcance()
        {
            return Nombre;
        }

        public override string ToString()
        {
            return Nombre;
        }
    }
}
