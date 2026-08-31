using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class ResguardoDeFeriado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResguardoDeFeriado",
                schema: "flota",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    VehiculoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Desde = table.Column<DateOnly>(type: "date", nullable: false),
                    Hasta = table.Column<DateOnly>(type: "date", nullable: false),
                    Predio = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Evidencia = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    ConfirmadoEl = table.Column<DateOnly>(type: "date", nullable: false),
                    ConfirmadoPor = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    RegistradoEnUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResguardoDeFeriado", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResguardoDeFeriado_VehiculoId_Desde",
                schema: "flota",
                table: "ResguardoDeFeriado",
                columns: new[] { "VehiculoId", "Desde" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResguardoDeFeriado",
                schema: "flota");
        }
    }
}
