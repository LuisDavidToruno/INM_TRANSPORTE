using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class BandejaDeTareas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TareaPendiente",
                schema: "organizacion",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Asunto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Detalle = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Expediente = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    QuienLaOrigino = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PuestoDestino = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PersonasDestino = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    MomentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NotificadoUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Resuelve = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ResueltaUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Resolucion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TareaPendiente", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TareaPendiente_Expediente",
                schema: "organizacion",
                table: "TareaPendiente",
                column: "Expediente");

            migrationBuilder.CreateIndex(
                name: "IX_TareaPendiente_PuestoDestino",
                schema: "organizacion",
                table: "TareaPendiente",
                column: "PuestoDestino");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TareaPendiente",
                schema: "organizacion");
        }
    }
}
