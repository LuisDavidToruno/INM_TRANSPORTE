using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class EscalamientoYJerarquiaDePuestos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EscalaA",
                schema: "organizacion",
                table: "IntentoBloqueado",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PorQueNoAntes",
                schema: "organizacion",
                table: "IntentoBloqueado",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Salto",
                schema: "organizacion",
                table: "IntentoBloqueado",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PuestoEspejo",
                schema: "organizacion",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Puesto = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Denominacion = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Superior = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Delegacion = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    ConfirmadoAlUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuestoEspejo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RespaldoDeSede",
                schema: "organizacion",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Delegacion = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Puesto = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Designa = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Desde = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespaldoDeSede", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PuestoEspejo_Puesto",
                schema: "organizacion",
                table: "PuestoEspejo",
                column: "Puesto",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RespaldoDeSede_Delegacion",
                schema: "organizacion",
                table: "RespaldoDeSede",
                column: "Delegacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PuestoEspejo",
                schema: "organizacion");

            migrationBuilder.DropTable(
                name: "RespaldoDeSede",
                schema: "organizacion");

            migrationBuilder.DropColumn(
                name: "EscalaA",
                schema: "organizacion",
                table: "IntentoBloqueado");

            migrationBuilder.DropColumn(
                name: "PorQueNoAntes",
                schema: "organizacion",
                table: "IntentoBloqueado");

            migrationBuilder.DropColumn(
                name: "Salto",
                schema: "organizacion",
                table: "IntentoBloqueado");
        }
    }
}
