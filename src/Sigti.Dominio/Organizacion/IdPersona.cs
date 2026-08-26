namespace Sigti.Dominio.Organizacion;

/// <summary>
/// Identidad de <b>persona</b>, no de usuario. `BD-01` es explícito: un mismo servidor
/// con dos cuentas sigue siendo la misma persona, y la comparación se hace contra el
/// identificador de persona del espejo de Talento Humano.
///
/// Es un tipo propio precisamente para que el compilador impida pasar un identificador
/// de usuario donde la segregación de funciones exige identidad de persona.
/// </summary>
public readonly record struct IdPersona(string Valor)
{
    public override string ToString() => Valor;
}
