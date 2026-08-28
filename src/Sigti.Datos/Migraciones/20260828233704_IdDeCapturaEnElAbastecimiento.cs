using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class IdDeCapturaEnElAbastecimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "IdDeCaptura",
                schema: "combustible",
                table: "Abastecimiento",
                type: "binary(16)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Abastecimiento_IdDeCaptura",
                schema: "combustible",
                table: "Abastecimiento",
                column: "IdDeCaptura",
                unique: true,
                filter: "[IdDeCaptura] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Abastecimiento_IdDeCaptura",
                schema: "combustible",
                table: "Abastecimiento");

            migrationBuilder.DropColumn(
                name: "IdDeCaptura",
                schema: "combustible",
                table: "Abastecimiento");
        }
    }
}
