using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class ReemisionDelPermiso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AnuladoEnUtc",
                schema: "mision",
                table: "Salvoconducto",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnuladoPor",
                schema: "mision",
                table: "Salvoconducto",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoDeLaAnulacion",
                schema: "mision",
                table: "Salvoconducto",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Reemplaza",
                schema: "mision",
                table: "PermisoDeCirculacion",
                type: "binary(16)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnuladoEnUtc",
                schema: "mision",
                table: "Salvoconducto");

            migrationBuilder.DropColumn(
                name: "AnuladoPor",
                schema: "mision",
                table: "Salvoconducto");

            migrationBuilder.DropColumn(
                name: "MotivoDeLaAnulacion",
                schema: "mision",
                table: "Salvoconducto");

            migrationBuilder.DropColumn(
                name: "Reemplaza",
                schema: "mision",
                table: "PermisoDeCirculacion");
        }
    }
}
