using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class ConciliacionContraFuentesExternas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "auditoria");

            migrationBuilder.AddColumn<string>(
                name: "BienDelInventario",
                schema: "flota",
                table: "Vehiculo",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Chasis",
                schema: "flota",
                table: "Vehiculo",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelativoInstitucional",
                schema: "flota",
                table: "Vehiculo",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Motor",
                schema: "flota",
                table: "Vehiculo",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EjecucionDeConciliacion",
                schema: "auditoria",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    FuenteId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Desde = table.Column<DateOnly>(type: "date", nullable: false),
                    Hasta = table.Column<DateOnly>(type: "date", nullable: false),
                    DocumentoFuente = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FechaDeCorteUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseMinutos = table.Column<int>(type: "int", nullable: false),
                    Ejecuta = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Coincidentes = table.Column<int>(type: "int", nullable: false),
                    SoloEnLaFuente = table.Column<int>(type: "int", nullable: false),
                    SoloEnSigti = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EjecucionDeConciliacion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FuenteExterna",
                schema: "auditoria",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Emisor = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Formato = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ResponsableDeLaCarga = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Disponible = table.Column<bool>(type: "bit", nullable: false),
                    PorQueNoEstaDisponible = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PeriodicidadEnDias = table.Column<int>(type: "int", nullable: true),
                    UltimaConciliacion = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuenteExterna", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiferenciaDeConciliacion",
                schema: "auditoria",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    EjecucionId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Lado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaDelHecho = table.Column<DateOnly>(type: "date", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Referencia = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LineaExterna = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    AsientoId = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    Origen = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    VehiculoId = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    Ancla = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Explicacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ResponsableDeSeguimiento = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Plazo = table.Column<DateOnly>(type: "date", nullable: true),
                    Resolucion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ResueltaUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiferenciaDeConciliacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiferenciaDeConciliacion_EjecucionDeConciliacion_EjecucionId",
                        column: x => x.EjecucionId,
                        principalSchema: "auditoria",
                        principalTable: "EjecucionDeConciliacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculo_BienDelInventario",
                schema: "flota",
                table: "Vehiculo",
                column: "BienDelInventario");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculo_Chasis",
                schema: "flota",
                table: "Vehiculo",
                column: "Chasis");

            migrationBuilder.CreateIndex(
                name: "IX_DiferenciaDeConciliacion_EjecucionId",
                schema: "auditoria",
                table: "DiferenciaDeConciliacion",
                column: "EjecucionId");

            migrationBuilder.CreateIndex(
                name: "IX_DiferenciaDeConciliacion_Referencia",
                schema: "auditoria",
                table: "DiferenciaDeConciliacion",
                column: "Referencia");

            migrationBuilder.CreateIndex(
                name: "IX_DiferenciaDeConciliacion_VehiculoId",
                schema: "auditoria",
                table: "DiferenciaDeConciliacion",
                column: "VehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_EjecucionDeConciliacion_FuenteId_FechaDeCorteUtc",
                schema: "auditoria",
                table: "EjecucionDeConciliacion",
                columns: new[] { "FuenteId", "FechaDeCorteUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiferenciaDeConciliacion",
                schema: "auditoria");

            migrationBuilder.DropTable(
                name: "FuenteExterna",
                schema: "auditoria");

            migrationBuilder.DropTable(
                name: "EjecucionDeConciliacion",
                schema: "auditoria");

            migrationBuilder.DropIndex(
                name: "IX_Vehiculo_BienDelInventario",
                schema: "flota",
                table: "Vehiculo");

            migrationBuilder.DropIndex(
                name: "IX_Vehiculo_Chasis",
                schema: "flota",
                table: "Vehiculo");

            migrationBuilder.DropColumn(
                name: "BienDelInventario",
                schema: "flota",
                table: "Vehiculo");

            migrationBuilder.DropColumn(
                name: "Chasis",
                schema: "flota",
                table: "Vehiculo");

            migrationBuilder.DropColumn(
                name: "CorrelativoInstitucional",
                schema: "flota",
                table: "Vehiculo");

            migrationBuilder.DropColumn(
                name: "Motor",
                schema: "flota",
                table: "Vehiculo");
        }
    }
}
