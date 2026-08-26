using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class ParametrosNormativos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalogo");

            migrationBuilder.CreateTable(
                name: "VersionDeParametro",
                schema: "catalogo",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Clave = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Valor = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    VigenteDesde = table.Column<DateOnly>(type: "date", nullable: false),
                    VigenteHasta = table.Column<DateOnly>(type: "date", nullable: true),
                    RegistradoDesde = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RegistradoHasta = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CargadoPor = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AprobadoPor = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VersionDeParametro", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VersionDeParametro_Clave_VigenteDesde",
                schema: "catalogo",
                table: "VersionDeParametro",
                columns: new[] { "Clave", "VigenteDesde" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VersionDeParametro",
                schema: "catalogo");
        }
    }
}
