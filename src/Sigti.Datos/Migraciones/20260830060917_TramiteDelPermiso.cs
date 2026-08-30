using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class TramiteDelPermiso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "Vehiculo",
                schema: "mision",
                table: "PermisoDeCirculacion",
                type: "binary(16)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "binary(16)");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Motorista",
                schema: "mision",
                table: "PermisoDeCirculacion",
                type: "binary(16)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "binary(16)");

            migrationBuilder.AlterColumn<string>(
                name: "EmitidoPor",
                schema: "mision",
                table: "PermisoDeCirculacion",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                schema: "mision",
                table: "PermisoDeCirculacion",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FirmadoEnUtc",
                schema: "mision",
                table: "PermisoDeCirculacion",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Justificacion",
                schema: "mision",
                table: "PermisoDeCirculacion",
                type: "nvarchar(600)",
                maxLength: 600,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MotivoDelDesistimiento",
                schema: "mision",
                table: "PermisoDeCirculacion",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Solicita",
                schema: "mision",
                table: "PermisoDeCirculacion",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "SolicitadoEnUtc",
                schema: "mision",
                table: "PermisoDeCirculacion",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "TramosInhabiles",
                schema: "mision",
                table: "PermisoDeCirculacion",
                type: "nvarchar(600)",
                maxLength: 600,
                nullable: false,
                defaultValue: "");

            // ── Relleno de lo que ya existía ────────────────────────────────────
            //
            // Antes de esta migración la tabla SOLO podía contener permisos firmados: la
            // columna `EmitidoPor` era obligatoria y no había estado. Así que toda fila
            // preexistente es un FIRMADO, y quien lo solicitó no se registró nunca —
            // se marca como desconocido en vez de atribuírselo al firmante, que sería
            // inventar un dato de auditoría.
            //
            // En desarrollo la tabla está vacía porque nunca existió un escritor. Esto es
            // para las instalaciones que sí tengan filas.
            migrationBuilder.Sql(@"
                UPDATE mision.PermisoDeCirculacion
                SET Estado = 'Firmado',
                    Solicita = '(no registrado antes del trámite)',
                    Justificacion = '(no registrada antes del trámite)',
                    TramosInhabiles = ''
                WHERE Estado = '';");

            migrationBuilder.CreateIndex(
                name: "IX_PermisoDeCirculacion_Estado",
                schema: "mision",
                table: "PermisoDeCirculacion",
                column: "Estado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PermisoDeCirculacion_Estado",
                schema: "mision",
                table: "PermisoDeCirculacion");

            migrationBuilder.DropColumn(
                name: "Estado",
                schema: "mision",
                table: "PermisoDeCirculacion");

            migrationBuilder.DropColumn(
                name: "FirmadoEnUtc",
                schema: "mision",
                table: "PermisoDeCirculacion");

            migrationBuilder.DropColumn(
                name: "Justificacion",
                schema: "mision",
                table: "PermisoDeCirculacion");

            migrationBuilder.DropColumn(
                name: "MotivoDelDesistimiento",
                schema: "mision",
                table: "PermisoDeCirculacion");

            migrationBuilder.DropColumn(
                name: "Solicita",
                schema: "mision",
                table: "PermisoDeCirculacion");

            migrationBuilder.DropColumn(
                name: "SolicitadoEnUtc",
                schema: "mision",
                table: "PermisoDeCirculacion");

            migrationBuilder.DropColumn(
                name: "TramosInhabiles",
                schema: "mision",
                table: "PermisoDeCirculacion");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Vehiculo",
                schema: "mision",
                table: "PermisoDeCirculacion",
                type: "binary(16)",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "binary(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Motorista",
                schema: "mision",
                table: "PermisoDeCirculacion",
                type: "binary(16)",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "binary(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmitidoPor",
                schema: "mision",
                table: "PermisoDeCirculacion",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);
        }
    }
}
