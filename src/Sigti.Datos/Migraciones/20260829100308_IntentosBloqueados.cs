using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class IntentosBloqueados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntentoBloqueado",
                schema: "organizacion",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Quien = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Pretendia = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Expediente = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Par = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ChocaCon = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Referencia = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    MomentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Origen = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntentoBloqueado", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntentoBloqueado_Expediente",
                schema: "organizacion",
                table: "IntentoBloqueado",
                column: "Expediente");

            migrationBuilder.CreateIndex(
                name: "IX_IntentoBloqueado_Quien",
                schema: "organizacion",
                table: "IntentoBloqueado",
                column: "Quien");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntentoBloqueado",
                schema: "organizacion");
        }
    }
}
