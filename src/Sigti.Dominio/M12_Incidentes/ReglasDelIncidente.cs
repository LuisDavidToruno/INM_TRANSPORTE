using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Dominio.M12_Incidentes;

/// <summary>
/// `RN-74` — el registro de campo <b>no captura atribución de responsabilidad</b>.
///
/// ── Por qué es un bloqueo y no una guía de estilo ───────────────────────────
/// Porque el daño no lo hace el campo: lo hace la pregunta. `RN-74`: <i>«un campo "¿de quién fue
/// la culpa?" en esa pantalla produce dos daños: una declaración tomada bajo presión que después
/// pesa en un expediente, y una atribución hecha por quien no tiene competencia para hacerla»</i>.
///
/// Y con la sustracción es peor: <i>«el motorista asaltado a mano armada no es responsable de
/// nada, y un sistema que le pide declarar responsabilidad lo pone en la posición de
/// acusarse»</i>.
/// </summary>
public static class ReglasDelRegistroDeIncidente
{
    /// <summary>
    /// Lo mínimo que un registro de campo tiene que traer — `RN-74` punto 2: <i>«los formularios
    /// de campo contienen hechos observables»</i>.
    ///
    /// La descripción es obligatoria y libre; lo que no existe es un campo de causa jurídica.
    /// </summary>
    public static void ExigirElHecho(
        string causa, string descripcion, string registra, string responsableDeSeguimiento)
    {
        if (string.IsNullOrWhiteSpace(causa))
            throw new BloqueoDuro("RN-70",
                "El incidente exige causa del catálogo. Sin ella el expediente no se puede " +
                "agrupar, y un módulo de incidentes que no se puede agrupar por causa no dice " +
                "nada sobre dónde está el problema.");

        if (string.IsNullOrWhiteSpace(descripcion))
            throw new BloqueoDuro("RN-74",
                "El incidente exige descripción del hecho. Es el campo que reemplaza a los que " +
                "esta regla prohíbe: se pregunta qué pasó, nunca de quién fue la culpa.");

        if (string.IsNullOrWhiteSpace(registra))
            throw new BloqueoDuro("RN-74", "El incidente exige quién lo registró.");

        // `RN-74` punto 4: el evento abre expediente **con responsable de seguimiento y plazo**.
        // Un expediente sin responsable es el mismo expediente muerto que `RN-97` describe.
        if (string.IsNullOrWhiteSpace(responsableDeSeguimiento))
            throw new BloqueoDuro("RN-74",
                "El expediente de incidente exige responsable de seguimiento nominado. Sin él, " +
                "el expediente queda abierto sin que nadie lo tenga en la mano.");
    }

    /// <summary>
    /// El acto de determinación de responsabilidad — `RN-74` punto 4.
    ///
    /// ── SIGTI lo adjunta, y por eso exige de dónde salió ────────────────────
    /// Sin número ni instancia emisora no es un acto: es una opinión escrita en el expediente,
    /// que es exactamente lo que la regla existe para impedir. <i>«El sistema registra esa
    /// determinación cuando existe, con su acto y su autor; no la produce»</i>.
    /// </summary>
    public static void ExigirActoDeLaInstanciaCompetente(DeterminacionDeResponsabilidad acto)
    {
        if (string.IsNullOrWhiteSpace(acto.Numero) ||
            string.IsNullOrWhiteSpace(acto.InstanciaQueLaEmite))
            throw new BloqueoDuro("RN-74",
                "La determinación de responsabilidad se adjunta como acto de la instancia " +
                "competente, con su número y quién lo emite. SIGTI la registra, no la produce: " +
                "sin número ni emisor esto sería una atribución hecha por quien no tiene " +
                "competencia para hacerla.");

        if (string.IsNullOrWhiteSpace(acto.Resolucion))
            throw new BloqueoDuro("RN-74",
                "El acto exige qué resolvió. Adjuntar un número sin la resolución deja el " +
                "expediente diciendo que hubo determinación sin decir cuál.");
    }
}

/// <summary>
/// `RN-70` — la interrupción en ruta <b>exige desenlace explícito</b>.
///
/// ── Y no le cambia el estado a la misión ────────────────────────────────────
/// <i>«El evento marca la misión como interrumpida y no le cambia el estado. La Orden de Misión
/// sigue `EN_RUTA`: el vehículo salió y hubo consumo real de recursos públicos»</i>.
///
/// Es la misma disciplina de `RN-96` un módulo más allá: lo que ocurre se registra, y el
/// expediente sigue su vida hasta que alguien lo resuelva — no por efecto de un hecho.
/// </summary>
public static class ReglasDeLaInterrupcion
{
    /// <summary>
    /// `RN-70` — <b>ninguna misión con marca de interrupción sin desenlace puede quedar viva al
    /// cierre del período</b> (`RN-97` punto 4).
    ///
    /// ── El bloqueo que hasta ahora no podía disparar ────────────────────────
    /// `RN-97` le da a esta fuente poder de bloqueo sobre el cierre, y el saldo de apertura la
    /// declaraba <i>«no consultable»</i> porque no existía como registro. Ahora existe.
    /// </summary>
    /// <param name="declaracionExplicita">
    /// El motivo por el que se cierra el período con interrupciones sin desenlace vivas. `RN-97`
    /// punto 4: <i>«hay que resolverlos o declararlos explícitamente»</i>. <b>Nulo es no
    /// declarado</b>, y entonces el cierre no procede.
    /// </param>
    public static void ExigirDesenlaceAntesDelCierre(
        IReadOnlyList<ExpedienteDeIncidente> incidentes, string? declaracionExplicita)
    {
        var sinDesenlace = incidentes
            .Where(i => i.EsInterrupcionSinDesenlace && i.EstaAbierto)
            .ToList();

        if (sinDesenlace.Count == 0) return;
        if (!string.IsNullOrWhiteSpace(declaracionExplicita)) return;

        var detalle = string.Join("; ", sinDesenlace.Select(i =>
            $"{i.Tipo} del {i.FechaDelHecho:dd/MM/yyyy} a cargo de {i.ResponsableDeSeguimiento}"));

        throw new BloqueoDuro("RN-70",
            $"{sinDesenlace.Count} interrupción(es) en ruta sin desenlace: {detalle}. Toda " +
            "interrupción se resuelve con un desenlace explícito y tipificado —continuar, " +
            "continuar con sustitución, retorno anticipado o retorno sin vehículo— y ninguna " +
            "puede quedar viva al cierre del período. Se resuelven, o se declara " +
            "explícitamente por qué se cierra con ellas.");
    }

    /// <summary>
    /// Registrar el desenlace — `RN-70`.
    ///
    /// ── Una sola vez, y con constancia ──────────────────────────────────────
    /// Reescribir el desenlace borraría el que constaba. Si el primero estuvo mal, lo que
    /// corresponde es el asiento de corrección con referencia al anterior (`RN-42`), no
    /// sobreescribir la historia.
    /// </summary>
    public static void ExigirDesenlaceRegistrable(
        ExpedienteDeIncidente expediente, string detalle)
    {
        if (!expediente.Interrumpe)
            throw new BloqueoDuro("RN-70",
                "Este incidente no está marcado como interrupción, así que no tiene desenlace " +
                "que registrar. El desenlace resuelve la marca de interrupción; ponerle uno a " +
                "un hecho que no interrumpió inventaría una interrupción que no ocurrió.");

        if (expediente.Desenlace is not null)
            throw new BloqueoDuro("RN-70",
                $"Esta interrupción ya se resolvió con desenlace {expediente.Desenlace}. " +
                "Reescribirlo borraría el que constaba: una corrección se registra como asiento " +
                "nuevo con referencia al anterior (`RN-42`).");

        // `RN-70`: continuar exige **constancia de quién lo autorizó**, y las otras tres exigen
        // decir contra qué acto se resolvieron. En los cuatro casos, el detalle es esa constancia.
        if (string.IsNullOrWhiteSpace(detalle))
            throw new BloqueoDuro("RN-70",
                "El desenlace exige constancia: quién lo autorizó y contra qué acto. Un " +
                "desenlace sin constancia deja la interrupción resuelta sin que se pueda decir " +
                "por quién.");
    }
}

/// <summary>
/// `RN-75` — el bien retenido, sustraído o no recuperado <b>no sale del registro</b>.
///
/// ── Nunca se elimina ────────────────────────────────────────────────────────
/// <i>«El bien permanece en el registro patrimonial hasta su recuperación o su descargo formal.
/// <b>Nunca se elimina</b>»</i>. Es el deber que `NRM-02` impone sobre el bien del Estado, y la
/// razón por la que este módulo tiene estados y no una bandera de «activo».
/// </summary>
public static class ReglasDelBienNoRecuperado
{
    /// <summary>
    /// Mientras el bien esté afuera, el expediente conserva <b>ubicación, autoridad custodia y
    /// número de expediente</b> — `RN-75`.
    ///
    /// ── Lo que se exige depende de si se sabe dónde está ────────────────────
    /// De una <b>retención por autoridad</b> se sabe quién lo tiene y bajo qué expediente: no
    /// saberlo es un dato que falta, no una situación distinta. De una <b>sustracción</b> puede
    /// no saberse nada, y exigir la ubicación impediría registrar el robo — que es el peor de
    /// los resultados posibles.
    /// </summary>
    public static void ExigirCustodiaConocida(TipoDeIncidente tipo, BienAfectado bien)
    {
        if (tipo is not TipoDeIncidente.RetencionPorAutoridad) return;
        if (!bien.SigueEnElRegistro) return;

        if (string.IsNullOrWhiteSpace(bien.AutoridadCustodia) ||
            string.IsNullOrWhiteSpace(bien.NumeroDeExpedienteExterno))
            throw new BloqueoDuro("RN-75",
                $"El bien «{bien.Descripcion}» está retenido por autoridad y el expediente no " +
                "dice cuál ni bajo qué número. En una retención eso se sabe —el acta lo dice— y " +
                "sin ello no hay a quién reclamarle la devolución.");
    }

    /// <summary>
    /// Descargar un bien del registro — `RN-75`.
    ///
    /// Es la <b>única salida que no es la recuperación</b>, y por eso exige el acto formal con
    /// número y autoridad. Sin él sería una baja sin respaldo sobre un bien del Estado.
    /// </summary>
    public static void ExigirDescargoFormal(BienAfectado bien, ConstanciaDeDescargo descargo)
    {
        if (bien.Estado is not EstadoDelBien.NoRecuperado)
            throw new BloqueoDuro("RN-75",
                $"El bien «{bien.Descripcion}» está en {bien.Estado} y no se puede descargar: " +
                "el descargo saca del registro lo que sigue afuera, y esto ya salió por otra " +
                "vía.");

        if (string.IsNullOrWhiteSpace(descargo.Numero) ||
            string.IsNullOrWhiteSpace(descargo.Autoridad))
            throw new BloqueoDuro("RN-75",
                "El descargo de un bien del Estado exige acto formal con número y autoridad. " +
                "Sin él, esto es una baja sin respaldo: el bien deja el registro y nadie puede " +
                "decir contra qué documento salió.");
    }

    /// <summary>
    /// Resolver el expediente — `RN-75`.
    ///
    /// ── No se cierra con bienes afuera sin decirlo ──────────────────────────
    /// El bien permanece hasta su recuperación o su descargo. Cerrar el expediente con bienes no
    /// recuperados y sin declararlo los haría desaparecer de la vista sin que ninguno de los dos
    /// hechos hubiera ocurrido — el mismo abandono silencioso que `RN-97` persigue, un módulo
    /// más acá.
    /// </summary>
    public static void ExigirCierrePosible(
        ExpedienteDeIncidente expediente, string comoSeResolvio, string? declaracionDeBienes)
    {
        if (string.IsNullOrWhiteSpace(comoSeResolvio))
            throw new BloqueoDuro("RN-75",
                "Resolver un expediente de incidente exige decir cómo. Sin eso, resolver es " +
                "indistinguible de archivar el problema.");

        if (expediente.EsInterrupcionSinDesenlace)
            throw new BloqueoDuro("RN-70",
                "Este expediente marca una interrupción en ruta y no tiene desenlace. Toda " +
                "interrupción se resuelve con desenlace explícito antes de cerrar el " +
                "expediente: cerrarlo sin él dejaría la misión marcada como interrumpida para " +
                "siempre, sin decir cómo siguió.");

        var afuera = expediente.BienesNoRecuperados;

        if (afuera.Count == 0) return;
        if (!string.IsNullOrWhiteSpace(declaracionDeBienes)) return;

        var detalle = string.Join("; ", afuera.Select(b => b.Descripcion));

        throw new BloqueoDuro("RN-75",
            $"El expediente tiene {afuera.Count} bien(es) sin recuperar: {detalle}. El bien " +
            "permanece en el registro patrimonial hasta su recuperación o su descargo formal, y " +
            "ninguna de las dos cosas ocurrió. Se resuelven, o se declara explícitamente por " +
            "qué se cierra el expediente con ellos afuera — que no es lo mismo que ignorarlos.");
    }
}
