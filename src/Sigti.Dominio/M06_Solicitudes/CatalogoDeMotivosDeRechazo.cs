namespace Sigti.Dominio.M06_Solicitudes;

/// <summary>
/// Los motivos con que una jefatura puede rechazar una solicitud — `T-06`.
///
/// ── Por qué es un catálogo recibido y no un `enum` ───────────────────────────
/// Porque `HU-014` lo declara <b>configurable por la institución</b> y sus valores <b>de
/// ejemplo</b>: el contenido es el insumo #1, `[C]`. Un `enum` en el dominio congelaría en
/// código lo que la institución tiene que poder decidir, y el día que quiera un motivo
/// nuevo habría que recompilar el sistema para dárselo.
///
/// Es la diferencia con <see cref="M07_ProgramacionYDespacho.MotivoDeAnulacion"/>, que
/// <b>sí</b> es cerrado y a propósito: aquella tipificación <b>es</b> el indicador de déficit
/// de flota, y un catálogo que crece deja de ser comparable entre períodos. Acá lo que se
/// mide es por qué una jefatura dijo que no, y eso es del criterio de cada institución.
///
/// ── Lo que el dominio SÍ impone ──────────────────────────────────────────────
/// Que el motivo <b>esté en el catálogo</b>. `HU-014`: <i>«seleccione un motivo del catálogo.
/// El texto libre complementa el motivo tipificado, no lo sustituye»</i>. Sin esa regla, el
/// catálogo sería una sugerencia y en un mes habría cuatro redacciones del mismo rechazo.
/// </summary>
public sealed class CatalogoDeMotivosDeRechazo
{
    private readonly HashSet<string> _codigos;

    public CatalogoDeMotivosDeRechazo(IEnumerable<string> codigos)
    {
        // Comparación sin distinguir mayúsculas: el código viaja desde una pantalla y desde
        // un dispositivo de campo, y que el rechazo dependa de cómo se tecleó sería una
        // fuente de fallo sin ninguna contrapartida.
        _codigos = new HashSet<string>(codigos, StringComparer.OrdinalIgnoreCase);
    }

    public bool Contiene(string codigo) => _codigos.Contains(codigo);

    /// <summary>Para el mensaje del rechazo: qué se podía elegir.</summary>
    public IReadOnlyCollection<string> Codigos => _codigos;
}
