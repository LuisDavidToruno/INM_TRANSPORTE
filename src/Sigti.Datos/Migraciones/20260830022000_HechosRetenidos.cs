using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class HechosRetenidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HechoRetenido",
                schema: "sincronizacion",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    IdDeCaptura = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    EsperaExpediente = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Transicion = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Ejecuta = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OcurridoEnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseMinutos = table.Column<int>(type: "int", nullable: false),
                    Odometro = table.Column<int>(type: "int", nullable: true),
                    Dispositivo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    RetenidoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Intentos = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HechoRetenido", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HechoRetenido_EsperaExpediente",
                schema: "sincronizacion",
                table: "HechoRetenido",
                column: "EsperaExpediente");

            migrationBuilder.CreateIndex(
                name: "IX_HechoRetenido_IdDeCaptura",
                schema: "sincronizacion",
                table: "HechoRetenido",
                column: "IdDeCaptura",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HechoRetenido",
                schema: "sincronizacion");
        }
    }
}
