// Infra/Repos/IEventoRepository.cs
using System.Collections.Generic;
using PPAI_Revisiones.Modelos;

namespace PPAI_Revisiones.Controladores
{
    public interface IEventoRepository
    {
        // Lectura
        IEnumerable<EventoSismico> GetEventosParaRevision();
        EventoSismico GetEventoConSeriesYDetalles(EventoSismico candidato);

        // Estado / Persistencia
        void GuardarCambiosDeEstado(EventoSismico evento);

        // Reversión del bloqueo temporal
        void RevertirBloqueo(EventoSismico evento);

        // Actualizar instancia domain desde BD (re-cargar Cambios + Responsables)
        void Refresh(EventoSismico evento);

        // Sesión/Usuario actual → Empleado del dominio
        Empleado GetUsuarioLogueado();

        EventoSismico GetEventoParaReversionDeBloqueo(EventoSismico evento);
    }

}
