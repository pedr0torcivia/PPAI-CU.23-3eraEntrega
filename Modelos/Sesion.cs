using System;

namespace PPAI_Revisiones.Modelos
{
    public class Sesion
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        private Usuario usuarioLogueado; 

        public Sesion(Usuario usuario)    
        {
            usuarioLogueado = usuario;
        }

        public DateTime FechaHoraInicio { get; set; }
        public DateTime? FechaHoraFin { get; set; }

        public Usuario GetUsuario() => usuarioLogueado;
    }
}