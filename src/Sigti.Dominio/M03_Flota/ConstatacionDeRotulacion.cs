namespace Sigti.Dominio.M03_Flota;

/// <summary>
/// Un elemento de identificación obligatoria del vehículo del Estado — `RN-18`.
///
/// Se enumeran porque <b>cada uno se constata por separado</b>: un vehículo puede tener las
/// franjas y no la leyenda, y decir «rotulación verificada» sobre eso es afirmar de más.
/// </summary>
public enum ElementoDeIdentificacion
{
    /// <summary>Tres franjas horizontales de 10 cm, azul–blanco–azul, en puertas laterales.</summary>
    Franjas,

    /// <summary>«PROPIEDAD DEL ESTADO DE HONDURAS», en letras de 2.54 cm.</summary>
    Leyenda,

    /// <summary>Siglas o nombre de la institución.</summary>
    Siglas,

    /// <summary>Numeración consecutiva institucional.</summary>
    Correlativo,
}

/// <summary>
/// Una constatación de un elemento — <b>con fecha, fotografía y quién constató</b>.
/// </summary>
/// <param name="Fotografia">
/// ⚠️ <b>Obligatoria.</b> `RN-18` es literal: <i>«una constatación sin fotografía no debe
/// aceptarse»</i>. Sin ella lo que queda registrado es que alguien dijo que miró, y eso es
/// precisamente lo que un hallazgo de auditoría discute.
/// </param>
public sealed record Constatacion(
    ElementoDeIdentificacion Elemento,
    bool Presente,
    DateOnly ConstatadoEl,
    Ulid Fotografia,
    string ConstatadoPor,
    string? Observacion);

/// <summary>En qué situación está la identificación del vehículo a una fecha.</summary>
public enum EstadoDeLaIdentificacion
{
    /// <summary>Los cuatro elementos constatados, presentes y dentro de vigencia.</summary>
    Constatada,

    /// <summary>
    /// Se constató y <b>caducó</b>: pasó el plazo del parámetro. No dice que la rotulación se
    /// haya borrado — dice que <b>nadie lo ha vuelto a mirar</b>, que es otra afirmación.
    /// </summary>
    Caducada,

    /// <summary>Falta constatar al menos un elemento. <b>Nunca se miró</b>.</summary>
    NoConstatada,

    /// <summary>Se constató y algún elemento <b>no está</b>. Es un hallazgo, no una omisión.</summary>
    ConElementoFaltante,
}

/// <param name="Faltantes">
/// Los elementos que se constataron y <b>no estaban</b>. Distinto de
/// <paramref name="SinConstatar"/>: uno es un hallazgo y el otro una tarea pendiente.
/// </param>
/// <param name="CaducaEl">
/// Cuándo caduca la constatación más vieja. <b>Nulo cuando no hay ninguna</b> — y eso no es un
/// dato que falte: es que no hay nada que caduque.
/// </param>
public sealed record IdentificacionDelVehiculo(
    EstadoDeLaIdentificacion Estado,
    IReadOnlyList<ElementoDeIdentificacion> Faltantes,
    IReadOnlyList<ElementoDeIdentificacion> SinConstatar,
    DateOnly? CaducaEl,
    string Detalle);

/// <summary>
/// `RN-18` — la identificación del vehículo del Estado, <b>constatada con fecha y foto</b>.
///
/// ── El defecto que esta regla vino a corregir ───────────────────────────────
/// Era un booleano: <c>IdentificacionInstitucionalVerificada</c>. Y `CLAUDE.md` lo dice sin
/// rodeos entre las restricciones que condicionan el diseño: <i>«es campo verificable con fecha
/// y foto: es hallazgo frecuente de auditoría»</i>.
///
/// Un booleano en <c>true</c> no dice <b>cuándo</b> se miró ni <b>quién</b> lo miró ni deja
/// nada que mostrar. Una constatación de hace tres años se ve igual que una de ayer, y ante un
/// operativo lo único que queda es la palabra de alguien.
///
/// ── Y por qué caduca ────────────────────────────────────────────────────────
/// Porque la pintura se despinta y las calcomanías se caen. Un vehículo que se constató una vez
/// no está constatado para siempre: <b>«caducada» no dice que la rotulación se haya borrado,
/// dice que nadie lo ha vuelto a mirar</b> — y esa distinción es la que hace accionable el
/// aviso.
///
/// ── El plazo es más corto sin lámina, y no es un detalle ────────────────────
/// En un vehículo sin lámina metálica <b>la rotulación es su única identificación visible</b>
/// como bien del Estado. Si caducara al mismo ritmo que la del resto de la flota, el vehículo
/// que más depende de ella sería el que más tiempo pasa sin que nadie la mire.
/// </summary>
public static class ReglasDeLaRotulacion
{
    /// <summary>Los cuatro que `RN-18` exige. Constatar tres no es constatar.</summary>
    public static readonly IReadOnlyList<ElementoDeIdentificacion> Obligatorios =
        [.. Enum.GetValues<ElementoDeIdentificacion>()];

    /// <summary>La clave del plazo de vigencia — parámetro configurable de `RN-18`.</summary>
    public const string ClaveDeVigencia = "vigencia_constatacion_rotulacion";

    /// <summary>La clave del plazo <b>más corto</b>, para vehículos sin lámina.</summary>
    public const string ClaveDeVigenciaSinLamina = "vigencia_constatacion_rotulacion_sin_lamina";

    /// <summary>
    /// Por qué no se acepta esta constatación. <b>Nulo es que sí se acepta.</b>
    /// </summary>
    public static string? PorQueNoSeAcepta(bool tieneFotografia) =>
        tieneFotografia
            ? null
            : "Una constatación sin fotografía no se acepta (RN-18). Sin la foto lo único que " +
              "queda registrado es que alguien dijo que miró, y eso es exactamente lo que un " +
              "hallazgo de auditoría discute.";

    /// <summary>
    /// En qué situación está la identificación del vehículo.
    /// </summary>
    /// <param name="vigenciaEnDias">
    /// El plazo del parámetro. <b>Nulo es que la institución no lo cargó</b>, y entonces la
    /// constatación <b>no caduca</b>: inventar un plazo sería fijar por omisión una regla que
    /// `RN-18` deja explícitamente configurable. El detalle lo dice.
    /// </param>
    public static IdentificacionDelVehiculo Evaluar(
        IReadOnlyList<Constatacion> constataciones,
        int? vigenciaEnDias,
        DateOnly aLaFecha)
    {
        // La última de cada elemento: constatar de nuevo supera lo anterior, y el historial
        // queda para la pregunta de auditoría.
        var ultimas = Obligatorios
            .Select(e => constataciones
                .Where(c => c.Elemento == e)
                .MaxBy(c => c.ConstatadoEl))
            .ToList();

        var sinConstatar = Obligatorios
            .Where((_, i) => ultimas[i] is null)
            .ToList();

        var faltantes = ultimas
            .Where(c => c is { Presente: false })
            .Select(c => c!.Elemento)
            .ToList();

        // ⚠️ El orden importa: **un elemento que no está es un hallazgo**, y uno que nunca se
        // miró es una tarea. Reportar «no constatada» sobre un vehículo al que se le vio la
        // leyenda borrada escondería el hallazgo detrás de la omisión.
        if (faltantes.Count > 0)
        {
            return new IdentificacionDelVehiculo(
                EstadoDeLaIdentificacion.ConElementoFaltante, faltantes, sinConstatar, null,
                $"Se constató y falta: {string.Join(", ", faltantes.Select(Texto))}. " +
                "Es hallazgo de auditoría (RN-18).");
        }

        if (sinConstatar.Count > 0)
        {
            return new IdentificacionDelVehiculo(
                EstadoDeLaIdentificacion.NoConstatada, faltantes, sinConstatar, null,
                $"Nunca se constató: {string.Join(", ", sinConstatar.Select(Texto))}.");
        }

        var masVieja = ultimas.Min(c => c!.ConstatadoEl);

        if (vigenciaEnDias is not { } dias)
        {
            return new IdentificacionDelVehiculo(
                EstadoDeLaIdentificacion.Constatada, faltantes, sinConstatar, null,
                // ⚠️ Sin marcas de formato: este texto se IMPRIME en el paquete de
                // identificacion, y un asterisco en un documento oficial es basura visible.
                $"Constatada el {masVieja:dd/MM/yyyy}. No caduca: la institución no ha " +
                "cargado el plazo de vigencia de la constatación, y no se inventa uno.");
        }

        var caduca = masVieja.AddDays(dias);

        return caduca < aLaFecha
            ? new IdentificacionDelVehiculo(
                EstadoDeLaIdentificacion.Caducada, faltantes, sinConstatar, caduca,
                $"La constatación caducó el {caduca:dd/MM/yyyy}. No dice que la rotulación se " +
                "haya borrado: dice que nadie la ha vuelto a mirar desde el " +
                $"{masVieja:dd/MM/yyyy}.")
            : new IdentificacionDelVehiculo(
                EstadoDeLaIdentificacion.Constatada, faltantes, sinConstatar, caduca,
                $"Constatada el {masVieja:dd/MM/yyyy}, vigente hasta el {caduca:dd/MM/yyyy}.");
    }

    /// <summary>
    /// Qué plazo aplica. <b>Más corto sin lámina</b>, y con la misma disciplina de siempre:
    /// nulo es que no se cargó, no cero.
    /// </summary>
    public static int? VigenciaQueAplica(
        EstadoDePlaca estado, int? general, int? sinLamina) =>
        estado == EstadoDePlaca.ConLamina ? general : sinLamina ?? general;

    public static string Texto(ElementoDeIdentificacion elemento) => elemento switch
    {
        ElementoDeIdentificacion.Franjas => "franjas azul–blanco–azul",
        ElementoDeIdentificacion.Leyenda => "leyenda «PROPIEDAD DEL ESTADO DE HONDURAS»",
        ElementoDeIdentificacion.Siglas => "siglas de la institución",
        ElementoDeIdentificacion.Correlativo => "correlativo institucional",
        _ => elemento.ToString(),
    };
}
