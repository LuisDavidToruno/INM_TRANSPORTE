using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class FlotaVehicular : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "flota");

            migrationBuilder.CreateTable(
                name: "Vehiculo",
                schema: "flota",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Siglas = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Placa = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    TieneConstanciaSustitutaDePlaca = table.Column<bool>(type: "bit", nullable: false),
                    TipoDeVehiculo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Clase = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    PesoBrutoKg = table.Column<int>(type: "int", nullable: false),
                    CapacidadPasajeros = table.Column<int>(type: "int", nullable: false),
                    LlevaRemolque = table.Column<bool>(type: "bit", nullable: false),
                    VenceMatricula = table.Column<DateOnly>(type: "date", nullable: false),
                    VencePoliza = table.Column<DateOnly>(type: "date", nullable: true),
                    VenceRevisionMecanica = table.Column<DateOnly>(type: "date", nullable: true),
                    IdentificacionInstitucionalVerificada = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehiculo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculo_Siglas",
                schema: "flota",
                table: "Vehiculo",
                column: "Siglas",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculo_VenceMatricula",
                schema: "flota",
                table: "Vehiculo",
                column: "VenceMatricula");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Vehiculo",
                schema: "flota");
        }
    }
}
