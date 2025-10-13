// Modelos/ClasificacionSismo.cs
namespace PPAI_Revisiones.Modelos
{
    public class ClasificacionSismo
    {
        public string Nombre { get; set; }
        public double KmProfundidadDesde { get; set; }
        public double KmProfundidadHasta { get; set; }

        public string GetNombreClasificacion() => Nombre;

        public override string ToString()
        {
            return Nombre;
        }

    }
}

