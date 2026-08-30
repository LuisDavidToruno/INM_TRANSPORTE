using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Dominio.M03_Flota;

/// <summary>
/// Una transición del estado operativo del vehículo — `W-01` a `W-19` de
/// <c>docs/03-arquitectura/estados/orden-de-mision.md</c> §10.2.
///
/// ── Transcrita de la autoridad, no inventada ────────────────────────────────
/// La máquina de estados es autoridad sobre transiciones, precondiciones e invariantes. Esta
/// tabla es su diagrama pasado a código: <b>si las dos difieren, manda el documento</b> y esto es
/// el defecto.
/// </summary>
/// <param name="Automatica">
/// Si la fija el sistema por una transición de la Orden de Misión. §10.2: <i>«`ASIGNADO` y
/// `EN_MISION` <b>los fija el sistema</b>, no una persona. Son consecuencia de transiciones de la
/// Orden de Misión, y permitir fijarlos a mano abre la puerta a un vehículo "en misión" sin
/// misión»</i>.
/// </param>
public sealed record TransicionOperativa(
    string Id,
    EstadoOperativo? Desde,
    EstadoOperativo Hasta,
    string Nombre,
    bool Automatica = false);

/// <summary>
/// La tabla de `W-xx` de §10.2, y los controles que la acompañan.
///
/// ── Por qué esta tabla existe ahora y antes no ──────────────────────────────
/// <c>EstadoDeLaFlota.AnotarAsync</c> decía: <i>«no valida la transición entre estados, y es
/// deliberado: §10.2 no publica una tabla de transiciones permitidas del vehículo como sí lo hace
/// para la misión»</i>. <b>Sí la publica</b> — el diagrama de §10.2 enumera `W-01` a `W-19`. Lo
/// que faltaba era transcribirla, no inventarla.
/// </summary>
public static class ReglasDelEstadoOperativo
{
    /// <summary>
    /// Las diecinueve transiciones del diagrama de §10.2, en su orden.
    ///
    /// <b>`W-16b` es la que no sigue el patrón</b>: prestar desde `NO_DISPONIBLE`. La autoridad
    /// la numera así y acá se conserva el identificador tal cual — renumerarla haría que el
    /// asiento del sistema no se pudiera cruzar contra el documento.
    /// </summary>
    /// <summary>
    /// Identificador de las precondiciones que <b>no corresponden a una transición concreta</b>
    /// de la tabla `W`, sino a la sección entera.
    ///
    /// ── Por qué no se les inventa un `W-nn` ──────────────────────────────────
    /// Los identificadores de la tabla nombran transiciones que existen, y estas precondiciones
    /// se disparan justamente cuando <b>no hay transición</b> que las nombre — ir a un estado
    /// no contemplado, o dar de baja con misiones abiertas. Un `W-nn` ahí sería un
    /// identificador que la autoridad no reconoce.
    ///
    /// Reemplaza al literal `"W-xx"`, que <b>siete bloqueos distintos compartían</b>: la
    /// pantalla de bloqueo de `PT-004` muestra este identificador, y «W-xx» no le dice a nadie
    /// qué regla lo detuvo ni permite rastrearla.
    /// </summary>
    public const string PrecondicionDeSeccion = "§10.2";

    public static readonly IReadOnlyList<TransicionOperativa> Tabla =
    [
        // Nace NO_DISPONIBLE: §10.2 lista «alta reciente sin habilitar» entre sus causas. Un
        // vehículo no se habilita solo por existir.
        new("W-01", null, EstadoOperativo.NoDisponible, "alta en flota"),
        new("W-02", EstadoOperativo.NoDisponible, EstadoOperativo.Disponible, "habilitar"),

        // Las dos automáticas: consecuencia de `T-08`/`T-11` y de `T-14`/`T-18`.
        new("W-03", EstadoOperativo.Disponible, EstadoOperativo.Asignado,
            "programar misión", Automatica: true),
        new("W-04", EstadoOperativo.Asignado, EstadoOperativo.Disponible,
            "liberar", Automatica: true),
        new("W-05", EstadoOperativo.Asignado, EstadoOperativo.EnMision,
            "registrar salida", Automatica: true),
        new("W-06", EstadoOperativo.EnMision, EstadoOperativo.Disponible,
            "retorno sin novedad", Automatica: true),

        // `W-07` se dispara desde las **novedades declaradas por el motorista** en el acta de
        // recepción: el estado del vehículo lo registran los propios motoristas desde el campo
        // (`DP-001` D-08). Por eso no es automática: alguien la declara.
        new("W-07", EstadoOperativo.EnMision, EstadoOperativo.EnTaller, "retorno con falla"),

        // `W-08` cubre el vehículo que **no volvió**: siniestro total, robo, decomiso. Exige
        // expediente de incidente (M-12) y se acompaña de `T-18` subtipo «retorno sin vehículo».
        new("W-08", EstadoOperativo.EnMision, EstadoOperativo.NoDisponible,
            "incidente o no retorno"),

        new("W-09", EstadoOperativo.Disponible, EstadoOperativo.EnTaller, "ingreso a taller"),
        new("W-10", EstadoOperativo.EnTaller, EstadoOperativo.Disponible, "alta de taller"),
        new("W-11", EstadoOperativo.Disponible, EstadoOperativo.NoDisponible, "inhabilitar"),
        new("W-12", EstadoOperativo.NoDisponible, EstadoOperativo.EnTaller, "enviar a taller"),
        new("W-13", EstadoOperativo.EnTaller, EstadoOperativo.NoDisponible,
            "irreparable o pendiente"),

        // Los terminales de bien PROPIO — descargo con acta (`NRM-02`).
        new("W-14", EstadoOperativo.NoDisponible, EstadoOperativo.DadoDeBaja, "descargo"),
        new("W-15", EstadoOperativo.EnTaller, EstadoOperativo.DadoDeBaja,
            "descargo por irreparable"),

        // El préstamo — `RN-63`. Sigue siendo bien nuestro y devenga responsabilidad.
        new("W-16", EstadoOperativo.Disponible, EstadoOperativo.Prestado,
            "prestar a otra dependencia"),
        new("W-16b", EstadoOperativo.NoDisponible, EstadoOperativo.Prestado,
            "prestar desde no disponible"),
        new("W-17", EstadoOperativo.Prestado, EstadoOperativo.Disponible,
            "devolución del préstamo"),

        // Los terminales de bien AJENO. **No son descargo**: el bien nunca fue del Estado.
        new("W-18", EstadoOperativo.NoDisponible, EstadoOperativo.RetiradoDeFlota,
            "fin de tenencia"),
        new("W-19", EstadoOperativo.EnTaller, EstadoOperativo.RetiradoDeFlota,
            "sustitución por el arrendador"),
    ];

    /// <summary>
    /// La transición que lleva de un estado a otro, si existe en la tabla.
    ///
    /// <b>Nula cuando no existe</b>, y eso es un bloqueo: §10.2 no la contempla, y permitirla
    /// sería escribir en el documento desde el código.
    /// </summary>
    public static TransicionOperativa? Buscar(EstadoOperativo? desde, EstadoOperativo hasta) =>
        Tabla.FirstOrDefault(t => t.Desde == desde && t.Hasta == hasta);

    /// <summary>
    /// Exige que el cambio de estado sea una transición de §10.2.
    ///
    /// ── El mensaje dice a dónde SÍ se puede ir ──────────────────────────────
    /// Un «transición no permitida» a secas obliga a quien opera a adivinar el camino. Enumerar
    /// los destinos legales desde el estado actual convierte el bloqueo en una instrucción.
    /// </summary>
    public static TransicionOperativa ExigirTransicion(
        EstadoOperativo? desde, EstadoOperativo hasta)
    {
        if (Buscar(desde, hasta) is { } transicion) return transicion;

        // ── El vehículo sin estado declarado, y por qué no se bloquea acá ────
        // `W-01` dice que el vehículo nace `NO_DISPONIBLE`, así que un vehículo sin ningún
        // asiento de estado no debería poder programarse. Pero `BD-07` **ya decidió otra cosa**:
        // con estado nulo no bloquea, lo declara en el diario —«BD-07 NO evaluada: el vehículo
        // no tiene estado operativo declarado»—. Esa decisión es de la máquina de estados y no
        // se contradice desde acá.
        //
        // Y si `BD-07` dejó programar, negarse a anotar la consecuencia dejaría la misión
        // programada y el vehículo sin asiento: peor que el asiento que falta.
        //
        // ⚠️ Sólo para las automáticas. Una persona que declara un estado sobre un vehículo sin
        // historial sigue teniendo que empezar por `W-01`.
        if (desde is null && Tabla.FirstOrDefault(t => t.Hasta == hasta && t.Automatica) is
            { } consecuencia)
            return consecuencia;

        var posibles = Tabla
            .Where(t => t.Desde == desde)
            .Select(t => $"{t.Hasta} ({t.Id} {t.Nombre})")
            .ToList();

        var origen = desde is null ? "sin estado declarado" : $"{desde}";

        throw new BloqueoDuro(PrecondicionDeSeccion,
            $"§10.2 no contempla ir de {origen} a {hasta}. " +
            (posibles.Count == 0
                ? "Desde ahí no hay ninguna transición: es un estado terminal."
                : $"Desde {origen} se puede ir a: {string.Join(", ", posibles)}."));
    }

    /// <summary>
    /// §10.2 — <b>`ASIGNADO` y `EN_MISION` los fija el sistema, no una persona</b>.
    ///
    /// <i>«Permitir fijarlos a mano abre la puerta a un vehículo "en misión" sin misión»</i>. Y
    /// al revés: las que una persona declara no pueden anotarse como automáticas, porque
    /// entonces el asiento diría que las puso el sistema y nadie respondería por ellas.
    /// </summary>
    public static void ExigirQuienLaFija(TransicionOperativa transicion, bool automatica)
    {
        if (transicion.Automatica && !automatica)
            throw new BloqueoDuro(transicion.Id,
                $"{transicion.Id} ({transicion.Nombre}) la fija el sistema como consecuencia de " +
                "una transición de la Orden de Misión, no una persona. Declararla a mano abre " +
                "la puerta a un vehículo «en misión» sin misión.");

        if (!transicion.Automatica && automatica)
            throw new BloqueoDuro(transicion.Id,
                $"{transicion.Id} ({transicion.Nombre}) la declara una persona, y el asiento " +
                "diría que la puso el sistema. Nadie respondería por ella.");
    }

    /// <summary>
    /// §10.2 — <b>la transición a `NO_DISPONIBLE` siempre exige causa tipificada</b>.
    ///
    /// <i>«Sin tipificación, este estado se convierte en el cementerio donde se esconde la flota
    /// que nadie repara»</i>.
    ///
    /// Y los terminales y el préstamo exigen <b>acta</b>: `NRM-02` no admite que un bien del
    /// Estado salga del registro contra un campo de texto.
    /// </summary>
    public static void ExigirCausaOActa(EstadoOperativo hasta, string? motivo)
    {
        if (!string.IsNullOrWhiteSpace(motivo)) return;

        var razon = hasta switch
        {
            EstadoOperativo.NoDisponible =>
                "La transición a NO_DISPONIBLE siempre exige causa tipificada. Sin ella, este " +
                "estado se convierte en el cementerio donde se esconde la flota que nadie repara.",

            EstadoOperativo.DadoDeBaja =>
                "El descargo exige acta conforme a las normas de bienes del Estado (`NRM-02`).",

            EstadoOperativo.RetiradoDeFlota =>
                "El fin de tenencia de un bien ajeno exige acta de devolución. No es descargo: " +
                "el bien nunca fue del Estado.",

            EstadoOperativo.Prestado =>
                "El préstamo exige acta (`RN-63`): sigue siendo bien nuestro y devenga " +
                "responsabilidad patrimonial mientras está afuera.",

            EstadoOperativo.EnTaller =>
                "El ingreso a taller exige decir por qué: es la causa que agrupa el reporte de " +
                "indisponibilidad de flota (`RN-60`).",

            _ => null,
        };

        if (razon is not null) throw new BloqueoDuro("RN-60", razon);
    }

    /// <summary>
    /// §10.2 — <b>un vehículo con misiones abiertas no puede ser dado de baja.</b>
    ///
    /// Cubre los dos terminales: dar de baja o retirar de flota una unidad con una misión viva
    /// dejaría un expediente apuntando a un vehículo que ya no existe para el sistema.
    /// </summary>
    public static void ExigirSinMisionesAbiertas(EstadoOperativo hasta, int misionesAbiertas)
    {
        if (hasta is not (EstadoOperativo.DadoDeBaja or EstadoOperativo.RetiradoDeFlota)) return;
        if (misionesAbiertas == 0) return;

        throw new BloqueoDuro(PrecondicionDeSeccion,
            $"Este vehículo tiene {misionesAbiertas} misión(es) sin estado terminal. Un vehículo " +
            "con misiones abiertas no se da de baja ni se retira de flota: el expediente " +
            "quedaría apuntando a una unidad que para el sistema ya no existe.");
    }

    /// <summary>
    /// La corrección del hallazgo `HB3-17` — <b>el descargo es de bienes propios; el retiro, de
    /// ajenos</b>.
    ///
    /// ── Y confundirlos produce un asiento falso ─────────────────────────────
    /// §10.2, textual: devolver un vehículo en comodato obligaba a declararlo <i>«dado de baja
    /// del registro de bienes del Estado — <b>un asiento falso sobre un bien ajeno</b>, detectable
    /// cruzando el inventario institucional contra el padrón de flota»</i>.
    ///
    /// <i>«Son cosas distintas: el descargo extingue un bien propio; el retiro devuelve uno que
    /// nunca lo fue»</i>.
    /// </summary>
    /// <param name="esBienPropio">
    /// Si el vehículo pertenece al Estado. <b>Nulo es «no se sabe»</b> — el régimen de tenencia
    /// no está cargado para toda la flota— y entonces no se puede juzgar: se deja pasar con la
    /// advertencia, porque bloquear el descargo de toda la flota por un dato de alta que nadie
    /// llenó sería peor que el asiento que se quiere evitar.
    /// </param>
    public static string? ExigirTerminalDelRegimenCorrecto(
        EstadoOperativo hasta, bool? esBienPropio)
    {
        if (hasta is not (EstadoOperativo.DadoDeBaja or EstadoOperativo.RetiradoDeFlota))
            return null;

        if (esBienPropio is null)
            return "⚠️ El régimen de tenencia del vehículo no está declarado, así que no se " +
                   "pudo verificar que el terminal corresponda: el descargo es para bienes " +
                   "propios y el retiro de flota para ajenos.";

        if (hasta is EstadoOperativo.DadoDeBaja && esBienPropio is false)
            throw new BloqueoDuro("RN-62",
                "Este vehículo no es un bien del Estado, así que no se puede descargar del " +
                "registro de bienes: sería un asiento falso sobre un bien ajeno, detectable " +
                "cruzando el inventario institucional contra el padrón de flota. Lo que " +
                "corresponde es RETIRADO_DE_FLOTA — fin de tenencia, con acta de devolución.");

        if (hasta is EstadoOperativo.RetiradoDeFlota && esBienPropio is true)
            throw new BloqueoDuro("RN-62",
                "Este vehículo es un bien del Estado, y el retiro de flota es para bienes " +
                "ajenos: devolver un comodato o terminar un alquiler. Un bien propio sale del " +
                "registro por DESCARGO, con acta conforme a `NRM-02`.");

        return null;
    }
}
