// Modelos/EventoSismico.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using PPAI_Revisiones.Modelos.Estados;

namespace PPAI_Revisiones.Modelos
{
    public class EventoSismico
    {
        // === Atributos de dominio (según tu lista) ===
        public DateTime FechaHoraOcurrencia { get; private set; }
        public DateTime? FechaHoraFin { get; private set; }

        public double LatitudEpicentro { get; private set; }
        public double LongitudEpicentro { get; private set; }
        public double LatitudHipocentro { get; private set; }
        public double LongitudHipocentro { get; private set; }
        public double ValorMagnitud { get; private set; }

        public AlcanceSismo Alcance { get; private set; }                 // (1.)
        public OrigenDeGeneracion OrigenDeGeneracion { get; private set; } // (1.)
        public ClasificacionSismo Clasificacion { get; private set; }      // (1.)
        public Estado EstadoActual { get; private set; }                   // (1.)

        public List<CambioDeEstado> CambiosDeEstado { get; } = new();      // (1..*)
        public List<SerieTemporal> SeriesTemporales { get; private set; } = new();

        // === Consultas simples ===
        public string GetDatosOcurrencia() =>
            $"Inicio: {FechaHoraOcurrencia:g}, " +
            $"Latitud Epicentro: {LatitudEpicentro}, Longitud Epicentro: {LongitudEpicentro}, " +
            $"Latitud Hipocentro: {LatitudHipocentro}, Longitud Hipocentro: {LongitudHipocentro}, " +
            $"Magnitud: {ValorMagnitud}";

        public double GetLatitudEpicentro() => LatitudEpicentro;
        public double GetLongitudEpicentro() => LongitudEpicentro;
        public double GetLatitudHipocentro() => LatitudHipocentro;
        public double GetLongitudHipocentro() => LongitudHipocentro;
        public double GetMagnitud() => ValorMagnitud;

        // === Comportamiento ===
        public void EstablecerFin(DateTime fin)
        {
            if (fin < FechaHoraOcurrencia)
                throw new ArgumentException("La fecha de fin no puede ser anterior a la de ocurrencia.");
            FechaHoraFin = fin;
        }

        public void SetEstado(Estado estado)
        {
            EstadoActual = estado ?? throw new ArgumentNullException(nameof(estado));
        }

        public void AgregarCambioEstado(CambioDeEstado cambio)
        {
            if (cambio == null) throw new ArgumentNullException(nameof(cambio));
            CambiosDeEstado.Add(cambio);
        }

        // Delegación a estados concretos (State)
        public void RegistrarEstadoBloqueado(DateTime ahora, Empleado responsable)
            => (EstadoActual as Autodetectado)
               ?.registrarEstadoBloqueado(this, CambiosDeEstado, ahora, responsable);

        public void Rechazar(DateTime ahora, Empleado responsable)
            => (EstadoActual as Bloqueado)
               ?.rechazar(CambiosDeEstado, this, ahora, responsable);

        // === Detalle y series ===
        public string GetDetalleEventoSismico()
        {
            var sb = new StringBuilder();
            sb.AppendLine("===== DETALLE DEL EVENTO SÍSMICO =====");
            sb.AppendLine($"Fecha de inicio: {FechaHoraOcurrencia:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Epicentro: Lat {LatitudEpicentro} / Lon {LongitudEpicentro}");
            sb.AppendLine($"Hipocentro: Lat {LatitudHipocentro} / Lon {LongitudHipocentro}");
            sb.AppendLine($"Magnitud: {ValorMagnitud}");
            sb.AppendLine($"Alcance: {Alcance?.GetNombreAlcance() ?? "(sin datos)"}");
            sb.AppendLine($"Clasificación: {Clasificacion?.GetNombreClasificacion() ?? "(sin datos)"}");
            sb.AppendLine($"Origen: {OrigenDeGeneracion?.GetNombreOrigen() ?? "(sin datos)"}");
            sb.AppendLine(ObtenerDatosSeriesTemporales());

            Debug.WriteLine($"[Evento] detalle armado (len={sb.Length})");
            return sb.ToString();
        }

        public string ObtenerDatosSeriesTemporales()
        {
            var sb = new StringBuilder();
            var series = SeriesTemporales ?? new List<SerieTemporal>();
            sb.AppendLine($"[TRACE] Series asociadas al evento: {series.Count}");

            foreach (var serie in series)
                sb.Append(serie.GetSeries());

            return sb.ToString();
        }

        // === Constructores ===
        public EventoSismico(
            DateTime fechaHoraOcurrencia,
            double latEpicentro,
            double lonEpicentro,
            double latHipocentro,
            double lonHipocentro,
            double valorMagnitud,
            AlcanceSismo alcance,
            OrigenDeGeneracion origen,
            ClasificacionSismo clasificacion,
            Estado estadoInicial,
            List<SerieTemporal> series = null)
        {
            FechaHoraOcurrencia = fechaHoraOcurrencia;
            LatitudEpicentro = latEpicentro;
            LongitudEpicentro = lonEpicentro;
            LatitudHipocentro = latHipocentro;
            LongitudHipocentro = lonHipocentro;
            ValorMagnitud = valorMagnitud;

            Alcance = alcance ?? throw new ArgumentNullException(nameof(alcance));
            OrigenDeGeneracion = origen ?? throw new ArgumentNullException(nameof(origen));
            Clasificacion = clasificacion ?? throw new ArgumentNullException(nameof(clasificacion));
            EstadoActual = estadoInicial ?? throw new ArgumentNullException(nameof(estadoInicial));

            SeriesTemporales = series ?? new List<SerieTemporal>();
        }

        // === MÉTODOS DE DOMINIO REQUERIDOS POR EL MANEJADOR ===
        public void MaterializarEstadoDesdeNombre() { /* Lógica para EstadoActual = Estado.FromName(EstadoActualNombre) */ }
        public void MaterializarEstadosDeCambios() { /* Lógica para materializar estados de los CE */ }
        public bool sosAutodetectado() => EstadoActual.Nombre == "Autodetectado";
        public bool sosEventoSinRevision() => EstadoActual.Nombre != "Confirmado" && EstadoActual.Nombre != "Rechazado";



        protected EventoSismico() { }
    }
}
