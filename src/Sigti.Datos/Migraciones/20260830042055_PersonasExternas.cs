using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class PersonasExternas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "personas");

            migrationBuilder.CreateTable(
                name: "CampoDelManifiesto",
                schema: "personas",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Clave = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Etiqueta = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Clase = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    BaseLegal = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NecesidadOperativa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FundamentaPersona = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    FundamentadoUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activa = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ActivadoUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampoDelManifiesto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConsultaAManifiesto",
                schema: "personas",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Consultante = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    MomentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegistroConsultado = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Alcance = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NecesidadDeConocer = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Origen = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultaAManifiesto", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampoDelManifiesto_Clave",
                schema: "personas",
                table: "CampoDelManifiesto",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsultaAManifiesto_Consultante_MomentoUtc",
                schema: "personas",
                table: "ConsultaAManifiesto",
                columns: new[] { "Consultante", "MomentoUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultaAManifiesto_RegistroConsultado_MomentoUtc",
                schema: "personas",
                table: "ConsultaAManifiesto",
                columns: new[] { "RegistroConsultado", "MomentoUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampoDelManifiesto",
                schema: "personas");

            migrationBuilder.DropTable(
                name: "ConsultaAManifiesto",
                schema: "personas");
        }
    }
}
