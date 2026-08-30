using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class ManifiestoYNovedades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Manifiesto",
                schema: "personas",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    MisionId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    CerradoUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CierraQuien = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manifiesto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NovedadDeRuta",
                schema: "personas",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    ManifiestoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AQuien = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DondePaso = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FechaDelHechoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseMinutos = table.Column<int>(type: "int", nullable: false),
                    Registra = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Autoriza = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NovedadDeRuta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NovedadDeRuta_Manifiesto_ManifiestoId",
                        column: x => x.ManifiestoId,
                        principalSchema: "personas",
                        principalTable: "Manifiesto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonaEnManifiesto",
                schema: "personas",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    ManifiestoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Identificacion = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Forma = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    QueMotivaElTraslado = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Origen = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Destino = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    RequerimientoOperativo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonaEnManifiesto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonaEnManifiesto_Manifiesto_ManifiestoId",
                        column: x => x.ManifiestoId,
                        principalSchema: "personas",
                        principalTable: "Manifiesto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Manifiesto_MisionId",
                schema: "personas",
                table: "Manifiesto",
                column: "MisionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NovedadDeRuta_ManifiestoId",
                schema: "personas",
                table: "NovedadDeRuta",
                column: "ManifiestoId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonaEnManifiesto_Identificacion",
                schema: "personas",
                table: "PersonaEnManifiesto",
                column: "Identificacion",
                filter: "[Identificacion] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PersonaEnManifiesto_ManifiestoId",
                schema: "personas",
                table: "PersonaEnManifiesto",
                column: "ManifiestoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NovedadDeRuta",
                schema: "personas");

            migrationBuilder.DropTable(
                name: "PersonaEnManifiesto",
                schema: "personas");

            migrationBuilder.DropTable(
                name: "Manifiesto",
                schema: "personas");
        }
    }
}
