using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class ReservaEnElDiario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ConductorTomado",
                schema: "mision",
                table: "Transicion",
                type: "binary(16)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "VehiculoTomado",
                schema: "mision",
                table: "Transicion",
                type: "binary(16)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transicion_VehiculoTomado",
                schema: "mision",
                table: "Transicion",
                column: "VehiculoTomado",
                filter: "[VehiculoTomado] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transicion_VehiculoTomado",
                schema: "mision",
                table: "Transicion");

            migrationBuilder.DropColumn(
                name: "ConductorTomado",
                schema: "mision",
                table: "Transicion");

            migrationBuilder.DropColumn(
                name: "VehiculoTomado",
                schema: "mision",
                table: "Transicion");
        }
    }
}
