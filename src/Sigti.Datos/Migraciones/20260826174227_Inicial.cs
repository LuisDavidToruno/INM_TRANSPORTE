using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "bitacora");

            migrationBuilder.EnsureSchema(
                name: "mision");

            migrationBuilder.CreateTable(
                name: "Asiento",
                schema: "bitacora",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Cola = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Secuencia = table.Column<long>(type: "bigint", nullable: false),
                    Contenido = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Hash = table.Column<string>(type: "nchar(64)", fixedLength: true, maxLength: 64, nullable: false),
                    MomentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseMinutos = table.Column<int>(type: "int", nullable: false),
                    MomentoRecibidoUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asiento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Expediente",
                schema: "mision",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    CapturadaPor = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SolicitanteDeDerecho = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expediente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transicion",
                schema: "mision",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    ExpedienteId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Transicion = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Destino = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Ejecuta = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MomentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseMinutos = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transicion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transicion_Expediente_ExpedienteId",
                        column: x => x.ExpedienteId,
                        principalSchema: "mision",
                        principalTable: "Expediente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asiento_Cola_Secuencia",
                schema: "bitacora",
                table: "Asiento",
                columns: new[] { "Cola", "Secuencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transicion_ExpedienteId_Orden",
                schema: "mision",
                table: "Transicion",
                columns: new[] { "ExpedienteId", "Orden" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Asiento",
                schema: "bitacora");

            migrationBuilder.DropTable(
                name: "Transicion",
                schema: "mision");

            migrationBuilder.DropTable(
                name: "Expediente",
                schema: "mision");
        }
    }
}
