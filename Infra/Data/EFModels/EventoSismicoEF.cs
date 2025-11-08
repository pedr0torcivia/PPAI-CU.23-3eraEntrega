// Infra/Data/EFModels/EventoSismicoEF.cs
using PPAI_Revisiones.Dominio;
using PPAI_Revisiones.Modelos;
using System;
using System.Collections.Generic;

namespace PPAI_2.Infra.Data.EFModels
{
    public class EventoSismicoEF
    {
        public Guid Id { get; set; }

        // Schema: FechaHoraInicio / FechaHoraDeteccion
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraDeteccion { get; set; }

        public double LatitudEpicentro { get; set; }
        public double LongitudEpicentro { get; set; }
        public double LatitudHipocentro { get; set; }
        public double LongitudHipocentro { get; set; }
        public double ValorMagnitud { get; set; }

        public string EstadoActualNombre { get; set; }

        public Guid AlcanceId { get; set; }
        public Guid ClasificacionId { get; set; }
        public Guid OrigenId { get; set; }
        public Guid ResponsableId { get; set; }

        public AlcanceSismoEF Alcance { get; set; }
        public ClasificacionSismoEF Clasificacion { get; set; }
        public OrigenDeGeneracionEF Origen { get; set; }
        public EmpleadoEF Responsable { get; set; }

        public List<CambioDeEstadoEF> CambiosDeEstado { get; set; } = new();
        public List<SerieTemporalEF> SeriesTemporales { get; set; } = new();
    }
}
