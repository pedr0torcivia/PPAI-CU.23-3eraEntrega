// Infra/Data/Mapping/DomainEfMapper.cs
using System;
using System.Collections.Generic;
using System.Linq;
// Alias para las clases de Infraestructura/EF
using E = PPAI_2.Infra.Data.EFModels;
// Alias para las clases de Dominio/Modelos
using M = PPAI_Revisiones.Modelos;
using PPAI_Revisiones.Modelos.Estados;

namespace PPAI_2.Infra.Data.Mapping
{
    public static class DomainEfMapper
    {
        // ==========================
        // ===== EF -> Dominio ======
        // ==========================

        // ---- Catálogos / básicos ----
        public static M.AlcanceSismo ToDomain(this E.AlcanceSismoEF ef)
            => new M.AlcanceSismo(ef.Nombre, ef.Descripcion);

        public static M.OrigenDeGeneracion ToDomain(this E.OrigenDeGeneracionEF ef)
            => new M.OrigenDeGeneracion(ef.Nombre, ef.Descripcion);

        public static M.ClasificacionSismo ToDomain(this E.ClasificacionSismoEF ef)
            => new M.ClasificacionSismo(ef.Nombre, ef.KmProfundidadDesde, ef.KmProfundidadHasta);

        public static M.TipoDeDato ToDomain(this E.TipoDeDatoEF ef)
            => new M.TipoDeDato(ef.Denominacion, ef.NombreUnidadMedida, ef.ValorUmbral);

        public static M.Empleado ToDomain(this E.EmpleadoEF ef)
            => new M.Empleado(ef.Apellido, ef.Nombre, ef.Mail, ef.Telefono, ef.Rol);

        public static M.Usuario ToDomain(this E.UsuarioEF ef)
        {
            // La relación 1:1 con Empleado debe ser mapeada a dominio
            var empleado = ef.Empleado?.ToDomain()
                               ?? throw new InvalidOperationException("UsuarioEF sin Empleado asociado.");

            return new M.Usuario(
                nombreUsuario: ef.NombreUsuario,
                contrasenia: ef.Contrasenia,
                empleado: empleado
            );
        }

        // Mapeo de Sismógrafo (asumimos que en el Dominio no necesita la Estación completa)
        public static M.Sismografo ToDomain(this E.SismografoEF ef)
        {
            // 1. Mapear la Estación (que es requerida por el constructor de Sismografo)
            var estacion = ef.Estacion?.ToDomain()
                                     ?? throw new InvalidOperationException("Sismógrafo sin estación asociada.");

            // 2. Usar el constructor de 4 argumentos: identificador, nroSerie, fechaAdquisicion, estacion
            return new M.Sismografo(
                identificadorSismografo: ef.IdentificadorSismografo,
                nroSerie: ef.NroSerie,
                fechaAdquisicion: ef.FechaAdquisicion,
                estacion: estacion // <-- Esto es el 4to parámetro requerido
            );
        }

        public static M.EstacionSismologica ToDomain(this E.EstacionSismologicaEF ef)
            // El constructor solo acepta 7 argumentos, NO la colección de Sismógrafos.
            => new M.EstacionSismologica(
                codigoEstacion: ef.CodigoEstacion,
                documentoCertificacionAdq: ef.DocumentoCertificacionAdq,
                fechaSolicitudCertificacion: ef.FechaSolicitudCertificacion,
                latitud: ef.Latitud,
                longitud: ef.Longitud,
                nombre: ef.Nombre,
                nroCertificacionAdquisicion: ef.NroCertificacionAdquisicion
            );

        // ---- Series / Muestras / Detalles ----
        public static M.DetalleMuestraSismica ToDomain(this E.DetalleMuestraSismicaEF ef)
            => new M.DetalleMuestraSismica(ef.Valor, ef.TipoDeDato.ToDomain());

        public static M.MuestraSismica ToDomain(this E.MuestraSismicaEF ef)
        {
            var detalles = (ef.Detalles ?? new List<E.DetalleMuestraSismicaEF>())
                .Select(d => d.ToDomain()).ToList();

            return new M.MuestraSismica(ef.FechaHoraMuestra, detalles);
        }

        public static M.SerieTemporal ToDomain(this E.SerieTemporalEF ef)
        {
            var muestras = (ef.Muestras ?? new List<E.MuestraSismicaEF>())
                .Select(m => m.ToDomain()).ToList();

            return new M.SerieTemporal(
                condicionAlarma: ef.CondicionAlarma,
                fechaHoraInicioRegistroMuestras: ef.FechaHoraInicioRegistroMuestras,
                fechaHoraRegistro: ef.FechaHoraRegistro,
                frecuenciaMuestreo: ef.FrecuenciaMuestreo,
                muestras: muestras
            );
        }

        // ---- Cambios de estado ----
        public static M.CambioDeEstado ToDomain(this E.CambioDeEstadoEF ef)
        {
            var estado = Estado.FromName(ef.EstadoNombre)
                         ?? throw new InvalidOperationException($"Estado desconocido: '{ef.EstadoNombre}'");

            var responsable = ef.Responsable?.ToDomain();

            var dom = M.CambioDeEstado.Crear(
                inicio: ef.FechaHoraInicio,
                estado: estado,
                responsable: responsable
            );

            if (ef.FechaHoraFin.HasValue)
                dom.SetFechaHoraFin(ef.FechaHoraFin.Value);

            return dom;
        }

        // ---- Evento ----
        public static M.EventoSismico ToDomain(this E.EventoSismicoEF ef)
        {
            var alcance = ef.Alcance?.ToDomain()
                             ?? throw new InvalidOperationException("Alcance nulo en EventoSismicoEF.");
            var origen = ef.Origen?.ToDomain()
                             ?? throw new InvalidOperationException("Origen nulo en EventoSismicoEF.");
            var clasif = ef.Clasificacion?.ToDomain()
                             ?? throw new InvalidOperationException("Clasificación nula en EventoSismicoEF.");
            var estado = Estado.FromName(ef.EstadoActualNombre)
                             ?? throw new InvalidOperationException($"Estado desconocido: '{ef.EstadoActualNombre}'");

            var series = (ef.SeriesTemporales ?? new List<E.SerieTemporalEF>())
                .Select(s => s.ToDomain()).ToList();

            // Usamos FechaHoraOcurrencia si existe, o FechaHoraInicio si la BD no lo tiene separado
            var dom = new M.EventoSismico(
                fechaHoraOcurrencia: ef.FechaHoraOcurrencia, // Asumo que EventoSismicoEF tiene FechaHoraOcurrencia
                latEpicentro: ef.LatitudEpicentro,
                lonEpicentro: ef.LongitudEpicentro,
                latHipocentro: ef.LatitudHipocentro,
                lonHipocentro: ef.LongitudHipocentro,
                valorMagnitud: ef.ValorMagnitud,
                alcance: alcance,
                origen: origen,
                clasificacion: clasif,
                estadoInicial: estado,
                series: series
            );

            if (ef.FechaHoraFin.HasValue)
                dom.EstablecerFin(ef.FechaHoraFin.Value);

            // Agregar cambios de estado ya que EventoSismico los maneja internamente
            foreach (var ce in (ef.CambiosDeEstado ?? new List<E.CambioDeEstadoEF>())
                                 .OrderBy(c => c.FechaHoraInicio))
                dom.AgregarCambioEstado(ce.ToDomain());

            return dom;
        }

        // ==========================
        // == Dominio -> EF (mín) ==
        // ==========================

        /// <summary>
        /// Mapea un CambioDeEstado de dominio a EF, inyectando IDs y FKs que el dominio desconoce.
        /// </summary>
        public static E.CambioDeEstadoEF ToEf(this M.CambioDeEstado dom, Guid eventoId, Guid? responsableId)
        {
            if (dom == null) throw new ArgumentNullException(nameof(dom));
            if (dom.EstadoActual == null) throw new InvalidOperationException("CambioDeEstado sin EstadoActual.");

            return new E.CambioDeEstadoEF
            {
                Id = Guid.NewGuid(),
                EventoSismicoId = eventoId,

                FechaHoraInicio = dom.FechaHoraInicio ?? DateTime.Now,
                FechaHoraFin = dom.FechaHoraFin,

                EstadoNombre = dom.EstadoActual.Nombre,
                ResponsableId = responsableId
            };
        }

        /// <summary>
        /// Mapea un Usuario de dominio a EF, inyectando IDs y FKs que el dominio desconoce.
        /// </summary>
        public static E.UsuarioEF ToEf(this M.Usuario dom, Guid usuarioId, Guid empleadoId)
        {
            if (dom == null) throw new ArgumentNullException(nameof(dom));

            return new E.UsuarioEF
            {
                Id = usuarioId,
                EmpleadoId = empleadoId,

                NombreUsuario = dom.NombreUsuario,
                Contrasenia = dom.Contrasenia,
            };
        }
    }
}