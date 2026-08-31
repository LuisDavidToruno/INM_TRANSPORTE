namespace Sigti.Aplicacion.M20_Integraciones;

/// <summary>
/// Una persona del padrón institucional, tal como la entrega el sistema que es su dueño.
/// </summary>
/// <param name="Persona">
/// El identificador con que este sistema atribuye actos. <b>Es el mismo que viaja en el token</b>
/// — si no lo fuera, quien entra no se encontraría a sí mismo en el organigrama.
/// </param>
/// <param name="Puesto">
/// El cargo vigente. <b>Nulo es que no tiene ninguno hoy</b>, y eso es un dato, no un error: la
/// persona existe, sus actos históricos la referencian, y no puede ejercer competencia alguna.
/// </param>
/// <param name="Hasta">
/// Fin de la ocupación. <b>Nulo es «sin fecha de fin declarada»</b>, no «para siempre»: la
/// distinción la resuelve `RN-100` al juzgar un acto contra la fecha del hecho.
/// </param>
public sealed record PersonaDelPadron(
    string Persona,
    string Nombre,
    string? Puesto,
    DateOnly? Desde,
    DateOnly? Hasta,
    string? Gerencia,
    string? Unidad,
    string? Oficina);

/// <summary>
/// De dónde sale el organigrama de la institución.
///
/// ── ⚠️ Por qué esto es una interfaz y no una llamada directa ────────────────
/// Porque <b>SIGTI es genérico</b>: se despliega en cualquier institución pública hondureña, y
/// en cada una el dueño del padrón es otro. En el piloto del INM es ARGOS; en la siguiente será
/// lo que esa institución tenga. Lo que no cambia es la forma de la respuesta.
///
/// Una llamada directa a ARGOS desde el dominio ataría el producto a una institución, y el día
/// del segundo despliegue habría que desatarlo tocando reglas de negocio.
///
/// ── Y por qué SIGTI no lo edita ─────────────────────────────────────────────
/// `RN-48`: los datos de otro dueño se guardan <b>marcados como espejo</b>, y ninguna pantalla
/// de SIGTI debe permitir editarlos. Quien necesite corregir un puesto lo corrige donde vive.
/// </summary>
public interface IEspejoDeOrganizacion
{
    /// <summary>
    /// El padrón completo. <b>Sin filtrar por «vigentes hoy»</b>: `RN-100` resuelve la
    /// competencia a la fecha del hecho, y filtrar acá impediría reevaluar un expediente de
    /// febrero con la ocupación de febrero.
    /// </summary>
    Task<IReadOnlyList<PersonaDelPadron>> PadronAsync(CancellationToken cancelacion = default);

    /// <summary>Cómo se llama la fuente, para poder decirlo en pantalla y en la bitácora.</summary>
    string Fuente { get; }
}
