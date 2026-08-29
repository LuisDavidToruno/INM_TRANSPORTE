using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Dominio.M14_Auditoria;

/// <summary>
/// Una línea del estado de cuenta, tal como la emitió el proveedor.
///
/// ── Se conserva como vino ───────────────────────────────────────────────────
/// Ni se corrige ni se normaliza: es la afirmación de un tercero, y el valor de conciliar contra
/// ella depende de que sea suya. Lo que se ajuste acá deja de ser evidencia y pasa a ser
/// interpretación.
/// </summary>
/// <param name="FechaDelHecho">
/// Cuándo ocurrió, <b>no el período del estado de cuenta</b>. `RN-95` casos límite: el consumo
/// del 31 aparece en el estado de cuenta del mes siguiente, y conciliar por período lo dejaría
/// como diferencia todos los meses.
/// </param>
/// <param name="Referencia">
/// El comprobante, el ticket o el número de boleta. <b>Es el criterio de coincidencia fuerte</b>
/// porque `RN-84` lo hace único en la institución.
/// </param>
public sealed record LineaExterna(
    string Id,
    DateOnly FechaDelHecho,
    decimal Monto,
    IdentificacionExterna Identificacion,
    string? Referencia = null,
    string? Descripcion = null);

/// <summary>
/// Un asiento nuestro, reducido a lo que la conciliación compara. Se arma en la capa de
/// aplicación desde los consumos, los abastecimientos o los pasos por caseta.
/// </summary>
/// <param name="Origen">
/// Qué es: consumo de vale, abastecimiento, paso por caseta. Va al resultado porque una
/// diferencia se resuelve distinto según de dónde salió el asiento.
/// </param>
public sealed record AsientoPropio(
    Ulid Id,
    string Origen,
    DateOnly FechaDelHecho,
    decimal Monto,
    Ulid? Vehiculo,
    string? Referencia = null);

/// <summary>Por qué una línea y un asiento se dieron por el mismo hecho.</summary>
public enum CriterioDeCoincidencia
{
    /// <summary>
    /// Por comprobante. <b>Es el fuerte</b>: `RN-84` hace único el comprobante en la
    /// institución, así que dos registros con el mismo son el mismo hecho.
    /// </summary>
    Referencia,

    /// <summary>
    /// Por vehículo, monto y fecha dentro de la tolerancia. <b>Es el débil</b>, y se usa sólo
    /// cuando alguno de los dos lados no trae referencia — que pasa: hay estaciones que no
    /// numeran el cupón.
    /// </summary>
    VehiculoMontoYFecha,
}

/// <summary>Una coincidencia, con el criterio que la produjo.</summary>
public sealed record Coincidencia(
    LineaExterna Linea,
    AsientoPropio Asiento,
    CriterioDeCoincidencia Criterio,
    VehiculoResuelto Vehiculo);

/// <summary>
/// Una línea que el proveedor tiene y nosotros no.
///
/// <b>La conciliación no presume qué es.</b> `RN-95`: puede ser un cobro indebido, un consumo
/// que nadie registró, o una línea que no corresponde a ningún vehículo de la flota —
/// <i>«puede ser un error del proveedor y puede no serlo»</i>.
/// </summary>
public sealed record SoloEnLaFuente(LineaExterna Linea, VehiculoResuelto Vehiculo);

/// <summary>
/// Un asiento nuestro que el proveedor no reporta.
///
/// `RN-95`: <i>«puede ser un comprobante falso, o una estación que no reportó. <b>La
/// conciliación no presume cuál</b>»</i>.
/// </summary>
public sealed record SoloEnSigti(AsientoPropio Asiento);

/// <summary>
/// El resultado de una conciliación — `RN-95` punto 2: <b>tres listas</b>.
/// </summary>
/// <param name="FechaDeCorte">
/// Hasta qué momento se conoce lo que este resultado afirma — `RN-94`. Sin ella, dos ejecuciones
/// del mismo reporte con datos distintos se ven idénticas y no se pueden comparar.
/// </param>
public sealed record ResultadoDeConciliacion(
    Ulid Fuente,
    DateOnly Desde,
    DateOnly Hasta,
    IReadOnlyList<Coincidencia> Coincidentes,
    IReadOnlyList<SoloEnLaFuente> SoloEnLaFuente,
    IReadOnlyList<SoloEnSigti> SoloEnSigti,
    DateTimeOffset FechaDeCorte,
    string DocumentoFuente)
{
    /// <summary>
    /// Las dos listas que abren expediente — `RN-95`: <i>«en ambos sentidos: lo que la fuente
    /// externa tiene y el sistema no, y lo que el sistema tiene y la fuente externa no»</i>.
    /// </summary>
    public int Diferencias => SoloEnLaFuente.Count + SoloEnSigti.Count;

    /// <summary>
    /// Lo que la fuente cobró y nosotros no tenemos registrado. Es la cifra que va al reclamo.
    /// </summary>
    public decimal MontoSoloEnLaFuente => SoloEnLaFuente.Sum(d => d.Linea.Monto);

    public decimal MontoSoloEnSigti => SoloEnSigti.Sum(d => d.Asiento.Monto);

    /// <summary>
    /// Diferencias que <b>ni siquiera se pudieron atribuir a un vehículo</b>. Van aparte porque
    /// se resuelven distinto: no hay a quién preguntarle, hay que ir al proveedor.
    /// </summary>
    public int SinVehiculoResuelto => SoloEnLaFuente.Count(d => !d.Vehiculo.EstaResuelto);
}

/// <summary>
/// La conciliación contra fuentes externas — `RN-95`.
///
/// ── Lo que revela y `RN-30` no puede ver ────────────────────────────────────
/// <i>«Una conciliación que solo compara nuestros datos con nuestros datos verifica coherencia
/// interna, no veracidad. <b>Un registro completo y coherente puede ser completamente falso</b>,
/// y solo la fuente externa lo revela»</i>.
/// </summary>
public static class ReglasDeConciliacionExterna
{
    /// <param name="toleranciaEnDias">
    /// Cuántos días de desfase se admiten al casar por vehículo, monto y fecha. Existe porque el
    /// proveedor factura con la fecha que él tiene, y un día de diferencia en un cupón no es una
    /// diferencia de conciliación.
    ///
    /// <b>Sólo aplica al criterio débil.</b> Casar por comprobante no necesita tolerancia: el
    /// comprobante es único (`RN-84`) y la fecha discrepante es, ella misma, el dato interesante.
    /// </param>
    public static ResultadoDeConciliacion Conciliar(
        Ulid fuente,
        DateOnly desde,
        DateOnly hasta,
        IReadOnlyList<LineaExterna> lineas,
        IReadOnlyList<AsientoPropio> asientos,
        IReadOnlyList<AnclasDelVehiculo> flota,
        int toleranciaEnDias,
        DateTimeOffset fechaDeCorte,
        string documentoFuente)
    {
        // **Se filtra por fecha del hecho, no por período del estado de cuenta.** `RN-95`: el
        // consumo del 31 aparece en el estado de cuenta del mes siguiente, y conciliar por
        // período lo dejaría como diferencia todos los meses.
        var enRango = asientos
            .Where(a => a.FechaDelHecho >= desde && a.FechaDelHecho <= hasta)
            .ToList();

        var coincidentes = new List<Coincidencia>();
        var soloEnLaFuente = new List<SoloEnLaFuente>();
        var casados = new HashSet<Ulid>();

        foreach (var linea in lineas)
        {
            var vehiculo = ReglasDeImputacionExterna.Resolver(linea.Identificacion, flota);

            var asiento = Casar(linea, enRango, casados, vehiculo, toleranciaEnDias);

            if (asiento is null)
            {
                soloEnLaFuente.Add(new SoloEnLaFuente(linea, vehiculo));
                continue;
            }

            casados.Add(asiento.Value.Asiento.Id);
            coincidentes.Add(new Coincidencia(
                linea, asiento.Value.Asiento, asiento.Value.Criterio, vehiculo));
        }

        var soloEnSigti = enRango
            .Where(a => !casados.Contains(a.Id))
            .Select(a => new SoloEnSigti(a))
            .ToList();

        return new ResultadoDeConciliacion(
            fuente, desde, hasta, coincidentes, soloEnLaFuente, soloEnSigti,
            fechaDeCorte, documentoFuente);
    }

    /// <summary>
    /// Casa una línea con un asiento. <b>Primero por referencia, después por parecido
    /// controlado.</b>
    ///
    /// ── Y un asiento se casa una sola vez ───────────────────────────────────
    /// Es lo que hace que el <b>comprobante duplicado</b> aparezca. `CE-28` lo tiene entre sus
    /// tres casos de origen: dos líneas del estado de cuenta con el mismo comprobante, y sólo un
    /// consumo registrado. La segunda línea queda en «solo en la fuente», que es exactamente lo
    /// que hay que ver.
    /// </summary>
    private static (AsientoPropio Asiento, CriterioDeCoincidencia Criterio)? Casar(
        LineaExterna linea,
        IReadOnlyList<AsientoPropio> asientos,
        IReadOnlySet<Ulid> yaCasados,
        VehiculoResuelto vehiculo,
        int toleranciaEnDias)
    {
        var libres = asientos.Where(a => !yaCasados.Contains(a.Id)).ToList();

        if (!string.IsNullOrWhiteSpace(linea.Referencia))
        {
            var porReferencia = libres.FirstOrDefault(a =>
                !string.IsNullOrWhiteSpace(a.Referencia) &&
                string.Equals(a.Referencia.Trim(), linea.Referencia.Trim(),
                    StringComparison.OrdinalIgnoreCase));

            if (porReferencia is not null)
                return (porReferencia, CriterioDeCoincidencia.Referencia);
        }

        // El criterio débil necesita saber de qué vehículo se habla. Sin vehículo resuelto no se
        // casa por parecido: sería adivinar a quién pertenece la línea, que es justo lo que
        // `RN-66` prohíbe.
        if (!vehiculo.EstaResuelto) return null;

        var porParecido = libres.FirstOrDefault(a =>
            a.Vehiculo == vehiculo.Vehiculo &&
            a.Monto == linea.Monto &&
            Math.Abs(a.FechaDelHecho.DayNumber - linea.FechaDelHecho.DayNumber) <= toleranciaEnDias);

        return porParecido is null
            ? null
            : (porParecido, CriterioDeCoincidencia.VehiculoMontoYFecha);
    }

    /// <summary>
    /// `RN-95` — <b>la fuente no disponible no se puede conciliar, y eso no es un error.</b>
    ///
    /// Se bloquea para que nadie ejecute una conciliación vacía sobre una fuente que la
    /// institución no tiene y después lea «cero diferencias» como conformidad.
    /// </summary>
    public static void ExigirFuenteDisponible(FuenteExterna fuente)
    {
        if (fuente.Disponible) return;

        throw new BloqueoDuro("RN-95",
            $"«{fuente.Emisor}» está declarada NO disponible: " +
            $"{fuente.PorQueNoEstaDisponible}. Conciliar contra ella produciría cero " +
            "diferencias sobre cero líneas, y ese cero se lee después como conformidad. " +
            "No disponible es distinto de conciliada.");
    }

    /// <summary>
    /// El rango se declara y se comprueba: un rango invertido conciliaría sobre un conjunto
    /// vacío y produciría el mismo cero engañoso.
    /// </summary>
    public static void ExigirRangoValido(DateOnly desde, DateOnly hasta)
    {
        if (hasta < desde)
            throw new BloqueoDuro("RN-95",
                $"El rango va del {desde:dd/MM/yyyy} al {hasta:dd/MM/yyyy}. Con ese rango la " +
                "conciliación no mira ningún asiento y devuelve cero diferencias sobre cero " +
                "datos.");
    }

    /// <summary>
    /// `RN-95` punto 6 — el resultado se identifica con <b>el archivo o documento fuente
    /// usado</b>. Sin él, dos conciliaciones de la misma fuente no se distinguen y una
    /// diferencia no se puede volver a comprobar contra el papel del que salió.
    /// </summary>
    public static void ExigirDocumentoFuente(string documento)
    {
        if (string.IsNullOrWhiteSpace(documento))
            throw new BloqueoDuro("RN-95",
                "La conciliación exige identificar el archivo o documento fuente. Sin él, una " +
                "diferencia no se puede volver a comprobar contra el papel del que salió — y " +
                "eso es lo primero que pide quien la recibe.");
    }
}
