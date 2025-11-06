using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PPAI2.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alcances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: true),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alcances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clasificaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: true),
                    KmProfundidadDesde = table.Column<double>(type: "REAL", nullable: false),
                    KmProfundidadHasta = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clasificaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Estaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Id_Estacion = table.Column<int>(type: "INTEGER", nullable: false),
                    CodigoEstacion = table.Column<string>(type: "TEXT", nullable: true),
                    DocumentoCertificacionAdq = table.Column<string>(type: "TEXT", nullable: true),
                    FechaSolicitudCertificacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Latitud = table.Column<double>(type: "REAL", nullable: false),
                    Longitud = table.Column<double>(type: "REAL", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: true),
                    NroCertificacionAdquisicion = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrigenesDeGeneracion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: true),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrigenesDeGeneracion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposDeDato",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Denominacion = table.Column<string>(type: "TEXT", nullable: true),
                    NombreUnidadMedida = table.Column<string>(type: "TEXT", nullable: true),
                    ValorUmbral = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposDeDato", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    NombreUsuario = table.Column<string>(type: "TEXT", nullable: true),
                    Contraseña = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sismografos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IdentificadorSismografo = table.Column<string>(type: "TEXT", nullable: true),
                    NroSerie = table.Column<string>(type: "TEXT", nullable: true),
                    EstacionSismologicaId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sismografos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sismografos_Estaciones_EstacionSismologicaId",
                        column: x => x.EstacionSismologicaId,
                        principalTable: "Estaciones",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Empleados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: true),
                    Apellido = table.Column<string>(type: "TEXT", nullable: true),
                    Mail = table.Column<string>(type: "TEXT", nullable: true),
                    Telefono = table.Column<string>(type: "TEXT", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empleados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Empleados_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EventosSismicos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FechaHoraInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaHoraDeteccion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LatitudEpicentro = table.Column<double>(type: "REAL", nullable: false),
                    LongitudEpicentro = table.Column<double>(type: "REAL", nullable: false),
                    LatitudHipocentro = table.Column<double>(type: "REAL", nullable: false),
                    LongitudHipocentro = table.Column<double>(type: "REAL", nullable: false),
                    ValorMagnitud = table.Column<double>(type: "REAL", nullable: false),
                    AlcanceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ClasificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OrigenId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ResponsableId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosSismicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventosSismicos_Alcances_AlcanceId",
                        column: x => x.AlcanceId,
                        principalTable: "Alcances",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EventosSismicos_Clasificaciones_ClasificacionId",
                        column: x => x.ClasificacionId,
                        principalTable: "Clasificaciones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EventosSismicos_Empleados_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Empleados",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EventosSismicos_OrigenesDeGeneracion_OrigenId",
                        column: x => x.OrigenId,
                        principalTable: "OrigenesDeGeneracion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CambiosDeEstado",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FechaHoraInicio = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaHoraFin = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResponsableId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EventoSismicoId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CambiosDeEstado", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CambiosDeEstado_Empleados_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CambiosDeEstado_EventosSismicos_EventoSismicoId",
                        column: x => x.EventoSismicoId,
                        principalTable: "EventosSismicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeriesTemporales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CondicionAlarma = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaHoraInicioRegistroMuestras = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaHoraFinRegistroMuestras = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FrecuenciaMuestreo = table.Column<double>(type: "REAL", nullable: false),
                    SismografoId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EventoSismicoId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeriesTemporales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeriesTemporales_EventosSismicos_EventoSismicoId",
                        column: x => x.EventoSismicoId,
                        principalTable: "EventosSismicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeriesTemporales_Sismografos_SismografoId",
                        column: x => x.SismografoId,
                        principalTable: "Sismografos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MuestrasSismicas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SerieTemporalId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuestrasSismicas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MuestrasSismicas_SeriesTemporales_SerieTemporalId",
                        column: x => x.SerieTemporalId,
                        principalTable: "SeriesTemporales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetallesMuestra",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Valor = table.Column<double>(type: "REAL", nullable: false),
                    TipoDeDatoId = table.Column<Guid>(type: "TEXT", nullable: true),
                    MuestraSismicaId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesMuestra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesMuestra_MuestrasSismicas_MuestraSismicaId",
                        column: x => x.MuestraSismicaId,
                        principalTable: "MuestrasSismicas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesMuestra_TiposDeDato_TipoDeDatoId",
                        column: x => x.TipoDeDatoId,
                        principalTable: "TiposDeDato",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CambiosDeEstado_EventoSismicoId",
                table: "CambiosDeEstado",
                column: "EventoSismicoId");

            migrationBuilder.CreateIndex(
                name: "IX_CambiosDeEstado_ResponsableId",
                table: "CambiosDeEstado",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesMuestra_MuestraSismicaId",
                table: "DetallesMuestra",
                column: "MuestraSismicaId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesMuestra_TipoDeDatoId",
                table: "DetallesMuestra",
                column: "TipoDeDatoId");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_UsuarioId",
                table: "Empleados",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventosSismicos_AlcanceId",
                table: "EventosSismicos",
                column: "AlcanceId");

            migrationBuilder.CreateIndex(
                name: "IX_EventosSismicos_ClasificacionId",
                table: "EventosSismicos",
                column: "ClasificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_EventosSismicos_OrigenId",
                table: "EventosSismicos",
                column: "OrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_EventosSismicos_ResponsableId",
                table: "EventosSismicos",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_MuestrasSismicas_SerieTemporalId",
                table: "MuestrasSismicas",
                column: "SerieTemporalId");

            migrationBuilder.CreateIndex(
                name: "IX_SeriesTemporales_EventoSismicoId",
                table: "SeriesTemporales",
                column: "EventoSismicoId");

            migrationBuilder.CreateIndex(
                name: "IX_SeriesTemporales_SismografoId",
                table: "SeriesTemporales",
                column: "SismografoId");

            migrationBuilder.CreateIndex(
                name: "IX_Sismografos_EstacionSismologicaId",
                table: "Sismografos",
                column: "EstacionSismologicaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CambiosDeEstado");

            migrationBuilder.DropTable(
                name: "DetallesMuestra");

            migrationBuilder.DropTable(
                name: "MuestrasSismicas");

            migrationBuilder.DropTable(
                name: "TiposDeDato");

            migrationBuilder.DropTable(
                name: "SeriesTemporales");

            migrationBuilder.DropTable(
                name: "EventosSismicos");

            migrationBuilder.DropTable(
                name: "Sismografos");

            migrationBuilder.DropTable(
                name: "Alcances");

            migrationBuilder.DropTable(
                name: "Clasificaciones");

            migrationBuilder.DropTable(
                name: "Empleados");

            migrationBuilder.DropTable(
                name: "OrigenesDeGeneracion");

            migrationBuilder.DropTable(
                name: "Estaciones");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
