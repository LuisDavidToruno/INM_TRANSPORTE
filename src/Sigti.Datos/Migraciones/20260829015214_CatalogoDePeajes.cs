using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class CatalogoDePeajes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "peajes");

            migrationBuilder.CreateTable(
                name: "Categoria",
                schema: "peajes",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categoria", x => x.Codigo);
                });

            migrationBuilder.CreateTable(
                name: "Exoneracion",
                schema: "peajes",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    VehiculoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    PuntoId = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    Operador = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Fundamento = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    VigenteDesde = table.Column<DateOnly>(type: "date", nullable: false),
                    VigenteHasta = table.Column<DateOnly>(type: "date", nullable: true),
                    RegistradoDesdeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegistradoHastaUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exoneracion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PasoPorCaseta",
                schema: "peajes",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    PuntoId = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    VehiculoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    MisionId = table.Column<byte[]>(type: "binary(16)", nullable: true),
                    MomentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesfaseMinutos = table.Column<int>(type: "int", nullable: false),
                    Odometro = table.Column<int>(type: "int", nullable: false),
                    MontoPagado = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Medio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Registra = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CategoriaEsperada = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    CategoriaCobrada = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    MontoEsperado = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    Ticket = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CausaSinTicket = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PuntoNoCatalogado = table.Column<bool>(type: "bit", nullable: false),
                    UbicacionDeclarada = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IdDeCaptura = table.Column<byte[]>(type: "binary(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasoPorCaseta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Punto",
                schema: "peajes",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Operador = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Carretera = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SentidoDeCobro = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Punto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReglaDeCategoria",
                schema: "peajes",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Prioridad = table.Column<int>(type: "int", nullable: false),
                    Fundamento = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Clase = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    TipoDeVehiculo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    PesoBrutoDesdeKg = table.Column<int>(type: "int", nullable: true),
                    PesoBrutoHastaKg = table.Column<int>(type: "int", nullable: true),
                    EjesDesde = table.Column<int>(type: "int", nullable: true),
                    EjesHasta = table.Column<int>(type: "int", nullable: true),
                    PasajerosDesde = table.Column<int>(type: "int", nullable: true),
                    PasajerosHasta = table.Column<int>(type: "int", nullable: true),
                    LlevaRemolque = table.Column<bool>(type: "bit", nullable: true),
                    VigenteDesde = table.Column<DateOnly>(type: "date", nullable: false),
                    VigenteHasta = table.Column<DateOnly>(type: "date", nullable: true),
                    RegistradoDesdeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegistradoHastaUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglaDeCategoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tarifa",
                schema: "peajes",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    PuntoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Fuente = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    FechaDeVerificacion = table.Column<DateOnly>(type: "date", nullable: false),
                    VigenteDesde = table.Column<DateOnly>(type: "date", nullable: false),
                    VigenteHasta = table.Column<DateOnly>(type: "date", nullable: true),
                    RegistradoDesdeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegistradoHastaUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tarifa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VigenciaDelPunto",
                schema: "peajes",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    PuntoId = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Fundamento = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    VigenteDesde = table.Column<DateOnly>(type: "date", nullable: false),
                    VigenteHasta = table.Column<DateOnly>(type: "date", nullable: true),
                    RegistradoDesdeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegistradoHastaUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VigenciaDelPunto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VigenciaDelPunto_Punto_PuntoId",
                        column: x => x.PuntoId,
                        principalSchema: "peajes",
                        principalTable: "Punto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exoneracion_VehiculoId",
                schema: "peajes",
                table: "Exoneracion",
                column: "VehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_PasoPorCaseta_IdDeCaptura",
                schema: "peajes",
                table: "PasoPorCaseta",
                column: "IdDeCaptura",
                unique: true,
                filter: "[IdDeCaptura] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PasoPorCaseta_MisionId",
                schema: "peajes",
                table: "PasoPorCaseta",
                column: "MisionId");

            migrationBuilder.CreateIndex(
                name: "IX_PasoPorCaseta_VehiculoId",
                schema: "peajes",
                table: "PasoPorCaseta",
                column: "VehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_Punto_Operador",
                schema: "peajes",
                table: "Punto",
                column: "Operador");

            migrationBuilder.CreateIndex(
                name: "IX_ReglaDeCategoria_Prioridad",
                schema: "peajes",
                table: "ReglaDeCategoria",
                column: "Prioridad");

            migrationBuilder.CreateIndex(
                name: "IX_Tarifa_PuntoId_Categoria_VigenteDesde",
                schema: "peajes",
                table: "Tarifa",
                columns: new[] { "PuntoId", "Categoria", "VigenteDesde" });

            migrationBuilder.CreateIndex(
                name: "IX_VigenciaDelPunto_PuntoId_VigenteDesde",
                schema: "peajes",
                table: "VigenciaDelPunto",
                columns: new[] { "PuntoId", "VigenteDesde" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categoria",
                schema: "peajes");

            migrationBuilder.DropTable(
                name: "Exoneracion",
                schema: "peajes");

            migrationBuilder.DropTable(
                name: "PasoPorCaseta",
                schema: "peajes");

            migrationBuilder.DropTable(
                name: "ReglaDeCategoria",
                schema: "peajes");

            migrationBuilder.DropTable(
                name: "Tarifa",
                schema: "peajes");

            migrationBuilder.DropTable(
                name: "VigenciaDelPunto",
                schema: "peajes");

            migrationBuilder.DropTable(
                name: "Punto",
                schema: "peajes");
        }
    }
}
