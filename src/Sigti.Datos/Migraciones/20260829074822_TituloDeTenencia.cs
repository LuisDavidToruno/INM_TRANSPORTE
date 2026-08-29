using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class TituloDeTenencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TituloDeTenencia",
                schema: "flota",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    VehiculoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Regimen = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Titular = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Documento = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Desde = table.Column<DateOnly>(type: "date", nullable: false),
                    Hasta = table.Column<DateOnly>(type: "date", nullable: true),
                    Combustible = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Mantenimiento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Llantas = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Seguro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Peajes = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Multas = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Danios = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TituloDeTenencia", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TituloDeTenencia_VehiculoId_Desde",
                schema: "flota",
                table: "TituloDeTenencia",
                columns: new[] { "VehiculoId", "Desde" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TituloDeTenencia",
                schema: "flota");
        }
    }
}
