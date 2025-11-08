// Modelos/Sesion.cs
using System;

namespace PPAI_Revisiones.Modelos
{
    public class Sesion
    {
        // === Atributos de dominio ===
        private Usuario usuarioLogueado;
        public DateTime FechaHoraInicio { get; private set; }
        public DateTime? FechaHoraFin { get; private set; }

        // === Comportamiento del dominio ===
        public Usuario GetUsuario() => usuarioLogueado;

        public void CerrarSesion(DateTime fechaFin)
        {
            if (fechaFin < FechaHoraInicio)
                throw new ArgumentException("La fecha de cierre no puede ser anterior a la de inicio.");
            FechaHoraFin = fechaFin;
        }

        public bool EstaActiva() => !FechaHoraFin.HasValue;

        // === Constructores ===
        public Sesion(Usuario usuario)
        {
            usuarioLogueado = usuario ?? throw new ArgumentNullException(nameof(usuario));
            FechaHoraInicio = DateTime.Now;
        }

        protected Sesion() { }
    }
}
