// Modelos/ClasificacionSismo.cs
using System;

namespace PPAI_Revisiones.Modelos
{
    public class ClasificacionSismo
    {
        // === Atributos de dominio ===
        public string Nombre { get; private set; }
        public double KmProfundidadDesde { get; private set; }
        public double KmProfundidadHasta { get; private set; }

        // === Comportamiento del dominio ===
        public string GetNombreClasificacion() => Nombre;

        public override string ToString() => Nombre;

        // === Constructor ===
        public ClasificacionSismo(string nombre, double kmDesde, double kmHasta)
        {
            if (kmDesde < 0 || kmHasta < 0)
                throw new ArgumentException("La profundidad no puede ser negativa.");

            if (kmHasta < kmDesde)
                throw new ArgumentException("El límite superior no puede ser menor que el inferior.");

            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            KmProfundidadDesde = kmDesde;
            KmProfundidadHasta = kmHasta;
        }

        protected ClasificacionSismo() { }
    }
}
