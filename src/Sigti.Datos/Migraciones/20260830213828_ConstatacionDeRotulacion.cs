using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class ConstatacionDeRotulacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConstatacionDeRotulacion",
                schema: "flota",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    VehiculoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Elemento = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Presente = table.Column<bool>(type: "bit", nullable: false),
                    ConstatadoEl = table.Column<DateOnly>(type: "date", nullable: false),
                    Fotografia = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    ConstatadoPor = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    RegistradoEnUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConstatacionDeRotulacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConstatacionDeRotulacion_Vehiculo_VehiculoId",
                        column: x => x.VehiculoId,
                        principalSchema: "flota",
                        principalTable: "Vehiculo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConstatacionDeRotulacion_VehiculoId_Elemento_ConstatadoEl",
                schema: "flota",
                table: "ConstatacionDeRotulacion",
                columns: new[] { "VehiculoId", "Elemento", "ConstatadoEl" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConstatacionDeRotulacion",
                schema: "flota");
        }
    }
}
