// Infra/Data/EFModels/UsuarioEF.cs
using PPAI_Revisiones.Dominio;
using System;

namespace PPAI_2.Infra.Data.EFModels
{
    public class UsuarioEF
    {
        public Guid Id { get; set; }
        public string NombreUsuario { get; set; }
        public string Contraseña { get; set; }

        public EmpleadoEF Empleado { get; set; } // 1-1 inverso
    }
}
