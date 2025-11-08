namespace PPAI_Revisiones.Dominio
{
    public class AlcanceSismo
    {
        public string Descripcion { get; set; }
        public string Nombre { get; set; }

        public string GetNombreAlcance() => Nombre;
        public override string ToString() => Nombre;
    }
}

