using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AdjuntoDeParametro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "IdTransicion",
                schema: "mision",
                table: "Adjunto",
                type: "binary(16)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "binary(16)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "IdTransicion",
                schema: "mision",
                table: "Adjunto",
                type: "binary(16)",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "binary(16)",
                oldNullable: true);
        }
    }
}
