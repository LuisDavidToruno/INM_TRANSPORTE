using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class HoraDeSalidaYRetorno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "HoraDeRetorno",
                schema: "mision",
                table: "Expediente",
                type: "time(0)",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HoraDeSalida",
                schema: "mision",
                table: "Expediente",
                type: "time(0)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HoraDeRetorno",
                schema: "mision",
                table: "Expediente");

            migrationBuilder.DropColumn(
                name: "HoraDeSalida",
                schema: "mision",
                table: "Expediente");
        }
    }
}
