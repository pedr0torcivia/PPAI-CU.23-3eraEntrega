// Infra/Data/BulkTxtImporter.cs (ADAPTADO)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using EFCore.BulkExtensions; // para catálogos sin FK
using Microsoft.EntityFrameworkCore;
using PPAI_2.Infra.Data;
using PPAI_2.Infra.Data.EFModels;

// La clase es interna static para no tener que usar un using en PantallaNuevaRevision
internal static class BulkTxtImporter
{
    // ======= CONFIG =======
    private static readonly string[] DateFormats = new[]
    {
        "yyyy-MM-dd HH:mm:ss", "yyyy/MM/dd HH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss", "yyyy/MM/ddTHH:mm:ss"
    };
    private static readonly CultureInfo CI = CultureInfo.InvariantCulture;

    // ======= ENTRYPOINT =======
    public static void Run(RedSismicaContext ctx, string folder)
    {
        Directory.CreateDirectory(folder);

        using var tx = ctx.Database.BeginTransaction();
        try
        {
            // 1) Catálogos y básicos (sin FK o con FK simple)
            ImportAlcances(ctx, Path.Combine(folder, "Alcances.txt"));
            ImportClasificaciones(ctx, Path.Combine(folder, "Clasificaciones.txt"));
            ImportOrigenes(ctx, Path.Combine(folder, "Origenes.txt"));
            ImportTiposDeDato(ctx, Path.Combine(folder, "TiposDeDato.txt"));

            ImportEstaciones(ctx, Path.Combine(folder, "Estaciones.txt"));
            ImportSismografos(ctx, Path.Combine(folder, "Sismografos.txt"));

            ImportUsuarios(ctx, Path.Combine(folder, "Usuarios.txt"));
            ImportEmpleados(ctx, Path.Combine(folder, "Empleados.txt"));

            // 2) Agregados dependientes
            ImportEventos(ctx, Path.Combine(folder, "EventosSismicos.txt"));
            ImportCambiosDeEstado(ctx, Path.Combine(folder, "CambiosDeEstado.txt"));

            // 3) Series / Muestras / Detalles
            ImportSeriesTemporales(ctx, Path.Combine(folder, "SeriesTemporales.txt"));
            ImportMuestras(ctx, Path.Combine(folder, "Muestras.txt"));
            ImportDetallesMuestra(ctx, Path.Combine(folder, "DetallesMuestra.txt"));

            tx.Commit();
        }
        catch (Exception ex)
        {
            try { tx.Rollback(); } catch { /* ignore */ }
            System.Windows.Forms.MessageBox.Show("Error en importación masiva:\n\n" + ex);
            throw;
        }
    }

    // ================== IMPORTS (CATÁLOGOS) ==================
    private static void ImportAlcances(RedSismicaContext ctx, string file)
    {
        if (!File.Exists(file)) return;
        var rows = ReadRows(file, 3);
        var nuevos = new List<AlcanceSismoEF>();

        var existentes = ctx.Set<AlcanceSismoEF>().AsNoTracking()
            .ToDictionary(x => x.Id, _ => true);

        foreach (var r in rows)
        {
            var id = ParseGuid(r[0]); if (id == Guid.Empty) continue;
            if (existentes.ContainsKey(id)) continue;

            nuevos.Add(new AlcanceSismoEF
            {
                Id = id,
                Nombre = Nz(r[1]),
                Descripcion = Nz(r[2])
            });
        }
        BulkInsertIfAny(ctx, nuevos);
    }

    private static void ImportClasificaciones(RedSismicaContext ctx, string file)
    {
        if (!File.Exists(file)) return;
        var rows = ReadRows(file, 4);
        var nuevos = new List<ClasificacionSismoEF>();
        var existentes = ctx.Set<ClasificacionSismoEF>().AsNoTracking()
            .ToDictionary(x => x.Id, _ => true);

        foreach (var r in rows)
        {
            var id = ParseGuid(r[0]); if (id == Guid.Empty) continue;
            if (existentes.ContainsKey(id)) continue;

            nuevos.Add(new ClasificacionSismoEF
            {
                Id = id,
                Nombre = Nz(r[1]),
                KmProfundidadDesde = ParseDouble(r[2]),
                KmProfundidadHasta = ParseDouble(r[3]),
            });
        }
        BulkInsertIfAny(ctx, nuevos);
    }

    private static void ImportOrigenes(RedSismicaContext ctx, string file)
    {
        if (!File.Exists(file)) return;
        var rows = ReadRows(file, 3);
        var nuevos = new List<OrigenDeGeneracionEF>();
        var existentes = ctx.Set<OrigenDeGeneracionEF>().AsNoTracking()
            .ToDictionary(x => x.Id, _ => true);

        foreach (var r in rows)
        {
            var id = ParseGuid(r[0]); if (id == Guid.Empty) continue;
            if (existentes.ContainsKey(id)) continue;

            nuevos.Add(new OrigenDeGeneracionEF
            {
                Id = id,
                Nombre = Nz(r[1]),
                Descripcion = Nz(r[2]),
            });
        }
        BulkInsertIfAny(ctx, nuevos);
    }

    private static void ImportTiposDeDato(RedSismicaContext ctx, string file)
    {
        if (!File.Exists(file)) return;
        var rows = ReadRows(file, 4);
        var nuevos = new List<TipoDeDatoEF>();
        var existentes = ctx.Set<TipoDeDatoEF>().AsNoTracking()
            .ToDictionary(x => x.Id, _ => true);

        foreach (var r in rows)
        {
            var id = ParseGuid(r[0]); if (id == Guid.Empty) continue;
            if (existentes.ContainsKey(id)) continue;

            nuevos.Add(new TipoDeDatoEF
            {
                Id = id,
                Denominacion = Nz(r[1]),
                NombreUnidadMedida = Nz(r[2]),
                ValorUmbral = ParseDouble(r[3]) // si no hay valor, 0
            });
        }
        BulkInsertIfAny(ctx, nuevos);
    }

    private static void ImportEstaciones(RedSismicaContext ctx, string file)
    {
        if (!File.Exists(file)) return;
        // Formato mínimo esperado: Id;CodigoEstacion;Nombre;Latitud;Longitud
        // Columnas 5, 6, 7 son opcionales
        var rows = ReadRows(file, 5);
        var nuevos = new List<EstacionSismologicaEF>();
        var existentes = ctx.Set<EstacionSismologicaEF>().AsNoTracking()
            .ToDictionary(x => x.Id, _ => true);

        foreach (var r in rows)
        {
            var id = ParseGuid(r[0]); if (id == Guid.Empty) continue;
            if (existentes.ContainsKey(id)) continue;

            var est = new EstacionSismologicaEF
            {
                Id = id,
                CodigoEstacion = Safe(r, 1),
                Nombre = Safe(r, 2),
                Latitud = ParseDouble(Safe(r, 3)),
                Longitud = ParseDouble(Safe(r, 4)),
            };
            // opcionales del DER si vinieran
            est.DocumentoCertificacionAdq = Safe(r, 5);
            est.NroCertificacionAdquisicion = Safe(r, 6);
            est.FechaSolicitudCertificacion = ParseDate(Safe(r, 7)) ?? DateTime.MinValue;

            nuevos.Add(est);
        }
        ctx.Set<EstacionSismologicaEF>().AddRange(nuevos);
        ctx.SaveChanges();
    }

    private static void ImportSismografos(RedSismicaContext ctx, string file)
    {
        if (!File.Exists(file)) return;
        // Esperado: Id;IdentificadorSismografo;NroSerie;FechaAdquisicion;CodigoEstacion
        var rows = ReadRows(file, 5);
        var nuevos = new List<SismografoEF>();

        // Resolución por CodigoEstacion
        var codigo2EstacionId = ctx.Set<EstacionSismologicaEF>().AsNoTracking()
            .Where(e => e.CodigoEstacion != null)
            .ToDictionary(e => e.CodigoEstacion!, e => e.Id);

        var existentes = ctx.Set<SismografoEF>().AsNoTracking()
            .ToDictionary(x => x.Id, _ => true);

        foreach (var r in rows)
        {
            var id = ParseGuid(r[0]); if (id == Guid.Empty) continue;
            if (existentes.ContainsKey(id)) continue;

            var codigoEst = Safe(r, 4);
            if (!codigo2EstacionId.TryGetValue(codigoEst, out var estacionId))
            {
                Debug.WriteLine($"[SISM] SKIP: Estación inexistente para código '{codigoEst}'");
                continue; // ADAPTACIÓN: evitar Guid.Empty en FK
            }

            nuevos.Add(new SismografoEF
            {
                Id = id,
                IdentificadorSismografo = Nz(r[1]),
                NroSerie = Nz(r[2]),
                FechaAdquisicion = ParseDate(Safe(r, 3)),
                EstacionId = estacionId
            });
        }
        ctx.Set<SismografoEF>().AddRange(nuevos);
        ctx.SaveChanges();
    }

    private static void ImportUsuarios(RedSismicaContext ctx, string file)
    {
        if (!File.Exists(file)) return;
        var rows = ReadRows(file, 3);
        var nuevos = new List<UsuarioEF>();
        var existentes = ctx.Set<UsuarioEF>().AsNoTracking()
            .ToDictionary(x => x.Id, _ => true);

        foreach (var r in rows)
        {
            var id = ParseGuid(r[0]); if (id == Guid.Empty) continue;
            if (existentes.ContainsKey(id)) continue;

            nuevos.Add(new UsuarioEF
            {
                Id = id,
                NombreUsuario = Nz(r[1]),
                Contrasenia = Nz(r[2]) // header puede venir como "Contraseña"
            });
        }
        BulkInsertIfAny(ctx, nuevos);
    }

    private static void ImportEmpleados(RedSismicaContext ctx, string file)
    {
        if (!File.Exists(file)) return;
        // Id;Nombre;Apellido;Mail;Telefono;UsuarioId
        var rows = ReadRows(file, 1);
        var nuevos = new List<EmpleadoEF>();

        var usuariosId = ctx.Set<UsuarioEF>().AsNoTracking()
            .Select(u => u.Id)
            .ToHashSet();

        var existentes = ctx.Set<EmpleadoEF>().AsNoTracking()
            .ToDictionary(x => x.Id, _ => true);

        foreach (var r in rows)
        {
            var id = ParseGuid(r[0]); if (id == Guid.Empty) continue;
            if (existentes.ContainsKey(id)) continue;

            Guid? usuarioId = null;
            var uTxt = Safe(r, 5);
            var uid = ParseGuid(uTxt);
            if (uid != Guid.Empty && usuariosId.Contains(uid)) usuarioId = uid;

            nuevos.Add(new EmpleadoEF
            {
                Id = id,
                Nombre = Safe(r, 1),
                Apellido = Safe(r, 2),
                Mail = Safe(r, 3),
                Telefono = Safe(r, 4),
                UsuarioId = usuarioId
            });
        }
        ctx.Set<EmpleadoEF>().AddRange(nuevos);
        ctx.SaveChanges();
    }

    // ================== IMPORTS (AGREGADOS) ==================
    private static void ImportEventos(RedSismicaContext ctx, string file)
    {
        if (!File.Exists(file))
        {
            System.Windows.Forms.MessageBox.Show($"No existe: {file}", "Import Eventos - Debug");
            return;
        }

        // Diccionarios normalizados (trim + lower)
        var alcanceByNombre = ctx.Set<AlcanceSismoEF>().AsNoTracking()
            .Where(a => a.Nombre != null).AsEnumerable().GroupBy(a => a.Nombre!.Trim().ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.First().Id);

        var clasifByNombre = ctx.Set<ClasificacionSismoEF>().AsNoTracking()
            .Where(c => c.Nombre != null).AsEnumerable().GroupBy(c => c.Nombre!.Trim().ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.First().Id);

        var origenByNombre = ctx.Set<OrigenDeGeneracionEF>().AsNoTracking()
            .Where(o => o.Nombre != null).AsEnumerable().GroupBy(o => o.Nombre!.Trim().ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.First().Id);

        // Empleados solo referencia (no se usa ResponsableId en EventoSismicoEF)
        var empleadoByMail = ctx.Set<EmpleadoEF>().AsNoTracking()
            .Where(e => e.Mail != null).AsEnumerable().GroupBy(e => e.Mail!.Trim().ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.First().Id);

        var existentes = ctx.Set<EventoSismicoEF>().AsNoTracking()
            .ToDictionary(e => e.Id, _ => true);

        // Id;FechaHoraInicio;FechaHoraDeteccion;LatEpi;LongEpi;LatHipo;LongHipo;Magnitud;EstadoActualNombre;AlcanceNombre;ClasificacionNombre;OrigenNombre;ResponsableMail
        var rows = ReadRows(file, 13);
        var nuevos = new List<EventoSismicoEF>();

        int total = 0, ok = 0, dup = 0, skipAlc = 0, skipCla = 0, skipOri = 0, skipResp = 0, parse = 0;

        foreach (var r in rows)
        {
            total++;

            var id = ParseGuid(r[0]);
            if (id == Guid.Empty) { parse++; continue; }
            if (existentes.ContainsKey(id)) { dup++; continue; }

            var alcanceNombre = Safe(r, 9).Trim().ToLowerInvariant();
            var clasifNombre = Safe(r, 10).Trim().ToLowerInvariant();
            var origenNombre = Safe(r, 11).Trim().ToLowerInvariant();
            var responsableMl = Safe(r, 12).Trim().ToLowerInvariant();

            if (!alcanceByNombre.TryGetValue(alcanceNombre, out var alcanceId)) { skipAlc++; Debug.WriteLine($"[EV] SKIP Alcance no encontrado: '{r[9]}'"); continue; }
            if (!clasifByNombre.TryGetValue(clasifNombre, out var clasifId)) { skipCla++; Debug.WriteLine($"[EV] SKIP Clasificación no encontrada: '{r[10]}'"); continue; }
            if (!origenByNombre.TryGetValue(origenNombre, out var origenId)) { skipOri++; Debug.WriteLine($"[EV] SKIP Origen no encontrado: '{r[11]}'"); continue; }

            if (!empleadoByMail.ContainsKey(responsableMl) && !string.IsNullOrWhiteSpace(responsableMl)) { skipResp++; Debug.WriteLine($"[EV] ResponsableMail no encontrado: '{r[12]}'"); }

            nuevos.Add(new EventoSismicoEF
            {
                Id = id,
                // Tomamos FechaHoraDeteccion si existe, o FechaHoraInicio
                FechaHoraOcurrencia = ParseDate(r[2]) ?? ParseDate(r[1]) ?? DateTime.MinValue,
                FechaHoraFin = null,
                LatitudEpicentro = ParseDouble(r[3]),
                LongitudEpicentro = ParseDouble(r[4]),
                LatitudHipocentro = ParseDouble(r[5]),
                LongitudHipocentro = ParseDouble(r[6]),
                ValorMagnitud = ParseDouble(r[7]),
                EstadoActualNombre = Nz(r[8], "Autodetectado"),
                AlcanceId = alcanceId,
                ClasificacionId = clasifId,
                OrigenId = origenId
            });
            ok++;
        }

        if (nuevos.Count > 0)
        {
            ctx.Set<EventoSismicoEF>().AddRange(nuevos);
            ctx.SaveChanges();
        }

        Debug.WriteLine($"[EV] filas={total}, ok={ok}, dup={dup}, parseErr={parse}, skipAlc={skipAlc}, skipCla={skipCla}, skipOri={skipOri}, skipResp={skipResp}");
        System.Windows.Forms.MessageBox.Show(
            $"EVENTOS importados: {ok}\n" +
            $"dup={dup} | parseErr={parse}\n" +
            $"skipAlc={skipAlc} | skipCla={skipCla} | skipOri={skipOri} | skipResp={skipResp}",
            "Import Eventos - Debug");
    }

    private static void ImportCambiosDeEstado(RedSismicaContext ctx, string file)
    {
        if (!File.Exists(file)) return;

        var empleadoByMail = ctx.Set<EmpleadoEF>().AsNoTracking()
            .Where(e => e.Mail != null).AsEnumerable().GroupBy(e => e.Mail!.Trim().ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.First().Id);

        var eventoExists = ctx.Set<EventoSismicoEF>().AsNoTracking()
            .ToDictionary(e => e.Id, _ => true);

        var existentes = ctx.Set<CambioDeEstadoEF>().AsNoTracking()
            .ToDictionary(c => c.Id, _ => true);

        // Id;EventoSismicoId;EstadoNombre;FechaHoraInicio;FechaHoraFin;ResponsableMail
        var rows = ReadRows(file, 6);
        var nuevos = new List<CambioDeEstadoEF>();

        int total = 0, ok = 0, dup = 0, skipEv = 0;

        foreach (var r in rows)
        {
            total++;

            var id = ParseGuid(r[0]); if (id == Guid.Empty) continue;
            if (existentes.ContainsKey(id)) { dup++; continue; }

            var eventoId = ParseGuid(Safe(r, 1));
            if (eventoId == Guid.Empty || !eventoExists.ContainsKey(eventoId)) { skipEv++; continue; }

            Guid? responsableId = null;
            var mail = Safe(r, 5).Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(mail) && empleadoByMail.TryGetValue(mail, out var rid))
                responsableId = rid;

            nuevos.Add(new CambioDeEstadoEF
            {
                Id = id,
                EventoSismicoId = eventoId,
                EstadoNombre = Nz(Safe(r, 2), "Autodetectado"),
                FechaHoraInicio = ParseDate(Safe(r, 3)) ?? DateTime.MinValue,
                FechaHoraFin = ParseDate(Safe(r, 4)),
                ResponsableId = responsableId
            });
            ok++;
        }

        if (nuevos.Count > 0)
        {
            ctx.Set<CambioDeEstadoEF>().AddRange(nuevos);
            ctx.SaveChanges();
        }

        System.Windows.Forms.MessageBox.Show(
            $"CAMBIOS importados: {ok}\n" +
            $"dup={dup} | skipEv={skipEv}",
            "Import Cambios - Debug");
    }

    // ================== IMPORTS (SERIES / MUESTRAS / DETALLES) ==================
    private static void ImportSeriesTemporales(RedSismicaContext ctx, string file)
    {
        if (!File.Exists(file)) return;

        var eventoExists = ctx.Set<EventoSismicoEF>().AsNoTracking()
            .ToDictionary(e => e.Id, _ => true);

        var sismografos = ctx.Set<SismografoEF>().AsNoTracking()
            .ToDictionary(s => s.Id, _ => true);

        var existentes = ctx.Set<SerieTemporalEF>().AsNoTracking()
            .ToDictionary(s => s.Id, _ => true);

        // Id;CondicionAlarma;FechaHoraInicioRegistroMuestras;FechaHoraRegistro;FrecuenciaMuestreo;SismografoId;EventoSismicoId
        var rows = ReadRows(file, 7);
        var nuevos = new List<SerieTemporalEF>();

        foreach (var r in rows)
        {
            var id = ParseGuid(r[0]); if (id == Guid.Empty) continue;
            if (existentes.ContainsKey(id)) continue;

            var sismId = ParseGuid(Safe(r, 5));
            if (!sismografos.ContainsKey(sismId))
            {
                Debug.WriteLine($"[SERIE] SKIP: Sismógrafo inexistente {Safe(r, 5)}");
                continue;
            }

            var evId = ParseGuid(Safe(r, 6));
            if (!eventoExists.ContainsKey(evId))
            {
                Debug.WriteLine($"[SERIE] SKIP: Evento inexistente {Safe(r, 6)}");
                continue; // ADAPTACIÓN: evitar Guid.Empty en FK
            }

            nuevos.Add(new SerieTemporalEF
            {
                Id = id,
                CondicionAlarma = ParseBool(r[1]),
                FechaHoraInicioRegistroMuestras = ParseDate(r[2]) ?? DateTime.MinValue,
                FechaHoraRegistro = ParseDate(r[3]) ?? DateTime.MinValue,
                FrecuenciaMuestreo = ParseDouble(r[4]),
                SismografoId = sismId,
                EventoSismicoId = evId
            });
        }

        ctx.Set<SerieTemporalEF>().AddRange(nuevos);
        ctx.SaveChanges();
    }

    private static void ImportMuestras(RedSismicaContext ctx, string file)
    {
        if (!File.Exists(file)) return;

        var serieExists = ctx.Set<SerieTemporalEF>().AsNoTracking()
            .ToDictionary(s => s.Id, _ => true);

        var existentes = ctx.Set<MuestraSismicaEF>().AsNoTracking()
            .ToDictionary(m => m.Id, _ => true);

        // Id;SerieTemporalId;FechaHoraMuestra
        var rows = ReadRows(file, 3);
        var nuevos = new List<MuestraSismicaEF>();

        foreach (var r in rows)
        {
            var id = ParseGuid(r[0]); if (id == Guid.Empty) continue;
            if (existentes.ContainsKey(id)) continue;

            var serieId = ParseGuid(r[1]);
            if (serieId == Guid.Empty || !serieExists.ContainsKey(serieId)) continue;

            nuevos.Add(new MuestraSismicaEF
            {
                Id = id,
                SerieTemporalId = serieId,
                FechaHoraMuestra = ParseDate(r[2]) ?? DateTime.MinValue
            });
        }

        ctx.Set<MuestraSismicaEF>().AddRange(nuevos);
        ctx.SaveChanges();
    }

    private static void ImportDetallesMuestra(RedSismicaContext ctx, string file)
    {
        if (!File.Exists(file)) return;

        var rawLines = File.ReadAllLines(file)
                           .Select(l => l?.Trim())
                           .Where(l => !string.IsNullOrWhiteSpace(l))
                           .ToList();
        if (rawLines.Count == 0) return;

        if (rawLines[0].StartsWith("Id;", StringComparison.OrdinalIgnoreCase))
            rawLines.RemoveAt(0);

        // 2) Diccionarios de FKs
        var muestras = ctx.Set<MuestraSismicaEF>().AsNoTracking()
            .ToDictionary(m => m.Id, _ => true);

        var tipoByDen = ctx.Set<TipoDeDatoEF>().AsNoTracking()
            .Where(t => t.Denominacion != null).AsEnumerable().GroupBy(t => t.Denominacion!.Trim().ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.First().Id);

        var existentes = ctx.Set<DetalleMuestraSismicaEF>().AsNoTracking()
            .ToDictionary(d => d.Id, _ => true);

        var nuevos = new List<DetalleMuestraSismicaEF>();
        int total = 0, ok = 0, skipFkMuestra = 0, skipTipo = 0, dup = 0, parse = 0;

        foreach (var line in rawLines)
        {
            total++;
            var r = line.Split(';').Select(p => p.Trim()).ToArray();
            if (r.Length < 4) { parse++; continue; }

            var id = ParseGuid(r[0]);
            if (id == Guid.Empty) { parse++; continue; }
            if (existentes.ContainsKey(id)) { dup++; continue; }

            var muestraId = ParseGuid(r[1]);
            if (muestraId == Guid.Empty || !muestras.ContainsKey(muestraId))
            {
                skipFkMuestra++;
                Debug.WriteLine($"[DETALLE] SKIP muestra inexistente: {r[1]} (línea {total})");
                continue;
            }

            var denKey = (Nz(r[2])).Trim().ToLowerInvariant();
            if (!tipoByDen.TryGetValue(denKey, out var tipoId))
            {
                skipTipo++;
                Debug.WriteLine($"[DETALLE] SKIP tipo no encontrado por denominación: '{r[2]}' (línea {total})");
                continue;
            }

            double valor = ParseDouble(r[3]);

            nuevos.Add(new DetalleMuestraSismicaEF
            {
                Id = id,
                MuestraSismicaId = muestraId,
                TipoDeDatoId = tipoId,
                Valor = valor
            });
            ok++;
        }

        if (nuevos.Count > 0)
        {
            ctx.Set<DetalleMuestraSismicaEF>().AddRange(nuevos);
            ctx.SaveChanges();
        }

        Debug.WriteLine($"[DETALLE] filas={total}, ok={ok}, dup={dup}, parseErr={parse}, skipMuestra={skipFkMuestra}, skipTipo={skipTipo}");

        System.Windows.Forms.MessageBox.Show(
            $"DETALLES importados: {ok}\n" +
            $"dup={dup} | parseErr={parse} | skipMuestra={skipFkMuestra} | skipTipo={skipTipo}",
            "Import Detalles - Debug");
    }

    // ================== HELPERS ==================
    private static void BulkInsertIfAny<T>(RedSismicaContext ctx, IList<T> data) where T : class
    {
        if (data == null || data.Count == 0) return;
        ctx.BulkInsert(data);
    }

    private static List<string[]> ReadRows(string file, int minCols = 1)
    {
        var list = new List<string[]>();
        using var sr = new StreamReader(file);
        string? line;
        bool headerChecked = false;

        while ((line = sr.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = line.Split(';').Select(p => p.Trim()).ToArray();

            // Saltear encabezado si detectamos nombres
            if (!headerChecked)
            {
                headerChecked = true;
                if (LooksLikeHeader(parts)) continue;
            }

            if (parts.Length < minCols)
            {
                Debug.WriteLine($"[IMPORT] Línea salteada (cols<{minCols}): {line}");
                continue;
            }
            list.Add(parts);
        }
        return list;
    }

    private static bool LooksLikeHeader(string[] cols)
    {
        // normaliza
        var norm = cols.Select(c => (c ?? "").Trim().ToLowerInvariant()).ToArray();

        // columnas típicas (se amplió para “contraseña”, “condicionalarma”)
        var known = new HashSet<string>(new[]
        {
            "id","nombre","descripcion","fecha","fechahorainicio","fechahoradeteccion",
            "latitud","longitud","valor","muestrasismicaid","tipodedatodenominacion",
            "frecuenciamuestreo","estadonombre","eventoid","responsablemail",
            "serietemporalid","fechahoramuestra","codigoestacion","nombreunidadmedida",
            "contraseña","condicionalarma","nombreusuario"
        });

        bool noneLooksLikeData = norm.All(s =>
            !Guid.TryParse(s, out _) &&
            !DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out _) &&
            !double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out _)
        );

        bool hasKnownNames = norm.Any(s => known.Contains(s));

        return noneLooksLikeData && hasKnownNames;
    }

    private static Guid ParseGuid(string s)
        => Guid.TryParse(Nz(s), out var g) ? g : Guid.Empty;

    private static bool ParseBool(string s)
        => bool.TryParse(Nz(s), out var b) ? b : Nz(s) == "1";

    private static double ParseDouble(string s)
        => double.TryParse(Nz(s), NumberStyles.Float, CI, out var d) ? d : 0d;

    private static DateTime? ParseDate(string s)
    {
        s = Nz(s);
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (DateTime.TryParseExact(s, DateFormats, CI, DateTimeStyles.AssumeLocal, out var dt)) return dt;
        if (DateTime.TryParse(s, CI, DateTimeStyles.AssumeLocal, out dt)) return dt;
        return null;
    }

    private static string Nz(string s, string def = "")
        => string.IsNullOrWhiteSpace(s) ? def : s.Trim();

    private static string Safe(string[] arr, int idx, string def = "")
        => (idx >= 0 && idx < arr.Length) ? arr[idx]?.Trim() ?? def : def;
}
