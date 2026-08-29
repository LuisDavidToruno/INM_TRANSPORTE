using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class PrestamoDeVehiculo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Prestamo",
                schema: "flota",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    VehiculoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    ActoFolio = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ActoFirmante = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ActoFecha = table.Column<DateOnly>(type: "date", nullable: false),
                    ActoAdjunto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Autoriza = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ReceptorPersona = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ReceptorCargo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ReceptorInstitucion = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    ReceptorConstancia = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Desde = table.Column<DateOnly>(type: "date", nullable: false),
                    DevolucionComprometida = table.Column<DateOnly>(type: "date", nullable: false),
                    EntregaFecha = table.Column<DateOnly>(type: "date", nullable: false),
                    EntregaOdometro = table.Column<int>(type: "int", nullable: false),
                    EntregaFirma = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    EntregaCombustible = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    EntregaAccesorios = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EntregaDocumentos = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EntregaRotulacion = table.Column<bool>(type: "bit", nullable: false),
                    EntregaNovedades = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RubroCombustible = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    RubroPeajes = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    RubroMantenimiento = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    RubroMultas = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    RubroDanios = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    DevolucionFecha = table.Column<DateOnly>(type: "date", nullable: true),
                    DevolucionOdometro = table.Column<int>(type: "int", nullable: true),
                    DevolucionFirma = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DevolucionCombustible = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    DevolucionNovedades = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DevolucionRotulacion = table.Column<bool>(type: "bit", nullable: true),
                    QuienFirmaLaDevolucion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prestamo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prestamo_ActoFolio",
                schema: "flota",
                table: "Prestamo",
                column: "ActoFolio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prestamo_DevolucionFecha_DevolucionComprometida",
                schema: "flota",
                table: "Prestamo",
                columns: new[] { "DevolucionFecha", "DevolucionComprometida" });

            migrationBuilder.CreateIndex(
                name: "IX_Prestamo_VehiculoId_Desde",
                schema: "flota",
                table: "Prestamo",
                columns: new[] { "VehiculoId", "Desde" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Prestamo",
                schema: "flota");
        }
    }
}
