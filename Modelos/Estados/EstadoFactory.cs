// Estados/EstadoFactory.cs
using PPAI_Revisiones.Modelos.Estados;

public static class EstadoFactory
{
    public static Estado FromNombre(string? nombre) =>
        (nombre ?? "").ToLowerInvariant() switch
        {
            "autodetectado" => new Autodetectado(),
            "bloqueado" => new Bloqueado(),
            "rechazado" => new Rechazado(),
            "confirmado" => new Confirmado(),
            _ => null
        };
}
