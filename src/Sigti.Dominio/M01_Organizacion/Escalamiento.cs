using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M01_Organizacion;

/// <summary>
/// Un puesto de la estructura — <b>espejo de ARGOS</b>.
///
/// ── Lo que agrega sobre <see cref="AsignacionDePuesto"/> ────────────────────
/// La asignación dice <i>quién ocupa qué</i>; esto dice <b>qué es cada puesto y de quién
/// depende</b>. Sin la jerarquía, el escalamiento de §5.3.B.3 no puede dar su primer salto y
/// todo bloqueo termina en Gerencia Administrativa, que es el último recurso y no el primero.
/// </summary>
/// <param name="Superior">
/// Nulo es <b>la cima de su rama</b>, no «falta el dato». Un puesto sin superior existe —la
/// máxima autoridad no depende de nadie— y tratarlo como dato faltante haría que el
/// escalamiento buscara para siempre.
/// </param>
/// <param name="Delegacion">
/// La delegación territorial, si el puesto pertenece a una. <b>Nulo es sede</b>: el corte
/// territorial y el jerárquico coexisten (§3.1), y un puesto de sede no está en ninguna
/// delegación.
/// </param>
public sealed record Puesto(
    IdPuesto Id,
    string Denominacion,
    string Unidad,
    IdPuesto? Superior,
    string? Delegacion);

/// <summary>Por cuál de los tres saltos de §5.3.B.3 se resolvió el destino.</summary>
public enum SaltoDelEscalamiento
{
    /// <summary>El puesto superior, dentro de la misma unidad.</summary>
    PuestoSuperior,
    /// <summary>El puesto de sede designado como respaldo de esa delegación.</summary>
    RespaldoDeSede,
    /// <summary>Gerencia Administrativa — <b>último recurso</b>.</summary>
    GerenciaAdministrativa,
}

/// <summary>
/// A dónde va el acto bloqueado, y <b>por qué no fue a los anteriores</b>.
/// </summary>
/// <param name="PorQueNoAntes">
/// Qué falló en los saltos previos. <b>No es decoración</b>: un escalamiento que siempre termina
/// en `ACT-08` sin decir por qué se lee como que la jerarquía no sirve, cuando lo que puede estar
/// pasando es que el puesto superior esté vacante — que es un problema de organización que
/// alguien tiene que resolver, y sólo se ve si se dice.
/// </param>
public sealed record DestinoDelActo(
    SaltoDelEscalamiento Salto,
    IdPuesto? Puesto,
    IReadOnlyList<IdPersona> Ocupantes,
    string PorQueNoAntes);

/// <summary>
/// La estructura de puestos, con su jerarquía y su corte territorial.
/// </summary>
public sealed class EstructuraDePuestos(IReadOnlyList<Puesto> puestos)
{
    public static EstructuraDePuestos Vacia { get; } = new([]);

    /// <summary><b>Nulo es «el espejo no lo conoce»</b>, que no es «no existe».</summary>
    public Puesto? De(IdPuesto id) => puestos.FirstOrDefault(p => p.Id == id);

    /// <summary>Si el espejo trae jerarquía. Sin ella el primer salto no se puede intentar.</summary>
    public bool TieneJerarquia => puestos.Any(p => p.Superior is not null);
}

/// <summary>
/// Dónde queda el respaldo de sede de una delegación — <b>maestro de SIGTI</b>.
///
/// ── Por qué esto NO es de ARGOS ─────────────────────────────────────────────
/// ARGOS conoce la estructura; <b>no conoce nuestra política de escalamiento</b>. Que la
/// Delegación de Choluteca escale a tal puesto de sede cuando su encargado queda bloqueado por
/// segregación es una decisión de control interno de SIGTI, no un dato del organigrama.
/// </summary>
public sealed record RespaldoDeSede(string Delegacion, IdPuesto Puesto);

/// <summary>
/// <b>§5.3.B.3</b> — el escalamiento. *«Se ofrece escalamiento en el acto, no un callejón sin
/// salida.»*
///
/// ── Por qué esto existe y no basta con bloquear ─────────────────────────────
/// <i>«La misión no queda trabada por un problema de organización: queda visiblemente pendiente
/// en la bandeja de alguien.»</i> Y más arriba, §5.4: *«bloquear sin alternativa no produce
/// control: produce evasión»*. Un bloqueo perfecto que deja a la delegación sin salida termina
/// con la delegación operando en papel, y entonces no hay ni control ni sistema.
///
/// ── Los tres saltos, y por qué el orden importa ─────────────────────────────
/// El superior dentro de la misma unidad conoce el caso y puede resolverlo hoy. El respaldo de
/// sede ya es un rodeo. Gerencia Administrativa es el último recurso, y mandarle todo la
/// convierte en un cuello de botella que nadie atiende.
/// </summary>
public static class ReglasDelEscalamiento
{
    /// <summary>
    /// Resuelve a dónde va el acto que se bloqueó.
    /// </summary>
    /// <param name="quienIntento">
    /// <b>Se excluye del destino.</b> Escalar a la misma persona que quedó bloqueada es un
    /// callejón sin salida disfrazado de bandeja — y ocurre de verdad: quien ocupa el puesto y
    /// el superior a la vez es justamente el caso de la delegación chica.
    /// </param>
    public static DestinoDelActo Resolver(
        IdPersona quienIntento,
        IdPuesto? puestoDeQuienIntento,
        EstructuraDePuestos estructura,
        Organigrama organigrama,
        IReadOnlyList<RespaldoDeSede> respaldos,
        DateOnly fechaDelHecho)
    {
        var motivos = new List<string>();

        // ── Salto 1: el puesto superior, dentro de la misma unidad ──────────
        var suyo = puestoDeQuienIntento is { } p ? estructura.De(p) : null;

        if (suyo is null)
        {
            motivos.Add(estructura.TieneJerarquia
                ? "el espejo no conoce el puesto de quien intentó"
                : "el espejo del organigrama no trae la jerarquía de puestos");
        }
        else if (suyo.Superior is not { } idSuperior)
        {
            motivos.Add($"{suyo.Denominacion} no tiene puesto superior: es la cima de su rama");
        }
        else if (estructura.De(idSuperior) is not { } superior)
        {
            motivos.Add("el espejo no conoce el puesto superior");
        }
        else if (superior.Unidad != suyo.Unidad)
        {
            // §5.3.B.3 dice «dentro de la misma unidad». Un superior de otra unidad ya es el
            // rodeo del segundo salto, y llamarlo el primero borraría la distinción.
            motivos.Add(
                $"el superior ({superior.Denominacion}) es de otra unidad, {superior.Unidad}");
        }
        else
        {
            var ocupantes = SinQuienIntento(
                organigrama.QuienesOcupan(idSuperior, fechaDelHecho), quienIntento);

            if (ocupantes.Count > 0)
            {
                return new DestinoDelActo(
                    SaltoDelEscalamiento.PuestoSuperior, idSuperior, ocupantes, string.Empty);
            }

            // «Si no existe **o está vacante**». Y vacante incluye el caso en que el único
            // ocupante es quien quedó bloqueado.
            motivos.Add($"{superior.Denominacion} está vacante");
        }

        // ── Salto 2: el respaldo de sede de su delegación ───────────────────
        if (suyo?.Delegacion is { } delegacion)
        {
            var respaldo = respaldos.FirstOrDefault(r => r.Delegacion == delegacion);

            if (respaldo is null)
            {
                motivos.Add($"la delegación {delegacion} no tiene respaldo de sede designado");
            }
            else
            {
                var ocupantes = SinQuienIntento(
                    organigrama.QuienesOcupan(respaldo.Puesto, fechaDelHecho), quienIntento);

                if (ocupantes.Count > 0)
                {
                    return new DestinoDelActo(
                        SaltoDelEscalamiento.RespaldoDeSede, respaldo.Puesto, ocupantes,
                        string.Join("; ", motivos));
                }

                motivos.Add($"el respaldo de sede de {delegacion} está vacante");
            }
        }
        else if (suyo is not null)
        {
            // Un puesto de sede no tiene respaldo de delegación, y decirlo evita que el salto
            // se lea como un hueco de configuración.
            motivos.Add("el puesto es de sede y no pertenece a ninguna delegación");
        }

        // ── Salto 3: Gerencia Administrativa ────────────────────────────────
        return new DestinoDelActo(
            SaltoDelEscalamiento.GerenciaAdministrativa, null, [], string.Join("; ", motivos));
    }

    /// <summary>
    /// Quita a quien quedó bloqueado de la lista de destinatarios.
    ///
    /// <b>Es la diferencia entre una bandeja y un callejón sin salida.</b> §5.4 describe la
    /// delegación donde una persona ocupa varios puestos: escalarle el acto a ella misma le
    /// devolvería el mismo bloqueo, y el sistema se leería como roto.
    /// </summary>
    private static IReadOnlyList<IdPersona> SinQuienIntento(
        IReadOnlyList<IdPersona> ocupantes, IdPersona quienIntento) =>
        [.. ocupantes.Where(o => o != quienIntento)];

    /// <summary>
    /// El destino dicho como lo lee quien quedó bloqueado.
    /// </summary>
    public static string EnPalabras(DestinoDelActo destino, EstructuraDePuestos estructura)
    {
        var quienes = destino.Ocupantes.Count == 0
            ? string.Empty
            : $" ({string.Join(" o ", destino.Ocupantes.Select(o => o.Valor))})";

        var nombre = destino.Puesto is { } p
            ? estructura.De(p)?.Denominacion ?? p.Valor
            : "Gerencia Administrativa (ACT-08)";

        var porQue = destino.PorQueNoAntes.Length == 0
            ? string.Empty
            : $" Los saltos anteriores no aplicaron: {destino.PorQueNoAntes}.";

        return destino.Salto switch
        {
            SaltoDelEscalamiento.PuestoSuperior =>
                $"Queda pendiente de resolución en {nombre}{quienes}, que es el puesto superior " +
                "dentro de su misma unidad.",

            SaltoDelEscalamiento.RespaldoDeSede =>
                $"Queda pendiente de resolución en {nombre}{quienes}, el respaldo de sede de su " +
                $"delegación.{porQue}",

            _ =>
                $"Queda pendiente de resolución en {nombre} — el último recurso.{porQue}",
        };
    }
}
