using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class EspejoDelOrganigrama : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "organizacion");

            migrationBuilder.CreateTable(
                name: "AsignacionDePuestoEspejo",
                schema: "organizacion",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Persona = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Puesto = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Desde = table.Column<DateOnly>(type: "date", nullable: false),
                    Hasta = table.Column<DateOnly>(type: "date", nullable: true),
                    ConfirmadoAlUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionDePuestoEspejo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionDePuestoEspejo_Persona",
                schema: "organizacion",
                table: "AsignacionDePuestoEspejo",
                column: "Persona");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionDePuestoEspejo_Puesto",
                schema: "organizacion",
                table: "AsignacionDePuestoEspejo",
                column: "Puesto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsignacionDePuestoEspejo",
                schema: "organizacion");
        }
    }
}
