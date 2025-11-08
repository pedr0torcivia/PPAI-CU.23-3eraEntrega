// Infra/Data/EFModels/SerieTemporalEF.cs (Corregido)
using PPAI_Revisiones.Modelos;
using System;
using System.Collections.Generic;

namespace PPAI_2.Infra.Data.EFModels
{
    public class SerieTemporalEF
    {
        // === Clave primaria técnica ===
        public Guid Id { get; set; }

        // === Atributos persistidos ===
        public bool CondicionAlarma { get; set; }
        public DateTime FechaHoraInicioRegistroMuestras { get; set; }
        public DateTime FechaHoraRegistro { get; set; }
        public double FrecuenciaMuestreo { get; set; }

        // === Relaciones (solo en EF) ===

        // --- RELACIÓN CON ESTACIÓN ---
        public Guid EstacionId { get; set; }
        public EstacionSismologicaEF Estacion { get; set; } = null!;

        // --- RELACIÓN CON SISMÓGRAFO (Propiedad faltante) ---
        // Asumo que esta FK puede ser nula (Guid?) ya que la lógica de importación la maneja.
        public Guid? SismografoId { get; set; }
        public SismografoEF? Sismografo { get; set; } = null!; // Navegación opcional

        // --- RELACIÓN CON EVENTO ---
        public Guid EventoSismicoId { get; set; }
        public EventoSismicoEF Evento { get; set; } = null!;

        public List<MuestraSismicaEF> Muestras { get; set; } = new();
    }
}