// Modelos/OrigenDeGeneracion.cs
namespace PPAI_Revisiones.Modelos
{
    public class OrigenDeGeneracion
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        public string GetNombreOrigen() => Nombre;

        public override string ToString()
        {
            return Nombre;
        }

    }
}
