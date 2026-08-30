using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class RecongelamientoDePeaje : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "SupersedidaPor",
                schema: "peajes",
                table: "RutaAutorizada",
                type: "binary(16)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Recongelamiento",
                schema: "peajes",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    MisionId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    VehiculoSaliente = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    VehiculoEntrante = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    CategoriaAnterior = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    CategoriaNueva = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    TotalAnterior = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    TotalNuevo = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Recongela = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MomentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseMinutos = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recongelamiento", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recongelamiento_MisionId",
                schema: "peajes",
                table: "Recongelamiento",
                column: "MisionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Recongelamiento",
                schema: "peajes");

            migrationBuilder.DropColumn(
                name: "SupersedidaPor",
                schema: "peajes",
                table: "RutaAutorizada");
        }
    }
}
