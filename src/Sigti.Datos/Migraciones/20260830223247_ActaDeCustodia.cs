using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class ActaDeCustodia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActaDeCustodia",
                schema: "flota",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    MisionId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    VehiculoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Entrega = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Recibe = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MomentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseMinutos = table.Column<int>(type: "int", nullable: false),
                    Odometro = table.Column<int>(type: "int", nullable: false),
                    NivelDeTanque = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    EstadoDeLaUnidad = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActaDeCustodia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ElementoDelActa",
                schema: "flota",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    ActaId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Presente = table.Column<bool>(type: "bit", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElementoDelActa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElementoDelActa_ActaDeCustodia_ActaId",
                        column: x => x.ActaId,
                        principalSchema: "flota",
                        principalTable: "ActaDeCustodia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActaDeCustodia_MisionId_Tipo",
                schema: "flota",
                table: "ActaDeCustodia",
                columns: new[] { "MisionId", "Tipo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElementoDelActa_ActaId",
                schema: "flota",
                table: "ElementoDelActa",
                column: "ActaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ElementoDelActa",
                schema: "flota");

            migrationBuilder.DropTable(
                name: "ActaDeCustodia",
                schema: "flota");
        }
    }
}
