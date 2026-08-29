namespace Sigti.Dominio.M18_Peajes;

/// <summary>
/// Cómo puede ser incoherente una secuencia de pasos — `RN-37` punto 2.
///
/// ── Cada una tiene su nombre a propósito ────────────────────────────────────
/// `NRM-01`: <i>el auditor no revisa archivos, cruza datos</i>. Un reporte que dijera
/// «incoherente» y nada más le entrega al auditor un número sin significado — el mismo error
/// que `CE-26` §3 señala del faltante sin tipificar.
/// </summary>
public enum TipoDeIncoherencia
{
    /// <summary>
    /// La secuencia salta sobre una caseta que había que cruzar.
    ///
    /// ── Por qué esto y no «cambió de sentido» ───────────────────────────────
    /// Cambiar de sentido es el <b>retorno</b>, y cambiarlo dos veces es una misión
    /// multi-destino (`CE-08`). Marcar eso produciría un hallazgo en cada misión del año —
    /// los <i>«hallazgos falsos en masa»</i> que `RN-37` advierte.
    ///
    /// Lo que de verdad no puede pasar es estar en el km 190 habiendo venido del km 35 sin
    /// haber cruzado el km 85. Y esa lectura además <b>encuentra la omisión</b>, que es la
    /// otra mitad de lo que el auditor busca: un paso que no se registró.
    /// </summary>
    SecuenciaGeograficamenteImposible,

    /// <summary>
    /// El intervalo entre dos casetas no es viable a la velocidad media máxima del tipo de
    /// vehículo. <b>Demasiado corto es físicamente imposible</b>; demasiado largo indica parada
    /// no registrada — y las dos cosas se dicen aparte porque significan cosas distintas.
    /// </summary>
    IntervaloInviable,

    /// <summary>
    /// Un peaje pagado en un punto que la ruta autorizada no atraviesa. `NRM-10`: <i>«un peaje
    /// de Yojoa en una misión autorizada a Choluteca es un hallazgo, y el sistema tiene que
    /// producirlo solo»</i>.
    /// </summary>
    PuntoFueraDeRutaAutorizada,

    /// <summary>Dos pasos por el mismo punto, en el mismo sentido, casi a la misma hora.</summary>
    PasoDuplicado,

    /// <summary>
    /// Los kilómetros de la bitácora no alcanzan para llegar a las casetas que se cruzaron.
    /// <b>La contradicción es doble</b> y `RN-30` y `RN-31` la señalan por su lado: si la misión
    /// declara 90 km y registra pasos por tres casetas separadas por cientos, una de las dos
    /// cifras está mal.
    /// </summary>
    PeajeSinKilometrajeQueLoRespalde,
}

/// <summary>
/// Una incoherencia concreta.
/// </summary>
/// <param name="Concluyente">
/// <b>Falso cuando el dato de base no es confiable.</b> `RN-37` casos límite: un reloj de
/// dispositivo desajustado fabrica intervalos imposibles, y presentar eso como hallazgo produce
/// exactamente el ruido que hace que en tres meses nadie mire el indicador.
/// </param>
/// <param name="Justificada">
/// Hay un desvío declarado desde el campo que la cubre. <b>Se conserva igual, no se borra</b>:
/// que la incoherencia existió y que alguien la explicó son dos hechos, y el auditor pregunta
/// por los dos.
/// </param>
public sealed record Incoherencia(
    TipoDeIncoherencia Tipo,
    string Explicacion,
    IReadOnlyList<Ulid> Pasos,
    bool Concluyente = true,
    bool Justificada = false,
    string? Justificacion = null)
{
    /// <summary>
    /// <b>Cuenta como hallazgo</b>: es concluyente y nadie la explicó.
    /// </summary>
    public bool EsHallazgo => Concluyente && !Justificada;
}

/// <summary>
/// Qué dimensiones se pudieron evaluar. <b>Va siempre</b>, y es la mitad del dictamen.
///
/// `RN-37` casos límite, sobre la misión de ruta abierta: <i>«la tercera validación no aplica;
/// las dos primeras sí. <b>Se marca así explícitamente para que la ausencia de hallazgos no se
/// lea como conformidad</b>»</i>. Es el mismo principio que gobierna todo el sistema: nulo no
/// es cero, y «no se evaluó» no es «salió limpio».
/// </summary>
/// <param name="Geografica">
/// Falso cuando algún punto no declara kilómetro. Sin él no hay orden geográfico contra el que
/// juzgar, y suponerlo por el orden de captura invertiría la respuesta.
/// </param>
/// <param name="Temporal">
/// Falso sin <c>velocidad_media_maxima_por_tipo_vehiculo</c> — `[C]`, o con el reloj del
/// dispositivo declarado no confiable.
/// </param>
/// <param name="ContraLaRutaAutorizada">
/// Falso cuando no hay estimado congelado: sin él no se sabe qué puntos autorizó quien
/// autorizó, y toda caseta parecería fuera de ruta.
/// </param>
/// <param name="ContraElKilometraje">
/// Falso cuando la bitácora no tiene los dos odómetros.
/// </param>
public sealed record DimensionesEvaluadas(
    bool Geografica,
    bool Temporal,
    bool ContraLaRutaAutorizada,
    bool ContraElKilometraje,
    IReadOnlyList<string> PorQueNo)
{
    public bool Todas =>
        Geografica && Temporal && ContraLaRutaAutorizada && ContraElKilometraje;
}

/// <summary>
/// El resultado del cruce — `RN-37`.
///
/// <b>Advertencia, nunca bloqueo.</b> La regla es explícita: <i>«no aplica como bloqueo durante
/// la ejecución: un paso incoherente ya ocurrió y debe registrarse tal cual. La regla observa,
/// no impide»</i>.
/// </summary>
public sealed record DictamenDeCoherencia(
    IReadOnlyList<Incoherencia> Incoherencias,
    DimensionesEvaluadas Dimensiones,
    int PasosEvaluados)
{
    public IReadOnlyList<Incoherencia> Hallazgos =>
        [.. Incoherencias.Where(i => i.EsHallazgo)];

    /// <summary>
    /// <b>Limpio de verdad</b> — sin hallazgos <i>y</i> con las cuatro dimensiones evaluadas.
    /// Un dictamen sin hallazgos que no pudo mirar nada no es conformidad: es silencio.
    /// </summary>
    public bool Coherente => Hallazgos.Count == 0 && Dimensiones.Todas;
}

/// <summary>
/// Un desvío declarado desde el campo — el mínimo que `RN-37` necesita de `RN-76`.
///
/// ── Por qué sin esto la regla no se puede encender ──────────────────────────
/// `RN-37`, casos límite, sobre el desvío por derrumbe o cierre de carretera: <i>«Honduras los
/// tiene con regularidad. La incoherencia geográfica será real y justificada... <b>Sin esa
/// capacidad, la regla produciría hallazgos falsos en masa</b>»</i>.
///
/// Un control que produce hallazgos falsos en masa muere en tres meses, igual que el
/// rendimiento inventado de `RN-30`. Por eso esto entra con la regla y no después.
///
/// ⚠️ <b>Es el mínimo, no `RN-76` completo.</b> La regla del estado en ruta declarado por el
/// motorista es más grande —incluye el estado del vehículo, las esperas en sitio y el
/// seguimiento de `M-19`—; acá sólo está el hecho que la liquidación consume.
/// </summary>
/// <param name="Desde">
/// Desde cuándo cubre. Es la <b>fecha del hecho</b>: el derrumbe ocurrió a una hora, no cuando
/// hubo señal para reportarlo.
/// </param>
public sealed record DesvioDeclarado(
    Ulid Id,
    Ulid Mision,
    Ulid Vehiculo,
    DateTimeOffset Desde,
    DateTimeOffset? Hasta,
    string Motivo)
{
    public bool Cubre(DateTimeOffset momento) =>
        Desde <= momento && (Hasta is null || momento <= Hasta);
}

/// <summary>
/// Un paso, reducido a lo que el cruce necesita. Se arma en la capa de aplicación porque el
/// punto y el paso viven en tablas distintas.
/// </summary>
/// <param name="Kilometro">
/// El kilómetro del punto dentro de su corredor. <b>Nulo deja la dimensión geográfica sin
/// evaluar</b> para toda la misión: un orden parcial es un orden inventado.
/// </param>
public sealed record PasoParaCruzar(
    Ulid Id,
    Ulid Punto,
    string NombreDelPunto,
    string? Corredor,
    int? Kilometro,
    Ulid Vehiculo,
    DateTimeOffset OcurridoEn,
    int Odometro);

/// <summary>
/// El cruce de `RN-37`: peaje × kilometraje × ruta autorizada.
///
/// ── Por qué esto importa más que la suma de montos ──────────────────────────
/// `NRM-10`, textual: <i>«El sistema debe correlacionar peaje × kilometraje × ruta autorizada.
/// Un peaje de Yojoa en una misión autorizada a Choluteca es un hallazgo, y el sistema tiene que
/// producirlo solo. <b>Esto es exactamente lo que busca el auditor del TSC: correlación, no
/// comprobantes archivados</b>»</i>.
///
/// El sistema tiene que llegar al cruce antes que él.
/// </summary>
public static class ReglasDeCoherenciaDeSecuencia
{
    /// <summary>Dos pasos por el mismo punto separados por menos de esto son un duplicado.</summary>
    private static readonly TimeSpan VentanaDeDuplicado = TimeSpan.FromMinutes(20);

    /// <param name="pasos">
    /// <b>De un solo vehículo.</b> `RN-37`: la secuencia se valida por vehículo y no por misión —
    /// en una sustitución en ruta, dos vehículos pueden pasar por la misma caseta a horas
    /// distintas legítimamente.
    /// </param>
    /// <param name="rutaAutorizada">
    /// Los puntos del estimado congelado al aprobar (`RN-35` punto 4, `RN-41`). <b>Nula</b>
    /// cuando no hay estimado congelado — misión de ruta abierta, o aprobada antes de que el
    /// estimado existiera—, y entonces la tercera dimensión no se evalúa en vez de marcar toda
    /// caseta como fuera de ruta.
    /// </param>
    /// <param name="kilometrosDeLaBitacora">
    /// `T-18` menos `T-14`. Nulo cuando la misión no ha retornado o falta un odómetro.
    /// </param>
    /// <param name="velocidadMediaMaximaKmH">
    /// `velocidad_media_maxima_por_tipo_vehiculo`. <b>Nula es `[C]`</b>, y entonces la dimensión
    /// temporal no se evalúa: sin velocidad declarada, cualquier intervalo se podría llamar
    /// imposible y ninguno se podría defender.
    /// </param>
    /// <param name="relojConfiable">
    /// Falso marca las incoherencias temporales como <b>no concluyentes</b>. Un reloj de
    /// dispositivo desajustado fabrica intervalos imposibles, y `RN-37` manda no tratarlos como
    /// hallazgo.
    /// </param>
    /// <param name="casetasActivas">
    /// Las casetas <b>que cobraban ese día</b>, con su corredor y su kilómetro. Es lo que
    /// permite ver el salto sobre una caseta intermedia.
    ///
    /// Sólo las activas, y eso es de `RN-37`: <i>«caseta cerrada o con libre paso ese día — el
    /// estado del punto con vigencia evita marcar como omisión un peaje que nadie cobró»</i>.
    /// </param>
    public static DictamenDeCoherencia Evaluar(
        IReadOnlyList<PasoParaCruzar> pasos,
        IReadOnlySet<Ulid>? rutaAutorizada,
        int? kilometrosDeLaBitacora,
        int? velocidadMediaMaximaKmH,
        bool relojConfiable,
        IReadOnlyList<DesvioDeclarado> desvios,
        IReadOnlyList<CasetaEnElCorredor> casetasActivas)
    {
        // **Por fecha del hecho, no por orden de captura.** `RN-46` y el caso límite de `RN-37`:
        // el motorista que captura todos los pasos al final del día no cometió una incoherencia
        // de secuencia — cometió un orden de ingreso, que es otra cosa.
        var orden = pasos.OrderBy(p => p.OcurridoEn).ToList();

        var incoherencias = new List<Incoherencia>();
        var porQueNo = new List<string>();

        var puedeGeografica = orden.All(p => p.Kilometro is not null && p.Corredor is not null);
        var puedeTemporal = velocidadMediaMaximaKmH is not null;
        var puedeRuta = rutaAutorizada is not null;
        var puedeKilometraje = kilometrosDeLaBitacora is not null;

        if (!puedeGeografica)
            porQueNo.Add(
                "Algún punto no declara corredor y kilómetro, así que no hay orden geográfico " +
                "contra el que juzgar. Deducirlo del orden de captura invertiría la respuesta.");

        if (!puedeTemporal)
            porQueNo.Add(
                "`velocidad_media_maxima_por_tipo_vehiculo` no está definida (`[C]`). Sin " +
                "velocidad declarada, cualquier intervalo se podría llamar imposible y ninguno " +
                "se podría defender.");

        if (!puedeRuta)
            porQueNo.Add(
                "La misión no tiene estimado de peajes congelado, así que no se sabe qué puntos " +
                "autorizó quien autorizó. Sin eso, toda caseta parecería fuera de ruta.");

        if (!puedeKilometraje)
            porQueNo.Add(
                "La bitácora no tiene los dos odómetros —`T-14` y `T-18`—, así que no hay " +
                "kilometraje contra el que contrastar las casetas cruzadas.");

        if (puedeGeografica) incoherencias.AddRange(Geografica(orden, casetasActivas));
        if (puedeTemporal) incoherencias.AddRange(
            Temporal(orden, velocidadMediaMaximaKmH!.Value, relojConfiable));

        incoherencias.AddRange(Duplicados(orden));

        if (puedeRuta) incoherencias.AddRange(FueraDeRuta(orden, rutaAutorizada!));
        if (puedeGeografica && puedeKilometraje)
            incoherencias.AddRange(ContraElKilometraje(orden, kilometrosDeLaBitacora!.Value));

        return new DictamenDeCoherencia(
            [.. incoherencias.Select(i => Justificar(i, orden, desvios))],
            new DimensionesEvaluadas(
                puedeGeografica, puedeTemporal && relojConfiable, puedeRuta,
                puedeKilometraje && puedeGeografica, porQueNo),
            orden.Count);
    }

    /// <summary>
    /// Dimensión 1 — el orden de los puntos corresponde a un sentido de circulación posible.
    ///
    /// ── Lo que se busca es el SALTO, no el cambio de sentido ───────────────
    /// Un viaje de ida y vuelta cambia de sentido una vez y eso es el retorno; una misión
    /// multi-destino lo cambia varias y eso es `CE-08`. Marcar el cambio produciría un
    /// hallazgo en cada misión del año, que es justo el ruido que `RN-37` advierte.
    ///
    /// Lo imposible es estar en el km 190 habiendo venido del km 35 <b>sin haber cruzado el
    /// km 85</b>. Y esa lectura encuentra además la omisión —el paso que nadie registró—, que
    /// es la otra mitad de lo que el auditor busca.
    /// </summary>
    private static IEnumerable<Incoherencia> Geografica(
        IReadOnlyList<PasoParaCruzar> orden, IReadOnlyList<CasetaEnElCorredor> activas)
    {
        for (var i = 1; i < orden.Count; i++)
        {
            var (a, b) = (orden[i - 1], orden[i]);

            // Corredores distintos: no hay tramo que recorrer entre ellos que este catálogo
            // pueda describir. Cambiar de corredor es legítimo y frecuente.
            if (a.Corredor != b.Corredor) continue;
            if (a.Kilometro is not { } kmA || b.Kilometro is not { } kmB) continue;

            var (desde, hasta) = kmA < kmB ? (kmA, kmB) : (kmB, kmA);

            var saltadas = activas
                .Where(c =>
                    c.Corredor == a.Corredor &&
                    c.Kilometro > desde && c.Kilometro < hasta &&
                    c.Punto != a.Punto && c.Punto != b.Punto)
                .OrderBy(c => c.Kilometro)
                .ToList();

            if (saltadas.Count == 0) continue;

            yield return new Incoherencia(
                TipoDeIncoherencia.SecuenciaGeograficamenteImposible,
                $"De «{a.NombreDelPunto}» (km {kmA}) a «{b.NombreDelPunto}» (km {kmB}) del " +
                $"corredor {a.Corredor} hay que cruzar " +
                string.Join(", ", saltadas.Select(c => $"«{c.Nombre}» (km {c.Kilometro})")) +
                ", y no hay paso registrado. O falta anotar ese paso, o el vehículo no fue " +
                "por donde dicen estas dos casetas.",
                [a.Id, b.Id]);
        }
    }
    /// <summary>
    /// Dimensión 2 — el intervalo entre dos casetas consecutivas es viable.
    ///
    /// <b>Los dos extremos, y se dicen distinto.</b> Demasiado rápido es físicamente imposible y
    /// apunta a un dato falso; demasiado lento es una parada que nadie registró, que es una
    /// pregunta legítima y no una acusación.
    /// </summary>
    private static IEnumerable<Incoherencia> Temporal(
        IReadOnlyList<PasoParaCruzar> orden, int velocidadMaxima, bool relojConfiable)
    {
        for (var i = 1; i < orden.Count; i++)
        {
            var (a, b) = (orden[i - 1], orden[i]);

            if (a.Corredor is null || b.Corredor is null || a.Corredor != b.Corredor) continue;
            if (a.Kilometro is not { } kmA || b.Kilometro is not { } kmB) continue;

            var distancia = Math.Abs(kmB - kmA);
            if (distancia == 0) continue;

            var horas = (b.OcurridoEn - a.OcurridoEn).TotalHours;
            if (horas <= 0) continue;

            var velocidad = distancia / horas;

            if (velocidad <= velocidadMaxima) continue;

            yield return new Incoherencia(
                TipoDeIncoherencia.IntervaloInviable,
                $"De «{a.NombreDelPunto}» a «{b.NombreDelPunto}» hay {distancia} km y se " +
                $"recorrieron en {horas:N1} h: {velocidad:N0} km/h, sobre un máximo de " +
                $"{velocidadMaxima} km/h para este tipo de vehículo. " +
                (relojConfiable
                    ? "Uno de los dos momentos está mal, o uno de los dos pasos no ocurrió."
                    : "⚠️ El reloj del dispositivo está declarado NO CONFIABLE, así que esto no " +
                      "es concluyente: un reloj desajustado fabrica intervalos imposibles."),
                [a.Id, b.Id],
                Concluyente: relojConfiable);
        }
    }

    /// <summary>
    /// Paso duplicado. <b>Se detecta sin necesitar kilómetro ni velocidad</b>: es el mismo punto
    /// dos veces en veinte minutos, y eso no es una ruta, es una captura repetida o un cobro
    /// doble.
    /// </summary>
    private static IEnumerable<Incoherencia> Duplicados(IReadOnlyList<PasoParaCruzar> orden)
    {
        for (var i = 1; i < orden.Count; i++)
        {
            var (a, b) = (orden[i - 1], orden[i]);

            if (a.Punto != b.Punto) continue;
            if (b.OcurridoEn - a.OcurridoEn > VentanaDeDuplicado) continue;

            yield return new Incoherencia(
                TipoDeIncoherencia.PasoDuplicado,
                $"Dos pasos por «{a.NombreDelPunto}» separados por " +
                $"{(b.OcurridoEn - a.OcurridoEn).TotalMinutes:N0} minutos. O se capturó dos " +
                "veces el mismo paso, o la caseta cobró dos veces — y las dos cosas se " +
                "resuelven distinto.",
                [a.Id, b.Id]);
        }
    }

    /// <summary>
    /// Dimensión 3 — <b>el hallazgo que `NRM-10` pide que el sistema produzca solo</b>: un peaje
    /// de Yojoa en una misión autorizada a Choluteca.
    /// </summary>
    private static IEnumerable<Incoherencia> FueraDeRuta(
        IReadOnlyList<PasoParaCruzar> orden, IReadOnlySet<Ulid> autorizada)
    {
        foreach (var paso in orden)
        {
            if (autorizada.Contains(paso.Punto)) continue;

            yield return new Incoherencia(
                TipoDeIncoherencia.PuntoFueraDeRutaAutorizada,
                $"«{paso.NombreDelPunto}» no está en la ruta que se autorizó. Puede ser un " +
                "desvío legítimo —derrumbe, cierre de carretera— y entonces tiene que constar " +
                "como evento en ruta; o puede ser uso del vehículo fuera de la misión.",
                [paso.Id]);
        }
    }

    /// <summary>
    /// El cruce contra la bitácora — `RN-37` punto 3.
    ///
    /// <b>La contradicción es doble</b>: si la misión declara 90 km y las casetas cruzadas están
    /// separadas por cientos, `RN-30` y `RN-31` la señalan por su lado. Acá se nombra desde el
    /// peaje.
    /// </summary>
    private static IEnumerable<Incoherencia> ContraElKilometraje(
        IReadOnlyList<PasoParaCruzar> orden, int kilometrosDeLaBitacora)
    {
        // La distancia mínima que la secuencia obliga a recorrer: la suma de los saltos dentro
        // de cada corredor. Es un piso, no la distancia real — el vehículo pudo andar mucho más
        // entre casetas. Un piso es justo lo que se necesita: si el piso ya no cabe en el
        // kilometraje declarado, la contradicción es segura.
        var minima = 0;

        for (var i = 1; i < orden.Count; i++)
        {
            var (a, b) = (orden[i - 1], orden[i]);

            if (a.Corredor != b.Corredor) continue;
            if (a.Kilometro is not { } kmA || b.Kilometro is not { } kmB) continue;

            minima += Math.Abs(kmB - kmA);
        }

        if (minima <= kilometrosDeLaBitacora) yield break;

        yield return new Incoherencia(
            TipoDeIncoherencia.PeajeSinKilometrajeQueLoRespalde,
            $"Las casetas cruzadas obligan a recorrer al menos {minima} km y la bitácora " +
            $"declara {kilometrosDeLaBitacora} km. Una de las dos cifras está mal: o el " +
            "odómetro no se leyó bien, o hay pasos que no corresponden a esta misión.",
            [.. orden.Select(p => p.Id)]);
    }

    /// <summary>
    /// Marca la incoherencia como justificada cuando hay un desvío declarado que la cubre.
    ///
    /// <b>No la borra.</b> Que la incoherencia existió y que alguien la explicó son dos hechos, y
    /// el auditor pregunta por los dos — borrarla dejaría el expediente diciendo que nunca hubo
    /// desvío.
    /// </summary>
    private static Incoherencia Justificar(
        Incoherencia incoherencia,
        IReadOnlyList<PasoParaCruzar> orden,
        IReadOnlyList<DesvioDeclarado> desvios)
    {
        if (desvios.Count == 0) return incoherencia;

        var momentos = orden
            .Where(p => incoherencia.Pasos.Contains(p.Id))
            .Select(p => p.OcurridoEn)
            .ToList();

        if (momentos.Count == 0) return incoherencia;

        // Tienen que estar cubiertos **todos** los pasos de la incoherencia. Un desvío que
        // cubre la mitad de un intervalo no explica el intervalo.
        var cubre = desvios.FirstOrDefault(d => momentos.All(d.Cubre));

        return cubre is null
            ? incoherencia
            : incoherencia with
            {
                Justificada = true,
                Justificacion =
                    $"Cubierta por desvío declarado desde el campo el " +
                    $"{cubre.Desde:dd/MM/yyyy HH:mm}: {cubre.Motivo}",
            };
    }
}

/// <summary>
/// Una caseta del catálogo, con lo que el orden geográfico necesita.
///
/// <b>Sólo entran las que cobraban ese día.</b> Una caseta cerrada o con libre paso no se
/// puede echar de menos: `RN-34` gobierna su estado con vigencia, y `RN-37` es explícita en
/// que eso <i>«evita marcar como omisión un peaje que nadie cobró»</i>.
/// </summary>
public sealed record CasetaEnElCorredor(
    Ulid Punto, string Nombre, string Corredor, int Kilometro);
