using PPAI_Revisiones.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using Estado = PPAI_Revisiones.Modelos.Estados.Estado;

namespace PPAI_Revisiones.Modelos
{
    public class EventoSismico
    {
        // ================== Datos básicos ==================
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraDeteccion { get; set; }
        public double LatitudEpicentro { get; set; }
        public double LongitudEpicentro { get; set; }
        public double LatitudHipocentro { get; set; }
        public double LongitudHipocentro { get; set; }
        public double ValorMagnitud { get; set; }

        // ================== Relaciones de dominio ==================
        public Estado EstadoActual { get; private set; }
        public AlcanceSismo Alcance { get; set; }
        public ClasificacionSismo Clasificacion { get; set; }
        public OrigenDeGeneracion Origen { get; set; }
        public Empleado Responsable { get; set; }

        // Series y cambios de estado
        public List<SerieTemporal> SeriesTemporales { get; private set; } = new();
        public List<CambioDeEstado> CambiosDeEstado { get; } = new();

        // ================== Consultas simples ==================
        public Estado getEstado() => EstadoActual;

        public string GetFechaHora() => FechaHoraInicio.ToString();
        public double GetLatitudEpicentro() => LatitudEpicentro;
        public double GetLongitudEpicentro() => LongitudEpicentro;
        public double GetLatitudHipocentro() => LatitudHipocentro;
        public double GetLongitudHipocentro() => LongitudHipocentro;
        public double GetMagnitud() => ValorMagnitud;

        // Flags según el Estado actual
        public bool sosAutodetectado() => EstadoActual?.EsAutodetectado == true;
        public bool sosEventoSinRevision() => EstadoActual?.EsEventoSinRevision == true;

        public string GetDatosOcurrencia()
        {
            var fecha = GetFechaHora();
            var latEpi = GetLatitudEpicentro();
            var lonEpi = GetLongitudEpicentro();
            var latHipo = GetLatitudHipocentro();
            var lonHipo = GetLongitudHipocentro();
            var magnitud = GetMagnitud();

            return $"Inicio: {fecha}, " +
                   $"Latitud Epicentro: {latEpi}, Longitud Epicentro: {lonEpi}, " +
                   $"Latitud Hipocentro: {latHipo}, Longitud Hipocentro: {lonHipo}, " +
                   $"Magnitud: {magnitud}";
        }

        // ================== Infra de cambios de estado ==================
   
        public void SetEstado(Estado e) => EstadoActual = nuevo;

        // ================== Detalle del evento y series ==================
        public string GetDetalleEventoSismico()
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("===== DETALLE DEL EVENTO SÍSMICO =====");
            sb.AppendLine($"Fecha de inicio: {FechaHoraInicio:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Epicentro: Lat {LatitudEpicentro} / Lon {LongitudEpicentro}");
            sb.AppendLine($"Hipocentro: Lat {LatitudHipocentro} / Lon {LongitudHipocentro}");
            sb.AppendLine($"Magnitud: {ValorMagnitud}");
            sb.AppendLine($"Alcance: {Alcance?.GetNombreAlcance() ?? "(sin datos)"}");
            sb.AppendLine($"Clasificación: {Clasificacion?.GetNombreClasificacion() ?? "(sin datos)"}");
            sb.AppendLine($"Origen: {Origen?.GetNombreOrigen() ?? "(sin datos)"}");

            sb.AppendLine(ObtenerDatosSeriesTemporales()); // dispara el recorrido completo
            return sb.ToString();
        }

        public string ObtenerDatosSeriesTemporales()
        {
            Console.WriteLine("[Evento] → obtenerDatosSeriesTemporales()");
            var sb = new System.Text.StringBuilder();

            foreach (var serie in (SeriesTemporales ?? new List<SerieTemporal>()))
            {
                Console.WriteLine("[Evento → SerieTemporal] getSeries()");
                sb.Append(serie.GetSeries()); // el loop interno vive en SerieTemporal.GetSeries()
            }

            AgruparInformacionSeriesPorEstacion(); // ordenar al final del recorrido
            return sb.ToString();
        }

        public void AgruparInformacionSeriesPorEstacion()
        {
            Console.WriteLine("[Evento] → AgruparInformacionSeriesPorEstacion()");
            SeriesTemporales = SeriesTemporales
                .OrderBy(s => s.Sismografo?.GetNombreEstacion()?.ToLowerInvariant())
                .ToList();
        }

        // ================== API explícita (delegación al Estado) ==================
        // IMPORTANTE: ya NO cerramos CE aquí; las clases Estado se ocupan de:
        // - buscarCambioEstadoAbierto()
        // - setFechaHoraFin()
        // - crearEstado()
        // - crearCambioEstado()
        // - AgregarCambioEstado()
        // - setEstado()

        /// <summary>
        /// Autodetectado → Bloqueado (llama a Estado.registrarEstadoBloqueado(ctx, cambios, fecha, responsable))
        /// </summary>
        public void RegistrarEstadoBloqueado(DateTime fechaHoraActual, Empleado responsable)
            => (EstadoActual as Estados.Autodetectado)
               ?.registrarEstadoBloqueado(this, CambiosDeEstado, fechaHoraActual, responsable);

        /// <summary>
        /// Bloqueado → Rechazado (llama a Estado.rechazar(cambios, es, fecha, responsable))
        /// </summary>
        public void Rechazar(DateTime fechaHoraActual, Empleado responsable)
            => (EstadoActual as Estados.Bloqueado)
               ?.rechazar(CambiosDeEstado, this, fechaHoraActual, responsable);

        // Si más adelante sumamos Confirmar/Derivar/Cerrar con firmas de la consigna,
        // se exponen aquí siguiendo el mismo esquema (delegando en la clase Estado).
    }
}
