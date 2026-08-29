using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class SeguimientoEnRuta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "seguimiento");

            migrationBuilder.CreateTable(
                name: "ReporteDeCampo",
                schema: "seguimiento",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    MisionId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Destino = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    MomentoDelHechoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseDelHechoMinutos = table.Column<int>(type: "int", nullable: false),
                    MomentoDeCapturaUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseDeCapturaMinutos = table.Column<int>(type: "int", nullable: false),
                    Latitud = table.Column<decimal>(type: "decimal(9,7)", nullable: true),
                    Longitud = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    PrecisionMetros = table.Column<int>(type: "int", nullable: true),
                    CausaDeEspera = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    SeAtribuyeA = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    MotorEncendido = table.Column<bool>(type: "bit", nullable: true),
                    Declara = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReporteDeCampo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReporteDeCampo_MisionId_MomentoDelHechoUtc",
                schema: "seguimiento",
                table: "ReporteDeCampo",
                columns: new[] { "MisionId", "MomentoDelHechoUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReporteDeCampo",
                schema: "seguimiento");
        }
    }
}
