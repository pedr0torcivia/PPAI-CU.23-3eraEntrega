// PPAI_Revisiones.Modelos.Estados/Confirmado.cs
using System;
namespace PPAI_Revisiones.Modelos.Estados
{
    public sealed class Confirmado : Estado
    {
        public override string Nombre => "Confirmado";

        // Estado final — sin transiciones salientes en este CU.
    }
}
