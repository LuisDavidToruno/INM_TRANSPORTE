using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class ColaDeConflictos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sincronizacion");

            migrationBuilder.CreateTable(
                name: "Conflicto",
                schema: "sincronizacion",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    ExpedienteId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Transicion = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Campo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    IdDeCaptura = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    ValorDelServidor = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    CapturadaPorServidor = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OcurrioServidorUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegistradoServidorUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DispositivoDelServidor = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    FotoDelServidor = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    ValorDeCampo = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    CapturadaPorCampo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OcurrioCampoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegistradoCampoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DispositivoDeCampo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    FotoDeCampo = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    SeTomo = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Resuelve = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ResueltoUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CriterioDelLote = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conflicto", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Conflicto_ExpedienteId_Estado",
                schema: "sincronizacion",
                table: "Conflicto",
                columns: new[] { "ExpedienteId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Conflicto_IdDeCaptura_Campo",
                schema: "sincronizacion",
                table: "Conflicto",
                columns: new[] { "IdDeCaptura", "Campo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Conflicto",
                schema: "sincronizacion");
        }
    }
}
