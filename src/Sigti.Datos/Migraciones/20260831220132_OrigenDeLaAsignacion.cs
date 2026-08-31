using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class OrigenDeLaAsignacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Origen",
                schema: "organizacion",
                table: "AsignacionDePuestoEspejo",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,

                // Provisional: lo corrige el UPDATE de abajo. Una columna obligatoria sobre una
                // tabla con filas necesita algo, y dejarlo asi seria peor — el enumerado no
                // tiene un valor vacio, y toda fila vieja fallaria al leerse.
                defaultValue: "Espejo");

            // ⚠️ **Las filas que ya estaban no son todas del mismo origen**, y la diferencia
            // decide si la sincronizacion nocturna las cierra.
            //
            // Los puestos funcionales de SIGTI llevan el prefijo `PUE-` desde la siembra de
            // desarrollo —`PUE-JEFE-TRANSPORTE`, `PUE-MAXIMA-AUTORIDAD`— y ARGOS no los conoce
            // ni los va a traer nunca: son `Propia`. Todo lo demas llego del padron.
            //
            // El prefijo es un criterio fragil y por eso vive aca y en ningun otro lado: es una
            // conversion de datos que corre una vez. De aca en adelante, cada fila declara su
            // origen al nacer.
            migrationBuilder.Sql(
                """
                UPDATE organizacion.AsignacionDePuestoEspejo
                SET Origen = 'Propia'
                WHERE Puesto LIKE 'PUE-%';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Origen",
                schema: "organizacion",
                table: "AsignacionDePuestoEspejo");
        }
    }
}
