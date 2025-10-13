// Modelos/Estado.cs
public class Estado
{
    public string Nombre { get; set; }
    public string Ambito { get; set; }

    public bool EsAutodetectado() => Nombre == "Autodetectado";
    public bool EsNoRevisado() => Nombre == "No Revisado";
    public bool EsBloqueado() => Nombre == "Bloqueado";
    public bool EsRechazado() => Nombre == "Rechazado";
    public bool EsAmbitoEvento() => Ambito == "Evento";

    public override string ToString()
    {
        return Nombre;
    }
}
