using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M09_Combustible;

/// <summary>
/// Los nueve estados de la asignación de combustible — §10.1, artefacto autoridad.
///
/// ── Esto es la ASIGNACIÓN, no el fondo ──────────────────────────────────────
/// §10.1 lo separa expresamente: <i>«Esta máquina es la de la asignación —el vale o la
/// porción de fondo entregada a una misión. El fondo global del período tiene su propio
/// ciclo»</i>. Son dos objetos con dos vidas: el fondo es de <b>período</b>, la asignación es
/// de <b>misión</b>. Ver <see cref="EstadoDelFondo"/>.
/// </summary>
public enum EstadoDeAsignacion
{
    /// <summary>
    /// Tiene folio, monto o galonaje, misión vinculada y responsable. <b>Nadie la tiene en la
    /// mano</b>: no salió de la custodia de ACT-07.
    /// </summary>
    Emitida,

    /// <summary>El motorista firmó la recepción. <b>Hay dinero público fuera de la caja.</b></summary>
    Entregada,

    /// <summary>
    /// Hay al menos un consumo registrado. <b>Puede ser consumo parcial</b> — y por eso este
    /// estado no significa «se acabó», significa «ya se tocó».
    /// </summary>
    Consumida,

    /// <summary>Volvió íntegra y sin consumo, con acta firmada por quien entregó y quien devuelve.</summary>
    Devuelta,

    /// <summary>Declarada perdida con acta. `[C]` si la institución exige denuncia — insumo #1.</summary>
    Extraviada,

    /// <summary>Cuadran asignado, consumido, comprobado y saldo devuelto.</summary>
    Liquidada,

    /// <summary>Galones contra kilómetros contra rendimiento esperado, dentro de umbral.</summary>
    Conciliada,

    /// <summary>Fuera de umbral <b>en cualquier dirección</b>, con causa tipificada. Dispara `H-01`.</summary>
    ConciliadaConDesviacion,

    /// <summary>
    /// Anulada <b>antes</b> de entregar. El folio queda anulado y <b>no se recicla</b>: §9 lo
    /// dice de los folios de la misión y §10.1 lo repite de los vales.
    /// </summary>
    Anulada,
}

/// <summary>
/// Un asiento del diario de la asignación. Mismo principio que la misión (P-1): el estado es
/// la proyección del diario, nunca una columna que se pueda desincronizar.
/// </summary>
/// <param name="Id">El identificador de §10.1: `V-01` a `V-10`.</param>
/// <param name="Momento">
/// Se recibe, no se lee del reloj (`ADR-007`). Acá pesa el doble: `V-04` se ejecuta
/// <b>sin conectividad</b>, contra el reloj del dispositivo, y puede llegar días después.
/// </param>
/// <param name="Consumo">
/// Lo que este asiento consumió. <b>Sólo lo lleva `V-04`.</b> Va como dato y no dentro del
/// texto del motivo porque la liquidación lo vuelve a sumar: sacarlo de una cadena sería el
/// mismo error que tenía la reserva antes de `RecursosTomados`.
/// </param>
/// <param name="Devuelto">
/// Lo que este asiento devolvió al fondo. Lo llevan `V-05` —devolución íntegra— y `V-07`,
/// donde el saldo no consumido vuelve al cierre. <b>Sólo suma al saldo del fondo cuando la
/// devolución está constatada</b> (`RN-26`): una devolución declarada y no verificada no
/// libera nada.
/// </param>
public sealed record TransicionDeAsignacion(
    string Id,
    EstadoDeAsignacion Destino,
    IdPersona Ejecuta,
    DateTimeOffset Momento,
    string? Motivo,
    Ulid? IdDeCaptura = null,
    ConsumoRegistrado? Consumo = null,
    decimal? Devuelto = null);

/// <summary>
/// Un consumo concreto: la carga en la estación.
///
/// ── Los cinco datos, y por qué ninguno sobra ────────────────────────────────
/// §10.1 los exige juntos: <i>«galones, monto, estación, odómetro del momento y fotografía
/// del comprobante»</i>. El <b>odómetro del momento</b> es el que ancla el galón a un tramo
/// recorrido; sin él la conciliación de `RN-30` compara un total contra otro total y no puede
/// decir <b>dónde</b> se fue la diferencia.
/// </summary>
/// <param name="Comprobante">
/// La referencia de la factura o del cupón. <b>Nulo es un caso previsto, no un descuido</b>:
/// `RN-85` tipifica la ausencia de comprobante con causa y descargo alternativo, y el
/// principio es que <i>el registro del abastecimiento no se omite nunca por falta de papel</i>.
/// </param>
public sealed record ConsumoRegistrado(
    decimal Galones,
    decimal Monto,
    string Estacion,
    int Odometro,
    string? Comprobante = null);
