using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M19_Seguimiento;

/// <summary>
/// Lo que el motorista <b>declara</b> desde la ruta — M-19, `RN-76`.
///
/// ── Un solo asiento para las tres cosas ─────────────────────────────────────
/// El estado, el arribo y la salida comparten todo lo que importa: cuándo pasó, cuándo se supo,
/// quién lo dijo y dónde estaba. Separarlos en tres tablas obligaría a unirlas para contestar
/// la única pregunta que el tablero hace —<i>«¿qué es lo último que sabemos de esta misión?»</i>—
/// y esa unión se escribiría distinto en cada consulta.
/// </summary>
public sealed class ReporteDeCampo
{
    public required Ulid Id { get; init; }

    public required Ulid MisionId { get; init; }

    public required TipoDeReporte Tipo { get; init; }

    /// <summary>
    /// Del catálogo cerrado `estado_en_ruta`. Nulo en arribo y salida: ahí el hecho es el evento.
    /// </summary>
    public string? Estado { get; init; }

    /// <summary>El destino al que se llegó o del que se salió. Nulo en la declaración de estado.</summary>
    public string? Destino { get; init; }

    /// <summary>
    /// Cuándo pasó, por el reloj del dispositivo. <b>Es el eje que manda</b> (`RN-46`): la
    /// antigüedad se calcula sobre esto y los reportes se ordenan por esto, nunca por la captura.
    /// </summary>
    public required DateTimeOffset MomentoDelHecho { get; init; }

    /// <summary>
    /// Cuándo llegó al servidor. Cuatro días de distancia entre las dos fechas es
    /// <b>operación normal</b>, no un error: el dispositivo estuvo sin cobertura (`RN-43`).
    /// </summary>
    public required DateTimeOffset MomentoDeCaptura { get; init; }

    /// <summary>
    /// Dónde estaba. <b>Nulo es «el dispositivo no tenía posición»</b> — no es cero, y menos aún
    /// el punto (0, 0), que queda en el Golfo de Guinea y es el sentinel clásico del GPS sin fijar.
    /// </summary>
    public Posicion? Posicion { get; init; }

    /// <summary>Del catálogo `causa_de_espera`. Nula cuando el estado declarado no es una espera.</summary>
    public string? CausaDeEspera { get; init; }

    /// <summary>
    /// A qué destino o dependencia se atribuye la espera. Incluye <b>a la propia institución</b>
    /// cuando la causa fue del equipo: el indicador que solo mide culpas ajenas no lo cree nadie.
    /// </summary>
    public string? SeAtribuyeA { get; init; }

    /// <summary>
    /// Si el motor quedó encendido durante la espera — entra como variable en la conciliación
    /// galonaje–kilometraje (`RN-30`). <b>Nulo es «no se preguntó»</b>, que no es «apagado»:
    /// tratarlo como apagado convertiría el silencio en evidencia de un consumo indebido.
    /// </summary>
    public bool? MotorEncendido { get; init; }

    public required IdPersona Declara { get; init; }
}

public enum TipoDeReporte
{
    /// <summary>El motorista dice en qué está, con un toque y sin formulario.</summary>
    EstadoDeclarado,

    Arribo,
    Salida,
}

/// <summary>
/// Un punto conocido. El sistema <b>no lo dibuja</b>: el componente de mapas lo aporta ARGOS y
/// SIGTI no lo reimplementa (`DP-001`). Acá solo se guarda y se le mide la antigüedad.
/// </summary>
/// <param name="PrecisionMetros">
/// Nula cuando el dispositivo no la informó. Una posición con 3 km de error y una con 5 m se
/// dibujan igual en un mapa y significan cosas distintas.
/// </param>
public readonly record struct Posicion(decimal Latitud, decimal Longitud, int? PrecisionMetros);
