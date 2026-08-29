using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Dominio.M14_Auditoria;

/// <summary>
/// Qué clase de cosa quedó viva al corte — `RN-97` enunciado.
///
/// ── La lista es la de la regla, entera ──────────────────────────────────────
/// <i>«Órdenes de Misión sin cerrar, interrupciones sin desenlace, préstamos vencidos,
/// obligaciones de reintegro, expedientes de M-12, reclamos de peaje sin resolver, imputaciones
/// externas no resueltas, misiones con bitácora pendiente de digitación»</i>.
///
/// <b>Están todas, incluidas las que el sistema todavía no puede contar.</b> Quitar de este
/// enum lo que no se puede consultar haría que el saldo se viera completo estando incompleto —
/// que es exactamente el abandono silencioso que la regla existe para impedir.
/// </summary>
public enum TipoDeRenglon
{
    /// <summary>La orden de misión que no llegó a un estado terminal.</summary>
    MisionSinCerrar,

    /// <summary>El vale entregado que no se liquidó ni se anuló.</summary>
    ValeSinLiquidar,

    /// <summary>`RN-86` — el dinero que una persona debe y que sobrevive al cierre de la misión.</summary>
    ObligacionDeReintegro,

    /// <summary>`RN-93` — el expediente descubierto después del cierre y todavía sin resolver.</summary>
    HallazgoPosteriorAbierto,

    /// <summary>`RN-95` — la diferencia contra el proveedor que nadie resolvió.</summary>
    ImputacionExternaNoResuelta,

    /// <summary>
    /// `RN-63` — el vehículo prestado que no volvió en plazo. <b>`RN-97` punto 4 le da poder de
    /// bloqueo</b>: ningún período se cierra con préstamos vencidos.
    /// </summary>
    PrestamoVencido,

    /// <summary>
    /// `RN-70` — la misión interrumpida en ruta sin desenlace registrado. <b>También bloquea.</b>
    /// </summary>
    InterrupcionSinDesenlace,

    /// <summary>`RN-92` — el reclamo por discrepancia de peaje sin resolver.</summary>
    ReclamoDePeaje,

    /// <summary>El expediente de incidente, siniestro o sanción abierto (M-12).</summary>
    ExpedienteDeIncidente,

    /// <summary>La misión cuya bitácora de campo nunca se digitó.</summary>
    BitacoraPendienteDeDigitacion,
}

/// <summary>
/// Por qué sigue vivo — `RN-97` punto 2 exige causa tipificada.
///
/// <b>Tipificada y no libre.</b> Una causa en texto libre no se agrupa, y un saldo de apertura
/// que no se puede agrupar por causa no dice nada sobre dónde está el problema.
/// </summary>
public enum CausaDelRenglon
{
    /// <summary>Depende de la institución y nadie lo ha movido.</summary>
    PendienteDeGestionInterna,

    /// <summary>
    /// <b>Fuera del control institucional</b> — un proceso judicial, una resolución de otra
    /// entidad. `RN-97` casos límite: <i>«que no dependa de nosotros no lo hace inexistente»</i>,
    /// y su antigüedad sigue corriendo igual.
    /// </summary>
    FueraDelControlInstitucional,

    /// <summary>
    /// El bien sustraído o retenido sin recuperar. Permanece hasta su recuperación o su descargo
    /// formal (`RN-75`).
    /// </summary>
    BienNoRecuperado,

    /// <summary>
    /// El arrastre del primer corte tras el despliegue. `RN-97`: <i>«es esperable: es la primera
    /// vez que la institución ve todo junto»</i>.
    /// </summary>
    SaldoInicialDeImplantacion,
}

/// <summary>
/// Un renglón del saldo — `RN-97` punto 2.
/// </summary>
/// <param name="FechaDelHecho">
/// La del hecho <b>original</b>. `RN-97` punto 3: la antigüedad <b>no se reinicia con el cambio
/// de ejercicio</b> — <i>«un expediente que llega al tercer ejercicio con 800 días de antigüedad
/// no se puede presentar como pendiente reciente»</i>.
/// </param>
/// <param name="Responsable">
/// Nominado, obligatorio. `RN-97` casos límite: si la persona ya no trabaja en la institución
/// <b>no se borra ni se deja sin responsable</b>: se reasigna a la jefatura, con constancia.
/// <i>«Un expediente sin responsable es un expediente muerto»</i>.
/// </param>
/// <param name="SaldosAnteriores">
/// En cuántos saldos de apertura anteriores ya venía. <b>Un renglón que aparece en tres
/// consecutivos es visible como tal</b> (`RN-97` punto 3) — y eso es lo que impide presentarlo
/// como nuevo cada enero.
/// </param>
public sealed record RenglonDelSaldo(
    TipoDeRenglon Tipo,
    string Referencia,
    string Descripcion,
    DateOnly FechaDelHecho,
    CausaDelRenglon Causa,
    string Responsable,
    string Estado,
    int SaldosAnteriores = 0,
    decimal? Monto = null)
{
    /// <summary>
    /// Los días desde el hecho, <b>no desde el corte</b>. Es la parte incómoda de la regla, y por
    /// eso mismo la que sirve.
    /// </summary>
    public int AntiguedadEnDias(DateOnly corte) => corte.DayNumber - FechaDelHecho.DayNumber;

    /// <summary>
    /// <b>Bloquea el cierre del período</b> — `RN-97` punto 4: ningún período se cierra con
    /// préstamos vencidos ni con interrupciones sin desenlace.
    /// </summary>
    public bool ImpideCerrarElPeriodo =>
        Tipo is TipoDeRenglon.PrestamoVencido or TipoDeRenglon.InterrupcionSinDesenlace;
}

/// <summary>
/// Una fuente del saldo y si se pudo consultar.
///
/// ── Por qué esto va en el documento y no en una nota al pie ─────────────────
/// Porque un saldo de apertura que omite en silencio los préstamos vencidos <b>es el abandono
/// que la regla existe para impedir, con formato de reporte</b>. `RN-97` describe el mecanismo:
/// <i>«llega enero, el sistema arranca con reportes en cero, y una misión interrumpida en
/// noviembre... simplemente deja de aparecer en ninguna pantalla. Nadie decidió abandonarlos: se
/// abandonaron solos»</i>.
///
/// Declarar la fuente como no consultable no la resuelve. Lo que hace es que su ausencia sea un
/// hecho declarado en vez de un vacío.
/// </summary>
public sealed record FuenteDelSaldo(
    TipoDeRenglon Tipo,
    bool SePudoConsultar,
    int Renglones,
    string? PorQueNo = null);

/// <summary>
/// El saldo de apertura de control interno — `RN-97`.
///
/// ── La regla que impide el abandono ─────────────────────────────────────────
/// `RN-97`: <i>«sin saldo de apertura, el mecanismo de olvido es automático y no requiere mala
/// fe»</i>. `RN-08` resuelve el expediente individual; <b>nada resolvía el inventario de lo que
/// queda vivo al cambiar de año</b>.
/// </summary>
/// <param name="Folio">
/// `RN-97` punto 1 — se produce como <b>documento con folio</b>, junto al acta de cierre, y
/// ambos se conservan. Un inventario sin folio no se puede citar en el acta.
/// </param>
/// <param name="EsInicialDeImplantacion">
/// El primero tras el despliegue. `RN-97`: <i>«es esperable: es la primera vez que la institución
/// ve todo junto»</i>. Se declara para que <b>no se compare contra los siguientes como si fueran
/// la misma medición</b>.
/// </param>
public sealed record SaldoDeApertura(
    Ulid Id,
    string Folio,
    string Ejercicio,
    DateOnly Corte,
    IReadOnlyList<RenglonDelSaldo> Renglones,
    IReadOnlyList<FuenteDelSaldo> Fuentes,
    Autoria Produce,
    DateTimeOffset Momento,
    bool EsInicialDeImplantacion = false)
{
    /// <summary>
    /// Lo que impide cerrar el período. `RN-97` punto 4: <i>«se listan con responsable y plazo, y
    /// hay que resolverlos o declararlos explícitamente»</i>.
    /// </summary>
    public IReadOnlyList<RenglonDelSaldo> Bloqueantes =>
        [.. Renglones.Where(r => r.ImpideCerrarElPeriodo)];

    /// <summary>
    /// Las fuentes que no se pudieron consultar. <b>Va al documento</b>: sin esto, el saldo se ve
    /// completo estando incompleto.
    /// </summary>
    public IReadOnlyList<FuenteDelSaldo> SinConsultar =>
        [.. Fuentes.Where(f => !f.SePudoConsultar)];

    public bool EsCompleto => SinConsultar.Count == 0;

    /// <summary>
    /// Los que ya venían de saldos anteriores. <b>Son los que más importan</b>: el arrastre es
    /// justamente lo que la regla existe para hacer visible.
    /// </summary>
    public IReadOnlyList<RenglonDelSaldo> Arrastrados =>
        [.. Renglones.Where(r => r.SaldosAnteriores > 0)];

    /// <summary>El renglón más viejo. Es la cifra que un auditor mira primero.</summary>
    public int AntiguedadMaximaEnDias =>
        Renglones.Count == 0 ? 0 : Renglones.Max(r => r.AntiguedadEnDias(Corte));

    public decimal MontoTotal => Renglones.Sum(r => r.Monto ?? 0m);
}

/// <summary>
/// Los controles del saldo de apertura — `RN-97`.
/// </summary>
public static class ReglasDelSaldoDeApertura
{
    /// <summary>
    /// `RN-97` punto 2 — todo renglón lleva <b>responsable nominado y causa tipificada</b>.
    ///
    /// El responsable no es formalidad: <i>«un expediente sin responsable es un expediente
    /// muerto»</i>. Y si la persona ya no está, el renglón <b>no se borra ni se deja huérfano</b>:
    /// se reasigna a la jefatura con constancia del motivo.
    /// </summary>
    public static void ExigirResponsable(TipoDeRenglon tipo, string referencia, string responsable)
    {
        if (!string.IsNullOrWhiteSpace(responsable)) return;

        throw new BloqueoDuro("RN-97",
            $"El renglón {tipo} «{referencia}» no tiene responsable nominado. Un expediente sin " +
            "responsable es un expediente muerto: si quien lo tenía ya no está en la " +
            "institución, se reasigna a la jefatura que corresponde con constancia del motivo — " +
            "no se deja huérfano ni se borra.");
    }

    /// <summary>
    /// `RN-97` punto 4 — <b>ningún período se cierra con préstamos vencidos ni con interrupciones
    /// sin desenlace.</b>
    ///
    /// ── Y se pueden declarar, que no es lo mismo que ignorarlos ─────────────
    /// La regla dice <i>«hay que resolverlos o declararlos explícitamente»</i>. Declararlos es un
    /// acto con autor y motivo que queda en el documento; ignorarlos no es una opción, y por eso
    /// esto bloquea en vez de advertir.
    /// </summary>
    /// <param name="declaracionExplicita">
    /// El motivo por el que se cierra el período con ellos vivos. <b>Nulo es no declarado</b>, y
    /// entonces el cierre no procede.
    /// </param>
    /// <param name="renglones">
    /// <b>Todos</b>, no sólo los que bloquean: <b>el filtro lo hace esta función</b>. Recibir la
    /// lista ya filtrada dejaría que quien llama se olvidara de filtrar —o filtrara de más— y el
    /// bloqueo que impide cerrar con préstamos vencidos dependería de que el próximo endpoint se
    /// acordara. Salió al escribir la prueba: la primera versión confiaba en quien llamaba.
    /// </param>
    public static void ExigirCierrePosible(
        IReadOnlyList<RenglonDelSaldo> renglones, string? declaracionExplicita)
    {
        var bloqueantes = renglones.Where(r => r.ImpideCerrarElPeriodo).ToList();

        if (bloqueantes.Count == 0) return;
        if (!string.IsNullOrWhiteSpace(declaracionExplicita)) return;

        var detalle = string.Join("; ", bloqueantes.Select(r =>
            $"{r.Tipo} «{r.Referencia}» a cargo de {r.Responsable}"));

        throw new BloqueoDuro("RN-97",
            $"El período no se cierra con {bloqueantes.Count} renglón(es) que lo impiden: " +
            $"{detalle}. Ningún período se cierra con préstamos vencidos ni con interrupciones " +
            "sin desenlace: hay que resolverlos, o declararlo explícitamente con motivo — y esa " +
            "declaración queda en el documento.");
    }

    /// <summary>
    /// `RN-97` punto 1 — el saldo es un <b>documento con folio</b>.
    ///
    /// Sin folio no se puede citar en el acta de cierre, y un inventario que no se puede citar es
    /// un inventario que no existió.
    /// </summary>
    public static void ExigirFolioYEjercicio(string folio, string ejercicio)
    {
        if (string.IsNullOrWhiteSpace(folio))
            throw new BloqueoDuro("RN-97",
                "El saldo de apertura es un documento con folio, que se conserva junto al acta " +
                "de cierre. Sin folio no se puede citar en el acta — y un inventario que no se " +
                "puede citar es un inventario que no existió.");

        if (string.IsNullOrWhiteSpace(ejercicio))
            throw new BloqueoDuro("RN-97",
                "El saldo exige a qué ejercicio abre. Sin él la serie histórica no se puede " +
                "ordenar, y el arrastre entre ejercicios deja de verse.");
    }

    /// <summary>
    /// Arrastra la antigüedad del saldo anterior — `RN-97` punto 3.
    ///
    /// ── Lo que esto impide ──────────────────────────────────────────────────
    /// Que un renglón se presente como nuevo cada enero. <b>La antigüedad no se reinicia</b>: se
    /// cuenta siempre desde el hecho original, y el contador de saldos anteriores hace visible
    /// que ya venía.
    ///
    /// Se casa por <b>tipo y referencia</b>, que es lo que identifica al mismo pendiente entre
    /// dos cortes.
    /// </summary>
    public static IReadOnlyList<RenglonDelSaldo> ArrastrarDesde(
        IReadOnlyList<RenglonDelSaldo> nuevos, IReadOnlyList<RenglonDelSaldo> anteriores)
    {
        var previos = anteriores.ToDictionary(
            r => (r.Tipo, r.Referencia), r => r, ComparadorDeRenglon.Instancia);

        return
        [
            .. nuevos.Select(r =>
                previos.TryGetValue((r.Tipo, r.Referencia), out var antes)
                    ? r with
                    {
                        SaldosAnteriores = antes.SaldosAnteriores + 1,

                        // **La fecha del hecho la manda el saldo anterior.** Si la fuente hoy
                        // reporta otra, la que vale es la que se registró la primera vez: la
                        // antigüedad no se reinicia, ni siquiera por una corrección de dato.
                        FechaDelHecho = antes.FechaDelHecho,
                    }
                    : r),
        ];
    }

    /// <summary>
    /// `RN-97` — el saldo <b>coincide renglón por renglón con el inventario al corte</b>.
    ///
    /// Se comprueba porque el saldo se congela y el inventario sigue vivo: verificar meses
    /// después que lo congelado era lo que había es la única forma de saber que nadie lo editó.
    /// </summary>
    public static IReadOnlyList<string> DiferenciasContraElInventario(
        IReadOnlyList<RenglonDelSaldo> saldo, IReadOnlyList<RenglonDelSaldo> inventario)
    {
        var enElSaldo = saldo.Select(r => (r.Tipo, r.Referencia)).ToHashSet();
        var enElInventario = inventario.Select(r => (r.Tipo, r.Referencia)).ToHashSet();

        var faltan = enElInventario.Except(enElSaldo)
            .Select(x => $"{x.Tipo} «{x.Referencia}» está vivo y no figura en el saldo");

        var sobran = enElSaldo.Except(enElInventario)
            .Select(x => $"{x.Tipo} «{x.Referencia}» figura en el saldo y ya no está vivo");

        return [.. faltan, .. sobran];
    }

    private sealed class ComparadorDeRenglon : IEqualityComparer<(TipoDeRenglon, string)>
    {
        public static readonly ComparadorDeRenglon Instancia = new();

        public bool Equals((TipoDeRenglon, string) x, (TipoDeRenglon, string) y) =>
            x.Item1 == y.Item1 &&
            string.Equals(x.Item2, y.Item2, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode((TipoDeRenglon, string) obj) =>
            HashCode.Combine(obj.Item1, obj.Item2.ToUpperInvariant());
    }
}
