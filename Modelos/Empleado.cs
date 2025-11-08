namespace PPAI_Revisiones.Dominio
{
    public class Empleado
    {
        public string Apellido { get; set; }
        public string Mail { get; set; }
        public string Telefono { get; set; }
        public string Nombre { get; set; }
        public string Rol { get; set; }

        // Helper que ya usabas
        public bool EsTuUsuario(Usuario u) => u?.Empleado == this;
    }
}
