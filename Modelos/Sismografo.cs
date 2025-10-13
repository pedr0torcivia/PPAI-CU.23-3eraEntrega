// Modelos/Sismografo.cs
namespace PPAI_Revisiones.Modelos
{
    public class Sismografo
    {
        public string IdentificadorSismografo { get; set; }
        public string NroSerie { get; set; }
        public EstacionSismologica Estacion { get; set; }

        public string GetNombreEstacion()
        {
            return Estacion?.GetNombreEstacion();
        }
    }
}
