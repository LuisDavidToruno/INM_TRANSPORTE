namespace Sigti.Dominio.M13_Cierre;

/// <summary>
/// Los eslabones de la cadena que `RN-08` exige recorrer de una punta a la otra.
///
/// <b>El orden es el de la regla</b>, y no es decorativo: el auditor del TSC no pide
/// comprobantes sueltos, pide <i>«poder recorrer la cadena de una punta a la otra sobre un
/// expediente concreto»</i>. Una lista desordenada obliga a reconstruir mentalmente qué viene
/// después de qué, que es justo el trabajo que la cadena existe para ahorrar.
/// </summary>
public enum EslabonDeLaCadena
{
    Solicitud,
    Autorizacion,
    OrdenDeMision,
    AsignacionDeVehiculoYMotorista,
    BitacoraConOdometros,
    Combustible,
    Peajes,
    Liquidacion,
}

/// <summary>
/// En qué estado está un eslabón.
///
/// ── ⚠️ Cuatro estados, y los cuatro hacen falta ─────────────────────────────
/// <b>«No aplicable» no es «presente»</b> — `RN-08` es explícita: <i>«los eslabones no
/// aplicables se marcan como tales con fundamento, no se dan por cumplidos»</i>. Y el caso que
/// la regla nombra: <i>«lo que no se admite es cerrarlo como presente con consumo cero»</i>.
///
/// <b>«Pendiente de sincronización» no es «ausente»</b> — y esa diferencia decide si el
/// expediente se cierra con hallazgo o <b>no se cierra todavía</b>: <i>«no se cierra con
/// hallazgo por falta de datos que están en camino»</i> (`RN-50`). Marcar de hallazgo un
/// expediente cuya bitácora está en el teléfono de un motorista en Tocoa es acusar a alguien
/// de una omisión que no cometió.
/// </summary>
public enum EstadoDelEslabon
{
    Presente,

    /// <summary>Falta, y no está en camino. <b>Esto sí es hallazgo</b> — `H-09`.</summary>
    Ausente,

    /// <summary>
    /// No corresponde a esta misión — combustible en una misión sin consumo, peajes en ruta sin
    /// casetas. <b>Exige fundamento</b>: sin él es indistinguible de una omisión.
    /// </summary>
    NoAplicable,

    /// <summary>
    /// Hay datos de campo de esta misión sin sincronizar. <b>Bloquea el cierre</b>, no produce
    /// hallazgo: los datos están en camino y cerrar ahora fabricaría una falta inexistente.
    /// </summary>
    PendienteDeSincronizacion,
}

/// <param name="Detalle">
/// Qué se encontró, o qué falta. En los <b>no aplicables</b> es el <b>fundamento</b>, que
/// `RN-08` exige: <i>«se marcan como tales con fundamento, no se dan por cumplidos»</i>.
/// </param>
public sealed record EslabonEvaluado(
    EslabonDeLaCadena Eslabon,
    string Nombre,
    EstadoDelEslabon Estado,
    string Detalle);

/// <summary>
/// La <b>lista de verificación de la cadena</b> que `RN-08` manda presentar al liquidador,
/// eslabón por eslabón.
/// </summary>
public sealed record CadenaDeTrazabilidad(IReadOnlyList<EslabonEvaluado> Eslabones)
{
    /// <summary>Los que faltan y no están en camino. <b>Cada uno es motivo de `H-09`.</b></summary>
    public IReadOnlyList<EslabonEvaluado> Faltantes =>
        [.. Eslabones.Where(e => e.Estado == EstadoDelEslabon.Ausente)];

    /// <summary>
    /// Los que esperan sincronización. <b>Bloquean, no marcan</b> — `RN-50`.
    /// </summary>
    public IReadOnlyList<EslabonEvaluado> EnCamino =>
        [.. Eslabones.Where(e => e.Estado == EstadoDelEslabon.PendienteDeSincronizacion)];

    /// <summary>
    /// Completa <b>de verdad</b>: sin faltantes y sin nada en camino.
    ///
    /// Los no aplicables no la rompen — pero sólo porque cada uno lleva su fundamento, que es
    /// lo que los separa de una omisión disfrazada.
    /// </summary>
    public bool Completa => Faltantes.Count == 0 && EnCamino.Count == 0;
}

/// <param name="Autorizada">
/// Si el expediente registra su autorización — `T-05`. Una misión ejecutada sin autorización
/// previa es <b>eslabón ausente y no subsanable</b>: `RN-08` es explícita en que no se fabrica
/// autorización retroactiva.
/// </param>
/// <param name="Folio">
/// El folio con que la orden se identifica. <b>Nunca nulo</b>: cuando no hay oficial, es el
/// provisional, que igual identifica el documento.
/// </param>
/// <param name="FolioOficial">
/// ⚠️ Si consumió folio del rango de la delegación.
///
/// <b>Falso NO es eslabón ausente.</b> Hoy ninguna delegación tiene rango asignado —es
/// configuración pendiente, insumo #34— y el sistema decidió a propósito que eso no bloquee.
/// Marcarlo faltante produciría un hallazgo en <b>todos</b> los expedientes por una tabla que
/// nadie cargó, y un control que produce hallazgos falsos en masa muere en tres meses.
/// </param>
/// <param name="OdometroDeSalida">
/// Las dos lecturas. <b>Nulas por separado</b>: sin la de salida no hay kilometraje, y sin la
/// de retorno tampoco — decir «hay bitácora» con una sola sería contar media cadena.
/// </param>
/// <param name="ValesDeLaMision">
/// Cuántas asignaciones de combustible tiene. <b>Cero no es «falta el eslabón»</b>: puede ser
/// un traslado corto con el tanque ya cargado, y ahí el eslabón es <i>no aplicable</i> — lo que
/// `RN-08` no admite es marcarlo <i>presente</i> con consumo cero.
/// </param>
/// <param name="CrucesAutorizados">
/// Cuántos cruces de peaje congeló la ruta autorizada. <b>Cero es ruta sin casetas</b>, y ahí
/// el eslabón no aplica con fundamento.
/// </param>
/// <param name="PasosRegistrados">
/// Cuántos pasos por caseta se registraron. Con cruces autorizados y ningún paso, falta el
/// eslabón — la misión atravesó peajes y nadie los registró.
/// </param>
/// <param name="HechosSinSincronizar">
/// Cuántos hechos de campo de esta misión siguen sin llegar o sin resolverse. <b>Mayor que cero
/// bloquea</b>, y por eso va aparte de todo lo demás.
/// </param>
public sealed record HechosDeLaCadena(
    bool Autorizada,
    string Folio,
    bool FolioOficial,
    bool ConVehiculoYMotorista,
    int? OdometroDeSalida,
    int? OdometroDeRetorno,
    int ValesDeLaMision,
    int CrucesAutorizados,
    int PasosRegistrados,
    bool Liquidada,
    int HechosSinSincronizar);

/// <summary>
/// `RN-08` — la cadena de trazabilidad para cerrar.
///
/// ── Por qué la salida por hallazgo no es una debilidad ──────────────────────
/// Es lo que hace funcionar la regla, y la propia `RN-08` lo dice: <i>«un sistema que no
/// permite cerrar expedientes imperfectos acumula expedientes abiertos que nadie mira, y los
/// hallazgos quedan invisibles. Es preferible cerrar señalando la falta»</i>.
///
/// ── ⚠️ Nivel de verificación ────────────────────────────────────────────────
/// `RN-08` es <b>`[I]`</b>: la cadena de eslabones es una <i>implicación de requerimiento</i>
/// escrita por el equipo sobre `NRM-01`, no articulado citable. Se corrigió desde `[V]` por la
/// regla de no escalar el nivel (`HN1-06`). Lo que sí es `[V]` es que el hallazgo típico del TSC
/// en flota es el consumo sin relación con el uso habitual.
/// </summary>
public static class ReglasDeLaCadena
{
    /// <summary>
    /// La clave del parámetro que configura qué eslabones se exigen — `RN-08`.
    ///
    /// <b>Con mínimo no desactivable</b>: la solicitud, la autorización y la liquidación no se
    /// pueden apagar por configuración, o la cadena deja de ser una cadena.
    /// </summary>
    public const string ClaveDeEslabonesExigidos = "eslabones_exigidos_para_cierre";

    public static CadenaDeTrazabilidad Evaluar(HechosDeLaCadena hechos)
    {
        // ⚠️ **Lo pendiente de sincronizar tiñe los eslabones de campo, no todos.** La solicitud
        // y la autorización nacieron en la oficina: declararlas «en camino» porque un teléfono
        // no ha sincronizado sería mover el problema a donde no está.
        var enCamino = hechos.HechosSinSincronizar > 0;

        return new(
        [
            new(EslabonDeLaCadena.Solicitud, "Solicitud", EstadoDelEslabon.Presente,
                "El expediente existe: la solicitud es su primer asiento."),

            hechos.Autorizada
                ? new(EslabonDeLaCadena.Autorizacion, "Autorización", EstadoDelEslabon.Presente,
                    "La autorización está asentada en el diario del expediente.")

                // No subsanable, y se dice: `RN-08` es explícita en que **no se fabrica
                // autorización retroactiva**. Quien lea esto tiene que saber que no hay nada
                // que ir a buscar.
                : new(EslabonDeLaCadena.Autorizacion, "Autorización", EstadoDelEslabon.Ausente,
                    "La misión se ejecutó sin autorización previa asentada. No es subsanable: " +
                    "no se fabrica autorización retroactiva (RN-08)."),

            new(EslabonDeLaCadena.OrdenDeMision, "Orden de misión", EstadoDelEslabon.Presente,
                hechos.FolioOficial
                    ? $"Folio {hechos.Folio}."

                    // Se dice, y no se calla ni se marca de hallazgo: el documento existe y su
                    // numeración oficial está pendiente de una configuración de despliegue.
                    : $"Folio provisional {hechos.Folio}: la delegación no tiene rango de " +
                      "folios asignado. El documento existe; su numeración oficial espera esa " +
                      "configuración."),

            hechos.ConVehiculoYMotorista
                ? new(EslabonDeLaCadena.AsignacionDeVehiculoYMotorista, "Vehículo y motorista",
                    EstadoDelEslabon.Presente,
                    "El diario registra los dos. Se lee de ahí y no de la reserva vigente: una " +
                    "misión liquidada ya no sostiene ninguna, y la pregunta del cierre es qué " +
                    "tomó mientras corría.")
                : new(EslabonDeLaCadena.AsignacionDeVehiculoYMotorista, "Vehículo y motorista",
                    EstadoDelEslabon.Ausente,
                    "El expediente no tiene reserva de vehículo y motorista. Sin ella no hay a " +
                    "quién ni a qué atribuir lo que ocurrió en ruta."),

            Bitacora(hechos, enCamino),
            Combustible(hechos, enCamino),
            Peajes(hechos, enCamino),

            hechos.Liquidada
                ? new(EslabonDeLaCadena.Liquidacion, "Liquidación", EstadoDelEslabon.Presente,
                    "La liquidación está asentada.")
                : new(EslabonDeLaCadena.Liquidacion, "Liquidación", EstadoDelEslabon.Ausente,
                    "El expediente no registra liquidación."),
        ]);
    }

    /// <summary>
    /// La bitácora con <b>los dos odómetros</b>.
    ///
    /// Se exigen los dos porque uno solo no produce kilometraje, y el kilometraje es el ancla de
    /// toda la conciliación: sin él, `RN-30` no puede dictaminar y `H-01` no se puede evaluar.
    /// </summary>
    private static EslabonEvaluado Bitacora(HechosDeLaCadena hechos, bool enCamino)
    {
        const EslabonDeLaCadena cual = EslabonDeLaCadena.BitacoraConOdometros;
        const string nombre = "Bitácora con odómetros";

        if (hechos is { OdometroDeSalida: { } salida, OdometroDeRetorno: { } retorno })
            return new(cual, nombre, EstadoDelEslabon.Presente, $"Salida {salida} km, retorno {retorno} km.");

        // ⚠️ **En camino no es ausente.** La bitácora de una misión larga viaja en el teléfono
        // del motorista, y marcar hallazgo por eso acusa de una omisión que no ocurrió.
        if (enCamino)
        {
            return new(cual, nombre, EstadoDelEslabon.PendienteDeSincronizacion,
                $"Hay {hechos.HechosSinSincronizar} hecho(s) de campo de esta misión sin " +
                "sincronizar. No se cierra por falta de datos que están en camino (RN-50).");
        }

        var falta = hechos.OdometroDeSalida is null
            ? hechos.OdometroDeRetorno is null ? "las dos lecturas" : "la lectura de salida"
            : "la lectura de retorno";

        return new(cual, nombre, EstadoDelEslabon.Ausente,
            $"Falta {falta} del odómetro. Sin las dos no hay kilometraje, y el kilometraje es " +
            "el ancla de toda la conciliación.");
    }

    /// <summary>
    /// El combustible. <b>Cero vales no es falta: puede ser que no aplique.</b>
    ///
    /// `RN-08` nombra el caso: <i>«misión de cortesía sin combustible ni peaje — traslado corto
    /// dentro de la ciudad con el tanque ya cargado. El eslabón se marca no aplicable con
    /// fundamento; lo que no se admite es cerrarlo como presente con consumo cero»</i>.
    /// </summary>
    private static EslabonEvaluado Combustible(HechosDeLaCadena hechos, bool enCamino)
    {
        const EslabonDeLaCadena cual = EslabonDeLaCadena.Combustible;
        const string nombre = "Combustible";

        if (hechos.ValesDeLaMision > 0)
        {
            return new(cual, nombre, EstadoDelEslabon.Presente,
                $"{hechos.ValesDeLaMision} asignación(es) de combustible vinculadas al expediente.");
        }

        if (enCamino)
        {
            return new(cual, nombre, EstadoDelEslabon.PendienteDeSincronizacion,
                "Hay hechos de campo sin sincronizar: un consumo registrado sin red todavía " +
                "puede llegar.");
        }

        // ⚠️ **No aplicable CON FUNDAMENTO**, que es lo que lo separa de una omisión. Y no
        // «presente con consumo cero», que es lo que `RN-08` prohíbe literalmente.
        return new(cual, nombre, EstadoDelEslabon.NoAplicable,
            "La misión no movió combustible institucional. El eslabón no aplica — no se da por " +
            "cumplido con consumo cero.");
    }

    /// <summary>
    /// Los peajes, <b>sólo si la ruta los atraviesa</b> — lo dice el enunciado de `RN-08`.
    ///
    /// El fundamento del «no aplica» sale de la <b>ruta autorizada congelada</b>, no de que
    /// nadie haya registrado pasos: si se dedujera de la ausencia de pasos, una misión que
    /// cruzó tres casetas sin registrar ninguna se declararía «sin casetas» sola.
    /// </summary>
    private static EslabonEvaluado Peajes(HechosDeLaCadena hechos, bool enCamino)
    {
        const EslabonDeLaCadena cual = EslabonDeLaCadena.Peajes;
        const string nombre = "Peajes";

        if (hechos.CrucesAutorizados == 0)
        {
            return new(cual, nombre, EstadoDelEslabon.NoAplicable,
                "La ruta autorizada no atraviesa puntos de peaje.");
        }

        if (hechos.PasosRegistrados > 0)
        {
            return new(cual, nombre, EstadoDelEslabon.Presente,
                $"{hechos.PasosRegistrados} paso(s) registrados contra " +
                $"{hechos.CrucesAutorizados} cruce(s) autorizados.");
        }

        if (enCamino)
        {
            return new(cual, nombre, EstadoDelEslabon.PendienteDeSincronizacion,
                "La ruta atraviesa peajes y hay hechos de campo sin sincronizar: los pasos " +
                "todavía pueden llegar.");
        }

        return new(cual, nombre, EstadoDelEslabon.Ausente,
            $"La ruta autorizada atraviesa {hechos.CrucesAutorizados} cruce(s) de peaje y no se " +
            "registró ninguno. La falta del ticket advierte y no bloquea (NRM-10), pero la del " +
            "paso entero deja la ruta sin rastro.");
    }
}
