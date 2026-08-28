using Sigti.Dominio.M03_Flota;

namespace Sigti.Dominio.M02_Parametros;

/// <summary>
/// Qué días son hábiles para esta institución — el insumo de `BD-04`.
///
/// ── Nada de esto se cablea ───────────────────────────────────────────────────
/// Premisa 6: los feriados y el horario hábil son <b>parámetros con vigencia por rango de
/// fechas</b>, y todo cálculo usa la tabla vigente <b>a la fecha del hecho</b>. La máquina de
/// estados lo repite sin margen: <i>«Nunca se cablean los feriados. El Art. 339 del Código
/// del Trabajo fija los nacionales, pero existe legislación posterior sobre los feriados de
/// octubre que no se pudo verificar»</i> — insumo #14, `[C]`.
///
/// Por eso los feriados <b>se reciben</b>, y por eso una lista vacía es un estado posible y
/// no un error: significa que la institución todavía no cargó su calendario.
///
/// ── La mitad que este calendario NO puede juzgar ─────────────────────────────
/// `BD-04` habla de día inhábil <b>u hora inhábil</b>. La hora <b>no se evalúa</b>, y no por
/// omisión: <see cref="VentanaDeMision"/> lleva <c>DateOnly</c>, sin horas. Una misión no
/// declara a qué hora sale, así que no hay contra qué contrastar un horario hábil. Fingir la
/// evaluación sería peor que declararla ausente. `[C]` el horario hábil oficial — insumo #1.
/// </summary>
/// <param name="Version">
/// Qué calendario se usó. Va al diario: dentro de dos años, reconstruir por qué un domingo
/// no exigió permiso requiere saber contra qué calendario se juzgó.
/// </param>
/// <param name="DiasHabiles">
/// Los días de la semana laborables. Se reciben porque son decisión institucional: una
/// delegación con turnos de fin de semana no tiene el mismo calendario que una oficina.
/// </param>
/// <param name="Feriados">
/// Las fechas concretas. <b>Vacía es un estado válido</b> — la institución no cargó su
/// calendario— y significa que este calendario <b>subdeclara</b>: dirá que el 15 de
/// septiembre es hábil. Es la dirección segura del error frente a la alternativa, que sería
/// inventar el articulado.
/// </param>
public sealed record CalendarioDeDiasHabiles(
    string Version,
    IReadOnlySet<DayOfWeek> DiasHabiles,
    IReadOnlySet<DateOnly> Feriados)
{
    public bool EsInhabil(DateOnly fecha) =>
        !DiasHabiles.Contains(fecha.DayOfWeek) || Feriados.Contains(fecha);

    /// <summary>
    /// Los días inhábiles que toca la ventana, <b>holgura incluida</b>.
    ///
    /// Incluye la holgura porque el vehículo sigue afuera durante ella: `BD-04` habla de
    /// <i>«cualquier parte de la ventana de la misión»</i>, y la parte que se pasa del retorno
    /// previsto es exactamente la que se recorre sin que nadie la haya previsto.
    /// </summary>
    public IReadOnlyList<DateOnly> InhabilesEn(VentanaDeMision ventana)
    {
        var dias = new List<DateOnly>();

        for (var d = ventana.Salida; d <= ventana.FinDelRango; d = d.AddDays(1))
            if (EsInhabil(d)) dias.Add(d);

        return dias;
    }
}
