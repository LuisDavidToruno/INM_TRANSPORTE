using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AvisosDeTareas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Aviso",
                schema: "organizacion",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Tarea = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Destinatario = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Canal = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Resultado = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    MomentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Detalle = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aviso", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aviso_Tarea",
                schema: "organizacion",
                table: "Aviso",
                column: "Tarea");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Aviso",
                schema: "organizacion");
        }
    }
}
