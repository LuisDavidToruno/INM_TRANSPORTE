using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AbastecimientosDeCualquierFuente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Abastecimiento",
                schema: "combustible",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    VehiculoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    MomentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseMinutos = table.Column<int>(type: "int", nullable: false),
                    Galones = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Odometro = table.Column<int>(type: "int", nullable: false),
                    Fuente = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Registra = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MisionId = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    AsignacionId = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    TransicionDelValeId = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Estacion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Comprobante = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CausaSinComprobante = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Excedido = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Abastecimiento", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Abastecimiento_MisionId",
                schema: "combustible",
                table: "Abastecimiento",
                column: "MisionId",
                filter: "[MisionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Abastecimiento_TransicionDelValeId",
                schema: "combustible",
                table: "Abastecimiento",
                column: "TransicionDelValeId",
                unique: true,
                filter: "[TransicionDelValeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Abastecimiento_VehiculoId_MomentoUtc",
                schema: "combustible",
                table: "Abastecimiento",
                columns: new[] { "VehiculoId", "MomentoUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Abastecimiento",
                schema: "combustible");
        }
    }
}
