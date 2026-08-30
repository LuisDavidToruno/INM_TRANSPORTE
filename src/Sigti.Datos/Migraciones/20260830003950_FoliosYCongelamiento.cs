using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class FoliosYCongelamiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FolioNumero",
                schema: "mision",
                table: "Expediente",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "FolioRangoId",
                schema: "mision",
                table: "Expediente",
                type: "binary(16)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FolioTexto",
                schema: "mision",
                table: "Expediente",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HuellaCongelada",
                schema: "mision",
                table: "Expediente",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RangoDeFolio",
                schema: "mision",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Delegacion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    TipoDeDocumento = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Desde = table.Column<int>(type: "int", nullable: false),
                    Hasta = table.Column<int>(type: "int", nullable: false),
                    Emitidos = table.Column<int>(type: "int", nullable: false),
                    Dispositivo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Asigna = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AsignadoEl = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RangoDeFolio", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Expediente_FolioTexto",
                schema: "mision",
                table: "Expediente",
                column: "FolioTexto",
                unique: true,
                filter: "[FolioTexto] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RangoDeFolio_TipoDeDocumento_Delegacion",
                schema: "mision",
                table: "RangoDeFolio",
                columns: new[] { "TipoDeDocumento", "Delegacion" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RangoDeFolio",
                schema: "mision");

            migrationBuilder.DropIndex(
                name: "IX_Expediente_FolioTexto",
                schema: "mision",
                table: "Expediente");

            migrationBuilder.DropColumn(
                name: "FolioNumero",
                schema: "mision",
                table: "Expediente");

            migrationBuilder.DropColumn(
                name: "FolioRangoId",
                schema: "mision",
                table: "Expediente");

            migrationBuilder.DropColumn(
                name: "FolioTexto",
                schema: "mision",
                table: "Expediente");

            migrationBuilder.DropColumn(
                name: "HuellaCongelada",
                schema: "mision",
                table: "Expediente");
        }
    }
}
