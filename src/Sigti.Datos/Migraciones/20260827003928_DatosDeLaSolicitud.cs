using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class DatosDeLaSolicitud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Dependencia",
                schema: "mision",
                table: "Expediente",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Destino",
                schema: "mision",
                table: "Expediente",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "HolguraDias",
                schema: "mision",
                table: "Expediente",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ObjetoDelTraslado",
                schema: "mision",
                table: "Expediente",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "Retorno",
                schema: "mision",
                table: "Expediente",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "Salida",
                schema: "mision",
                table: "Expediente",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dependencia",
                schema: "mision",
                table: "Expediente");

            migrationBuilder.DropColumn(
                name: "Destino",
                schema: "mision",
                table: "Expediente");

            migrationBuilder.DropColumn(
                name: "HolguraDias",
                schema: "mision",
                table: "Expediente");

            migrationBuilder.DropColumn(
                name: "ObjetoDelTraslado",
                schema: "mision",
                table: "Expediente");

            migrationBuilder.DropColumn(
                name: "Retorno",
                schema: "mision",
                table: "Expediente");

            migrationBuilder.DropColumn(
                name: "Salida",
                schema: "mision",
                table: "Expediente");
        }
    }
}
