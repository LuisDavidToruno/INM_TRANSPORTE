using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class EstadoOperativoDelVehiculo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CambioDeEstado",
                schema: "flota",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    VehiculoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    MomentoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Ejecuta = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Automatico = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CambioDeEstado", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CambioDeEstado_VehiculoId_Orden",
                schema: "flota",
                table: "CambioDeEstado",
                columns: new[] { "VehiculoId", "Orden" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CambioDeEstado",
                schema: "flota");
        }
    }
}
