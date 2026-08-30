namespace Sigti.Dominio.Reglas;

/// <summary>
/// La tercera parte de `R-3` — <b>cuál es el camino de salida</b>.
///
/// ── Por qué el servidor ya no alcanzaba ─────────────────────────────────────
/// `R-3` exige que una pantalla de bloqueo tenga <b>tres partes</b>: qué se impidió, por qué
/// exactamente —con nombres y números— y cómo salir. Los mensajes que el dominio ya emitía
/// cubren las dos primeras: dicen la placa, la categoría que falta, el saldo y el monto.
///
/// <b>La tercera no la tenía nadie.</b> Y es la que decide qué pasa después: <i>«un mensaje
/// genérico produce una llamada a soporte; un mensaje preciso produce la acción correcta»</i>.
/// Sin ella, quien queda bloqueado sabe que no puede seguir y no sabe a quién buscar, así que
/// busca a quien tenga más cerca — y con frecuencia esa persona tampoco puede.
///
/// ── Nulo cuando no está documentado ─────────────────────────────────────────
/// Devolver «comuníquese con el administrador» para todo lo que no se conoce sería peor que no
/// decir nada: convertiría el silencio en una instrucción, y la instrucción sería falsa la
/// mayoría de las veces. Lo que no está documentado se declara sin documentar.
/// </summary>
public static class ReglasDeLaSalida
{
    /// <summary>
    /// Qué puede hacer quien quedó bloqueado. <b>Nulo cuando no hay camino documentado.</b>
    /// </summary>
    public static CaminoDeSalida? De(string precondicion)
    {
        if (Documentados.TryGetValue(precondicion, out var camino)) return camino;

        // Toda regla de negocio tiene ficha, y la ficha es un camino real aunque genérico:
        // dice qué exige la regla y de qué norma sale. No es tan bueno como un camino
        // concreto, y es verdadero — que es la condición para ponerlo.
        if (precondicion.StartsWith("RN-", StringComparison.Ordinal))
            return new CaminoDeSalida(
                precondicion,
                Titulo: precondicion,
                SeEvaluaEn: "según la regla",
                "Esta regla no tiene un camino de salida documentado todavía. Su ficha dice qué " +
                "exige y de qué norma sale.",
                AQuienAcudir: null,
                Ficha: $"docs/01-negocio/reglas/{precondicion}");

        return null;
    }

    /// <summary>Todo lo que tiene camino documentado, para el catálogo de `PT-004`.</summary>
    public static IReadOnlyList<CaminoDeSalida> Todos => [.. Documentados.Values];

    /// <summary>
    /// Los caminos concretos, transcritos de la ficha de cada precondición en §10.2 sección 4.
    ///
    /// <b>Sólo entran los que la autoridad respalda.</b> Un camino inventado manda a alguien a
    /// una oficina que no resuelve nada, y lo hace con la confianza de estar leyendo al sistema.
    /// </summary>
    private static readonly Dictionary<string, CaminoDeSalida> Documentados = new()
    {
        // ── §10.2 sección 4 ─────────────────────────────────────────────────
        ["BD-01"] = new("BD-01", "Segregación entre solicitante y autorizador", "T-05, T-06",
            "Quien solicitó no puede autorizar la misma misión. La autorización tiene que " +
            "ejercerla otro puesto con competencia, o escalarse al puesto superior.",
            "El puesto superior de su misma unidad, o la Gerencia Administrativa.",
            "docs/03-arquitectura/estados/orden-de-mision.md"),

        ["BD-02"] = new("BD-02", "Licencia habilitante y vigente durante todo el rango", "T-08, T-10, T-12, T-17",
            "El motorista no tiene la categoría de licencia que el vehículo exige, o está " +
            "vencida dentro del rango de la misión. Se asigna otro motorista habilitado, o se " +
            "actualiza la licencia en el padrón si la renovó y no está cargada.",
            "Jefe de Transporte, para reasignar. Motoristas y habilitación (M-05), para cargar " +
            "la licencia vigente.",
            "docs/03-arquitectura/estados/orden-de-mision.md"),

        ["BD-03"] = new("BD-03", "Documentación del vehículo vigente", "T-08, T-10, T-12",
            "El vehículo tiene documentación vencida. Se asigna otro vehículo, o se carga el " +
            "documento renovado en su expediente.",
            "Jefe de Transporte. La renovación en sí la gestiona quien tenga la custodia.",
            "docs/03-arquitectura/estados/orden-de-mision.md"),

        ["BD-04"] = new("BD-04", "Salida en día u hora inhábil sin permiso de la máxima autoridad", "T-12, T-17",
            "La salida cae en día u hora inhábil y no hay permiso firmado. Circular así traslada " +
            "la responsabilidad a quien autorizó: hay que obtener el permiso de la máxima " +
            "autoridad, que genera el salvoconducto impreso.",
            "Máxima autoridad. El salvoconducto va impreso con el vehículo — sin papel no sirve.",
            "docs/03-arquitectura/estados/orden-de-mision.md"),

        ["BD-05"] = new("BD-05", "Coherencia del odómetro", "T-14, T-18",
            "La lectura del odómetro no es coherente con la anterior. No se corrige el número " +
            "registrado: se verifica la lectura contra el tablero y, si el asiento previo estaba " +
            "mal, se reversa con motivo y autor.",
            "Encargado de Despacho, que es quien lee el odómetro en el predio.",
            "docs/03-arquitectura/estados/orden-de-mision.md"),

        ["BD-06"] = new("BD-06", "Segregación de funciones operativas", "T-12, T-19, T-21, T-22 y la entrega de fondo",
            "Quien pretende ejecutar ya ejecutó otro acto incompatible en esta misma misión. El " +
            "acto lo ejecuta otro puesto, o se escala.",
            "El puesto superior de su misma unidad, o la Gerencia Administrativa.",
            "docs/01-negocio/actores-y-roles.md"),

        ["BD-07"] = new("BD-07", "Estado y compatibilidad del vehículo", "T-08, T-10",
            "El vehículo no está DISPONIBLE, o su tipo no es compatible con lo que se va a " +
            "mover. Se elige otro vehículo del tipo que corresponde — el tipo es el eje de " +
            "compatibilidad, no la marca ni el modelo.",
            "Jefe de Transporte.",
            "docs/03-arquitectura/estados/orden-de-mision.md"),

        ["BD-08"] = new("BD-08", "Sin divergencias de sincronización pendientes", "T-19",
            "La misión tiene divergencias de sincronización sin resolver, y liquidar sobre dos " +
            "versiones del retorno produce un número que no significa nada. Primero se resuelve " +
            "la cola de conflictos.",
            "Quien capturó en campo, para decidir cuál versión vale. La cola de conflictos es " +
            "de M-16.",
            "docs/03-arquitectura/estados/orden-de-mision.md"),

        ["BD-09"] = new("BD-09", "Compatibilidad entre lo solicitado y el tipo de vehículo", "T-02",
            "El tipo de vehículo pedido no puede mover lo que se declara. Se corrige el objeto " +
            "del traslado o el tipo requerido, contra la matriz de compatibilidad de M-02.",
            "Quien captura la solicitud.",
            "docs/03-arquitectura/estados/orden-de-mision.md"),

        // La ficha nombra la salida explícitamente, y por eso vale citarla: `T-10`.
        ["BD-10"] = new("BD-10", "Disponibilidad del motorista", "T-08, T-10, T-12",
            "El motorista no está disponible en la ventana: vacaciones, permiso, incapacidad, " +
            "otra misión o habilitación suspendida. La misión se cubre con otro sin perder la " +
            "trazabilidad de la asignación original — es `T-10`, no una reasignación cualquiera.",
            "Jefe de Transporte.",
            "docs/03-arquitectura/estados/orden-de-mision.md"),

        ["BD-11"] = new("BD-11", "Sin solapamiento de reserva", "T-08, T-10",
            "La reserva se solapa con otra ya existente. Se mueve la ventana, se elige otro " +
            "recurso, o se consolidan las dos misiones si son compatibles.",
            "Jefe de Transporte.",
            "docs/03-arquitectura/estados/orden-de-mision.md"),

        ["BD-12"] = new("BD-12", "Restricciones médicas compatibles con las condiciones de la misión", "T-08, T-10, T-12, T-17",
            "El motorista tiene una restricción médica incompatible con las condiciones de la " +
            "misión. Se asigna otro; la restricción no se levanta desde acá.",
            "Jefe de Transporte, para reasignar.",
            "docs/03-arquitectura/estados/orden-de-mision.md"),

        ["BD-13"] = new("BD-13", "Custodia vigente del vehículo al despachar", "T-12",
            "El vehículo no tiene custodia vigente, y sin custodio no hay de quién recibir el " +
            "bien ni a quién devolverlo. Se registra la custodia antes de despachar.",
            "Jefe de Transporte, que es quien asigna la custodia del vehículo.",
            "docs/01-negocio/reglas/RN-22-custodia-del-vehiculo.md"),

        // ── El estado operativo del vehículo ────────────────────────────────
        ["§10.2"] = new("§10.2", "Transición de estado operativo del vehículo", "W-01 a W-19",
            "La transición de estado del vehículo que se pidió no existe en §10.2, o el " +
            "vehículo todavía tiene misiones abiertas. El mensaje enumera a qué estados sí se " +
            "puede ir desde donde está.",
            "Jefe de Transporte.",
            "docs/03-arquitectura/estados/orden-de-mision.md"),
    };
}

/// <param name="AQuienAcudir">
/// Nulo cuando no se sabe quién resuelve. <b>No se rellena con «el administrador»</b>: el
/// administrador del sistema no tiene acceso al negocio y no puede resolver un bloqueo de
/// negocio, así que mandarlo ahí sólo agrega un paso.
/// </param>
/// <param name="Ficha">Dónde está escrita la regla, para quien quiera leerla entera.</param>
public sealed record CaminoDeSalida(
    string Precondicion,
    /// <summary>El título de su ficha en la autoridad, transcrito.</summary>
    string Titulo,
    /// <summary>
    /// En qué transiciones se evalúa. Sirve para contestar «¿por qué me apareció ahora?»:
    /// la misma precondición se revalida en varios momentos, y `BD-02` a propósito lo hace en
    /// el despacho aunque ya haya pasado en la programación — una licencia vence entre una y otra.
    /// </summary>
    string SeEvaluaEn,
    string QuePuedeHacer,
    string? AQuienAcudir,
    string? Ficha);
