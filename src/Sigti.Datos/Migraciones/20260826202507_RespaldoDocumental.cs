using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class RespaldoDocumental : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RespaldoAdjunto",
                schema: "catalogo",
                table: "VersionDeParametro",
                type: "binary(16)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "RespaldoFuente",
                schema: "catalogo",
                table: "VersionDeParametro",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "RespaldoVerificadoEl",
                schema: "catalogo",
                table: "VersionDeParametro",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RespaldoAdjunto",
                schema: "catalogo",
                table: "VersionDeParametro");

            migrationBuilder.DropColumn(
                name: "RespaldoFuente",
                schema: "catalogo",
                table: "VersionDeParametro");

            migrationBuilder.DropColumn(
                name: "RespaldoVerificadoEl",
                schema: "catalogo",
                table: "VersionDeParametro");
        }
    }
}
