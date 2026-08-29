using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Dominio.M03_Flota;

/// <summary>
/// El acto que autoriza el préstamo — `RN-63` punto 1.
///
/// Con folio, firmante identificado y documento adjunto. Sin él no hay préstamo: hay un vehículo
/// que se fue.
/// </summary>
public sealed record ActoAutorizante(
    string Folio,
    string Firmante,
    DateOnly Fecha,
    string? Adjunto = null);

/// <summary>
/// Quién recibe la tenencia — `RN-63` punto 2.
///
/// <b>Con cargo e institución</b>, porque el punto 7 de la regla exige poder responder, en
/// cualquier fecha del período, <i>quién respondía por la unidad</i>. Un nombre suelto no
/// contesta esa pregunta ante el Tribunal Superior de Cuentas.
/// </summary>
public sealed record ResponsableReceptor(
    string Persona,
    string Cargo,
    string Institucion,
    string ConstanciaDeRecepcion);

/// <summary>
/// Qué asume cada parte durante la tenencia ajena — `RN-63` punto 5.
///
/// ── Nulo es «no pactado», y eso es un hallazgo ──────────────────────────────
/// Un rubro sin pactar es el que aparece cuando llega la multa: nadie declaró quién la paga, y
/// la discusión empieza con el vehículo ya devuelto. El expediente lo dice desde el principio.
/// </summary>
public sealed record RubrosPactados(
    string? Combustible,
    string? Peajes,
    string? Mantenimiento,
    string? Multas,
    string? Danios)
{
    /// <summary>Los que nadie pactó. Van nombrados en el expediente, no supuestos.</summary>
    public IReadOnlyList<string> SinPactar
    {
        get
        {
            var faltan = new List<string>();

            if (string.IsNullOrWhiteSpace(Combustible)) faltan.Add("combustible");
            if (string.IsNullOrWhiteSpace(Peajes)) faltan.Add("peajes");
            if (string.IsNullOrWhiteSpace(Mantenimiento)) faltan.Add("mantenimiento");
            if (string.IsNullOrWhiteSpace(Multas)) faltan.Add("multas");
            if (string.IsNullOrWhiteSpace(Danios)) faltan.Add("daños");

            return faltan;
        }
    }
}

/// <summary>
/// El acta de entrega o de devolución — `RN-63` puntos 4 y 6.
///
/// ── La rotulación se constata las dos veces ─────────────────────────────────
/// La identificación del vehículo del Estado —franjas, leyenda, siglas— es <b>hallazgo frecuente
/// de auditoría</b>, y un vehículo que vuelve sin ella volvió distinto de como salió. Por eso la
/// regla pide constatarla al entregar y <b>reconstatarla</b> al devolver.
/// </summary>
/// <param name="Odometro">
/// Fotografiado, dice la regla. Acá va la lectura; la foto es un adjunto del expediente.
/// </param>
/// <param name="NivelDeCombustible">
/// Cómo se entregó. <b>Nulo es no consignado</b>, no vacío: sin las dos lecturas no se puede
/// decir si volvió con menos.
/// </param>
public sealed record ActaDeTenencia(
    DateOnly Fecha,
    int Odometro,
    string Firma,
    string? NivelDeCombustible,
    string? InventarioDeAccesorios,
    string? DocumentosEntregados,
    bool RotulacionConstatada,
    string? NovedadesODanios);

/// <summary>
/// El expediente de préstamo de un vehículo — `RN-63`.
///
/// ── Lo que NO es, y la regla lo dice sin margen ─────────────────────────────
/// <b>No es una Orden de Misión.</b> <i>«Cuando el vehículo se cede con motorista de la
/// institución propietaria, sí es una Orden de Misión con motivo apoyo institucional: ahí no se
/// cedió la tenencia, se prestó un servicio»</i>. La diferencia es quién responde por la unidad
/// mientras está afuera, y ése es todo el punto del expediente.
///
/// ── El entregable de la regla ───────────────────────────────────────────────
/// `RN-63` punto 7: <i>«en cualquier fecha del período, el sistema responde <b>quién respondía
/// por la unidad</b>. Esa consulta es el entregable de la regla»</i>. Ver
/// <see cref="ReglasDelPrestamo.QuienRespondiaPor"/>.
/// </summary>
/// <param name="DevolucionComprometida">
/// La fecha pactada. <b>Vencerla no cierra el préstamo</b>: lo pone en mora, con escalamiento
/// diario, y `RN-97` punto 4 impide cerrar el período con préstamos vencidos.
/// </param>
/// <param name="Devolucion">
/// Nula mientras el vehículo no vuelva. <b>El vehículo no vuelve a `DISPONIBLE` sin esta acta</b>.
/// </param>
public sealed record ExpedienteDePrestamo(
    Ulid Id,
    Ulid VehiculoId,
    ActoAutorizante Acto,
    string Autoriza,
    ResponsableReceptor Receptor,
    string MotivoDelPrestamo,
    DateOnly Desde,
    DateOnly DevolucionComprometida,
    ActaDeTenencia Entrega,
    RubrosPactados Rubros,
    ActaDeTenencia? Devolucion = null,
    string? QuienFirmaLaDevolucion = null)
{
    public bool EstaVigente => Devolucion is null;

    /// <summary>
    /// Días de mora al día indicado. <b>Cero mientras no venza</b>, y el préstamo devuelto no
    /// acumula: dejó de estar afuera.
    /// </summary>
    public int DiasDeMora(DateOnly hoy) =>
        EstaVigente && hoy > DevolucionComprometida
            ? hoy.DayNumber - DevolucionComprometida.DayNumber
            : 0;

    /// <summary>
    /// <b>Vencido.</b> `RN-97` punto 4 le da poder de bloqueo sobre el cierre del período, y
    /// `RN-63` punto 4 exige escalamiento diario mientras dure.
    /// </summary>
    public bool EstaVencido(DateOnly hoy) => DiasDeMora(hoy) > 0;

    /// <summary>
    /// Los kilómetros recorridos bajo tenencia ajena — `RN-63` punto 3.
    ///
    /// <b>No entran en la conciliación galonaje–kilometraje</b> (`RN-30`): no hubo consumo
    /// nuestro contra esos kilómetros. Se asientan con las dos lecturas y se separan.
    ///
    /// Nulo mientras no haya acta de devolución: con una sola lectura no hay recorrido que medir.
    /// </summary>
    public int? KilometrosBajoTenenciaAjena =>
        Devolucion is null ? null : Devolucion.Odometro - Entrega.Odometro;

    /// <summary>
    /// Si el vehículo volvió sin la identificación del Estado. Es hallazgo frecuente de
    /// auditoría, y `RN-63` punto 6 manda reconstatarla justamente por eso.
    /// </summary>
    public bool VolvioSinRotulacion =>
        Devolucion is { RotulacionConstatada: false } && Entrega.RotulacionConstatada;
}

/// <summary>
/// Quién respondía por una unidad en una fecha — `RN-63` punto 7, <b>el entregable de la regla</b>.
/// </summary>
/// <param name="EsTenenciaAjena">
/// Si en esa fecha la unidad estaba prestada. Cuando es falso, responde la institución
/// propietaria por su custodio ordinario.
/// </param>
public sealed record QuienRespondia(
    DateOnly Fecha,
    bool EsTenenciaAjena,
    string? Persona,
    string? Cargo,
    string? Institucion,
    Ulid? Prestamo);

/// <summary>
/// Los controles del préstamo — `RN-63`.
/// </summary>
public static class ReglasDelPrestamo
{
    /// <summary>
    /// `RN-63` — el préstamo <b>no se modela como Orden de Misión</b>.
    ///
    /// ── La diferencia es quién tiene la tenencia ────────────────────────────
    /// Cedido <b>con motorista de la institución propietaria</b>, la tenencia no se cedió: se
    /// prestó un servicio, y eso es una Orden de Misión con motivo <i>apoyo institucional</i>.
    /// Modelarlo como préstamo diría que la unidad salió del alcance de la institución cuando su
    /// propio motorista iba al volante.
    /// </summary>
    public static void ExigirCesionDeTenencia(bool conMotoristaPropio)
    {
        if (!conMotoristaPropio) return;

        throw new BloqueoDuro("RN-63",
            "El vehículo se cede con motorista de la institución propietaria, así que la " +
            "tenencia no se cedió: se prestó un servicio. Eso es una Orden de Misión con motivo " +
            "«apoyo institucional», no un expediente de préstamo. La diferencia decide quién " +
            "responde por la unidad mientras está afuera.");
    }

    /// <summary>
    /// `RN-63` puntos 1 a 3 — lo que el expediente exige para existir.
    /// </summary>
    public static void ExigirElExpediente(
        ActoAutorizante acto,
        ResponsableReceptor receptor,
        DateOnly desde,
        DateOnly devolucionComprometida,
        string motivo)
    {
        if (string.IsNullOrWhiteSpace(acto.Folio) || string.IsNullOrWhiteSpace(acto.Firmante))
            throw new BloqueoDuro("RN-63",
                "El préstamo exige acto autorizante con folio y firmante identificado. Sin él " +
                "no hay préstamo: hay un vehículo que se fue.");

        if (string.IsNullOrWhiteSpace(receptor.Persona) ||
            string.IsNullOrWhiteSpace(receptor.Cargo) ||
            string.IsNullOrWhiteSpace(receptor.Institucion))
            throw new BloqueoDuro("RN-63",
                "El préstamo exige responsable receptor nombrado, con cargo e institución. Es " +
                "lo que permite responder quién respondía por la unidad en cualquier fecha, y " +
                "un nombre suelto no contesta esa pregunta.");

        if (string.IsNullOrWhiteSpace(receptor.ConstanciaDeRecepcion))
            throw new BloqueoDuro("RN-63",
                "El receptor exige su constancia de recepción: es el acto por el que la otra " +
                "parte reconoce que tiene el bien.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new BloqueoDuro("RN-63",
                "El préstamo exige motivo del catálogo `motivo_de_prestamo`.");

        // La fecha de devolución **comprometida** es la que hace posible la mora. Sin ella el
        // préstamo no vence nunca, y un préstamo que no vence es una baja encubierta.
        if (devolucionComprometida < desde)
            throw new BloqueoDuro("RN-63",
                $"La devolución comprometida ({devolucionComprometida:dd/MM/yyyy}) es anterior " +
                $"al inicio del préstamo ({desde:dd/MM/yyyy}).");
    }

    /// <summary>
    /// `RN-63` punto 2 — <b>quien autoriza no puede ser el receptor</b>, y quien firma la
    /// devolución no puede ser quien recibió.
    ///
    /// ── Con una nota de hallazgo que la propia regla deja abierta ───────────
    /// `RN-63`: <i>«es una incompatibilidad de segregación; su lugar propio es
    /// `actores-y-roles.md` —autoridad en la materia— y desde `CE-14` se propuso como par `I-c`.
    /// <b>Nota de hallazgo abierta</b> hasta que se incorpore allí»</i>.
    ///
    /// Se implementa acá porque la regla es bloqueo duro y no puede quedar sin efecto esperando
    /// al documento. ⚠️ Pero el par sigue sin estar en la autoridad, y mientras no esté, esta
    /// comprobación es la única que lo sostiene.
    /// </summary>
    public static void ExigirSegregacion(string autoriza, ResponsableReceptor receptor)
    {
        if (!Misma(autoriza, receptor.Persona)) return;

        throw new BloqueoDuro("RN-63",
            $"«{autoriza}» autoriza el préstamo y además figura como responsable receptor. " +
            "Quien autoriza la salida del bien no puede ser quien lo recibe: es la misma " +
            "persona decidiendo entregarse a sí misma un vehículo del Estado.");
    }

    /// <summary>
    /// `RN-63` punto 2, segunda mitad — <b>quien firma la devolución no puede ser quien
    /// recibió</b>.
    ///
    /// Si lo fuera, el receptor declararía por sí mismo que devolvió en orden, y el acta de
    /// devolución dejaría de ser una constatación para volverse una autodeclaración.
    /// </summary>
    public static void ExigirQuienRecibeLaDevolucion(
        ResponsableReceptor receptor, string quienFirmaLaDevolucion)
    {
        if (string.IsNullOrWhiteSpace(quienFirmaLaDevolucion))
            throw new BloqueoDuro("RN-63",
                "El acta de devolución exige quién la firma por la institución propietaria: es " +
                "quien constata el odómetro, las novedades y la rotulación.");

        if (!Misma(receptor.Persona, quienFirmaLaDevolucion)) return;

        throw new BloqueoDuro("RN-63",
            $"«{quienFirmaLaDevolucion}» recibió el vehículo en préstamo y ahora firmaría su " +
            "devolución. El acta dejaría de ser una constatación para volverse una " +
            "autodeclaración de que devolvió en orden.");
    }

    /// <summary>
    /// `RN-63` — <b>el vehículo no vuelve a `DISPONIBLE` sin acta de devolución.</b>
    /// </summary>
    public static void ExigirActaDeDevolucion(ExpedienteDePrestamo prestamo)
    {
        if (!prestamo.EstaVigente) return;

        throw new BloqueoDuro("RN-63",
            "Este vehículo está prestado y no tiene acta de devolución. No vuelve a " +
            "DISPONIBLE sin ella: sin acta, nadie constató con qué odómetro volvió, en qué " +
            "estado ni si conserva la identificación del Estado.");
    }

    /// <summary>
    /// `RN-97` punto 4 — <b>no se cierra el período con préstamos vencidos.</b>
    ///
    /// ── La otra mitad del bloqueo que estaba declarado y vacío ──────────────
    /// El saldo de apertura declara dos fuentes con poder de bloqueo. Las interrupciones sin
    /// desenlace llegaron con M-12; ésta es la que faltaba.
    /// </summary>
    /// <param name="declaracionExplicita">
    /// El motivo por el que se cierra con préstamos vencidos vivos. `RN-97` punto 4:
    /// <i>«hay que resolverlos o declararlos explícitamente»</i>.
    /// </param>
    public static void ExigirDevolucionAntesDelCierre(
        IReadOnlyList<ExpedienteDePrestamo> prestamos,
        DateOnly corte,
        string? declaracionExplicita)
    {
        var vencidos = prestamos.Where(p => p.EstaVencido(corte)).ToList();

        if (vencidos.Count == 0) return;
        if (!string.IsNullOrWhiteSpace(declaracionExplicita)) return;

        var detalle = string.Join("; ", vencidos.Select(p =>
            $"vehículo {p.VehiculoId} en poder de {p.Receptor.Persona} " +
            $"({p.Receptor.Institucion}), {p.DiasDeMora(corte)} días de mora"));

        throw new BloqueoDuro("RN-63",
            $"{vencidos.Count} préstamo(s) vencido(s): {detalle}. No se cierra el período con " +
            "vehículos del Estado en tenencia ajena fuera de la fecha comprometida: se " +
            "recuperan, se prorroga el acto, o se declara explícitamente por qué se cierra con " +
            "ellos afuera.");
    }

    /// <summary>
    /// <b>El entregable de la regla</b> — `RN-63` punto 7: <i>«en cualquier fecha del período, el
    /// sistema responde quién respondía por la unidad»</i>.
    ///
    /// ── Se resuelve por la fecha, no por el estado de hoy ───────────────────
    /// La pregunta que se hace la auditoría es <i>«¿quién respondía por esta unidad el 14 de
    /// agosto?»</i>, y un vehículo que hoy está disponible pudo estar prestado ese día. Por eso
    /// se busca el préstamo que <b>cubría</b> la fecha, no el vigente.
    /// </summary>
    public static QuienRespondia QuienRespondiaPor(
        IReadOnlyList<ExpedienteDePrestamo> prestamosDelVehiculo, DateOnly fecha)
    {
        var prestamo = prestamosDelVehiculo
            .Where(p => p.Desde <= fecha)
            .Where(p => p.Devolucion is null || fecha <= p.Devolucion.Fecha)

            // Si dos se solapan —que no deberían— manda el que empezó después. Determinista, en
            // vez de depender del orden de la consulta.
            .OrderByDescending(p => p.Desde)
            .FirstOrDefault();

        return prestamo is null
            ? new QuienRespondia(fecha, false, null, null, null, null)
            : new QuienRespondia(
                fecha, true,
                prestamo.Receptor.Persona,
                prestamo.Receptor.Cargo,
                prestamo.Receptor.Institucion,
                prestamo.Id);
    }

    private static bool Misma(string a, string b) =>
        string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);
}
