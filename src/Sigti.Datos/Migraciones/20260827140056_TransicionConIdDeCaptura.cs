using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class TransicionConIdDeCaptura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "IdDeCaptura",
                schema: "mision",
                table: "Transicion",
                type: "binary(16)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transicion_IdDeCaptura",
                schema: "mision",
                table: "Transicion",
                column: "IdDeCaptura",
                unique: true,
                filter: "[IdDeCaptura] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transicion_IdDeCaptura",
                schema: "mision",
                table: "Transicion");

            migrationBuilder.DropColumn(
                name: "IdDeCaptura",
                schema: "mision",
                table: "Transicion");
        }
    }
}
