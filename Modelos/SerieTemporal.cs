using System;
using System.Collections.Generic;

namespace PPAI_Revisiones.Modelos
{
    public class SerieTemporal
    {
        public bool CondicionAlarma { get; set; }
        public DateTime FechaHoraInicioRegistroMuestras { get; set; }
        public DateTime FechaHoraFinRegistroMuestras { get; set; }
        public double FrecuenciaMuestreo { get; set; }
        public List<MuestraSismica> Muestras { get; set; } = new();
        public Sismografo Sismografo { get; set; }

        public string GetSeries()
        {
            var bloqueMuestras = new System.Text.StringBuilder();
            int numMuestra = 1;

            foreach (var muestra in Muestras ?? new List<MuestraSismica>())
            {
                Console.WriteLine("[SerieTemporal → Muestra] getDatos()");
                bloqueMuestras.AppendLine($"\n  • Muestra #{numMuestra++}");
                bloqueMuestras.Append(muestra.GetDatos()); // ← segundo loop vive adentro
            }

            Console.WriteLine("[SerieTemporal → Sismografo] getNombreEstacion()");
            var sismografo = Sismografo;

            Console.WriteLine("[Sismografo → EstacionSismologica] getNombreEstacion()");
            string nombreEstacion = sismografo?.GetNombreEstacion() ?? "(sin estación)";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"\nEstación: {nombreEstacion}");
            sb.Append(bloqueMuestras.ToString());
            return sb.ToString();
        }

        public static List<SerieTemporal> GenerarSeriesParaEvento(int eventoIndex)
        {
            var tipoVelocidad = new TipoDeDato { Denominacion = "Velocidad", NombreUnidadMedida = "m/s", ValorUmbral = 10 };
            var tipoFrecuencia = new TipoDeDato { Denominacion = "Frecuencia", NombreUnidadMedida = "Hz", ValorUmbral = 5 };
            var tipoLongitud = new TipoDeDato { Denominacion = "Longitud", NombreUnidadMedida = "m", ValorUmbral = 2 };

            var estacion1 = new EstacionSismologica { Nombre = $"Estación Sur #{eventoIndex + 1}" };
            var estacion2 = new EstacionSismologica { Nombre = $"Estación Norte #{eventoIndex + 1}" };

            return new List<SerieTemporal>
    {
        new SerieTemporal
        {
            Sismografo = new Sismografo { Estacion = estacion1 },
            FechaHoraInicioRegistroMuestras = DateTime.Now.AddMinutes(-10 - eventoIndex),
            FechaHoraFinRegistroMuestras = DateTime.Now.AddMinutes(-5 - eventoIndex),
            FrecuenciaMuestreo = 2.0,
            CondicionAlarma = false,
            Muestras = new List<MuestraSismica>
            {
                new MuestraSismica
                {
                    Detalles = new List<DetalleMuestra>
                    {
                        new DetalleMuestra { TipoDeDato = tipoVelocidad, Valor = 7 + eventoIndex * 0.5 },
                        new DetalleMuestra { TipoDeDato = tipoFrecuencia, Valor = 3 + eventoIndex * 0.3 },
                        new DetalleMuestra { TipoDeDato = tipoLongitud, Valor = 2 + eventoIndex * 0.2 }
                    }
                },
                new MuestraSismica
                {
                    Detalles = new List<DetalleMuestra>
                    {
                        new DetalleMuestra { TipoDeDato = tipoVelocidad, Valor = 6.8 + eventoIndex * 0.5 },
                        new DetalleMuestra { TipoDeDato = tipoFrecuencia, Valor = 2.9 + eventoIndex * 0.3 },
                        new DetalleMuestra { TipoDeDato = tipoLongitud, Valor = 1.9 + eventoIndex * 0.2 }
                    }
                }
            }
        },
        new SerieTemporal
        {
            Sismografo = new Sismografo { Estacion = estacion2 },
            FechaHoraInicioRegistroMuestras = DateTime.Now.AddMinutes(-8 - eventoIndex),
            FechaHoraFinRegistroMuestras = DateTime.Now.AddMinutes(-3 - eventoIndex),
            FrecuenciaMuestreo = 2.0,
            CondicionAlarma = false,
            Muestras = new List<MuestraSismica>
            {
                new MuestraSismica
                {
                    Detalles = new List<DetalleMuestra>
                    {
                        new DetalleMuestra { TipoDeDato = tipoVelocidad, Valor = 6 + eventoIndex * 0.4 },
                        new DetalleMuestra { TipoDeDato = tipoFrecuencia, Valor = 4 + eventoIndex * 0.25 },
                        new DetalleMuestra { TipoDeDato = tipoLongitud, Valor = 1.5 + eventoIndex * 0.15 }
                    }
                },
                new MuestraSismica
                {
                    Detalles = new List<DetalleMuestra>
                    {
                        new DetalleMuestra { TipoDeDato = tipoVelocidad, Valor = 5.9 + eventoIndex * 0.4 },
                        new DetalleMuestra { TipoDeDato = tipoFrecuencia, Valor = 3.9 + eventoIndex * 0.25 },
                        new DetalleMuestra { TipoDeDato = tipoLongitud, Valor = 1.4 + eventoIndex * 0.15 }
                    }
                }
            }
        }
    };
        }
    }
}
