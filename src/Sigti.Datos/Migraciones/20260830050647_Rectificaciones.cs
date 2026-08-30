using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class Rectificaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rectificacion",
                schema: "personas",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    ManifiestoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Campo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ValorAnterior = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    ValorRectificado = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    QuienLaPidio = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Registra = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MomentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rectificacion", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rectificacion_ManifiestoId",
                schema: "personas",
                table: "Rectificacion",
                column: "ManifiestoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Rectificacion",
                schema: "personas");
        }
    }
}
