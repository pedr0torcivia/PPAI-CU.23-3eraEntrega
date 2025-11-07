using PPAI_Revisiones.Modelos.Estados; // Autodetectado, Bloqueado, etc.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;       // <— agregado
using System.Linq;
using System.Diagnostics;
using System.Text;

namespace PPAI_Revisiones.Modelos
{
    public class EventoSismico
    {
        // =============== Datos básicos ===============
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraDeteccion { get; set; }
        public double LatitudEpicentro { get; set; }
        public double LongitudEpicentro { get; set; }
        public double LatitudHipocentro { get; set; }
        public double LongitudHipocentro { get; set; }
        public double ValorMagnitud { get; set; }

        // =============== Estado (patrón State) ===============
        // Objeto en memoria (NO se mapea)
        [NotMapped]                                         // <— agregado
        public Estado EstadoActual { get; private set; }

        // Valor persistente en BD (sí se mapea)
        public string EstadoActualNombre { get; set; } = "Autodetectado"; // <— default seguro

        // =============== Relaciones de catálogo/usuario ===============
        public Guid AlcanceId { get; set; }
        public Guid ClasificacionId { get; set; }
        public Guid OrigenId { get; set; }
        public Guid ResponsableId { get; set; }

        public AlcanceSismo Alcance { get; set; }
        public ClasificacionSismo Clasificacion { get; set; }
        public OrigenDeGeneracion Origen { get; set; }
        public Empleado Responsable { get; set; }

        // =============== Series y cambios de estado ===============
        public List<SerieTemporal> SeriesTemporales { get; private set; } = new();
        public List<CambioDeEstado> CambiosDeEstado { get; } = new();

        // =============== Consultas simples ===============
        public string GetFechaHora() => FechaHoraInicio.ToString("g");
        public double GetLatitudEpicentro() => LatitudEpicentro;
        public double GetLongitudEpicentro() => LongitudEpicentro;
        public double GetLatitudHipocentro() => LatitudHipocentro;
        public double GetLongitudHipocentro() => LongitudHipocentro;
        public double GetMagnitud() => ValorMagnitud;

        // Funciona aunque EstadoActual aún no esté reconstruido en memoria
        public bool sosAutodetectado() =>
            (EstadoActual?.EsAutodetectado == true) || EstadoActualNombre == "Autodetectado";

        public bool sosEventoSinRevision() =>
            (EstadoActual?.EsEventoSinRevision == true) || EstadoActualNombre == "Evento sin revisión";

        public string GetDatosOcurrencia() =>
            $"Inicio: {GetFechaHora()}, " +
            $"Latitud Epicentro: {LatitudEpicentro}, Longitud Epicentro: {LongitudEpicentro}, " +
            $"Latitud Hipocentro: {LatitudHipocentro}, Longitud Hipocentro: {LongitudHipocentro}, " +
            $"Magnitud: {ValorMagnitud}";

        // =============== Infra de cambios de estado ===============
        public void AgregarCambioEstado(CambioDeEstado ce)
{
    CambiosDeEstado.Add(ce);

    // 🔴 Si el contexto está disponible, EF lo detectará automáticamente al agregarlo.
    // Si no, se persistirá explícitamente en el manejador (ver siguiente paso).
}

        public void SetEstado(Estado estado)
        {
            EstadoActual = estado;
            EstadoActualNombre = estado?.Nombre; // <- CLAVE: persistimos el nombre
        }

        // --- NUEVO: reconstruir Estado desde el nombre persistido (para leer de BD) ---
        public void MaterializarEstadoDesdeNombre()
            => EstadoActual = Estado.FromName(EstadoActualNombre);

        public void MaterializarEstadosDeCambios()
        {
            if (CambiosDeEstado == null) return;
            foreach (var c in CambiosDeEstado)
                c.EstadoActual = Estado.FromName(c.EstadoNombre);
        }

        // =============== Detalle del evento y series ===============
        public string GetDetalleEventoSismico()
        {
            var sb = new StringBuilder();
            sb.AppendLine("===== DETALLE DEL EVENTO SÍSMICO =====");
            sb.AppendLine($"Fecha de inicio: {FechaHoraInicio:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Epicentro: Lat {LatitudEpicentro} / Lon {LongitudEpicentro}");
            sb.AppendLine($"Hipocentro: Lat {LatitudHipocentro} / Lon {LongitudHipocentro}");
            sb.AppendLine($"Magnitud: {ValorMagnitud}");
            sb.AppendLine($"Alcance: {Alcance?.GetNombreAlcance() ?? "(sin datos)"}");
            sb.AppendLine($"Clasificación: {Clasificacion?.GetNombreClasificacion() ?? "(sin datos)"}");
            sb.AppendLine($"Origen: {Origen?.GetNombreOrigen() ?? "(sin datos)"}");

            // === Mantiene tu cadena de llamadas ===
            sb.AppendLine(ObtenerDatosSeriesTemporales());

            Debug.WriteLine($"[Evento] {Id} detalle armado (len={sb.Length})");
            return sb.ToString();
        }

        public string ObtenerDatosSeriesTemporales()
        {
            var sb = new StringBuilder();

            var series = SeriesTemporales ?? new List<SerieTemporal>();
            Debug.WriteLine($"[Evento] {Id} series asociadas: {series.Count}");

            // 1) LOOP principal: Evento -> SerieTemporal.GetSeries()
            foreach (var serie in series)
            {
                sb.Append(serie.GetSeries());
            }

            // 2) Al finalizar el bucle, ordenar por estación (como pediste)
            AgruparInformacionSeriesPorEstacion();

            return sb.ToString();
        }

        public void AgruparInformacionSeriesPorEstacion()
            => SeriesTemporales = (SeriesTemporales ?? new List<SerieTemporal>())
                .OrderBy(s => s.Sismografo?.GetNombreEstacion()?.ToLowerInvariant())
                .ToList();


        // =============== Delegación a los ESTADOS (State) ===============
        public void RegistrarEstadoBloqueado(DateTime fechaHoraActual, Empleado responsable)
            => (EstadoActual as Autodetectado)
               ?.registrarEstadoBloqueado(this, CambiosDeEstado, fechaHoraActual, responsable);

        public void Rechazar(DateTime fechaHoraActual, Empleado responsable)
            => (EstadoActual as Bloqueado)
               ?.rechazar(CambiosDeEstado, this, fechaHoraActual, responsable);
    }


}
