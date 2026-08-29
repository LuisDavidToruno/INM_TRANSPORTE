using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class IndisponibilidadSobrevenida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "mantenimiento");

            migrationBuilder.CreateTable(
                name: "Indisponibilidad",
                schema: "mantenimiento",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    VehiculoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Causa = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Desde = table.Column<DateOnly>(type: "date", nullable: false),
                    FinEstimado = table.Column<DateOnly>(type: "date", nullable: false),
                    Ejecuta = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MomentoDelAcuseUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseMinutos = table.Column<int>(type: "int", nullable: false),
                    FinReal = table.Column<DateOnly>(type: "date", nullable: true),
                    OrdenDeTrabajo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    OdometroDeSalida = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Indisponibilidad", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReservaAfectada",
                schema: "mantenimiento",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    IndisponibilidadId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    MisionId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Referencia = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Dependencia = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Salida = table.Column<DateOnly>(type: "date", nullable: false),
                    Retorno = table.Column<DateOnly>(type: "date", nullable: false),
                    Motorista = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ObjetoDelTraslado = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EstadoAlAcusar = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservaAfectada", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservaAfectada_Indisponibilidad_IndisponibilidadId",
                        column: x => x.IndisponibilidadId,
                        principalSchema: "mantenimiento",
                        principalTable: "Indisponibilidad",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResolucionDeReserva",
                schema: "mantenimiento",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    IndisponibilidadId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    MisionId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Desenlace = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Ejecuta = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MomentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseMinutos = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResolucionDeReserva", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResolucionDeReserva_Indisponibilidad_IndisponibilidadId",
                        column: x => x.IndisponibilidadId,
                        principalSchema: "mantenimiento",
                        principalTable: "Indisponibilidad",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Indisponibilidad_VehiculoId_FinReal",
                schema: "mantenimiento",
                table: "Indisponibilidad",
                columns: new[] { "VehiculoId", "FinReal" });

            migrationBuilder.CreateIndex(
                name: "IX_ReservaAfectada_IndisponibilidadId_MisionId",
                schema: "mantenimiento",
                table: "ReservaAfectada",
                columns: new[] { "IndisponibilidadId", "MisionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReservaAfectada_MisionId",
                schema: "mantenimiento",
                table: "ReservaAfectada",
                column: "MisionId");

            migrationBuilder.CreateIndex(
                name: "IX_ResolucionDeReserva_IndisponibilidadId_MisionId",
                schema: "mantenimiento",
                table: "ResolucionDeReserva",
                columns: new[] { "IndisponibilidadId", "MisionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservaAfectada",
                schema: "mantenimiento");

            migrationBuilder.DropTable(
                name: "ResolucionDeReserva",
                schema: "mantenimiento");

            migrationBuilder.DropTable(
                name: "Indisponibilidad",
                schema: "mantenimiento");
        }
    }
}
