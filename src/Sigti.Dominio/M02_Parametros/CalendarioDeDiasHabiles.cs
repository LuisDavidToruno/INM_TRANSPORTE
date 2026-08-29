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
/// ── La hora: ahora sí se puede, y hace falta lo de los dos lados ─────────────
/// `BD-04` habla de día inhábil <b>u hora inhábil</b>. Para juzgarla hacen falta <b>dos</b>
/// datos: que la misión declare sus horas —<see cref="VentanaDeMision.HoraDeSalida"/>— y que
/// la institución declare su <see cref="HorarioHabil"/>. Falta cualquiera de los dos y la
/// hora <b>no se evalúa</b>, lo cual se dice en vez de fingirse.
///
/// `[C]` el horario hábil oficial — insumo #1. Por eso <see cref="Horario"/> es anulable.
/// </summary>
/// <param name="Horario">
/// Nulo mientras la institución no lo declare. <b>Nulo no es «todo el día es hábil»</b>: es
/// «no se sabe», y la diferencia importa porque de lo segundo no se deduce que una salida a
/// las cinco de la mañana no necesite permiso.
/// </param>
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
    IReadOnlySet<DateOnly> Feriados,
    HorarioHabil? Horario = null)
{
    public bool EsInhabil(DateOnly fecha) =>
        !DiasHabiles.Contains(fecha.DayOfWeek) || Feriados.Contains(fecha);

    /// <summary>
    /// El vencimiento de un plazo en <b>días hábiles</b> contado desde una fecha del hecho.
    ///
    /// ── Por qué hábiles y no corridos ────────────────────────────────────────
    /// `RN-86` cuenta el plazo de devolución del saldo <b>en días hábiles</b>, y no es un
    /// tecnicismo: la devolución es un acto presencial en horario de caja. Un plazo corrido
    /// vencería el sábado a un motorista que no tiene a quién entregarle el dinero, y el
    /// bloqueo de nueva asignación se dispararía contra alguien que no pudo cumplir.
    ///
    /// ── El día de partida no cuenta ──────────────────────────────────────────
    /// El motorista que retorna el jueves a las 8:40 de la noche no tuvo el jueves para
    /// devolver: la caja cerró a las 4:00. El plazo empieza a correr el <b>siguiente</b> día
    /// hábil, que es cómo se cuenta un plazo en el procedimiento administrativo. `[I]` — es
    /// práctica común y no articulado extraído; el articulado es del insumo #32.
    ///
    /// ⚠️ <b>Con la lista de feriados vacía este cálculo subdeclara el plazo</b>: dará por
    /// hábil el 15 de septiembre y vencerá antes de lo que corresponde. Es el mismo sesgo
    /// que ya tiene <see cref="EsInhabil"/>, y va en la dirección de bloquear de más — que
    /// acá es la dirección incómoda. Por eso el arqueo nombra siempre contra qué calendario
    /// se juzgó.
    /// </summary>
    public DateOnly SumarDiasHabiles(DateOnly desde, int dias)
    {
        if (dias < 0)
            throw new ArgumentOutOfRangeException(nameof(dias),
                "Un plazo negativo vencería antes de empezar.");

        var fecha = desde;

        // Con plazo cero, el vencimiento es el mismo día del hecho. No se corre al siguiente
        // hábil: cero significa «se devuelve en el acto», y adelantarlo un día lo negaría.
        for (var contados = 0; contados < dias;)
        {
            fecha = fecha.AddDays(1);
            if (!EsInhabil(fecha)) contados++;
        }

        return fecha;
    }

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

    /// <summary>
    /// Las horas de la ventana que caen fuera del horario hábil.
    ///
    /// ── Sólo los DOS extremos declarados, y es una decisión ──────────────────
    /// Se evalúan la hora de salida y la de retorno. <b>No las noches intermedias.</b>
    ///
    /// Una misión de cuatro días está fuera del horario todas sus madrugadas, y evaluarlas
    /// haría que <b>toda</b> misión de más de un día exigiera permiso — con lo cual la mitad
    /// del día de `BD-04` quedaría vacía de sentido y el salvoconducto dejaría de
    /// distinguir nada.
    ///
    /// Los extremos son además lo que el control real mira: el agente detiene al vehículo
    /// que sale a las cinco de la mañana o vuelve a las diez de la noche, no al que durmió
    /// en Danlí. Y son los dos únicos momentos que la misión declara.
    ///
    /// ⚠️ `[C]` <b>si la institución entiende otra cosa por «hora inhábil»</b> en una misión de
    /// varios días. La ficha de `BD-04` dice <i>«cualquier parte de la ventana»</i> y no aclara
    /// el pernocte. Acotarlo a los extremos es la lectura que deja el control con sentido;
    /// no es la única posible.
    /// </summary>
    public IReadOnlyList<string> HorasInhabilesEn(VentanaDeMision ventana)
    {
        // Sin horario declarado no se evalúa. Nulo es «no se sabe», no «todo es hábil».
        if (Horario is not { } horario || !ventana.DeclaraHoras) return [];

        var fuera = new List<string>();

        if (horario.EsInhabil(ventana.HoraDeSalida!.Value))
            fuera.Add($"salida {ventana.HoraDeSalida:HH\\:mm}");

        if (horario.EsInhabil(ventana.HoraDeRetorno!.Value))
            fuera.Add($"retorno {ventana.HoraDeRetorno:HH\\:mm}");

        return fuera;
    }
}

/// <summary>
/// El horario laboral de la institución — el otro insumo de la <i>hora</i> inhábil de `BD-04`.
///
/// ── Los dos extremos son inclusivos ─────────────────────────────────────────
/// Salir exactamente a la hora de apertura es salir en horario. Es la misma convención que
/// usa toda vigencia del sistema, y la alternativa —abrir a las 08:00:01— no la entiende
/// nadie que lea el mensaje del bloqueo.
///
/// ⚠️ <b>No cruza la medianoche.</b> Un horario de 22:00 a 06:00 no se puede expresar acá, y
/// no se finge que sí: una institución con turno nocturno necesita otra forma, y eso es una
/// decisión de producto y no un detalle de este tipo. `[C]` insumo #1.
/// </summary>
public sealed record HorarioHabil(TimeOnly Desde, TimeOnly Hasta)
{
    public bool EsInhabil(TimeOnly hora) => hora < Desde || hora > Hasta;
}
