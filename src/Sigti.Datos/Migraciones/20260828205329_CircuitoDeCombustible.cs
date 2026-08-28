using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class CircuitoDeCombustible : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "combustible");

            migrationBuilder.CreateTable(
                name: "Asignacion",
                schema: "combustible",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Folio = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FondoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    MisionId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    VehiculoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Receptor = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Galones = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    Instrumento = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    TipoDeCombustible = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asignacion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fondo",
                schema: "combustible",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Ambito = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    AmbitoDeclarado = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Desde = table.Column<DateOnly>(type: "date", nullable: false),
                    Hasta = table.Column<DateOnly>(type: "date", nullable: false),
                    Solicita = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Aprueba = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PartidaPresupuestaria = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fondo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransicionDeAsignacion",
                schema: "combustible",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    AsignacionId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Transicion = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Destino = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Ejecuta = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MomentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseMinutos = table.Column<int>(type: "int", nullable: false),
                    IdDeCaptura = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ConsumoGalones = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    ConsumoMonto = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ConsumoEstacion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ConsumoOdometro = table.Column<int>(type: "int", nullable: true),
                    ConsumoComprobante = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Devuelto = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransicionDeAsignacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransicionDeAsignacion_Asignacion_AsignacionId",
                        column: x => x.AsignacionId,
                        principalSchema: "combustible",
                        principalTable: "Asignacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientoDelFondo",
                schema: "combustible",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    FondoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Movimiento = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Destino = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Ejecuta = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MomentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseMinutos = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientoDelFondo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientoDelFondo_Fondo_FondoId",
                        column: x => x.FondoId,
                        principalSchema: "combustible",
                        principalTable: "Fondo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asignacion_Folio",
                schema: "combustible",
                table: "Asignacion",
                column: "Folio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Asignacion_FondoId",
                schema: "combustible",
                table: "Asignacion",
                column: "FondoId");

            migrationBuilder.CreateIndex(
                name: "IX_Asignacion_MisionId",
                schema: "combustible",
                table: "Asignacion",
                column: "MisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Fondo_AmbitoDeclarado_Desde_Hasta",
                schema: "combustible",
                table: "Fondo",
                columns: new[] { "AmbitoDeclarado", "Desde", "Hasta" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoDelFondo_FondoId_Orden",
                schema: "combustible",
                table: "MovimientoDelFondo",
                columns: new[] { "FondoId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransicionDeAsignacion_AsignacionId_Orden",
                schema: "combustible",
                table: "TransicionDeAsignacion",
                columns: new[] { "AsignacionId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransicionDeAsignacion_IdDeCaptura",
                schema: "combustible",
                table: "TransicionDeAsignacion",
                column: "IdDeCaptura",
                unique: true,
                filter: "[IdDeCaptura] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimientoDelFondo",
                schema: "combustible");

            migrationBuilder.DropTable(
                name: "TransicionDeAsignacion",
                schema: "combustible");

            migrationBuilder.DropTable(
                name: "Fondo",
                schema: "combustible");

            migrationBuilder.DropTable(
                name: "Asignacion",
                schema: "combustible");
        }
    }
}
