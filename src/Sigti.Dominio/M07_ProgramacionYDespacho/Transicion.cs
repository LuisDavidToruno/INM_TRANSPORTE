using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M07_ProgramacionYDespacho;

/// <summary>
/// Un asiento del diario del expediente. Cada transición registra actor, marca de
/// tiempo y motivo — «Nada se deshace: ambas transiciones quedan en el diario para
/// siempre» (P-3).
/// </summary>
/// <param name="Id">El identificador de la tabla de transiciones: `T-01` a `T-22`.</param>
/// <param name="Momento">
/// Se recibe, no se lee del reloj: `ADR-007`, y la guarda NingunaReglaLeeElReloj lo exige.
/// </param>
/// <param name="IdDeCaptura">
/// El identificador que le puso <b>quien capturó el hecho</b> — el dispositivo de campo
/// (`ADR-005`).
///
/// <b>Es lo que hace inofensivo el reenvío.</b> El dispositivo que no supo si el servidor
/// recibió va a reintentar, y este identificador es lo que permite reconocer el mismo
/// hecho en vez de duplicarlo. La unicidad la impone la base, no una comprobación que se
/// puede olvidar al agregar el próximo endpoint.
///
/// <b>Nulo cuando el hecho nació en la oficina</b>, contra la API y con red: ahí no hubo
/// captura diferida que reconciliar.
/// </param>
/// <param name="Recursos">
/// Qué quedó tomado. <b>Sólo lo lleva la transición que reserva</b> — hoy `T-08`. Es lo
/// que hace que la ocupación de la flota sea una proyección del diario y no una segunda
/// tabla que se pueda desincronizar. Ver <see cref="RecursosTomados"/>.
/// </param>
public sealed record Transicion(
    string Id,
    EstadoDeMision Destino,
    IdPersona Ejecuta,
    DateTimeOffset Momento,
    string? Motivo,
    Ulid? IdDeCaptura = null,
    RecursosTomados? Recursos = null);
