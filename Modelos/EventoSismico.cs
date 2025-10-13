using System;
using System.Collections.Generic;
using System.Linq;

namespace PPAI_Revisiones.Modelos
{
    public class EventoSismico
    {
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraDeteccion { get; set; }
        public double LatitudEpicentro { get; set; }
        public double LongitudEpicentro { get; set; }

        public double LatitudHipocentro { get; set; }
        public double LongitudHipocentro { get; set; }

        public Estado EstadoActual { get; set; }
        public AlcanceSismo Alcance { get; set; }
        public ClasificacionSismo Clasificacion { get; set; }
        public OrigenDeGeneracion Origen { get; set; }
        public double ValorMagnitud { get; set; } 

        public List<SerieTemporal> SeriesTemporales { get; set; } = new();
        public List<CambioDeEstado> CambiosDeEstado { get; set; } = new();

        public Empleado Responsable { get; set; }

        public Estado getEstado() => EstadoActual;

        public string GetFechaHora() => FechaHoraInicio.ToString();
        public double GetLatitudEpicentro() => LatitudEpicentro;
        public double GetLongitudEpicentro() => LongitudEpicentro;
        public double GetLatitudHipocentro() => LatitudHipocentro;
        public double GetLongitudHipocentro() => LongitudHipocentro;

        public double GetMagnitud() => ValorMagnitud;

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
        public void RegistrarEstadoBloqueado(Estado nuevoEstado, string nombreResponsable, DateTime fechaHoraActual)
        {
            var cambioAbierto = BuscarCambioEstadoAbierto();
            cambioAbierto?.SetFechaHoraFin(fechaHoraActual);

            SetEstado(nuevoEstado);
            CrearCambioEstado(nuevoEstado, nombreResponsable, fechaHoraActual);
        }

        public CambioDeEstado BuscarCambioEstadoAbierto()
        {
            // Solo invoca a la función que realiza la búsqueda del cambio de estado actual
            return EsEstadoActual();
        }

        private CambioDeEstado EsEstadoActual()
        {
            // Hace la búsqueda del cambio de estado actual (activo)
            return CambiosDeEstado.FirstOrDefault(c => c.FechaHoraFin == null);
        }

        public void SetEstado(Estado nuevoEstado) => EstadoActual = nuevoEstado;

        public void CrearCambioEstado(Estado nuevoEstado, string responsable, DateTime fecha)
        {
            var nuevoCambio = new CambioDeEstado
            {
                EstadoActual = nuevoEstado,
                Responsable = responsable,
                FechaHoraInicio = fecha,
                FechaHoraFin = null
            };
            CambiosDeEstado.Add(nuevoCambio);
        }

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

            // TRIGGER: Este método inicia todo el recorrido
            sb.AppendLine(ObtenerDatosSeriesTemporales());

            return sb.ToString();
        }


        public string ObtenerDatosSeriesTemporales()
        {
            Console.WriteLine("[Evento] → obtenerDatosSeriesTemporales()");
            var sb = new System.Text.StringBuilder();

            foreach (var serie in (SeriesTemporales ?? new List<SerieTemporal>()))
            {
                Console.WriteLine("[Evento → SerieTemporal] getSeries()");
                sb.Append(serie.GetSeries());   // ← el primer loop vive adentro

                AgruparInformacionSeriesPorEstacion(); // (no imprime)
            }

            return sb.ToString();
        }


        public void AgruparInformacionSeriesPorEstacion()
        {
            Console.WriteLine("[Evento] → AgruparInformacionSeriesPorEstacion()");
            SeriesTemporales = SeriesTemporales
                .OrderBy(s => s.Sismografo?.GetNombreEstacion()?.ToLowerInvariant())
                .ToList();
        }

        public void Rechazar(Estado estadoRechazado, string usuario, DateTime fecha)
        {
            var cambioBloqueado = CambiosDeEstado
             .LastOrDefault(c => c.EstadoActual != null && c.EstadoActual.EsBloqueado());

            cambioBloqueado?.SetFechaHoraFin(fecha);

            SetEstado(estadoRechazado);
            CrearCambioEstado(estadoRechazado, usuario ?? "(desconocido)", fecha);
        }
    }
}
