// PPAI_Revisiones.Modelos.Estados/Rechazado.cs
using System;

// Alias
using M = PPAI_Revisiones.Modelos;
using D = PPAI_Revisiones.Dominio;

namespace PPAI_Revisiones.Modelos.Estados
{
    public sealed class Rechazado : Estado
    {
        public override string Nombre => "Rechazado";

        // No redefine registrarEstadoBloqueado ni rechazar
        // porque no hay transiciones desde este estado en el CU actual.
    }
}
