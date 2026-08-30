using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class RespaldoDePlaca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EstadoDePlaca",
                schema: "flota",
                table: "Vehiculo",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "RespaldoDePlaca",
                schema: "flota",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    VehiculoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Emisor = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Folio = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Adjunto = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    VigenteDesde = table.Column<DateOnly>(type: "date", nullable: false),
                    VigenteHasta = table.Column<DateOnly>(type: "date", nullable: true),
                    EstadoDePlaca = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Registra = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    RegistradoEnUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespaldoDePlaca", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RespaldoDePlaca_Vehiculo_VehiculoId",
                        column: x => x.VehiculoId,
                        principalSchema: "flota",
                        principalTable: "Vehiculo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RespaldoDePlaca_VehiculoId_VigenteDesde",
                schema: "flota",
                table: "RespaldoDePlaca",
                columns: new[] { "VehiculoId", "VigenteDesde" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RespaldoDePlaca",
                schema: "flota");

            migrationBuilder.DropColumn(
                name: "EstadoDePlaca",
                schema: "flota",
                table: "Vehiculo");
        }
    }
}
