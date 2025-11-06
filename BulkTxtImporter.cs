// BulkTxtImporter.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using EFCore.BulkExtensions; // solo para catálogos SIN FKs
using Microsoft.EntityFrameworkCore;
using PPAI_2.Infra.Data;
using PPAI_Revisiones.Modelos;

internal static class BulkTxtImporter
{
    public static void Run(RedSismicaContext ctx, string folder)
    {
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        ImportOrigenes(ctx, Path.Combine(folder, "Origenes.txt"));
        ImportAlcances(ctx, Path.Combine(folder, "Alcances.txt"));
        ImportClasificaciones(ctx, Path.Combine(folder, "Clasificaciones.txt"));
        ImportTiposDeDato(ctx, Path.Combine(folder, "TiposDeDato.txt"));
        ImportUsuariosYEmpleados(ctx, Path.Combine(folder, "Usuarios.txt"), Path.Combine(folder, "Empleados.txt"));

        ImportEventosSismicos(ctx, Path.Combine(folder, "EventosSismicos.txt"));

        ImportSismografosYEstaciones(ctx, Path.Combine(folder, "Estaciones.txt"), Path.Combine(folder, "Sismografos.txt"));
        ImportSeriesMuestrasDetalles(ctx,
            Path.Combine(folder, "SeriesTemporales.txt"),
            Path.Combine(folder, "Muestras.txt"),
            Path.Combine(folder, "DetallesMuestra.txt"));
    }

    // ===== Catálogos (Bulk OK) =====
    private static void ImportOrigenes(RedSismicaContext ctx, string path)
    {
        if (!File.Exists(path)) return;
        var rows = ReadCsv(path);
        var nuevos = new List<OrigenDeGeneracion>();
        foreach (var r in rows)
        {
            var id = GetGuid(r, "Id") ?? Guid.NewGuid();
            var nombre = Get(r, "Nombre");
            if (string.IsNullOrWhiteSpace(nombre)) continue;
            if (!ctx.Origenes.Any(x => x.Id == id || x.Nombre == nombre))
                nuevos.Add(new OrigenDeGeneracion { Id = id, Nombre = nombre, Descripcion = Get(r, "Descripcion") });
        }
        if (nuevos.Count > 0) ctx.BulkInsert(nuevos);
    }

    private static void ImportAlcances(RedSismicaContext ctx, string path)
    {
        if (!File.Exists(path)) return;
        var rows = ReadCsv(path);
        var nuevos = new List<AlcanceSismo>();
        foreach (var r in rows)
        {
            var id = GetGuid(r, "Id") ?? Guid.NewGuid();
            var nombre = Get(r, "Nombre");
            if (string.IsNullOrWhiteSpace(nombre)) continue;
            if (!ctx.Alcances.Any(x => x.Id == id || x.Nombre == nombre))
                nuevos.Add(new AlcanceSismo { Id = id, Nombre = nombre, Descripcion = Get(r, "Descripcion") });
        }
        if (nuevos.Count > 0) ctx.BulkInsert(nuevos);
    }

    private static void ImportClasificaciones(RedSismicaContext ctx, string path)
    {
        if (!File.Exists(path)) return;
        var rows = ReadCsv(path);
        var nuevos = new List<ClasificacionSismo>();
        foreach (var r in rows)
        {
            var id = GetGuid(r, "Id") ?? Guid.NewGuid();
            var nombre = Get(r, "Nombre");
            if (string.IsNullOrWhiteSpace(nombre)) continue;
            if (!ctx.Clasificaciones.Any(x => x.Id == id || x.Nombre == nombre))
                nuevos.Add(new ClasificacionSismo
                {
                    Id = id,
                    Nombre = nombre,
                    KmProfundidadDesde = GetInt(r, "KmProfundidadDesde") ?? 0,
                    KmProfundidadHasta = GetInt(r, "KmProfundidadHasta") ?? 0
                });
        }
        if (nuevos.Count > 0) ctx.BulkInsert(nuevos);
    }

    private static void ImportTiposDeDato(RedSismicaContext ctx, string path)
    {
        if (!File.Exists(path)) return;
        var rows = ReadCsv(path);
        var nuevos = new List<TipoDeDato>();
        foreach (var r in rows)
        {
            var id = GetGuid(r, "Id") ?? Guid.NewGuid();
            var denom = Get(r, "Denominacion");
            if (string.IsNullOrWhiteSpace(denom)) continue;
            if (!ctx.TiposDeDato.Any(x => x.Id == id || x.Denominacion == denom))
                nuevos.Add(new TipoDeDato
                {
                    Id = id,
                    Denominacion = denom,
                    NombreUnidadMedida = Get(r, "NombreUnidadMedida"),
                    ValorUmbral = GetDouble(r, "ValorUmbral") ?? 0
                });
        }
        if (nuevos.Count > 0) ctx.BulkInsert(nuevos);
    }

    private static void ImportUsuariosYEmpleados(RedSismicaContext ctx, string usuariosPath, string empleadosPath)
    {
        if (File.Exists(usuariosPath))
        {
            var rows = ReadCsv(usuariosPath);
            var nuevosU = new List<Usuario>();
            foreach (var r in rows)
            {
                var id = GetGuid(r, "Id") ?? Guid.NewGuid();
                var nombreUsuario = Get(r, "NombreUsuario");
                if (string.IsNullOrWhiteSpace(nombreUsuario)) continue;
                if (!ctx.Usuarios.Any(x => x.Id == id || x.NombreUsuario == nombreUsuario))
                    nuevosU.Add(new Usuario { Id = id, NombreUsuario = nombreUsuario, Contraseña = Get(r, "Contraseña") ?? "operador" });
            }
            if (nuevosU.Count > 0)
            {
                ctx.AddRange(nuevosU);
                ctx.SaveChanges();
            }
        }

        if (File.Exists(empleadosPath))
        {
            var rows = ReadCsv(empleadosPath);
            var nuevosE = new List<Empleado>();
            foreach (var r in rows)
            {
                var id = GetGuid(r, "Id") ?? Guid.NewGuid();
                var mail = Get(r, "Mail");
                if (string.IsNullOrWhiteSpace(mail)) continue;
                if (ctx.Empleados.Any(x => x.Id == id || x.Mail == mail)) continue;

                var e = new Empleado
                {
                    Id = id,
                    Nombre = Get(r, "Nombre") ?? "",
                    Apellido = Get(r, "Apellido") ?? "",
                    Mail = mail,
                    Telefono = Get(r, "Telefono"),
                };

                var uName = Get(r, "UsuarioNombreUsuario");
                if (!string.IsNullOrWhiteSpace(uName))
                {
                    var u = ctx.Usuarios.FirstOrDefault(x => x.NombreUsuario == uName);
                    if (u != null) e.UsuarioId = u.Id;
                }

                nuevosE.Add(e);
            }
            if (nuevosE.Count > 0)
            {
                ctx.AddRange(nuevosE);
                ctx.SaveChanges();
            }
        }

        if (!ctx.Empleados.Any())
        {
            var u = new Usuario { Id = Guid.NewGuid(), NombreUsuario = "operador", Contraseña = "operador" };
            var e = new Empleado { Id = Guid.NewGuid(), Nombre = "Operador", Apellido = "Base", Mail = "operador@redsismica.local", Telefono = "000-000", UsuarioId = u.Id };
            ctx.AddRange(u, e);
            ctx.SaveChanges();
        }
    }

    // ===== Eventos (guardar con FKs explícitas) =====
    private static void ImportEventosSismicos(RedSismicaContext ctx, string path)
    {
        if (!File.Exists(path)) return;

        var rows = ReadCsv(path);
        var eventos = new List<EventoSismico>();
        foreach (var r in rows)
        {
            var alcance = GetOrCreateAlcance(ctx, Get(r, "AlcanceNombre") ?? "Local");
            var clasif = GetOrCreateClasificacion(ctx, Get(r, "ClasificacionNombre") ?? "Preliminar");
            var origen = GetOrCreateOrigen(ctx, Get(r, "OrigenNombre") ?? "Autodetectado");
            var resp = GetOrCreateEmpleadoPorMail(ctx, Get(r, "ResponsableMail") ?? "operador@redsismica.local");

            var ev = new EventoSismico
            {
                Id = GetGuid(r, "Id") ?? Guid.NewGuid(),
                FechaHoraInicio = GetDate(r, "FechaHoraInicio") ?? DateTime.Now,
                FechaHoraDeteccion = GetDate(r, "FechaHoraDeteccion") ?? DateTime.Now,
                LatitudEpicentro = GetDouble(r, "LatitudEpicentro") ?? 0,
                LongitudEpicentro = GetDouble(r, "LongitudEpicentro") ?? 0,
                LatitudHipocentro = GetDouble(r, "LatitudHipocentro") ?? 0,
                LongitudHipocentro = GetDouble(r, "LongitudHipocentro") ?? 0,
                ValorMagnitud = GetDouble(r, "ValorMagnitud") ?? 0,
                EstadoActualNombre = Get(r, "EstadoActualNombre") ?? "Autodetectado",
                AlcanceId = alcance.Id,
                ClasificacionId = clasif.Id,
                OrigenId = origen.Id,
                ResponsableId = resp.Id
            };

            // Estado de dominio
            ev.MaterializarEstadoDesdeNombre();

            eventos.Add(ev);
        }

        if (eventos.Count > 0)
        {
            ctx.AddRange(eventos);
            ctx.SaveChanges();

            // Cambios abiertos “Autodetectado” si aplica
            var cambios = new List<CambioDeEstado>();
            foreach (var ev in eventos.Where(x => (x.EstadoActualNombre ?? "").Equals("Autodetectado", StringComparison.OrdinalIgnoreCase)))
            {
                var ce = new CambioDeEstado
                {
                    Id = Guid.NewGuid(),
                    EventoSismicoId = ev.Id,
                    EstadoNombre = "Autodetectado",
                    EstadoActual = new PPAI_Revisiones.Modelos.Estados.Autodetectado(),
                    FechaHoraInicio = ev.FechaHoraDeteccion,
                    FechaHoraFin = null,
                    ResponsableId = ev.ResponsableId
                };
                cambios.Add(ce);
            }
            if (cambios.Count > 0)
            {
                ctx.AddRange(cambios);
                ctx.SaveChanges();
            }
        }
    }

    // ===== Estaciones / Sismógrafos / Series / Muestras / Detalles =====
    private static void ImportSismografosYEstaciones(RedSismicaContext ctx, string estacionesPath, string sismografosPath)
    {
        if (File.Exists(estacionesPath))
        {
            var rows = ReadCsv(estacionesPath);
            var nuevos = new List<EstacionSismologica>();
            foreach (var r in rows)
            {
                var e = new EstacionSismologica
                {
                    Id = GetGuid(r, "Id") ?? Guid.NewGuid(),
                    Id_Estacion = GetInt(r, "Id_Estacion") ?? 0,
                    CodigoEstacion = Get(r, "CodigoEstacion"),
                    DocumentoCertificacionAdq = Get(r, "DocumentoCertificacionAdq"),
                    FechaSolicitudCertificacion = GetDate(r, "FechaSolicitudCertificacion") ?? DateTime.Now,
                    Latitud = GetDouble(r, "Latitud") ?? 0,
                    Longitud = GetDouble(r, "Longitud") ?? 0,
                    Nombre = Get(r, "Nombre"),
                    NroCertificacionAdquisicion = Get(r, "NroCertificacionAdquisicion")
                };
                if (!ctx.Estaciones.Any(x => x.Id == e.Id)) nuevos.Add(e);
            }
            if (nuevos.Count > 0)
            {
                ctx.AddRange(nuevos);
                ctx.SaveChanges();
            }
        }

        if (File.Exists(sismografosPath))
        {
            var rows = ReadCsv(sismografosPath);
            var nuevos = new List<Sismografo>();
            foreach (var r in rows)
            {
                var s = new Sismografo
                {
                    Id = GetGuid(r, "Id") ?? Guid.NewGuid(),
                    IdentificadorSismografo = Get(r, "IdentificadorSismografo"),
                    NroSerie = Get(r, "NroSerie"),
                    FechaAdquisicion = GetDate(r, "FechaAdquisicion") ?? DateTime.Now
                };
                var estId = GetGuid(r, "EstacionId"); // o por CodigoEstacion si querés buscarlo antes
                if (estId.HasValue) s.EstacionId = estId.Value;

                nuevos.Add(s);
            }
            if (nuevos.Count > 0)
            {
                ctx.AddRange(nuevos);
                ctx.SaveChanges();
            }
        }
    }

    private static void ImportSeriesMuestrasDetalles(RedSismicaContext ctx, string seriesPath, string muestrasPath, string detallesPath)
    {
        if (File.Exists(seriesPath))
        {
            var rows = ReadCsv(seriesPath);
            var series = new List<SerieTemporal>();
            foreach (var r in rows)
            {
                var st = new SerieTemporal
                {
                    Id = GetGuid(r, "Id") ?? Guid.NewGuid(),
                    CondicionAlarma = (Get(r, "CondicionAlarma") ?? "").Equals("true", StringComparison.OrdinalIgnoreCase),
                    FechaHoraInicioRegistroMuestras = GetDate(r, "FechaHoraInicioRegistroMuestras") ?? DateTime.Now,
                    FechaHoraRegistro = GetDate(r, "FechaHoraRegistro") ?? DateTime.Now,
                    FrecuenciaMuestreo = GetDouble(r, "FrecuenciaMuestreo") ?? 0,
                    SismografoId = GetGuid(r, "SismografoId"),
                    EventoSismicoId = GetGuid(r, "EventoSismicoId")
                };
                series.Add(st);
            }
            if (series.Count > 0)
            {
                ctx.AddRange(series);
                ctx.SaveChanges();
            }
        }

        if (File.Exists(muestrasPath))
        {
            var rows = ReadCsv(muestrasPath);
            var muestras = new List<MuestraSismica>();
            foreach (var r in rows)
            {
                var m = new MuestraSismica
                {
                    Id = GetGuid(r, "Id") ?? Guid.NewGuid(),
                    FechaHoraMuestra = GetDate(r, "FechaHoraMuestra") ?? DateTime.Now,
                    SerieTemporalId = GetGuid(r, "SerieTemporalId") ?? Guid.Empty
                };
                muestras.Add(m);
            }
            if (muestras.Count > 0)
            {
                ctx.AddRange(muestras);
                ctx.SaveChanges();
            }
        }

        if (File.Exists(detallesPath))
        {
            var rows = ReadCsv(detallesPath);
            var detalles = new List<DetalleMuestra>();
            foreach (var r in rows)
            {
                var d = new DetalleMuestra
                {
                    Id = GetGuid(r, "Id") ?? Guid.NewGuid(),
                    Valor = GetDouble(r, "Valor") ?? 0,
                    MuestraSismicaId = GetGuid(r, "MuestraSismicaId") ?? Guid.Empty,
                    TipoDeDatoId = GetGuid(r, "TipoDeDatoId") ?? Guid.Empty
                };
                detalles.Add(d);
            }
            if (detalles.Count > 0)
            {
                ctx.AddRange(detalles);
                ctx.SaveChanges();
            }
        }
    }

    // ===== Helpers dominio (crear si no existe) =====
    private static AlcanceSismo GetOrCreateAlcance(RedSismicaContext ctx, string nombre)
    {
        var x = ctx.Alcances.FirstOrDefault(a => a.Nombre == nombre);
        if (x != null) return x;
        x = new AlcanceSismo { Id = Guid.NewGuid(), Nombre = nombre, Descripcion = "" };
        ctx.Add(x); ctx.SaveChanges();
        return x;
    }

    private static ClasificacionSismo GetOrCreateClasificacion(RedSismicaContext ctx, string nombre)
    {
        var x = ctx.Clasificaciones.FirstOrDefault(a => a.Nombre == nombre);
        if (x != null) return x;
        x = new ClasificacionSismo { Id = Guid.NewGuid(), Nombre = nombre, KmProfundidadDesde = 0, KmProfundidadHasta = 0 };
        ctx.Add(x); ctx.SaveChanges();
        return x;
    }

    private static OrigenDeGeneracion GetOrCreateOrigen(RedSismicaContext ctx, string nombre)
    {
        var x = ctx.Origenes.FirstOrDefault(a => a.Nombre == nombre);
        if (x != null) return x;
        x = new OrigenDeGeneracion { Id = Guid.NewGuid(), Nombre = nombre, Descripcion = "" };
        ctx.Add(x); ctx.SaveChanges();
        return x;
    }

    private static Empleado GetOrCreateEmpleadoPorMail(RedSismicaContext ctx, string mail)
    {
        var e = ctx.Empleados.FirstOrDefault(a => a.Mail == mail);
        if (e != null) return e;

        var u = new Usuario { Id = Guid.NewGuid(), NombreUsuario = mail.Split('@')[0], Contraseña = "operador" };
        var nuevo = new Empleado { Id = Guid.NewGuid(), Nombre = "Operador", Apellido = "Import", Mail = mail, Telefono = "000-000", UsuarioId = u.Id };

        ctx.AddRange(u, nuevo);
        ctx.SaveChanges();
        return nuevo;
    }

    // ===== Utilidades CSV =====
    private static List<Dictionary<string, string>> ReadCsv(string path)
    {
        var lines = File.ReadAllLines(path);
        if (lines.Length == 0) return new();
        var headers = lines[0].Split(';').Select(h => h.Trim()).ToArray();
        var rows = new List<Dictionary<string, string>>();
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cols = lines[i].Split(';');
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int c = 0; c < headers.Length && c < cols.Length; c++) dict[headers[c]] = cols[c].Trim();
            rows.Add(dict);
        }
        return rows;
    }

    private static string Get(Dictionary<string, string> r, string key)
        => r.TryGetValue(key, out var v) ? (string.IsNullOrWhiteSpace(v) ? null : v) : null;

    private static Guid? GetGuid(Dictionary<string, string> r, string key)
        => Guid.TryParse(Get(r, key), out var g) ? g : null;

    private static int? GetInt(Dictionary<string, string> r, string key)
        => int.TryParse(Get(r, key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;

    private static double? GetDouble(Dictionary<string, string> r, string key)
        => double.TryParse(Get(r, key), NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : null;

    private static DateTime? GetDate(Dictionary<string, string> r, string key)
        => DateTime.TryParse(Get(r, key), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var d) ? d : null;
}
