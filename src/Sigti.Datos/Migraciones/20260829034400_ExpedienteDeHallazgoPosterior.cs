using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class ExpedienteDeHallazgoPosterior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HallazgoPosterior",
                schema: "auditoria",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    FechaDelHecho = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaDelDescubrimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    ComoSeDescubrio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Fuente = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DocumentoAdjunto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    VehiculoId = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    MotoristaId = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    Periodo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Resolucion = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Fundamento = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HallazgoPosterior", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AsientoReverso",
                schema: "auditoria",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    HallazgoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    TipoDeAsiento = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    IdentificadorDelAsiento = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DescripcionDelAsiento = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Naturaleza = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ValorAnterior = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ValorNuevo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    FechaDelHechoOriginal = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaDelReversoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseMinutos = table.Column<int>(type: "int", nullable: false),
                    Persona = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Puesto = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Autoriza = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AutorDelAsientoOriginal = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MotivoTipificado = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Fundamento = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Adjunto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PeriodoAfectado = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PeriodoDeImputacion = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    EfectoEconomico = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TablasParametricas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsientoReverso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsientoReverso_HallazgoPosterior_HallazgoId",
                        column: x => x.HallazgoId,
                        principalSchema: "auditoria",
                        principalTable: "HallazgoPosterior",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MisionDelHallazgo",
                schema: "auditoria",
                columns: table => new
                {
                    HallazgoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    MisionId = table.Column<byte[]>(type: "binary(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MisionDelHallazgo", x => new { x.HallazgoId, x.MisionId });
                    table.ForeignKey(
                        name: "FK_MisionDelHallazgo_HallazgoPosterior_HallazgoId",
                        column: x => x.HallazgoId,
                        principalSchema: "auditoria",
                        principalTable: "HallazgoPosterior",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientoDelHallazgo",
                schema: "auditoria",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    HallazgoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Movimiento = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Persona = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Puesto = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FechaDelHecho = table.Column<DateOnly>(type: "date", nullable: false),
                    MomentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseMinutos = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ReversoId = table.Column<byte[]>(type: "binary(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientoDelHallazgo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientoDelHallazgo_HallazgoPosterior_HallazgoId",
                        column: x => x.HallazgoId,
                        principalSchema: "auditoria",
                        principalTable: "HallazgoPosterior",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsientoReverso_HallazgoId",
                schema: "auditoria",
                table: "AsientoReverso",
                column: "HallazgoId");

            migrationBuilder.CreateIndex(
                name: "IX_AsientoReverso_PeriodoDeImputacion",
                schema: "auditoria",
                table: "AsientoReverso",
                column: "PeriodoDeImputacion");

            migrationBuilder.CreateIndex(
                name: "IX_AsientoReverso_TipoDeAsiento_IdentificadorDelAsiento",
                schema: "auditoria",
                table: "AsientoReverso",
                columns: new[] { "TipoDeAsiento", "IdentificadorDelAsiento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HallazgoPosterior_FechaDelHecho",
                schema: "auditoria",
                table: "HallazgoPosterior",
                column: "FechaDelHecho");

            migrationBuilder.CreateIndex(
                name: "IX_HallazgoPosterior_VehiculoId",
                schema: "auditoria",
                table: "HallazgoPosterior",
                column: "VehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_MisionDelHallazgo_MisionId",
                schema: "auditoria",
                table: "MisionDelHallazgo",
                column: "MisionId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoDelHallazgo_HallazgoId_Orden",
                schema: "auditoria",
                table: "MovimientoDelHallazgo",
                columns: new[] { "HallazgoId", "Orden" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsientoReverso",
                schema: "auditoria");

            migrationBuilder.DropTable(
                name: "MisionDelHallazgo",
                schema: "auditoria");

            migrationBuilder.DropTable(
                name: "MovimientoDelHallazgo",
                schema: "auditoria");

            migrationBuilder.DropTable(
                name: "HallazgoPosterior",
                schema: "auditoria");
        }
    }
}
