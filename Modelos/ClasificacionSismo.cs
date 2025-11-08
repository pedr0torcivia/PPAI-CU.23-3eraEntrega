namespace PPAI_Revisiones.Dominio
{
    public class ClasificacionSismo
    {
        public double KmProfundidadDesde { get; set; }
        public double KmProfundidadHasta { get; set; }
        public string Nombre { get; set; }

        public string GetNombreClasificacion() => Nombre;
        public override string ToString() => Nombre;
    }
}
