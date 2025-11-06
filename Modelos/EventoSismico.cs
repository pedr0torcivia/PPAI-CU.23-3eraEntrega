using System;
using System.Collections.Generic;
using System.Linq;
using PPAI_Revisiones.Modelos.Estados; // necesario para Autodetectado, Bloqueado, etc.

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

        // ================== Relaciones ==================
        public Estado EstadoActual { get; private set; }
        public AlcanceSismo Alcance { get; set; }
        public ClasificacionSismo Clasificacion { get; set; }
        public OrigenDeGeneracion Origen { get; set; }
        public Empleado Responsable { get; set; }

        // Series y cambios de estado
        public List<SerieTemporal> SeriesTemporales { get; private set; } = new();
        public List<CambioDeEstado> CambiosDeEstado { get; } = new();

        // ================== Consultas simples ==================

        public string GetFechaHora() => FechaHoraInicio.ToString("g");
        public double GetLatitudEpicentro() => LatitudEpicentro;
        public double GetLongitudEpicentro() => LongitudEpicentro;
        public double GetLatitudHipocentro() => LatitudHipocentro;
        public double GetLongitudHipocentro() => LongitudHipocentro;
        public double GetMagnitud() => ValorMagnitud;

        public bool sosAutodetectado() => EstadoActual?.EsAutodetectado == true;
        public bool sosEventoSinRevision() => EstadoActual?.EsEventoSinRevision == true;

        public string GetDatosOcurrencia() =>
            $"Inicio: {GetFechaHora()}, " +
            $"Latitud Epicentro: {LatitudEpicentro}, Longitud Epicentro: {LongitudEpicentro}, " +
            $"Latitud Hipocentro: {LatitudHipocentro}, Longitud Hipocentro: {LongitudHipocentro}, " +
            $"Magnitud: {ValorMagnitud}";

        // ================== Infra de cambios de estado ==================
        public void AgregarCambioEstado(CambioDeEstado ce) => CambiosDeEstado.Add(ce);

        public void SetEstado(Estado estado)
        {
            // ← acá estaba el error (se usaba un identificador distinto)
            EstadoActual = estado;
        }

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
            sb.AppendLine(ObtenerDatosSeriesTemporales());
            return sb.ToString();
        }

        public string ObtenerDatosSeriesTemporales()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var serie in (SeriesTemporales ?? new List<SerieTemporal>()))
                sb.Append(serie.GetSeries());
            AgruparInformacionSeriesPorEstacion();
            return sb.ToString();
        }

        public void AgruparInformacionSeriesPorEstacion()
            => SeriesTemporales = SeriesTemporales
                .OrderBy(s => s.Sismografo?.GetNombreEstacion()?.ToLowerInvariant())
                .ToList();

        // ================== Delegación a los ESTADOS (patrón State) ==================
        // Firmas alineadas a tu consigna: los Estados hacen todo (cerrar CE, crear nuevo estado y CE).
        public void RegistrarEstadoBloqueado(DateTime fechaHoraActual, Empleado responsable)
            => (EstadoActual as Autodetectado)
               ?.registrarEstadoBloqueado(this, CambiosDeEstado, fechaHoraActual, responsable);

        public void Rechazar(DateTime fechaHoraActual, Empleado responsable)
            => (EstadoActual as Bloqueado)
               ?.rechazar(CambiosDeEstado, this, fechaHoraActual, responsable);
    }
}
