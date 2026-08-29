using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class CircuitoDeReintegro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LevantamientoDeBloqueo",
                schema: "combustible",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    MisionId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Responsable = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Persona = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Puesto = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FechaDelHecho = table.Column<DateOnly>(type: "date", nullable: false),
                    MomentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseMinutos = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevantamientoDeBloqueo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ObligacionDeReintegro",
                schema: "combustible",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Causa = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Responsable = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MisionId = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    AsignacionId = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    FechaDelHecho = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObligacionDeReintegro", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MovimientoDeObligacion",
                schema: "combustible",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    ObligacionId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Movimiento = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Destino = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Persona = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Puesto = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FechaDelHecho = table.Column<DateOnly>(type: "date", nullable: false),
                    MomentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseMinutos = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Pagado = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientoDeObligacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientoDeObligacion_ObligacionDeReintegro_ObligacionId",
                        column: x => x.ObligacionId,
                        principalSchema: "combustible",
                        principalTable: "ObligacionDeReintegro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LevantamientoDeBloqueo_MisionId_Responsable",
                schema: "combustible",
                table: "LevantamientoDeBloqueo",
                columns: new[] { "MisionId", "Responsable" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LevantamientoDeBloqueo_Responsable",
                schema: "combustible",
                table: "LevantamientoDeBloqueo",
                column: "Responsable");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoDeObligacion_ObligacionId_Orden",
                schema: "combustible",
                table: "MovimientoDeObligacion",
                columns: new[] { "ObligacionId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ObligacionDeReintegro_FechaDelHecho",
                schema: "combustible",
                table: "ObligacionDeReintegro",
                column: "FechaDelHecho");

            migrationBuilder.CreateIndex(
                name: "IX_ObligacionDeReintegro_Responsable",
                schema: "combustible",
                table: "ObligacionDeReintegro",
                column: "Responsable");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LevantamientoDeBloqueo",
                schema: "combustible");

            migrationBuilder.DropTable(
                name: "MovimientoDeObligacion",
                schema: "combustible");

            migrationBuilder.DropTable(
                name: "ObligacionDeReintegro",
                schema: "combustible");
        }
    }
}
