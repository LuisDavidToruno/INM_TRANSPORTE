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
public sealed record Transicion(
    string Id,
    EstadoDeMision Destino,
    IdPersona Ejecuta,
    DateTimeOffset Momento,
    string? Motivo);
