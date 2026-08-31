using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AcuseDeEntrega : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AcuseDeEntrega",
                schema: "mision",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    MisionId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Documento = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Folio = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Entrega = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Recibe = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MomentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseMinutos = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcuseDeEntrega", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcuseDeEntrega_MisionId_Documento",
                schema: "mision",
                table: "AcuseDeEntrega",
                columns: new[] { "MisionId", "Documento" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcuseDeEntrega",
                schema: "mision");
        }
    }
}
