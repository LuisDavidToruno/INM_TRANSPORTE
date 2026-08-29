using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class ExpedienteDeIncidente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "incidentes");

            migrationBuilder.CreateTable(
                name: "Incidente",
                schema: "incidentes",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Causa = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    FechaDelHecho = table.Column<DateOnly>(type: "date", nullable: false),
                    MomentoDelHechoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseDelHechoMinutos = table.Column<int>(type: "int", nullable: false),
                    MomentoDeCapturaUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Registra = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MisionId = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    VehiculoId = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    Ubicacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Odometro = table.Column<int>(type: "int", nullable: true),
                    Interrumpe = table.Column<bool>(type: "bit", nullable: false),
                    ResponsableDeSeguimiento = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Plazo = table.Column<DateOnly>(type: "date", nullable: false),
                    ConstanciaNumero = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ConstanciaAutoridad = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ConstanciaFecha = table.Column<DateOnly>(type: "date", nullable: true),
                    Desenlace = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    DetalleDelDesenlace = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DeterminacionNumero = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DeterminacionInstancia = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    DeterminacionFecha = table.Column<DateOnly>(type: "date", nullable: true),
                    DeterminacionResolucion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ResueltoEn = table.Column<DateOnly>(type: "date", nullable: true),
                    ComoSeResolvio = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DeclaracionDeBienes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BienAfectado",
                schema: "incidentes",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    IncidenteId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EsElVehiculo = table.Column<bool>(type: "bit", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaDelHecho = table.Column<DateOnly>(type: "date", nullable: false),
                    UbicacionConocida = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AutoridadCustodia = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    NumeroDeExpedienteExterno = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DescargoNumero = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DescargoAutoridad = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    DescargoFecha = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BienAfectado", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BienAfectado_Incidente_IncidenteId",
                        column: x => x.IncidenteId,
                        principalSchema: "incidentes",
                        principalTable: "Incidente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GestionDeRecuperacion",
                schema: "incidentes",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    IncidenteId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Responsable = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Plazo = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GestionDeRecuperacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GestionDeRecuperacion_Incidente_IncidenteId",
                        column: x => x.IncidenteId,
                        principalSchema: "incidentes",
                        principalTable: "Incidente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientoDelIncidente",
                schema: "incidentes",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    IncidenteId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Movimiento = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Ejecuta = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MomentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseMinutos = table.Column<int>(type: "int", nullable: false),
                    Detalle = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientoDelIncidente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientoDelIncidente_Incidente_IncidenteId",
                        column: x => x.IncidenteId,
                        principalSchema: "incidentes",
                        principalTable: "Incidente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BienAfectado_Estado",
                schema: "incidentes",
                table: "BienAfectado",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_BienAfectado_IncidenteId",
                schema: "incidentes",
                table: "BienAfectado",
                column: "IncidenteId");

            migrationBuilder.CreateIndex(
                name: "IX_GestionDeRecuperacion_IncidenteId",
                schema: "incidentes",
                table: "GestionDeRecuperacion",
                column: "IncidenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidente_FechaDelHecho",
                schema: "incidentes",
                table: "Incidente",
                column: "FechaDelHecho");

            migrationBuilder.CreateIndex(
                name: "IX_Incidente_Interrumpe_ResueltoEn",
                schema: "incidentes",
                table: "Incidente",
                columns: new[] { "Interrumpe", "ResueltoEn" });

            migrationBuilder.CreateIndex(
                name: "IX_Incidente_MisionId",
                schema: "incidentes",
                table: "Incidente",
                column: "MisionId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoDelIncidente_IncidenteId_Orden",
                schema: "incidentes",
                table: "MovimientoDelIncidente",
                columns: new[] { "IncidenteId", "Orden" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BienAfectado",
                schema: "incidentes");

            migrationBuilder.DropTable(
                name: "GestionDeRecuperacion",
                schema: "incidentes");

            migrationBuilder.DropTable(
                name: "MovimientoDelIncidente",
                schema: "incidentes");

            migrationBuilder.DropTable(
                name: "Incidente",
                schema: "incidentes");
        }
    }
}
