// Modelos/AlcanceSismo.cs
namespace PPAI_Revisiones.Modelos
{
    public class AlcanceSismo
    {
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
