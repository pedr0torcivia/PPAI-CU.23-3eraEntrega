using PPAI_Revisiones.Modelos;
using System;
using System.Collections.Generic;

namespace PPAI_2.Infra.Repos
{
    public interface IEventoRepository
    {
        IEnumerable<EventoSismico> GetEventosAutoDetectadosNoRevisados();
        IEnumerable<EventoSismico> GetEventosParaRevision();
        EventoSismico GetEventoParaReversionDeBloqueo(Guid eventoId);
        EventoSismico GetEventoConSeriesYDetalles(Guid eventoId);
        void Guardar();
    }
}
