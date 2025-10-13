// Modelos/CambioEstado.cs
using System;

public class CambioDeEstado
{

    public Estado EstadoActual { get; set; }
    public DateTime? FechaHoraFin { get; set; }
    public DateTime? FechaHoraInicio { get; set; }
    public string Responsable { get; set; }

    public bool EsEstadoActual() => FechaHoraFin == null;
    public void SetFechaHoraFin(DateTime fecha) => FechaHoraFin = fecha;
}
