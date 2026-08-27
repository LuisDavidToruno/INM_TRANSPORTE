using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class PadronDeConductores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "motoristas");

            migrationBuilder.CreateTable(
                name: "Conductor",
                schema: "motoristas",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    EsDelPadron = table.Column<bool>(type: "bit", nullable: false),
                    NumeroDeLicencia = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    VenceLicencia = table.Column<DateOnly>(type: "date", nullable: false),
                    Restricciones = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conductor", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Conductor_NumeroDeLicencia",
                schema: "motoristas",
                table: "Conductor",
                column: "NumeroDeLicencia",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conductor_VenceLicencia",
                schema: "motoristas",
                table: "Conductor",
                column: "VenceLicencia");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Conductor",
                schema: "motoristas");
        }
    }
}
