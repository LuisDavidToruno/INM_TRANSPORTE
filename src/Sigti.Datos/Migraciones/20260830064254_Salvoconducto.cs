using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class Salvoconducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Salvoconducto",
                schema: "mision",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    PermisoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    ExpedienteId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Folio = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FolioNumero = table.Column<int>(type: "int", nullable: true),
                    FolioRangoId = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    Huella = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CodigoCorto = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    FolioDelPermiso = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Vehiculo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Motorista = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Destino = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Desde = table.Column<DateOnly>(type: "date", nullable: false),
                    Hasta = table.Column<DateOnly>(type: "date", nullable: false),
                    TramosInhabiles = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    Justificacion = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    FirmadoPor = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FirmadoEnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmitidoPor = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    EmitidoEnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Anulado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salvoconducto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImpresionDeSalvoconducto",
                schema: "mision",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    SalvoconductoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Quien = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MomentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImpresionDeSalvoconducto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImpresionDeSalvoconducto_Salvoconducto_SalvoconductoId",
                        column: x => x.SalvoconductoId,
                        principalSchema: "mision",
                        principalTable: "Salvoconducto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImpresionDeSalvoconducto_SalvoconductoId_Orden",
                schema: "mision",
                table: "ImpresionDeSalvoconducto",
                columns: new[] { "SalvoconductoId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Salvoconducto_CodigoCorto",
                schema: "mision",
                table: "Salvoconducto",
                column: "CodigoCorto");

            migrationBuilder.CreateIndex(
                name: "IX_Salvoconducto_Folio",
                schema: "mision",
                table: "Salvoconducto",
                column: "Folio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Salvoconducto_PermisoId",
                schema: "mision",
                table: "Salvoconducto",
                column: "PermisoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImpresionDeSalvoconducto",
                schema: "mision");

            migrationBuilder.DropTable(
                name: "Salvoconducto",
                schema: "mision");
        }
    }
}
