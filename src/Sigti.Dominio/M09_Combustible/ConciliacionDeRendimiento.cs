namespace Sigti.Dominio.M09_Combustible;

/// <summary>
/// De dónde salió el rendimiento esperado. <b>Nunca se pierde por el camino.</b>
///
/// Un dictamen calculado contra una propuesta del propio histórico y otro calculado contra el
/// valor que fijó la institución <b>no valen lo mismo</b>, y el segundo es el único que
/// sostiene un hallazgo. Guardar sólo el número los volvería indistinguibles.
/// </summary>
public enum OrigenDelRendimiento
{
    /// <summary>Lo fijó la institución con vigencia. Es el único que sostiene un hallazgo firme.</summary>
    Institucional,

    /// <summary>De la ficha técnica del fabricante o del tipo. Provisional hasta tener histórico.</summary>
    Fabricante,

    /// <summary>
    /// Calculado del histórico del <b>propio vehículo</b>, y marcado como propuesta —`RN-30`
    /// punto 1 lo autoriza expresamente.
    ///
    /// ⚠️ <b>Compara el vehículo consigo mismo.</b> Un vehículo defraudado de forma constante
    /// se ve conforme contra su propia media: el patrón aparece en el agregado, no en la misión
    /// aislada. Por eso el origen viaja en el dictamen.
    /// </summary>
    PropuestoDelHistorico,
}

/// <param name="KmPorGalon">Cuántos kilómetros se esperan por galón.</param>
/// <param name="Version">
/// Qué tabla se usó. Va al asiento: una conciliación que no dice contra qué se juzgó no se
/// puede rehacer, y `RN-30` se evalúa contra el esperado <b>vigente a la fecha del hecho</b>.
/// </param>
public sealed record RendimientoEsperado(
    decimal KmPorGalon,
    OrigenDelRendimiento Origen,
    string Version);

/// <summary>
/// Los dos umbrales, <b>independientes</b>.
///
/// `RN-30` punto 2 es explícito: <i>«Un umbral único simétrico es un error de diseño: un exceso
/// de consumo del 20% y un ahorro del 20% no significan lo mismo»</i>. Un exceso puede ser
/// terreno de montaña; un ahorro imposible casi siempre es <b>un despacho que no se registró</b>.
/// </summary>
/// <param name="ToleranciaInferior">
/// Cuánto puede caer el rendimiento observado bajo el esperado antes de ser hallazgo. Fracción:
/// <c>0.20m</c> son veinte por ciento.
/// </param>
/// <param name="ToleranciaSuperior">Cuánto puede superarlo.</param>
public sealed record UmbralesDeDesviacion(decimal ToleranciaInferior, decimal ToleranciaSuperior);

public enum DictamenDeConciliacion
{
    /// <summary>
    /// <b>No se pudo calcular.</b> Faltan kilómetros, faltan galones, o no hay rendimiento
    /// esperado cargado.
    ///
    /// No es «conforme». Es la distinción que sostiene todo lo demás: un control que devuelve
    /// «dentro de umbral» cuando no pudo comparar nada es peor que no existir, porque además
    /// tranquiliza.
    /// </summary>
    NoEvaluable,

    /// <summary>
    /// Se calculó, y <b>el resultado no significa nada</b> — `RN-30`: odómetro averiado, saldo
    /// de tanque arrastrado, nivel muy distinto al inicial.
    ///
    /// Se conserva igual: la regla manda guardarlo <i>«para el análisis agregado, que sí es
    /// válido»</i>.
    /// </summary>
    NoConcluyente,

    DentroDeUmbral,

    /// <summary>
    /// Rendimiento <b>por debajo</b> del esperado: más galones de los que el recorrido
    /// justifica. Posible consumo no imputable a la misión.
    /// </summary>
    ConsumoExcesivo,

    /// <summary>
    /// Rendimiento <b>por encima</b> del esperado, y es el hallazgo que un control ingenuo no
    /// busca: menos galones de los que el recorrido exige.
    ///
    /// <b>Casi siempre significa un despacho que no se registró</b> — el vehículo cargó
    /// combustible que nadie anotó, y por eso los galones registrados no alcanzan a explicar
    /// los kilómetros.
    /// </summary>
    RendimientoImposible,
}

/// <summary>Lo que invalida el cálculo sin invalidar el registro — `RN-30`.</summary>
public sealed record ReparosDelCalculo(
    /// <summary>`RN-90`: el instrumento intervenido no mide, y su lectura no divide nada.</summary>
    bool OdometroAveriado = false,
    /// <summary>
    /// El vehículo salió con un nivel y volvió con otro muy distinto. Los galones consumidos
    /// no son los cargados, y la resta que falta es justo la que `RN-83` obliga a capturar.
    /// </summary>
    bool NivelDeTanqueDispar = false,
    /// <summary>
    /// Hubo espera prolongada con motor encendido. Consume sin recorrer, así que la desviación
    /// <b>no produce hallazgo por sí sola</b> — `RN-30` lo dice, y sin esa medición el hallazgo
    /// sería infundado.
    /// </summary>
    bool EsperaProlongadaRegistrada = false);

/// <param name="Desviacion">
/// Fracción sobre el esperado. Negativa es consumo de más; positiva, rendimiento de más.
/// </param>
public sealed record Conciliacion(
    DictamenDeConciliacion Dictamen,
    int KilometrosRecorridos,
    decimal GalonesConsumidos,
    decimal? RendimientoObservado,
    RendimientoEsperado? Esperado,
    decimal? Desviacion,
    string Evidencia)
{
    /// <summary>
    /// Si esto obliga a cerrar la misión con hallazgo. <b>`NoEvaluable` no lo hace</b> — no se
    /// puede declarar un hallazgo sobre una comparación que no ocurrió.
    /// </summary>
    public bool EsHallazgo =>
        Dictamen is DictamenDeConciliacion.ConsumoExcesivo
                 or DictamenDeConciliacion.RendimientoImposible;
}

/// <summary>
/// `RN-30` — el rendimiento observado contra el esperado, en las <b>dos</b> direcciones.
///
/// ── Lo que el auditor busca, y no es lo que se supone ───────────────────────
/// `NRM-01`, citado por la regla: <i>«el auditor no busca comprobantes, busca correlación entre
/// consumo, kilometraje y misión autorizada. Un sistema que solo archiva facturas no responde a
/// lo que se le va a preguntar»</i>. Este cálculo es esa correlación.
///
/// ── Por qué es puro ─────────────────────────────────────────────────────────
/// Recibe todo ya resuelto: kilómetros, galones, esperado y umbrales. Es lo que permite ejercer
/// los bordes —cero galones, esperado ausente, desviación exacta al umbral— sin montar una
/// misión, un vale y tres consumos para llegar a cada uno.
/// </summary>
public static class ReglasDeConciliacion
{
    /// <param name="kilometrosRecorridos">
    /// Los del <b>vehículo en esta misión</b>. `RN-30`: el recorrido bajo tenencia ajena no entra,
    /// y en una sustitución <b>cada vehículo se concilia por separado</b> — un agregado de la
    /// misión mezclaría dos rendimientos y no significaría nada.
    /// </param>
    /// <param name="galonesConsumidos">
    /// <b>Todos</b> los abastecimientos, cualquiera sea su fuente (`RN-83`).
    ///
    /// ⚠️ <b>Hoy sólo llegan los del fondo</b>, que es lo único que el sistema registra. Un
    /// despacho desde el tanque institucional no pasa por ningún folio y <b>no existe para el
    /// cálculo</b> — y es exactamente lo que produce un rendimiento imposiblemente bueno. Sin
    /// `RN-83`, esta regla señala un síntoma cuya causa el sistema no puede registrar.
    /// </param>
    /// <param name="esperado">
    /// Nulo cuando la institución no lo fijó y no hay histórico del que proponerlo. <b>Nulo no
    /// es cero ni es conforme</b>: es que no hay contra qué comparar.
    /// </param>
    public static Conciliacion Evaluar(
        int kilometrosRecorridos,
        decimal galonesConsumidos,
        RendimientoEsperado? esperado,
        UmbralesDeDesviacion? umbrales,
        ReparosDelCalculo? reparos = null)
    {
        reparos ??= new ReparosDelCalculo();

        if (kilometrosRecorridos <= 0)
            return NoEvaluable(kilometrosRecorridos, galonesConsumidos, esperado,
                "sin kilómetros recorridos: no hay recorrido que dividir");

        if (galonesConsumidos <= 0)
            // Es el caso normal de la misión que salió con el tanque lleno y no cargó. No es un
            // defecto: es que esta misión no tiene consumo que conciliar.
            return NoEvaluable(kilometrosRecorridos, galonesConsumidos, esperado,
                "sin galones consumidos: la misión no cargó combustible");

        if (esperado is null)
            return NoEvaluable(kilometrosRecorridos, galonesConsumidos, esperado,
                "NO hay rendimiento esperado para este vehículo. La institución no lo ha fijado " +
                "y no hay histórico del que proponerlo, así que no hay contra qué comparar");

        if (umbrales is null)
            return NoEvaluable(kilometrosRecorridos, galonesConsumidos, esperado,
                "NO hay umbrales de desviación cargados. Sin ellos cualquier diferencia sería " +
                "hallazgo o ninguna lo sería, y las dos cosas son falsas");

        var observado = kilometrosRecorridos / galonesConsumidos;
        var desviacion = (observado - esperado.KmPorGalon) / esperado.KmPorGalon;

        var cuentas =
            $"{kilometrosRecorridos:N0} km / {galonesConsumidos:N2} gal = {observado:N2} km/gal " +
            $"contra {esperado.KmPorGalon:N2} esperado ({Etiqueta(esperado.Origen)}, " +
            $"{esperado.Version}) · desviación {desviacion:P1}";

        // Los reparos se evalúan DESPUÉS de calcular, no antes: `RN-30` manda conservar el
        // cálculo para el análisis agregado, «que sí es válido». Descartarlo perdería el dato
        // justo donde el patrón se ve.
        if (reparos.OdometroAveriado)
            return new Conciliacion(
                DictamenDeConciliacion.NoConcluyente, kilometrosRecorridos, galonesConsumidos,
                observado, esperado, desviacion,
                $"{cuentas} · NO CONCLUYENTE: el odómetro está intervenido o averiado (`RN-90`), " +
                "así que su lectura no divide nada. Se conserva para el análisis agregado");

        if (reparos.NivelDeTanqueDispar)
            return new Conciliacion(
                DictamenDeConciliacion.NoConcluyente, kilometrosRecorridos, galonesConsumidos,
                observado, esperado, desviacion,
                $"{cuentas} · NO CONCLUYENTE: el nivel del tanque a la salida y al retorno es muy " +
                "distinto, así que los galones consumidos no son los cargados");

        if (desviacion < -umbrales.ToleranciaInferior)
        {
            if (reparos.EsperaProlongadaRegistrada)
                // `RN-30`: «una desviación con espera prolongada registrada no produce hallazgo
                // por sí sola. Sin esa medición, el hallazgo sería infundado.»
                return new Conciliacion(
                    DictamenDeConciliacion.NoConcluyente, kilometrosRecorridos, galonesConsumidos,
                    observado, esperado, desviacion,
                    $"{cuentas} · consumo por encima del umbral, PERO hay espera prolongada con " +
                    "motor encendido registrada: consume sin recorrer, y por sí sola la " +
                    "desviación no es hallazgo");

            return new Conciliacion(
                DictamenDeConciliacion.ConsumoExcesivo, kilometrosRecorridos, galonesConsumidos,
                observado, esperado, desviacion,
                $"{cuentas} · CONSUMO EXCESIVO: más galones de los que el recorrido justifica. " +
                $"Tolerancia inferior {umbrales.ToleranciaInferior:P0}. Posible consumo no " +
                "imputable a esta misión");
        }

        if (desviacion > umbrales.ToleranciaSuperior)
            return new Conciliacion(
                DictamenDeConciliacion.RendimientoImposible, kilometrosRecorridos, galonesConsumidos,
                observado, esperado, desviacion,
                $"{cuentas} · RENDIMIENTO IMPOSIBLE: menos galones de los que el recorrido exige. " +
                $"Tolerancia superior {umbrales.ToleranciaSuperior:P0}. Casi siempre significa un " +
                "despacho de combustible que no se registró");

        return new Conciliacion(
            DictamenDeConciliacion.DentroDeUmbral, kilometrosRecorridos, galonesConsumidos,
            observado, esperado, desviacion, cuentas);
    }

    /// <summary>
    /// `RN-30` punto 1 — el sistema <b>propone</b> el esperado del histórico del propio vehículo,
    /// <i>«marcando la propuesta como tal»</i>.
    ///
    /// ── Por qué esto no es hacer trampa con un `[C]` ────────────────────────
    /// Porque la regla lo autoriza y porque la alternativa es peor: sin ningún esperado, la
    /// conciliación no corre <b>nunca</b> y el control existe sin funcionar. Con la propuesta
    /// corre, y el origen viaja en el dictamen para que nadie confunda una media propia con el
    /// número que fijó la institución.
    ///
    /// ── Y lo que la propuesta NO puede ver ──────────────────────────────────
    /// <b>Compara el vehículo consigo mismo.</b> Si el desvío es constante desde siempre, la
    /// media ya lo incorpora y todo se ve conforme. Eso no se arregla con más datos del mismo
    /// vehículo: se arregla con el valor institucional y con el agregado por dependencia.
    /// </summary>
    /// <param name="historico">Pares de kilómetros y galones de misiones ya conciliadas.</param>
    /// <param name="minimoDeMisiones">
    /// Cuántas hacen falta para que la media signifique algo. Con dos misiones, una carga
    /// atípica mueve la media entera y la propuesta diría más de lo que sabe.
    /// </param>
    public static RendimientoEsperado? ProponerDelHistorico(
        IReadOnlyList<(int Kilometros, decimal Galones)> historico,
        int minimoDeMisiones = 5)
    {
        var utiles = historico.Where(h => h.Kilometros > 0 && h.Galones > 0).ToList();

        if (utiles.Count < minimoDeMisiones) return null;

        // Kilómetros totales sobre galones totales, no el promedio de los rendimientos: una
        // misión de 40 km pesaría lo mismo que una de 900, y la media saldría del viaje corto.
        var kilometros = utiles.Sum(h => h.Kilometros);
        var galones = utiles.Sum(h => h.Galones);

        return new RendimientoEsperado(
            kilometros / galones,
            OrigenDelRendimiento.PropuestoDelHistorico,
            $"PROPUESTA-DEL-HISTORICO-{utiles.Count}-MISIONES");
    }

    private static Conciliacion NoEvaluable(
        int kilometros, decimal galones, RendimientoEsperado? esperado, string porQue) =>
        new(DictamenDeConciliacion.NoEvaluable, kilometros, galones,
            RendimientoObservado: null, esperado, Desviacion: null,
            Evidencia: $"NO EVALUABLE: {porQue}");

    private static string Etiqueta(OrigenDelRendimiento origen) => origen switch
    {
        OrigenDelRendimiento.Institucional => "fijado por la institución",
        OrigenDelRendimiento.Fabricante => "de ficha técnica, provisional",
        // Se nombra en el asiento, no sólo en el enum: quien lea la conciliación dentro de dos
        // años tiene que saber que se comparó al vehículo consigo mismo.
        OrigenDelRendimiento.PropuestoDelHistorico => "PROPUESTA del propio histórico",
        _ => origen.ToString(),
    };
}
