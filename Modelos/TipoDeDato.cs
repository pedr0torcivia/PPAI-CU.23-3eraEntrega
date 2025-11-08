namespace PPAI_Revisiones.Dominio
{
    public class TipoDeDato
    {
        public string Denominacion { get; set; }
        public string NombreUnidadMedida { get; set; }
        public double ValorUmbral { get; set; }

        public string GetDatos() => GetDenominacion();
        public string GetDenominacion() => Denominacion;
    }
}
