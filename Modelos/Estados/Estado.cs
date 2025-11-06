// PPAI_Revisiones.Modelos.Estados/Estado.cs
using PPAI_Revisiones.Modelos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

[NotMapped] // <- CLAVE: EF no intentará mapear esta jerarquía
public abstract class Estado
{
    public abstract string Nombre { get; }

    public virtual bool EsBloqueado => false;
    public virtual bool EsAutodetectado => false;
    public virtual bool EsEventoSinRevision => false;

    public virtual void registrarEstadoBloqueado(
        EventoSismico ctx, List<CambioDeEstado> cambiosEstado,
        DateTime fechaHoraActual, Empleado responsable)
        => LanzarInvalida(nameof(registrarEstadoBloqueado));

    public virtual void rechazar(
        List<CambioDeEstado> cambiosEstado, EventoSismico es,
        DateTime fechaHoraActual, Empleado responsable)
        => LanzarInvalida(nameof(rechazar));

    protected static void LanzarInvalida(string transicion)
        => throw new InvalidOperationException($"Transición '{transicion}' no válida para el estado actual.");
}
