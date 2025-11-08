// PPAI_Revisiones.Modelos.Estados/Confirmado.cs
using System;

// Alias
using M = PPAI_Revisiones.Modelos;
using D = PPAI_Revisiones.Dominio;

namespace PPAI_Revisiones.Modelos.Estados
{
    public sealed class Confirmado : Estado
    {
        public override string Nombre => "Confirmado";

        // Estado final — sin transiciones salientes en este CU.
    }
}
