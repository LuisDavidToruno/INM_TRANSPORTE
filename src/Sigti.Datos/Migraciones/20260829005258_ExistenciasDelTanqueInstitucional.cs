using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class ExistenciasDelTanqueInstitucional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tanque",
                schema: "combustible",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    AmbitoDeclarado = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    TipoDeCombustible = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CapacidadGalones = table.Column<decimal>(type: "decimal(12,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tanque", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MovimientoDeExistencias",
                schema: "combustible",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    TanqueId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Movimiento = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Galones = table.Column<decimal>(type: "decimal(12,3)", nullable: false),
                    Persona = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Puesto = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FechaDelHecho = table.Column<DateOnly>(type: "date", nullable: false),
                    MomentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseMinutos = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    VehiculoId = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    MisionId = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    AbastecimientoId = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    ContraparteId = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    ExistenciaMedida = table.Column<decimal>(type: "decimal(12,3)", nullable: true),
                    MotivoDelAjuste = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Comprobante = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientoDeExistencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientoDeExistencias_Tanque_TanqueId",
                        column: x => x.TanqueId,
                        principalSchema: "combustible",
                        principalTable: "Tanque",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoDeExistencias_AbastecimientoId",
                schema: "combustible",
                table: "MovimientoDeExistencias",
                column: "AbastecimientoId",
                unique: true,
                filter: "[AbastecimientoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoDeExistencias_TanqueId_Orden",
                schema: "combustible",
                table: "MovimientoDeExistencias",
                columns: new[] { "TanqueId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoDeExistencias_VehiculoId",
                schema: "combustible",
                table: "MovimientoDeExistencias",
                column: "VehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_Tanque_AmbitoDeclarado",
                schema: "combustible",
                table: "Tanque",
                column: "AmbitoDeclarado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimientoDeExistencias",
                schema: "combustible");

            migrationBuilder.DropTable(
                name: "Tanque",
                schema: "combustible");
        }
    }
}
