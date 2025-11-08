// Infra/Data/EFModels/EmpleadoEF.cs
using System;

namespace PPAI_2.Infra.Data.EFModels
{
    public class EmpleadoEF
    {
        public Guid Id { get; set; }
        public Guid? UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Mail { get; set; }
        public string Telefono { get; set; }

        public UsuarioEF Usuario { get; set; }
    }
}
