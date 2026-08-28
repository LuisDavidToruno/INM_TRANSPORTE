using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigti.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AbastecimientosDeLosConsumosAnteriores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        // **Los consumos anteriores a `RN-83` no tenían abastecimiento**, y sin él sus galones
        // desaparecen del denominador de la conciliación: el dictamen decía «la misión no cargó
        // combustible» sobre una misión que sí cargó. Una afirmación falsa es peor que un hueco.
        //
        // El vehículo sale de la reserva de la misión —el último asiento que tomó recursos—,
        // que es de donde lo saca el resto del sistema. Los expedientes anteriores a
        // `RecursosTomados` no la tienen, y esos consumos **no se rellenan**: inventarles un
        // vehículo sería peor que dejarlos fuera, porque el galón quedaría cargado a un tanque
        // que quizá no fue el suyo.
        migrationBuilder.Sql(@"
            INSERT INTO combustible.Abastecimiento
                (Id, VehiculoId, MomentoUtc, DesfaseMinutos, Galones, Odometro, Fuente,
                 Registra, MisionId, AsignacionId, TransicionDelValeId, Monto, Estacion,
                 Comprobante, CausaSinComprobante, Excedido)
            SELECT
                t.Id,
                r.VehiculoTomado,
                t.MomentoUtc,
                t.DesfaseMinutos,
                t.ConsumoGalones,
                t.ConsumoOdometro,
                'FondoDeLaMision',
                t.Ejecuta,
                a.MisionId,
                t.AsignacionId,
                t.Id,
                t.ConsumoMonto,
                t.ConsumoEstacion,
                t.ConsumoComprobante,
                t.ConsumoCausaSinComprobante,
                0
            FROM combustible.TransicionDeAsignacion t
            INNER JOIN combustible.Asignacion a ON a.Id = t.AsignacionId
            OUTER APPLY (
                SELECT TOP 1 x.VehiculoTomado
                FROM mision.Transicion x
                WHERE x.ExpedienteId = a.MisionId AND x.VehiculoTomado IS NOT NULL
                ORDER BY x.Orden DESC
            ) r
            WHERE t.Transicion = 'V-04'
              AND t.ConsumoGalones IS NOT NULL
              AND t.ConsumoOdometro IS NOT NULL
              AND r.VehiculoTomado IS NOT NULL
              AND NOT EXISTS (
                  SELECT 1 FROM combustible.Abastecimiento b
                  WHERE b.TransicionDelValeId = t.Id
              );");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Se quita sólo lo que esta migración puso: los que apuntan a un asiento del vale.
            // Los registrados a mano por `RN-83` no son suyos y no se tocan.
            migrationBuilder.Sql(
                "DELETE FROM combustible.Abastecimiento WHERE TransicionDelValeId IS NOT NULL;");
        }
    }
}
