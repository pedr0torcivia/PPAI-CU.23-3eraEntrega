using System;
using System.Collections.Generic;
using PPAI_Revisiones.Modelos;
using PPAI_Revisiones.Modelos.Estados;

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
                new Autodetectado(),
                new EventoSinRevision(),
                new Bloqueado(),
                new Rechazado()
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
                    Usuario = new Usuario { NombreUsuario = "juan", Contraseña = "1234" }
                },
                new Empleado
                {
                    Nombre = "Ana",
                    Apellido = "López",
                    Mail = "ana@utn.com",
                    Telefono = "789012",
                    Usuario = new Usuario { NombreUsuario = "ana", Contraseña = "5678" }
                }
            };
        }

        private static void InicializarSesion()
        {
            SesionActual = new Sesion(Empleados[0].Usuario) { FechaHoraInicio = DateTime.Now };
        }

        private static void InicializarEventos()
        {
            var alcance = new AlcanceSismo { Nombre = "Provincial", Descripcion = "Impacto en una provincia" };
            var clasif = new ClasificacionSismo { Nombre = "Superficial", KmProfundidadDesde = 0, KmProfundidadHasta = 70 };
            var origen = new OrigenDeGeneracion { Nombre = "Tectónico", Descripcion = "Por desplazamiento de placas" };

            // E1: Autodetectado (abierto)
            var f1 = DateTime.Now.AddMinutes(-10);
            var e1 = new EventoSismico
            {
                FechaHoraInicio = f1,
                FechaHoraDeteccion = f1.AddMinutes(1),
                LatitudEpicentro = -31.42,
                LongitudEpicentro = -64.18,
                LatitudHipocentro = -22.42,
                LongitudHipocentro = -60.18,
                ValorMagnitud = 4.3,
                Alcance = alcance,
                Clasificacion = clasif,
                Origen = origen,
                Responsable = Empleados[0]
            };
            e1.SetEstado(new Autodetectado());
            e1.CambiosDeEstado.Add(CambioDeEstado.Crear(f1, new Autodetectado(), Empleados[0]));

            // E2: Bloqueado (abierto)
            var f2 = DateTime.Now.AddMinutes(-7);
            var e2 = new EventoSismico
            {
                FechaHoraInicio = f2,
                FechaHoraDeteccion = f2.AddMinutes(1),
                LatitudEpicentro = -31.5,
                LongitudEpicentro = -64.3,
                LatitudHipocentro = -22.5,
                LongitudHipocentro = -60.3,
                ValorMagnitud = 4.8,
                Alcance = alcance,
                Clasificacion = clasif,
                Origen = origen,
                Responsable = Empleados[1]
            };
            e2.SetEstado(new Bloqueado());
            // historial: Autodetectado (cerrado) -> Bloqueado (abierto)
            var cePrev = CambioDeEstado.Crear(f2.AddMinutes(-2), new Autodetectado(), Empleados[1]);
            cePrev.SetFechaHoraFin(f2);
            e2.CambiosDeEstado.Add(cePrev);
            e2.CambiosDeEstado.Add(CambioDeEstado.Crear(f2, new Bloqueado(), Empleados[1]));

            // E3: Rechazado (cerrado)
            var f3 = DateTime.Now.AddMinutes(-4);
            var e3 = new EventoSismico
            {
                FechaHoraInicio = f3,
                FechaHoraDeteccion = f3.AddMinutes(1),
                LatitudEpicentro = -31.6,
                LongitudEpicentro = -64.4,
                LatitudHipocentro = -22.6,
                LongitudHipocentro = -60.4,
                ValorMagnitud = 5.0,
                Alcance = alcance,
                Clasificacion = clasif,
                Origen = origen,
                Responsable = Empleados[0]
            };
            e3.SetEstado(new Rechazado());
            var ceBloq = CambioDeEstado.Crear(f3.AddMinutes(-2), new Bloqueado(), Empleados[0]);
            ceBloq.SetFechaHoraFin(f3.AddMinutes(-1));
            e3.CambiosDeEstado.Add(ceBloq);
            e3.CambiosDeEstado.Add(CambioDeEstado.Crear(f3.AddMinutes(-1), new Rechazado(), Empleados[0]));

            Eventos = new List<EventoSismico> { e1, e2, e3 };
        }
    }
}
