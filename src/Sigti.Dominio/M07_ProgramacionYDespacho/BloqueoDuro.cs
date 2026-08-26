namespace Sigti.Dominio.M07_ProgramacionYDespacho;

/// <summary>
/// Una precondición de bloqueo duro que no se cumplió.
///
/// No es una advertencia con acuse: es negativa. El cliente no debería siquiera haber
/// ofrecido la acción —las capacidades de `ADR-008` existen para eso—, pero el servidor
/// verifica igual: la capacidad publicada es para saber qué ofrecer, no es la autorización.
/// </summary>
public sealed class BloqueoDuro(string precondicion, string mensaje) : Exception(mensaje)
{
    /// <summary>El identificador de la precondición: `BD-01` a `BD-11`.</summary>
    public string Precondicion { get; } = precondicion;
}
