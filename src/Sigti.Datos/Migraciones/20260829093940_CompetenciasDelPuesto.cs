using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class CompetenciasDelPuesto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompetenciaDelPuesto",
                schema: "organizacion",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Puesto = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Alcance = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Desde = table.Column<DateOnly>(type: "date", nullable: false),
                    Hasta = table.Column<DateOnly>(type: "date", nullable: true),
                    Otorga = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ParesVigilados = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetenciaDelPuesto", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompetenciaDelPuesto_Puesto",
                schema: "organizacion",
                table: "CompetenciaDelPuesto",
                column: "Puesto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompetenciaDelPuesto",
                schema: "organizacion");
        }
    }
}
