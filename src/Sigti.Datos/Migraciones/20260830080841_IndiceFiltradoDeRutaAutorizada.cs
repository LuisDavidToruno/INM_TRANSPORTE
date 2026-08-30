using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class IndiceFiltradoDeRutaAutorizada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RutaAutorizada_MisionId_PuntoId",
                schema: "peajes",
                table: "RutaAutorizada");

            migrationBuilder.CreateIndex(
                name: "IX_RutaAutorizada_MisionId_PuntoId",
                schema: "peajes",
                table: "RutaAutorizada",
                columns: new[] { "MisionId", "PuntoId" },
                unique: true,
                filter: "[SupersedidaPor] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RutaAutorizada_MisionId_PuntoId",
                schema: "peajes",
                table: "RutaAutorizada");

            migrationBuilder.CreateIndex(
                name: "IX_RutaAutorizada_MisionId_PuntoId",
                schema: "peajes",
                table: "RutaAutorizada",
                columns: new[] { "MisionId", "PuntoId" },
                unique: true);
        }
    }
}
