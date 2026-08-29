using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Dominio.M03_Flota;

/// <summary>
/// Bajo qué régimen la institución tiene el vehículo — `RN-62`.
///
/// ── Sólo `Propiedad` hace propio el bien ────────────────────────────────────
/// Y de eso depende cuál de los dos terminales corresponde (`HB3-17`): el descargo extingue un
/// bien propio, el retiro de flota devuelve uno que nunca lo fue. Declarar <i>«dado de baja del
/// registro de bienes del Estado»</i> un vehículo en comodato es un asiento falso.
/// </summary>
public enum RegimenDeTenencia
{
    /// <summary>Del Estado. <b>El único sin fecha de fin</b>: no vence.</summary>
    Propiedad,

    /// <summary>Cedido en préstamo de uso por otra entidad, con convenio.</summary>
    Comodato,

    /// <summary>Arrendado, con contrato y canon.</summary>
    Alquiler,

    /// <summary>
    /// Donado pero sin traspaso inscrito. <b>Todavía no es propio</b>: hasta que el traspaso se
    /// perfeccione, darlo de baja del registro de bienes sería anticipar un título que no está.
    /// </summary>
    DonacionEnTramite,

    /// <summary>Asignado por otra institución del Estado, con resolución.</summary>
    AsignacionPorOtraInstitucion,
}

/// <summary>
/// Quién paga cada rubro durante la tenencia — `RN-62`.
///
/// ── Para qué sirve, más allá del dato ───────────────────────────────────────
/// `RN-62` punto 3: <i>«M-11 dirige la <b>orden de trabajo</b> y M-13 el <b>cargo</b> según el
/// rubro: lo que cubre el contrato <b>no se imputa al presupuesto de la institución</b>, y el
/// sistema deja constancia de esa derivación»</i>.
///
/// Un mantenimiento que cubre el arrendador y se carga igual al presupuesto es gasto público
/// pagado dos veces.
/// </summary>
public enum QuienAsume
{
    /// <summary>La institución que tiene el vehículo.</summary>
    Institucion,

    /// <summary>El propietario o cedente. <b>No se imputa a nuestro presupuesto.</b></summary>
    Titular,

    /// <summary>
    /// Nadie lo pactó. <b>No es «la institución» por omisión</b>: es el rubro que aparece cuando
    /// llega la factura y empieza la discusión con el contrato en la mano.
    /// </summary>
    SinPactar,
}

/// <summary>
/// La matriz de rubros de `RN-62`: combustible, mantenimiento, llantas, seguro, peajes, multas
/// y daños.
/// </summary>
public sealed record RubrosDelTitulo(
    QuienAsume Combustible = QuienAsume.SinPactar,
    QuienAsume Mantenimiento = QuienAsume.SinPactar,
    QuienAsume Llantas = QuienAsume.SinPactar,
    QuienAsume Seguro = QuienAsume.SinPactar,
    QuienAsume Peajes = QuienAsume.SinPactar,
    QuienAsume Multas = QuienAsume.SinPactar,
    QuienAsume Danios = QuienAsume.SinPactar)
{
    /// <summary>Los siete, con su nombre. Es la matriz que la ficha del vehículo muestra.</summary>
    public IReadOnlyList<(string Rubro, QuienAsume Quien)> Todos =>
    [
        ("combustible", Combustible),
        ("mantenimiento", Mantenimiento),
        ("llantas", Llantas),
        ("seguro", Seguro),
        ("peajes", Peajes),
        ("multas", Multas),
        ("daños", Danios),
    ];

    /// <summary>Los que nadie pactó. Van nombrados, no supuestos de la institución.</summary>
    public IReadOnlyList<string> SinPactar =>
        [.. Todos.Where(r => r.Quien is QuienAsume.SinPactar).Select(r => r.Rubro)];

    /// <summary>
    /// Los que cubre el titular. <b>No se imputan al presupuesto de la institución</b>, y el
    /// sistema deja constancia de la derivación (`RN-62` punto 3).
    /// </summary>
    public IReadOnlyList<string> DelTitular =>
        [.. Todos.Where(r => r.Quien is QuienAsume.Titular).Select(r => r.Rubro)];
}

/// <summary>
/// El título de tenencia del vehículo — `RN-62`.
///
/// ── Lo que la regla no deja pasar ───────────────────────────────────────────
/// <i>«<b>Sin título vigente el vehículo no se habilita en la flota</b>, y <b>ninguna misión se
/// programa ni se despacha si su ventana excede la vigencia del título</b> — bloqueo duro, con el
/// mismo patrón de `RN-10`»</i>.
///
/// Es el mismo argumento de la licencia: no alcanza con que el título esté vigente hoy; tiene que
/// cubrir <b>todo el rango</b> de la misión. Un comodato que vence el 20 no ampara una misión que
/// vuelve el 22.
/// </summary>
/// <param name="Hasta">
/// <b>Nula sólo en propiedad</b>, que es el único régimen que no vence. En los demás, su ausencia
/// haría que el título no venciera nunca — y un comodato que no vence es una apropiación.
/// </param>
/// <param name="Documento">
/// El convenio, contrato, acta o resolución. `RN-62` casos límite: <i>«comodato prorrogado
/// verbalmente <b>no existe para el sistema</b>. La vigencia es la del documento; sin adenda
/// adjunta, el título vence y el bloqueo opera. Es incómodo y es correcto»</i>.
/// </param>
public sealed record TituloDeTenencia(
    Ulid Id,
    Ulid VehiculoId,
    RegimenDeTenencia Regimen,
    string Titular,
    string Documento,
    DateOnly Desde,
    DateOnly? Hasta,
    RubrosDelTitulo Rubros)
{
    /// <summary>
    /// <b>Si el bien es del Estado.</b> Sólo la propiedad lo hace propio: es lo que decide, en
    /// `HB3-17`, cuál de los dos terminales corresponde.
    /// </summary>
    public bool EsBienPropio => Regimen is RegimenDeTenencia.Propiedad;

    public bool VigenteAl(DateOnly fecha) =>
        Desde <= fecha && (Hasta is null || fecha <= Hasta);

    /// <summary>
    /// Cuántos días quedan. <b>Nulo en propiedad</b>: no vence, y mostrar un número inventado
    /// haría que la ficha alertara sobre un vencimiento que no existe.
    /// </summary>
    public int? DiasRestantes(DateOnly hoy) =>
        Hasta is null ? null : Hasta.Value.DayNumber - hoy.DayNumber;
}

/// <summary>
/// Por qué una misión no se puede programar contra el título — `RN-62`.
///
/// ── Por qué es un tipo y no un booleano ─────────────────────────────────────
/// El mismo argumento que la custodia al despachar y que el conflicto por indisponibilidad: <i>«es
/// la diferencia entre "no hay" y "nadie preguntó", y en un bloqueo duro las dos no pueden verse
/// igual»</i>.
/// </summary>
/// <param name="Titulos">
/// <b>Todos los del vehículo, sin filtrar por fecha.</b> Cuál regía lo decide el dominio contra la
/// ventana de la solicitud — es el mismo criterio que las reservas de `BD-11`: <i>«se traen sin
/// filtrar por fecha: el solape lo decide el dominio»</i>. Filtrarlo en el endpoint obligaría a
/// que la capa web supiera a qué fecha se resuelve, que es justamente la regla.
/// </param>
public sealed record TituloAlProgramar(IReadOnlyList<TituloDeTenencia> Titulos)
{
    /// <summary>
    /// <b>Se construye explícitamente</b> cuando el vehículo no tiene ningún título: quien la use
    /// está afirmando que consultó y no encontró nada.
    /// </summary>
    public static TituloAlProgramar SinTitulo { get; } = new([]);

    /// <summary>El que regía a esa fecha, si alguno.</summary>
    public TituloDeTenencia? VigenteAl(DateOnly fecha) =>
        Titulos.Where(t => t.VigenteAl(fecha)).OrderByDescending(t => t.Desde).FirstOrDefault();
}

/// <summary>
/// Los controles del título de tenencia — `RN-62`.
/// </summary>
public static class ReglasDelTitulo
{
    /// <summary>
    /// `RN-62` — lo que el título exige para existir.
    /// </summary>
    public static void ExigirElTitulo(
        RegimenDeTenencia regimen,
        string titular,
        string documento,
        DateOnly desde,
        DateOnly? hasta)
    {
        if (string.IsNullOrWhiteSpace(titular))
            throw new BloqueoDuro("RN-62",
                "El título de tenencia exige titular: quién es el propietario o cedente. Sin él " +
                "no hay a quién devolverle el bien ni a quién reclamarle lo que el contrato " +
                "pone a su cargo.");

        if (string.IsNullOrWhiteSpace(documento))
            throw new BloqueoDuro("RN-62",
                "El título exige el documento que lo sustenta: convenio de comodato, contrato " +
                "de alquiler, acta de donación o resolución. Un comodato prorrogado verbalmente " +
                "no existe para el sistema.");

        // ── La propiedad es el único régimen que no vence ────────────────────
        if (regimen is RegimenDeTenencia.Propiedad)
        {
            if (hasta is not null)
                throw new BloqueoDuro("RN-62",
                    "El régimen de propiedad no lleva fecha de fin: el bien es del Estado y no " +
                    "vence. Ponerle una haría que el vehículo se inhabilitara solo el día que " +
                    "alguien eligió sin que ninguna norma lo mandara.");

            return;
        }

        if (hasta is null)
            throw new BloqueoDuro("RN-62",
                $"El régimen de {regimen} exige rango de vigencia con fecha de fin. Sin ella el " +
                "título no vence nunca — y un comodato que no vence es una apropiación.");

        if (hasta < desde)
            throw new BloqueoDuro("RN-62",
                $"La vigencia del título termina ({hasta:dd/MM/yyyy}) antes de empezar " +
                $"({desde:dd/MM/yyyy}).");
    }

    /// <summary>
    /// `RN-62` — <b>sin título vigente el vehículo no se habilita en la flota.</b>
    ///
    /// Es la precondición de `W-02`: habilitar un vehículo cuyo título venció lo pone a
    /// disposición de misiones que la institución no tiene derecho a hacer con él.
    /// </summary>
    public static void ExigirTituloParaHabilitar(TituloDeTenencia? titulo, DateOnly fecha)
    {
        if (titulo is null)
            throw new BloqueoDuro("RN-62",
                "Este vehículo no tiene título de tenencia y no se puede habilitar en la flota. " +
                "Sin título no consta bajo qué régimen lo tenemos, quién es su titular ni hasta " +
                "cuándo: un vehículo así no se puede asignar a nada.");

        if (titulo.VigenteAl(fecha)) return;

        throw new BloqueoDuro("RN-62",
            $"El título de tenencia ({titulo.Regimen}, {titulo.Titular}) no está vigente al " +
            $"{fecha:dd/MM/yyyy}: rige del {titulo.Desde:dd/MM/yyyy} al " +
            $"{titulo.Hasta:dd/MM/yyyy}. Sin título vigente el vehículo no se habilita — y una " +
            "prórroga verbal no existe para el sistema: se adjunta la adenda o el título vence.");
    }

    /// <summary>
    /// `RN-62` — <b>ninguna misión se programa ni se despacha si su ventana excede la vigencia
    /// del título</b>, con el mismo patrón de `RN-10`.
    ///
    /// ── No alcanza con que esté vigente hoy ─────────────────────────────────
    /// Tiene que cubrir <b>todo el rango</b>. Un comodato que vence el 20 no ampara una misión
    /// que vuelve el 22: los dos últimos días el vehículo ya no sería nuestro para usarlo.
    /// </summary>
    /// <returns>
    /// La evidencia para el diario. <b>Cuando no hay título se dice</b>, en vez de dejar creer
    /// que se verificó — igual que `BD-07` con el estado nulo.
    /// </returns>
    public static string ExigirVigenciaEnTodoElRango(
        TituloAlProgramar titulo, DateOnly salida, DateOnly retorno)
    {
        if (titulo.Titulos.Count == 0)
            return " · RN-62 NO evaluada: el vehículo no tiene título de tenencia registrado";

        // El que regía **al salir**. Si ninguno la cubre, el bloqueo cita el más reciente para
        // que el mensaje diga contra qué vigencia se comparó.
        var t = titulo.VigenteAl(salida)
            ?? titulo.Titulos.OrderByDescending(x => x.Desde).First();

        if (t.VigenteAl(salida) && t.VigenteAl(retorno))
            return t.Hasta is null
                ? $" · título {t.Regimen} sin vencimiento"
                : $" · título {t.Regimen} vigente hasta el {t.Hasta:dd/MM/yyyy}";

        throw new BloqueoDuro("RN-62",
            $"La ventana de la misión ({salida:dd/MM/yyyy} al {retorno:dd/MM/yyyy}) excede la " +
            $"vigencia del título de tenencia, que rige del {t.Desde:dd/MM/yyyy} al " +
            $"{t.Hasta:dd/MM/yyyy} ({t.Regimen}, {t.Titular}). No alcanza con que el título " +
            "esté vigente el día de la salida: tiene que cubrir todo el rango, o el vehículo " +
            "dejaría de ser nuestro para usarlo a mitad de la misión.");
    }

    /// <summary>
    /// `RN-62` punto 3 — a quién se le imputa un rubro.
    ///
    /// <b>«Sin pactar» no es «la institución»</b>: es el rubro que aparece cuando llega la
    /// factura y empieza la discusión con el contrato en la mano. Se responde nulo y quien
    /// pregunte lo declara.
    /// </summary>
    public static QuienAsume? AQuienSeImputa(TituloDeTenencia? titulo, string rubro)
    {
        if (titulo is null) return null;

        var quien = titulo.Rubros.Todos
            .FirstOrDefault(r => string.Equals(
                r.Rubro, rubro, StringComparison.OrdinalIgnoreCase))
            .Quien;

        return quien is QuienAsume.SinPactar ? null : quien;
    }
}
