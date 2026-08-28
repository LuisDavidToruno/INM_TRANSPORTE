using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class PermisoDeCirculacionYServicioExceptuado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "ExceptuadoDesde",
                schema: "flota",
                table: "Vehiculo",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExceptuadoHasta",
                schema: "flota",
                table: "Vehiculo",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FundamentoDeLaExcepcion",
                schema: "flota",
                table: "Vehiculo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoDeServicioExceptuado",
                schema: "flota",
                table: "Vehiculo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PermisoDeCirculacion",
                schema: "mision",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    ExpedienteId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Folio = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    EmitidoPor = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Vehiculo = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Motorista = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Destino = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Desde = table.Column<DateOnly>(type: "date", nullable: false),
                    Hasta = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermisoDeCirculacion", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PermisoDeCirculacion_ExpedienteId",
                schema: "mision",
                table: "PermisoDeCirculacion",
                column: "ExpedienteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermisoDeCirculacion",
                schema: "mision");

            migrationBuilder.DropColumn(
                name: "ExceptuadoDesde",
                schema: "flota",
                table: "Vehiculo");

            migrationBuilder.DropColumn(
                name: "ExceptuadoHasta",
                schema: "flota",
                table: "Vehiculo");

            migrationBuilder.DropColumn(
                name: "FundamentoDeLaExcepcion",
                schema: "flota",
                table: "Vehiculo");

            migrationBuilder.DropColumn(
                name: "TipoDeServicioExceptuado",
                schema: "flota",
                table: "Vehiculo");
        }
    }
}
