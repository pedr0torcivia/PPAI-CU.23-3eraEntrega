namespace PPAI_Revisiones.Dominio
{
    public class Usuario
    {
        public string Contraseña { get; set; }
        public string NombreUsuario { get; set; }
        public Empleado Empleado { get; set; }   // 1 a 1
        public string GetNombreUsuario() => NombreUsuario;
    }
}
