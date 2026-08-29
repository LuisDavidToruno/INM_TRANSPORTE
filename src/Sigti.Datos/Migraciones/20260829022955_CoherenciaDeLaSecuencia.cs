using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class CoherenciaDeLaSecuencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Corredor",
                schema: "peajes",
                table: "Punto",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Kilometro",
                schema: "peajes",
                table: "Punto",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DesvioDeclarado",
                schema: "peajes",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    MisionId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    VehiculoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    DesdeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseDesde = table.Column<int>(type: "int", nullable: false),
                    HastaUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DesfaseHasta = table.Column<int>(type: "int", nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Declara = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IdDeCaptura = table.Column<byte[]>(type: "binary(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesvioDeclarado", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RutaAutorizada",
                schema: "peajes",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    MisionId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    PuntoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Cruces = table.Column<int>(type: "int", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    TarifaId = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    CongeladoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseMinutos = table.Column<int>(type: "int", nullable: false),
                    Congela = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RutaAutorizada", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Punto_Corredor_Kilometro",
                schema: "peajes",
                table: "Punto",
                columns: new[] { "Corredor", "Kilometro" });

            migrationBuilder.CreateIndex(
                name: "IX_DesvioDeclarado_IdDeCaptura",
                schema: "peajes",
                table: "DesvioDeclarado",
                column: "IdDeCaptura",
                unique: true,
                filter: "[IdDeCaptura] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DesvioDeclarado_MisionId",
                schema: "peajes",
                table: "DesvioDeclarado",
                column: "MisionId");

            migrationBuilder.CreateIndex(
                name: "IX_RutaAutorizada_MisionId_PuntoId",
                schema: "peajes",
                table: "RutaAutorizada",
                columns: new[] { "MisionId", "PuntoId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DesvioDeclarado",
                schema: "peajes");

            migrationBuilder.DropTable(
                name: "RutaAutorizada",
                schema: "peajes");

            migrationBuilder.DropIndex(
                name: "IX_Punto_Corredor_Kilometro",
                schema: "peajes",
                table: "Punto");

            migrationBuilder.DropColumn(
                name: "Corredor",
                schema: "peajes",
                table: "Punto");

            migrationBuilder.DropColumn(
                name: "Kilometro",
                schema: "peajes",
                table: "Punto");
        }
    }
}
