// Infra/Data/Mapping/DomainEfMapper.cs
using System;
using System.Linq;
using D = PPAI_Revisiones.Dominio;
using PPAI_Revisiones.Modelos;
using PPAI_Revisiones.Modelos.Estados;
using E = PPAI_2.Infra.Data.EFModels;
using M = PPAI_Revisiones.Modelos;            // para DetalleMuestraSismica (está en Modelos)

namespace PPAI_2.Infra.Data.Mapping
{
    public static class DomainEfMapper
    {
        // ========= EventoSismico =========
        public static M.EventoSismico ToDomain(this E.EventoSismicoEF ef)
        {
            var d = new M.EventoSismico
            {
                Id = ef.Id, // shim que tu CU usa
                FechaHoraInicio = ef.FechaHoraInicio, // shim → Ocurrencia
                FechaHoraFin = ef.FechaHoraDeteccion, // dominio: opcional
                LatitudEpicentro = ef.LatitudEpicentro,
                LongitudEpicentro = ef.LongitudEpicentro,
                LatitudHipocentro = ef.LatitudHipocentro,
                LongitudHipocentro = ef.LongitudHipocentro,
                ValorMagnitud = ef.ValorMagnitud,
                Alcance = ef.Alcance?.ToDomain(),
                Clasificacion = ef.Clasificacion?.ToDomain(),
                Origen = ef.Origen?.ToDomain(), // shim hacia OrigenDeGeneracion
                EstadoActualNombre = ef.EstadoActualNombre,
                SeriesTemporales = ef.SeriesTemporales?.Select(ToDomain).ToList() ?? new()
            };

            d.MaterializarEstadoDesdeNombre();

            if (ef.CambiosDeEstado != null)
                foreach (var ce in ef.CambiosDeEstado)
                    d.CambiosDeEstado.Add(ce.ToDomain());

            d.MaterializarEstadosDeCambios();
            return d;
        }

        // Para crear/actualizar EF desde dominio: se pasan IDs ya resueltos por el repo
        public static E.EventoSismicoEF ToEF(this M.EventoSismico d,
            Guid id, Guid alcanceId, Guid clasifId, Guid origenId, Guid responsableId)
        {
            return new E.EventoSismicoEF
            {
                Id = id,
                FechaHoraInicio = d.FechaHoraInicio,
                FechaHoraDeteccion = d.FechaHoraFin ?? d.FechaHoraInicio,
                LatitudEpicentro = d.LatitudEpicentro,
                LongitudEpicentro = d.LongitudEpicentro,
                LatitudHipocentro = d.LatitudHipocentro,
                LongitudHipocentro = d.LongitudHipocentro,
                ValorMagnitud = d.ValorMagnitud,
                EstadoActualNombre = d.EstadoActualNombre ?? d.Estado?.Nombre ?? "Autodetectado",
                AlcanceId = alcanceId,
                ClasificacionId = clasifId,
                OrigenId = origenId,
                ResponsableId = responsableId,
                SeriesTemporales = d.SeriesTemporales?.Select(s => s.ToEF(Guid.NewGuid(), id, null)).ToList() ?? new()
            };
        }

        // ========= CambioDeEstado =========

        private static Estado MapEstadoFromName(string? nombre)
        {
            switch ((nombre ?? "Autodetectado").Trim())
            {
                case "Bloqueado": return new Bloqueado();
                case "Confirmado": return new Confirmado();
                case "Rechazado": return new Rechazado();
                case "Derivado": return new Derivado();
                case "Autodetectado":
                default: return new Autodetectado();
            }
        }

        public static CambioDeEstado ToDomain(this E.CambioDeEstadoEF ef)
            => new CambioDeEstado
            {
                // Id = ef.Id, // solo si tu modelo lo tiene
                FechaHoraInicio = ef.FechaHoraInicio,
                FechaHoraFin = ef.FechaHoraFin,

                // solo materializa el estado
                EstadoActual = MapEstadoFromName(ef.EstadoNombre),

                Responsable = ef.Responsable?.ToDomain()
            };



        public static E.CambioDeEstadoEF ToEF(this M.CambioDeEstado d, Guid id, Guid eventoId, Guid? responsableId)
            => new E.CambioDeEstadoEF
            {
                Id = id,
                EventoSismicoId = eventoId,
                ResponsableId = responsableId,
                // <- string solo en EF, derivado del objeto de dominio:
                EstadoNombre = d.EstadoActual?.Nombre ?? "Autodetectado",
                FechaHoraInicio = d.FechaHoraInicio,
                FechaHoraFin = d.FechaHoraFin
            };

        // ========= Serie/Muestra/Detalle =========
        public static D.SerieTemporal ToDomain(this E.SerieTemporalEF ef)
            => new D.SerieTemporal
            {
                CondicionAlarma = ef.CondicionAlarma,
                FechaHoraInicioRegistroMuestras = ef.FechaHoraInicioRegistroMuestras,
                FechaHoraRegistro = ef.FechaHoraRegistro,
                FrecuenciaMuestreo = ef.FrecuenciaMuestreo,
                Sismografo = ef.Sismografo?.ToDomain(),
                MuestrasSismicas = ef.Muestras?.Select(ToDomain).ToList() ?? new()
            };

        public static E.SerieTemporalEF ToEF(this D.SerieTemporal d, Guid id, Guid? eventoId, Guid? sismografoId)
            => new E.SerieTemporalEF
            {
                Id = id,
                EventoSismicoId = eventoId,
                SismografoId = sismografoId,
                CondicionAlarma = d.CondicionAlarma,
                FechaHoraInicioRegistroMuestras = d.FechaHoraInicioRegistroMuestras,
                FechaHoraRegistro = d.FechaHoraRegistro,
                FrecuenciaMuestreo = d.FrecuenciaMuestreo,
                Muestras = d.MuestrasSismicas?.Select(m => m.ToEF(Guid.NewGuid(), id)).ToList() ?? new()
            };

        public static D.MuestraSismica ToDomain(this E.MuestraSismicaEF ef)
            => new D.MuestraSismica
            {
                FechaHoraMuestra = ef.FechaHoraMuestra,
                DetalleMuestraSismica = ef.Detalles?.Select(ToDomain).ToList() ?? new()
            };

        public static E.MuestraSismicaEF ToEF(this D.MuestraSismica d, Guid id, Guid serieId)
            => new E.MuestraSismicaEF
            {
                Id = id,
                SerieTemporalId = serieId,
                FechaHoraMuestra = d.FechaHoraMuestra,
                Detalles = d.DetalleMuestraSismica?.Select(x => x.ToEF(Guid.NewGuid(), id)).ToList() ?? new()
            };

        public static M.DetalleMuestraSismica ToDomain(this E.DetalleMuestraEF ef)
            => new M.DetalleMuestraSismica
            {
                Valor = ef.Valor,
                TipoDeDato = ef.TipoDeDato?.ToDomain()
            };

        public static E.DetalleMuestraEF ToEF(this M.DetalleMuestraSismica d, Guid id, Guid muestraId, Guid tipoDeDatoId = default)
            => new E.DetalleMuestraEF
            {
                Id = id,
                MuestraSismicaId = muestraId,
                Valor = d.Valor,
                TipoDeDatoId = tipoDeDatoId // resolver por lookup en repo
            };

        // ========= Catálogos / básicos =========
        public static D.AlcanceSismo ToDomain(this E.AlcanceSismoEF ef)
            => new D.AlcanceSismo { Nombre = ef.Nombre, Descripcion = ef.Descripcion };

        public static D.ClasificacionSismo ToDomain(this E.ClasificacionSismoEF ef)
            => new D.ClasificacionSismo
            {
                Nombre = ef.Nombre,
                KmProfundidadDesde = ef.KmProfundidadDesde,
                KmProfundidadHasta = ef.KmProfundidadHasta
            };

        public static D.OrigenDeGeneracion ToDomain(this E.OrigenDeGeneracionEF ef)
            => new D.OrigenDeGeneracion { Nombre = ef.Nombre, Descripcion = ef.Descripcion };

        public static D.TipoDeDato ToDomain(this E.TipoDeDatoEF ef)
            => new D.TipoDeDato { Denominacion = ef.Denominacion, NombreUnidadMedida = ef.NombreUnidadMedida, ValorUmbral = ef.ValorUmbral };

        public static D.Sismografo ToDomain(this E.SismografoEF ef)
            => new D.Sismografo
            {
                IdentificadorSismografo = ef.IdentificadorSismografo,
                NroSerie = ef.NroSerie,
                FechaAdquisicion = ef.FechaAdquisicion,
                Estacion = ef.Estacion?.ToDomain()
            };

        public static M.EstacionSismologica ToDomain(this E.EstacionSismologicaEF ef)
            => new M.EstacionSismologica
            {
                CodigoEstacion = ef.CodigoEstacion,
                DocumentoCertificacionAdq = ef.DocumentoCertificacionAdq,
                FechaSolicitudCeritficacion = ef.FechaSolicitudCertificacion,
                Latitud = ef.Latitud,
                Longitud = ef.Longitud,
                Nombre = ef.Nombre,
                NroCertificadoAdquisicion = ef.NroCertificacionAdquisicion
            };

        public static D.Empleado ToDomain(this E.EmpleadoEF ef)
            => new D.Empleado { Nombre = ef.Nombre, Apellido = ef.Apellido, Mail = ef.Mail, Telefono = ef.Telefono };

        public static D.Usuario ToDomain(this E.UsuarioEF ef)
            => new D.Usuario { NombreUsuario = ef.NombreUsuario, Contraseña = ef.Contraseña };
    }
}
