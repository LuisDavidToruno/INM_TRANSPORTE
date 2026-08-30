namespace Sigti.Dominio.M03_Flota;

/// <summary>
/// El estado de la <b>lámina física</b> — `RN-64`, catálogo configurable `estado_de_placa`.
///
/// ── Por qué no basta con «tiene placa o no» ─────────────────────────────────
/// Son dos datos distintos y no intercambiables: el <b>número asignado en el registro</b>, que
/// puede existir aunque la lámina no, y el <b>estado de la lámina</b>.
///
/// Un vehículo con número asignado y sin lámina, uno con la lámina retenida por la DNVT y uno
/// que nunca tuvo número son <b>tres situaciones administrativas distintas con tres
/// tratamientos distintos</b>, y con un campo `placa` vacío las tres se ven iguales.
///
/// La distinción decide dos cosas concretas: qué se imprime en el paquete que viaja con el
/// vehículo (`RN-65`) y contra qué se concilia una imputación externa (`RN-66`).
/// </summary>
public enum EstadoDePlaca
{
    /// <summary>La lámina está puesta. El caso normal, y el único que no exige respaldo.</summary>
    ConLamina,

    /// <summary>
    /// El registro asignó número y la lámina no llegó — <b>desabastecimiento nacional</b>.
    ///
    /// Es el caso que `CE-17` documenta y la razón por la que un campo `placa` obligatorio y
    /// único rompería el sistema.
    /// </summary>
    NumeroAsignadoSinLamina,

    /// <summary>Ni número ni lámina: el trámite de registro no ha concluido.</summary>
    SinNumeroAsignado,

    LaminaExtraviada,

    /// <summary>Retenida por autoridad — DNVT, en un operativo.</summary>
    LaminaRetenidaPorAutoridad,

    EnTramiteDeReposicion,
}

/// <summary>
/// El documento que sostiene la circulación de un vehículo sin lámina — `RN-65`.
///
/// ── Lo que bloquea no es la ausencia de placa: es la ausencia de respaldo ───
/// Un permiso provisional, una constancia del registro, un acta de retención. Con
/// <b>emisor, folio, adjunto y vigencia</b>.
/// </summary>
/// <param name="Adjunto">
/// El documento escaneado. <b>Nulo es que se declaró y no se adjuntó</b>, y eso no alcanza: el
/// agente en carretera pide el papel, y un respaldo que sólo existe como texto en una pantalla
/// no se le puede mostrar.
/// </param>
/// <param name="VigenteHasta">
/// ⚠️ <b>Nulo NO es «vigente para siempre»</b>: es un permiso provisional sin fecha de
/// vencimiento declarada, que es precisamente lo que hay que preguntar antes de despachar.
/// La regla lo trata como insuficiente, igual que uno vencido.
/// </param>
public sealed record RespaldoDePlaca(
    string Tipo,
    string Emisor,
    string Folio,
    Ulid? Adjunto,
    DateOnly VigenteDesde,
    DateOnly? VigenteHasta);

/// <summary>Por qué el respaldo no alcanza. <c>Ninguno</c> es que sí.</summary>
public enum MotivoDeRespaldoInsuficiente
{
    Ninguno,

    /// <summary>El vehículo no tiene lámina y no hay ningún documento que lo sostenga.</summary>
    SinRespaldo,

    /// <summary>Hay respaldo y <b>no cubre todo el rango de la misión</b>.</summary>
    VenceDentroDelRango,

    /// <summary>Hay respaldo y todavía no empieza a regir a la fecha de salida.</summary>
    NoRigeAlSalir,

    /// <summary>
    /// Hay respaldo, con vigencia, y <b>sin el documento adjunto</b>.
    ///
    /// El agente en carretera pide el papel: un respaldo que sólo existe como texto no se le
    /// puede mostrar.
    /// </summary>
    SinAdjunto,
}

/// <param name="VenceElQueBloquea">
/// La fecha que causó el bloqueo. <b>Nula cuando el motivo no es de fechas</b> — sin respaldo o
/// sin adjunto—, y eso no es un dato que falte: es que no hay fecha que mostrar.
/// </param>
public sealed record ResultadoDelRespaldo(
    bool Habilita,
    MotivoDeRespaldoInsuficiente Motivo,
    DateOnly? VenceElQueBloquea,
    string Detalle);

/// <summary>
/// `RN-65` — <b>lo que bloquea el despacho de un vehículo sin lámina no es la ausencia de
/// placa: es la ausencia de respaldo.</b>
///
/// ── El defecto que esta regla vino a corregir ───────────────────────────────
/// La documentación del vehículo tenía un booleano, <c>TieneConstanciaSustitutaDePlaca</c>, y
/// eso decía <b>que hay una constancia</b> y nada más. Una constancia vencida a mitad de la
/// misión pasaba exactamente igual que una vigente, y un permiso provisional de treinta días
/// emitido hace un año se veía idéntico a uno de la semana pasada.
///
/// ── Vigente en TODO el rango, extremos incluidos ────────────────────────────
/// Mismo patrón que `RN-10` para la licencia. Un respaldo que cubre tres de los cinco días de
/// la misión <b>no sirve</b>: el agente que revise el cuarto tiene enfrente un vehículo del
/// Estado sin lámina y sin nada que lo explique, y el problema ya no se puede arreglar desde
/// una oficina.
/// </summary>
public static class ReglasDelRespaldoDePlaca
{
    /// <summary>
    /// ¿El vehículo puede circular con lo que tiene?
    /// </summary>
    /// <param name="estado">
    /// El de la lámina física. <c>ConLamina</c> no exige respaldo: la lámina <b>es</b> la
    /// identificación.
    /// </param>
    /// <param name="respaldo">
    /// El documento vigente más reciente, si lo hay. <b>Nulo es que no hay ninguno.</b>
    /// </param>
    public static ResultadoDelRespaldo Evaluar(
        EstadoDePlaca estado,
        RespaldoDePlaca? respaldo,
        DateOnly salida,
        DateOnly finDelRango)
    {
        if (estado == EstadoDePlaca.ConLamina)
        {
            return new ResultadoDelRespaldo(
                true, MotivoDeRespaldoInsuficiente.Ninguno, null,
                "El vehículo tiene su lámina puesta: no requiere respaldo.");
        }

        if (respaldo is null)
        {
            return new ResultadoDelRespaldo(
                false, MotivoDeRespaldoInsuficiente.SinRespaldo, null,
                $"El vehículo está en estado {Texto(estado)} y no tiene ningún documento de " +
                "respaldo registrado. Sin lámina y sin respaldo no hay nada que identifique al " +
                "vehículo en carretera (RN-65).");
        }

        if (respaldo.VigenteDesde > salida)
        {
            return new ResultadoDelRespaldo(
                false, MotivoDeRespaldoInsuficiente.NoRigeAlSalir, respaldo.VigenteDesde,
                $"El respaldo {respaldo.Folio} rige desde el " +
                $"{respaldo.VigenteDesde:dd/MM/yyyy} y la misión sale el " +
                $"{salida:dd/MM/yyyy}. Todavía no ampara nada.");
        }

        // ⚠️ **Nulo no es «para siempre».** Un permiso provisional sin fecha de vencimiento
        // declarada es exactamente lo que hay que preguntar antes de despachar, no algo que se
        // deba dar por indefinido.
        if (respaldo.VigenteHasta is not { } hasta)
        {
            return new ResultadoDelRespaldo(
                false, MotivoDeRespaldoInsuficiente.VenceDentroDelRango, null,
                $"El respaldo {respaldo.Folio} no declara hasta cuándo vige. Un documento " +
                "provisional sin fecha de vencimiento no se puede dar por vigente: confírmelo " +
                "con el emisor y regístrelo.");
        }

        // Mismo patrón que `RN-10`: **extremos incluidos**. Uno que vence el último día del
        // rango sí cubre ese día.
        if (hasta < finDelRango)
        {
            return new ResultadoDelRespaldo(
                false, MotivoDeRespaldoInsuficiente.VenceDentroDelRango, hasta,
                $"El respaldo {respaldo.Folio} vence el {hasta:dd/MM/yyyy} y la misión " +
                $"termina el {finDelRango:dd/MM/yyyy}, holgura incluida. Un respaldo que cubre " +
                "parte del rango deja al vehículo sin identificación los días restantes.");
        }

        if (respaldo.Adjunto is null)
        {
            return new ResultadoDelRespaldo(
                false, MotivoDeRespaldoInsuficiente.SinAdjunto, null,
                $"El respaldo {respaldo.Folio} está declarado y no tiene el documento " +
                "adjunto. El agente en carretera pide el papel: uno que sólo existe como texto " +
                "en una pantalla no se le puede mostrar.");
        }

        return new ResultadoDelRespaldo(
            true, MotivoDeRespaldoInsuficiente.Ninguno, hasta,
            $"{respaldo.Tipo} {respaldo.Folio} de {respaldo.Emisor}, vigente hasta el " +
            $"{hasta:dd/MM/yyyy}.");
    }

    /// <summary>Cómo se nombra el estado en un documento que lee una persona.</summary>
    public static string Texto(EstadoDePlaca estado) => estado switch
    {
        EstadoDePlaca.ConLamina => "con lámina",
        EstadoDePlaca.NumeroAsignadoSinLamina => "número asignado, sin lámina",
        EstadoDePlaca.SinNumeroAsignado => "sin número asignado",
        EstadoDePlaca.LaminaExtraviada => "lámina extraviada",
        EstadoDePlaca.LaminaRetenidaPorAutoridad => "lámina retenida por autoridad",
        EstadoDePlaca.EnTramiteDeReposicion => "en trámite de reposición",
        _ => estado.ToString(),
    };
}
