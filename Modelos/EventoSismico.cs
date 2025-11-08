using PPAI_Revisiones.Dominio;
using PPAI_Revisiones.Modelos.Estados;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PPAI_Revisiones.Modelos
{
    public class EventoSismico
    {
        // ===== Dominio requerido =====
        public DateTime? FechaHoraFin { get; set; }             // dominio
        public DateTime FechaHoraOcurrencia { get; set; }      // dominio
        public double LatitudEpicentro { get; set; }
        public double LatitudHipocentro { get; set; }
        public double LongitudEpicentro { get; set; }
        public double LongitudHipocentro { get; set; }
        public double ValorMagnitud { get; set; }

        public ClasificacionSismo Clasificacion { get; set; }       // 1..1
        public OrigenDeGeneracion OrigenDeGeneracion { get; set; }  // 1..1
        public AlcanceSismo Alcance { get; set; }             // 1..1
        public Estado Estado { get; private set; }      // 1..1 (State)
        private readonly List<CambioDeEstado> _historial = new();     // 1..*
        public List<CambioDeEstado> CambioEstado => _historial;       // nombre pedido

        public List<SerieTemporal> SeriesTemporales { get; set; } = new();

        // ====== Shims de compatibilidad para NO tocar el CU ======
        // El CU usa Guid Id; lo mantenemos como identificador lógico del dominio.
        public Guid Id { get; set; } = Guid.NewGuid();

        // El CU llama a FechaHoraInicio → es la ocurrencia en tu dominio
        public DateTime FechaHoraInicio
        {
            get => FechaHoraOcurrencia;
            set => FechaHoraOcurrencia = value;
        }

        // El CU usa EstadoActual/EstadoActualNombre
        public Estado EstadoActual
        {
            get => Estado;
            private set => Estado = value;
        }
        public string EstadoActualNombre { get; set; } = "Autodetectado";

        // El CU usa .CambiosDeEstado
        public List<CambioDeEstado> CambiosDeEstado => _historial;

        // El CU usa .Origen (no OrigenDeGeneracion)
        public OrigenDeGeneracion Origen
        {
            get => OrigenDeGeneracion;
            set => OrigenDeGeneracion = value;
        }

        // ===== Consultas simples (no se cambian) =====
        public string GetFechaHora() => FechaHoraInicio.ToString("g");
        public double GetLatitudEpicentro() => LatitudEpicentro;
        public double GetLongitudEpicentro() => LongitudEpicentro;
        public double GetLatitudHipocentro() => LatitudHipocentro;
        public double GetLongitudHipocentro() => LongitudHipocentro;
        public double GetMagnitud() => ValorMagnitud;

        public bool sosAutodetectado() =>
            (EstadoActual?.EsAutodetectado == true) || EstadoActualNombre == "Autodetectado";

        public bool sosEventoSinRevision() =>
            (EstadoActual?.EsEventoSinRevision == true) || EstadoActualNombre == "Evento sin revisión";

        public string GetDatosOcurrencia() =>
            $"Inicio: {GetFechaHora()}, " +
            $"Latitud Epicentro: {LatitudEpicentro}, Longitud Epicentro: {LongitudEpicentro}, " +
            $"Latitud Hipocentro: {LatitudHipocentro}, Longitud Hipocentro: {LongitudHipocentro}, " +
            $"Magnitud: {ValorMagnitud}";

        // ===== Infra de cambios de estado (firmas intactas) =====
        public void AgregarCambioEstado(CambioDeEstado ce) => CambiosDeEstado.Add(ce);

        public void SetEstado(Estado estado)
        {
            EstadoActual = estado;
            EstadoActualNombre = estado?.Nombre;
        }

        public void MaterializarEstadoDesdeNombre()
            => EstadoActual = Estado.FromName(EstadoActualNombre);

        public void MaterializarEstadosDeCambios()
        {
            if (CambiosDeEstado == null) return;
            foreach (var c in CambiosDeEstado)
                c.EstadoActual = Estado.FromName(c.EstadoNombre);
        }

        // ===== Detalle / series (igual que tenías) =====
        public string GetDetalleEventoSismico()
        {
            var sb = new StringBuilder();
            sb.AppendLine("===== DETALLE DEL EVENTO SÍSMICO =====");
            sb.AppendLine($"Fecha de inicio : {FechaHoraInicio:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Epicentro       : Lat {LatitudEpicentro:0.####} / Lon {LongitudEpicentro:0.####}");
            sb.AppendLine($"Hipocentro      : Lat {LatitudHipocentro:0.####} / Lon {LongitudHipocentro:0.####}");
            sb.AppendLine($"Magnitud        : {ValorMagnitud:0.0}");
            sb.AppendLine($"Alcance         : {Alcance?.GetNombreAlcance() ?? "—"}");
            sb.AppendLine($"Clasificación   : {Clasificacion?.GetNombreClasificacion() ?? "—"}");
            sb.AppendLine($"Origen          : {Origen?.GetNombreOrigen() ?? "—"}");
            sb.AppendLine();
            sb.Append(ObtenerDatosSeriesTemporales());
            return sb.ToString();
        }

        public string ObtenerDatosSeriesTemporales()
        {
            var sb = new StringBuilder();
            var series = (SeriesTemporales ?? new List<SerieTemporal>())
                         .OrderBy(s => s.Sismografo?.GetNombreEstacion())
                         .ToList();

            if (series.Count == 0)
            {
                sb.AppendLine("No hay series temporales asociadas.");
                return sb.ToString();
            }

            foreach (var serie in series)
                sb.Append(serie.GetSeries());

            return sb.ToString();
        }

        public void AgruparInformacionSeriesPorEstacion()
            => SeriesTemporales = (SeriesTemporales ?? new List<SerieTemporal>())
                .OrderBy(s => s.Sismografo?.GetNombreEstacion()?.ToLowerInvariant())
                .ToList();

        // ===== Delegación a ESTADOS (firmas intactas) =====
        public void RegistrarEstadoBloqueado(DateTime fechaHoraActual, Empleado responsable)
            => (EstadoActual as Autodetectado)
               ?.registrarEstadoBloqueado(this, CambiosDeEstado, fechaHoraActual, responsable);

        public void Rechazar(DateTime fechaHoraActual, Empleado responsable)
            => (EstadoActual as Bloqueado)
               ?.rechazar(CambiosDeEstado, this, fechaHoraActual, responsable);
    }
}
