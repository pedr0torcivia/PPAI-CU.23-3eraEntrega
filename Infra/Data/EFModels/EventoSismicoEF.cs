// Infra/Data/EFModels/EventoSismicoEF.cs
using System;
using System.Collections.Generic;

namespace PPAI_2.Infra.Data.EFModels
{
    public class EventoSismicoEF
    {
        // === PK técnica ===
        public Guid Id { get; set; }

        // === Atributos persistidos ===
        public DateTime FechaHoraOcurrencia { get; set; }
        public DateTime? FechaHoraFin { get; set; }

        public double LatitudEpicentro { get; set; }
        public double LongitudEpicentro { get; set; }
        public double LatitudHipocentro { get; set; }
        public double LongitudHipocentro { get; set; }
        public double ValorMagnitud { get; set; }

        // Persistencia del estado (dominio materializa el objeto Estado)
        public string EstadoActualNombre { get; set; } = "Autodetectado";

        // === FKs “semánticas” ===
        public Guid AlcanceId { get; set; }
        public Guid OrigenId { get; set; }
        public Guid ClasificacionId { get; set; }

        // === Navegaciones ===
        public AlcanceSismoEF Alcance { get; set; } = null!;
        public OrigenDeGeneracionEF Origen { get; set; } = null!;
        public ClasificacionSismoEF Clasificacion { get; set; } = null!;

        public List<CambioDeEstadoEF> CambiosDeEstado { get; set; } = new();
        public List<SerieTemporalEF> SeriesTemporales { get; set; } = new();
    }
}
