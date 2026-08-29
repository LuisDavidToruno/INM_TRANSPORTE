using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class SaldoDeAperturaDeControlInterno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SaldoDeApertura",
                schema: "auditoria",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Folio = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Ejercicio = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Corte = table.Column<DateOnly>(type: "date", nullable: false),
                    Persona = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Puesto = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MomentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseMinutos = table.Column<int>(type: "int", nullable: false),
                    EsInicialDeImplantacion = table.Column<bool>(type: "bit", nullable: false),
                    DeclaracionDeBloqueantes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FuentesNoConsultadas = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaldoDeApertura", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RenglonDelSaldo",
                schema: "auditoria",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    SaldoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Referencia = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaDelHecho = table.Column<DateOnly>(type: "date", nullable: false),
                    Causa = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Responsable = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SaldosAnteriores = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ResueltoEn = table.Column<DateOnly>(type: "date", nullable: true),
                    ComoSeResolvio = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RenglonDelSaldo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RenglonDelSaldo_SaldoDeApertura_SaldoId",
                        column: x => x.SaldoId,
                        principalSchema: "auditoria",
                        principalTable: "SaldoDeApertura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RenglonDelSaldo_SaldoId_Tipo_Referencia",
                schema: "auditoria",
                table: "RenglonDelSaldo",
                columns: new[] { "SaldoId", "Tipo", "Referencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RenglonDelSaldo_Tipo_Referencia",
                schema: "auditoria",
                table: "RenglonDelSaldo",
                columns: new[] { "Tipo", "Referencia" });

            migrationBuilder.CreateIndex(
                name: "IX_SaldoDeApertura_Ejercicio",
                schema: "auditoria",
                table: "SaldoDeApertura",
                column: "Ejercicio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaldoDeApertura_Folio",
                schema: "auditoria",
                table: "SaldoDeApertura",
                column: "Folio",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RenglonDelSaldo",
                schema: "auditoria");

            migrationBuilder.DropTable(
                name: "SaldoDeApertura",
                schema: "auditoria");
        }
    }
}
