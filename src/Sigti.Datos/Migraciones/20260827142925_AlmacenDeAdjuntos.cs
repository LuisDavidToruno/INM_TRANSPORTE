using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AlmacenDeAdjuntos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Adjunto",
                schema: "mision",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    IdTransicion = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Ruta = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Hash = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Bytes = table.Column<long>(type: "bigint", nullable: false),
                    Clasificacion = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CapturadoEnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecibidoEnUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adjunto", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Adjunto_Clasificacion",
                schema: "mision",
                table: "Adjunto",
                column: "Clasificacion");

            migrationBuilder.CreateIndex(
                name: "IX_Adjunto_IdTransicion",
                schema: "mision",
                table: "Adjunto",
                column: "IdTransicion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Adjunto",
                schema: "mision");
        }
    }
}
