using PPAI_Revisiones.Modelos;
using System;
using System.Collections.Generic;

namespace PPAI_Revisiones
{
    public static class DatosMock
    {
        public static List<EventoSismico> Eventos { get; private set; }
        public static List<Estado> Estados { get; private set; }
        public static Sesion SesionActual { get; private set; }
        public static List<Empleado> Empleados { get; private set; }

        static DatosMock()
        {
            InicializarEstados();
            InicializarEmpleados();
            InicializarSesion();
            InicializarEventos();
        }

        private static void InicializarEstados()
        {
            Estados = new List<Estado>
            {
                new Estado { Nombre = "Autodetectado", Ambito = "Evento" },
                new Estado { Nombre = "No Revisado", Ambito = "Evento" },
                new Estado { Nombre = "Bloqueado", Ambito = "Evento" },
                new Estado { Nombre = "Rechazado", Ambito = "Evento" }
            };
        }

        private static void InicializarEmpleados()
        {
            Empleados = new List<Empleado>
            {
                new Empleado
                {
                    Nombre = "Juan",
                    Apellido = "Pérez",
                    Mail = "juan@utn.com",
                    Telefono = "123456",
                    Usuario = new Usuario
                    {
                        NombreUsuario = "juan",
                        Contraseña = "1234"
                    }
                }
            };
        }

        private static void InicializarSesion()
        {
            SesionActual = new Sesion(Empleados[0].Usuario)
            {
                FechaHoraInicio = DateTime.Now
            };
        }

        private static void InicializarEventos()
        {
            var alcance = new AlcanceSismo { Nombre = "Provincial", Descripcion = "Impacto en una provincia" };
            var clasificacion = new ClasificacionSismo { Nombre = "Superficial", KmProfundidadDesde = 0, KmProfundidadHasta = 70 };
            var origen = new OrigenDeGeneracion { Nombre = "Tectónico", Descripcion = "Por desplazamiento de placas" };

            var estadoAuto = Estados.Find(e => e.Nombre == "Autodetectado");
            var estadoNoRev = Estados.Find(e => e.Nombre == "No Revisado");

            Eventos = new List<EventoSismico>();

            for (int i = 0; i < 3; i++)
            {
                var fechaBase = DateTime.Now.AddMinutes(-5 * (i + 1));
                var evento = new EventoSismico
                {
                    FechaHoraInicio = fechaBase,
                    FechaHoraDeteccion = fechaBase.AddMinutes(1),
                    LatitudEpicentro = Math.Round(-31.4245 + i * 0.1, 4),
                    LongitudEpicentro = Math.Round(-64.1826 + i * 0.1, 4),
                    LatitudHipocentro = Math.Round(-22.4244 + i * 0.1, 4),
                    LongitudHipocentro = Math.Round(-60.1851 + i * 0.1, 4),
                    Alcance = alcance,
                    Clasificacion = clasificacion,
                    Origen = origen,
                    ValorMagnitud = 4.5 + i * 0.2,
                    EstadoActual = estadoNoRev,
                    CambiosDeEstado = new List<CambioDeEstado>
                    {
                        new CambioDeEstado { EstadoActual = estadoAuto, FechaHoraFin = fechaBase.AddMinutes(2), Responsable = "Sistema" },
                        new CambioDeEstado { EstadoActual = estadoNoRev, FechaHoraInicio = fechaBase.AddMinutes(2), FechaHoraFin = null, Responsable = "Sistema" }
                    },
                    SeriesTemporales = SerieTemporal.GenerarSeriesParaEvento(i)
                };

                Eventos.Add(evento);
            }

            // Evento adicional: solo Autodetectado como estado actual
            var fechaExtra = DateTime.Now.AddMinutes(-2);
            var eventoAutodetectado = new EventoSismico
            {
                FechaHoraInicio = fechaExtra,
                FechaHoraDeteccion = fechaExtra.AddMinutes(1),
                LatitudEpicentro = -31.5,
                LongitudEpicentro = -64.3,
                LatitudHipocentro = -22.5,
                LongitudHipocentro = -60.3,
                Alcance = alcance,
                Clasificacion = clasificacion,
                Origen = origen,
                ValorMagnitud = 4.1,
                EstadoActual = estadoAuto,
                CambiosDeEstado = new List<CambioDeEstado>
                {
                    new CambioDeEstado
                    {
                        EstadoActual = estadoAuto,
                        FechaHoraInicio = fechaExtra,
                        FechaHoraFin = null,
                        Responsable = "Sistema"
                    }
                },
                SeriesTemporales = SerieTemporal.GenerarSeriesParaEvento(3)
            };

            Eventos.Add(eventoAutodetectado);
        }
    }
}
