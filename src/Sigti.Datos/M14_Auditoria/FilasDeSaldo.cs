using Sigti.Dominio.M14_Auditoria;

namespace Sigti.Datos.M14_Auditoria;

/// <summary>
/// El saldo de apertura, tal como se guarda — `RN-97` punto 1: <b>documento con folio</b>, que
/// se conserva junto al acta de cierre.
///
/// ── Se congela y no se rehace ───────────────────────────────────────────────
/// Es el inventario de un corte concreto. Rehacerlo con los datos de hoy haría que el documento
/// que se citó en el acta dejara de decir lo que decía — y entonces la serie histórica que
/// `RN-97` punto 5 manda reportar no compararía nada.
/// </summary>
public sealed class FilaDeSaldo
{
    public required Ulid Id { get; init; }

    /// <summary>Sin folio no se puede citar en el acta de cierre.</summary>
    public required string Folio { get; init; }

    /// <summary>A qué ejercicio abre. <b>Uno por ejercicio</b>: dos inventarios del mismo corte
    /// dejarían al acta sin poder decir cuál es.</summary>
    public required string Ejercicio { get; init; }

    public required DateOnly Corte { get; init; }

    public required string Persona { get; init; }

    public required string Puesto { get; init; }

    public required DateTime MomentoUtc { get; init; }

    public required int DesfaseMinutos { get; init; }

    /// <summary>
    /// El primero tras el despliegue. Se declara para que <b>no se compare contra los siguientes
    /// como si fueran la misma medición</b>.
    /// </summary>
    public required bool EsInicialDeImplantacion { get; init; }

    /// <summary>
    /// El motivo por el que se produjo con préstamos vencidos o interrupciones sin desenlace
    /// vivos. `RN-97` punto 4: <i>«hay que resolverlos o declararlos explícitamente»</i>.
    /// </summary>
    public string? DeclaracionDeBloqueantes { get; init; }

    /// <summary>
    /// Las fuentes que no se pudieron consultar, con su razón. <b>Va en el documento, no en una
    /// nota al pie</b>: un saldo que omite en silencio los préstamos vencidos es el abandono que
    /// la regla existe para impedir, con formato de reporte.
    /// </summary>
    public required string FuentesNoConsultadas { get; init; }

    public List<FilaDeRenglon> Renglones { get; } = [];
}

/// <summary>Un renglón del saldo — `RN-97` punto 2.</summary>
public sealed class FilaDeRenglon
{
    public required Ulid Id { get; init; }

    public required Ulid SaldoId { get; init; }

    public required TipoDeRenglon Tipo { get; init; }

    /// <summary>El folio o identificador con que se cita el pendiente.</summary>
    public required string Referencia { get; init; }

    public required string Descripcion { get; init; }

    /// <summary>
    /// La del hecho <b>original</b>. Se guarda y no se recalcula: la antigüedad no se reinicia
    /// con el cambio de ejercicio, ni siquiera por una corrección de dato posterior.
    /// </summary>
    public required DateOnly FechaDelHecho { get; init; }

    public required CausaDelRenglon Causa { get; init; }

    /// <summary>Nominado. <b>Un expediente sin responsable es un expediente muerto.</b></summary>
    public required string Responsable { get; init; }

    public required string Estado { get; init; }

    /// <summary>
    /// En cuántos saldos anteriores ya venía. Es lo que hace visible el arrastre — y lo que
    /// impide presentar el mismo pendiente como nuevo cada enero.
    /// </summary>
    public required int SaldosAnteriores { get; init; }

    public decimal? Monto { get; init; }

    /// <summary>
    /// Cuándo se resolvió durante el ejercicio — `RN-97` punto 6. Nula mientras sigue vivo.
    /// <b>No se borra al resolverse</b>: que estuvo en el saldo es parte de la serie.
    /// </summary>
    public DateOnly? ResueltoEn { get; set; }

    public string? ComoSeResolvio { get; set; }
}
