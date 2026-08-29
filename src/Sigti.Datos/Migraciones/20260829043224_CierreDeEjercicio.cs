using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class CierreDeEjercicio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActaDeCierreDeEjercicio",
                schema: "auditoria",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Folio = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Ejercicio = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    CorteLegal = table.Column<DateOnly>(type: "date", nullable: false),
                    CorteOperativo = table.Column<DateOnly>(type: "date", nullable: false),
                    Persona = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Puesto = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MomentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseMinutos = table.Column<int>(type: "int", nullable: false),
                    SaldoDeAperturaFolio = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    DiferenciasConElSaldo = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActaDeCierreDeEjercicio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FolioDelActaDeCierre",
                schema: "auditoria",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    ActaId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    AsignacionId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Folio = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Delegacion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Emitido = table.Column<DateOnly>(type: "date", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SePuedeAnular = table.Column<bool>(type: "bit", nullable: false),
                    AnuladoUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AnuladoPor = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolioDelActaDeCierre", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FolioDelActaDeCierre_ActaDeCierreDeEjercicio_ActaId",
                        column: x => x.ActaId,
                        principalSchema: "auditoria",
                        principalTable: "ActaDeCierreDeEjercicio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActaDeCierreDeEjercicio_Ejercicio",
                schema: "auditoria",
                table: "ActaDeCierreDeEjercicio",
                column: "Ejercicio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActaDeCierreDeEjercicio_Folio",
                schema: "auditoria",
                table: "ActaDeCierreDeEjercicio",
                column: "Folio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FolioDelActaDeCierre_ActaId_AsignacionId",
                schema: "auditoria",
                table: "FolioDelActaDeCierre",
                columns: new[] { "ActaId", "AsignacionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FolioDelActaDeCierre",
                schema: "auditoria");

            migrationBuilder.DropTable(
                name: "ActaDeCierreDeEjercicio",
                schema: "auditoria");
        }
    }
}
