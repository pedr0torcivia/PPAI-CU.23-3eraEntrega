// BulkTxtImporter.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using EFCore.BulkExtensions; // solo catálogos SIN FKs
using Microsoft.EntityFrameworkCore;
using PPAI_2.Infra.Data;
using PPAI_Revisiones.Modelos;
using PPAI_Revisiones.Modelos.Estados;

internal static class BulkTxtImporter
{
    public static void Run(RedSismicaContext ctx, string folder)
    {
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        using var tx = ctx.Database.BeginTransaction();

        ImportOrigenes(ctx, Path.Combine(folder, "Origenes.txt"));
        ImportAlcances(ctx, Path.Combine(folder, "Alcances.txt"));
        ImportClasificaciones(ctx, Path.Combine(folder, "Clasificaciones.txt"));
        ImportTiposDeDato(ctx, Path.Combine(folder, "TiposDeDato.txt"));
        ImportUsuariosYEmpleados(ctx, Path.Combine(folder, "Usuarios.txt"), Path.Combine(folder, "Empleados.txt"));

        // Detectar nombre real del archivo de eventos
        var eventosFile = File.Exists(Path.Combine(folder, "EventosSismicos.txt"))
            ? Path.Combine(folder, "EventosSismicos.txt")
            : Path.Combine(folder, "Eventos.txt"); // ← vos subiste "Eventos.txt"
        ImportEventosSismicos(ctx, eventosFile);

        ImportSismografosYEstaciones(ctx, Path.Combine(folder, "Estaciones.txt"), Path.Combine(folder, "Sismografos.txt"));
        ImportSeriesMuestrasDetalles(ctx,
            Path.Combine(folder, "SeriesTemporales.txt"),
            Path.Combine(folder, "Muestras.txt"),
            Path.Combine(folder, "DetalleMuestra.txt"));

        ctx.SaveChanges();
        tx.Commit();
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
                    KmProfundidadDesde = GetDouble(r, "KmProfundidadDesde") ?? 0,
                    KmProfundidadHasta = GetDouble(r, "KmProfundidadHasta") ?? 0
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
            if (nuevosU.Count > 0) { ctx.AddRange(nuevosU); ctx.SaveChanges(); }
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
            if (nuevosE.Count > 0) { ctx.AddRange(nuevosE); ctx.SaveChanges(); }
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
        var insertados = 0; var saltados = 0;

        foreach (var r in rows)
        {
            var id = GetGuid(r, "Id") ?? Guid.NewGuid();
            if (ctx.EventosSismicos.Any(x => x.Id == id)) { saltados++; continue; }

            // Admitir por NOMBRE (como en tus TXT): crea si no existe
            var alcance = GetOrCreateAlcance(ctx, Get(r, "AlcanceNombre") ?? "Local");
            var clasif = GetOrCreateClasificacion(ctx, Get(r, "ClasificacionNombre") ?? "Preliminar");
            var origen = GetOrCreateOrigen(ctx, Get(r, "OrigenNombre") ?? "Autodetectado");
            var resp = GetOrCreateEmpleadoPorMail(ctx, Get(r, "ResponsableMail") ?? "operador@redsismica.local");

            var ev = new EventoSismico
            {
                Id = id,
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

            ev.MaterializarEstadoDesdeNombre();

            ctx.Add(ev);
            insertados++;
        }

        if (insertados > 0) ctx.SaveChanges();

        // Cambios abiertos “Autodetectado” (solo si aplica y aún no existe CE)
        var nuevosCE = new List<CambioDeEstado>();
        foreach (var ev in ctx.EventosSismicos.AsNoTracking().Where(x => x.EstadoActualNombre == "Autodetectado"))
        {
            bool yaTieneCE = ctx.CambiosDeEstado.Any(c => c.EventoSismicoId == ev.Id && c.EstadoNombre == "Autodetectado");
            if (yaTieneCE) continue;

            nuevosCE.Add(new CambioDeEstado
            {
                Id = Guid.NewGuid(),
                EventoSismicoId = ev.Id,
                EstadoNombre = "Autodetectado",
                EstadoActual = new Autodetectado(),
                FechaHoraInicio = ev.FechaHoraDeteccion,
                FechaHoraFin = null,
                ResponsableId = ev.ResponsableId
            });
        }
        if (nuevosCE.Count > 0) { ctx.AddRange(nuevosCE); ctx.SaveChanges(); }
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
                if (!ctx.Estaciones.Any(x => x.Id == e.Id))
                    nuevos.Add(e);
            }
            if (nuevos.Count > 0) { ctx.AddRange(nuevos); ctx.SaveChanges(); }
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
                var estId = GetGuid(r, "EstacionId"); // nullable en schema → OK
                if (estId.HasValue) s.EstacionId = estId.Value;

                if (!ctx.Sismografos.Any(x => x.Id == s.Id))
                    nuevos.Add(s);
            }
            if (nuevos.Count > 0) { ctx.AddRange(nuevos); ctx.SaveChanges(); }
        }
    }

    private static void ImportSeriesMuestrasDetalles(RedSismicaContext ctx, string seriesPath, string muestrasPath, string detallesPath)
    {
        // SERIES (FK-safe)
        if (File.Exists(seriesPath))
        {
            var rows = ReadCsv(seriesPath);
            int ins = 0, fkSkip = 0;
            foreach (var r in rows)
            {
                var id = GetGuid(r, "Id") ?? Guid.NewGuid();
                if (ctx.SeriesTemporales.Any(x => x.Id == id)) continue;

                var evId = GetGuid(r, "EventoSismicoId");
                var sisId = GetGuid(r, "SismografoId");
                if (evId == null || sisId == null || ctx.EventosSismicos.Find(evId) == null || ctx.Sismografos.Find(sisId) == null)
                { fkSkip++; continue; }

                var st = new SerieTemporal
                {
                    Id = id,
                    CondicionAlarma = (Get(r, "CondicionAlarma") ?? "0").Trim() is "1" or "true" or "TRUE",
                    FechaHoraInicioRegistroMuestras = GetDate(r, "FechaHoraInicioRegistroMuestras") ?? DateTime.Now,
                    FechaHoraRegistro = GetDate(r, "FechaHoraRegistro") ?? DateTime.Now,
                    FrecuenciaMuestreo = GetDouble(r, "FrecuenciaMuestreo") ?? 0,
                    SismografoId = sisId,
                    EventoSismicoId = evId
                };
                ctx.Add(st); ins++;
            }
            if (ins > 0) ctx.SaveChanges();
            if (fkSkip > 0) Console.WriteLine($"[SEED] SeriesTemporales: saltadas por FK={fkSkip}");
        }

        // MUESTRAS (FK-safe)
        if (File.Exists(muestrasPath))
        {
            var rows = ReadCsv(muestrasPath);
            int ins = 0, fkSkip = 0;
            foreach (var r in rows)
            {
                var id = GetGuid(r, "Id") ?? Guid.NewGuid();
                if (ctx.Muestras.Any(x => x.Id == id)) continue;

                var serieId = GetGuid(r, "SerieTemporalId");
                if (serieId == null || ctx.SeriesTemporales.Find(serieId) == null)
                { fkSkip++; continue; }

                var m = new MuestraSismica
                {
                    Id = id,
                    FechaHoraMuestra = GetDate(r, "FechaHoraMuestra") ?? DateTime.Now,
                    SerieTemporalId = serieId.Value
                };
                ctx.Add(m); ins++;
            }
            if (ins > 0) ctx.SaveChanges();
            if (fkSkip > 0) Console.WriteLine($"[SEED] Muestras: saltadas por FK={fkSkip}");
        }

        // DETALLES (FK-safe con mapeo por Denominación o por Id) — SIN GroupBy en EF
        if (File.Exists(detallesPath))
        {
            // Asegura que Series/Muestras/Tipos estén persistidos
            ctx.SaveChanges();
            ctx.ChangeTracker.Clear();

            // Cache: Muestras por Id
            var muestrasOK = ctx.Muestras.AsNoTracking()
                                 .Select(m => m.Id)
                                 .ToHashSet();

            // Cache Tipos de Dato en memoria
            var tiposList = ctx.TiposDeDato.AsNoTracking().ToList();
            var tiposPorId = tiposList.ToDictionary(t => t.Id, t => t);
            var tiposPorDenom = new Dictionary<string, TipoDeDato>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in tiposList)
            {
                var key = (t.Denominacion ?? string.Empty).Trim();
                if (!tiposPorDenom.ContainsKey(key))
                    tiposPorDenom[key] = t;
            }

            var rows = ReadCsv(detallesPath);
            int ins = 0, fkSkip = 0;

            foreach (var r in rows)
            {
                var id = GetGuid(r, "Id") ?? Guid.NewGuid();
                if (ctx.DetallesMuestra.AsNoTracking().Any(x => x.Id == id))
                    continue;

                var muestraId = GetGuid(r, "MuestraSismicaId");
                if (muestraId == null || !muestrasOK.Contains(muestraId.Value))
                {
                    fkSkip++;
                    Console.WriteLine($"[SEED] Detalle {id}: muestra inexistente -> skip");
                    continue;
                }

                // Resolver TipoDeDato
                Guid? tipoId = GetGuid(r, "TipoDeDatoId");
                if (tipoId.HasValue && !tiposPorId.ContainsKey(tipoId.Value))
                    tipoId = null;

                if (!tipoId.HasValue)
                {
                    var denomKey = (Get(r, "TipoDeDatoDenominacion") ?? Get(r, "TipoDeDato") ?? string.Empty).Trim();
                    if (!string.IsNullOrWhiteSpace(denomKey) && tiposPorDenom.TryGetValue(denomKey, out var tipo))
                        tipoId = tipo.Id;
                }

                if (!tipoId.HasValue)
                {
                    fkSkip++;
                    Console.WriteLine($"[SEED] Detalle {id}: TipoDeDato no resuelto (ni Id ni Denominación) -> skip");
                    continue;
                }

                var d = new DetalleMuestra
                {
                    Id = id,
                    MuestraSismicaId = muestraId.Value,
                    TipoDeDatoId = tipoId.Value,
                    Valor = GetDouble(r, "Valor") ?? 0
                };

                ctx.DetallesMuestra.Add(d);
                ins++;
            }

            if (ins > 0) ctx.SaveChanges();
            Console.WriteLine($"[SEED] DetallesMuestra insertadas={ins}, saltadas por FK={fkSkip}");
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
        if (!File.Exists(path)) return new();
        var lines = File.ReadAllLines(path);
        if (lines.Length == 0) return new();
        var headers = lines[0].Split(';').Select(h => h.Trim()).ToArray();
        var rows = new List<Dictionary<string, string>>();
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cols = lines[i].Split(';');
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int c = 0; c < headers.Length && c < cols.Length; c++) dict[headers[c]] = (cols[c] ?? "").Trim();
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
