using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class NivelDeTanqueEnLaBitacora : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EscalaDelNivel",
                schema: "mision",
                table: "Transicion",
                type: "nvarchar(24)",
                maxLength: 24,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NivelDeTanque",
                schema: "mision",
                table: "Transicion",
                type: "decimal(9,4)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EscalaDelNivel",
                schema: "mision",
                table: "Transicion");

            migrationBuilder.DropColumn(
                name: "NivelDeTanque",
                schema: "mision",
                table: "Transicion");
        }
    }
}
