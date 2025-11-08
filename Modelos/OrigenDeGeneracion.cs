namespace PPAI_Revisiones.Dominio
{
    public class OrigenDeGeneracion
    {
        public string Descripcion { get; set; }
        public string Nombre { get; set; }

        public string GetNombreOrigen() => Nombre;
        public override string ToString() => Nombre;
    }
}
