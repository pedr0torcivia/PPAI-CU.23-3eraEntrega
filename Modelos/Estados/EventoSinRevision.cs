using System;
using System.Collections.Generic;

namespace PPAI_Revisiones.Modelos.Estados
{
    public sealed class EventoSinRevision : Estado
    {
        public override string Nombre => "Evento sin revisión";
        public override bool EsEventoSinRevision => true;

        private static CambioDeEstado BuscarCambioAbierto(List<CambioDeEstado> cambios)
            => cambios?.Find(c => c.EsEstadoActual());
    }
}
