using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M01_Organizacion;

/// <summary>
/// §5.3.B.3 — el escalamiento. *«Se ofrece escalamiento en el acto, no un callejón sin salida.»*
///
/// ── Lo que estas pruebas defienden ──────────────────────────────────────────
/// Que los tres saltos <b>se intenten en orden</b> y que **cada fallo diga por qué**. Un
/// escalamiento que siempre termina en Gerencia Administrativa sin explicarse se lee como que la
/// jerarquía no sirve, cuando lo que puede estar pasando es que el puesto superior esté vacante
/// — un problema de organización que alguien tiene que resolver, y que sólo se ve si se dice.
/// </summary>
public class ReglasDelEscalamientoPruebas
{
    private static readonly DateOnly Hoy = new(2026, 9, 5);

    private static readonly IdPersona Nery = new("P-NERY");
    private static readonly IdPersona Karla = new("P-KARLA");
    private static readonly IdPersona Marlon = new("P-MARLON");

    private static readonly IdPuesto Delegado = new("PUE-DELEGADO-CHOLUTECA");
    private static readonly IdPuesto JefeRegional = new("PUE-JEFE-REGIONAL");
    private static readonly IdPuesto RespaldoSede = new("PUE-RESPALDO-SEDE");

    /// <summary>La estructura de la delegación: delegado → jefe regional, misma unidad.</summary>
    private static EstructuraDePuestos Estructura(
        IdPuesto? superiorDelDelegado = null, string unidadDelSuperior = "Delegación de Choluteca") =>
        new(
        [
            new Puesto(Delegado, "Encargado de Delegación", "Delegación de Choluteca",
                superiorDelDelegado ?? JefeRegional, "Choluteca"),

            new Puesto(JefeRegional, "Jefe Regional", unidadDelSuperior, null, "Choluteca"),

            new Puesto(RespaldoSede, "Jefe de Transporte de sede", "Unidad de Transporte",
                null, null),
        ]);

    private static Organigrama Ocupan(params (IdPersona Quien, IdPuesto Puesto)[] pares) =>
        new([.. pares.Select(p =>
            new AsignacionDePuesto(p.Quien, p.Puesto, new DateOnly(2026, 1, 1), null))]);

    private static readonly IReadOnlyList<RespaldoDeSede> Respaldos =
        [new RespaldoDeSede("Choluteca", RespaldoSede)];

    // ── Salto 1 ─────────────────────────────────────────────────────────────

    /// <summary>
    /// <b>Primero, el puesto superior dentro de la misma unidad.</b> Es quien conoce el caso y
    /// puede resolverlo hoy.
    /// </summary>
    [Fact]
    public void Va_al_puesto_superior_de_la_misma_unidad()
    {
        var destino = ReglasDelEscalamiento.Resolver(
            Nery, Delegado, Estructura(),
            Ocupan((Nery, Delegado), (Karla, JefeRegional)),
            Respaldos, Hoy);

        Assert.Equal(SaltoDelEscalamiento.PuestoSuperior, destino.Salto);
        Assert.Equal(JefeRegional, destino.Puesto);
        Assert.Equal([Karla], destino.Ocupantes);

        // No hay nada que explicar: el primer salto sirvió.
        Assert.Empty(destino.PorQueNoAntes);
    }

    /// <summary>
    /// <b>El superior de OTRA unidad no cuenta como primer salto.</b> §5.3.B.3 dice «dentro de
    /// la misma unidad», y llamarlo el primero borraría la distinción con el segundo, que es
    /// justamente el rodeo por sede.
    /// </summary>
    [Fact]
    public void Un_superior_de_otra_unidad_no_es_el_primer_salto()
    {
        var destino = ReglasDelEscalamiento.Resolver(
            Nery, Delegado, Estructura(unidadDelSuperior: "Gerencia Administrativa"),
            Ocupan((Nery, Delegado), (Karla, JefeRegional), (Marlon, RespaldoSede)),
            Respaldos, Hoy);

        Assert.Equal(SaltoDelEscalamiento.RespaldoDeSede, destino.Salto);
        Assert.Contains("es de otra unidad", destino.PorQueNoAntes);
    }

    // ── Salto 2 ─────────────────────────────────────────────────────────────

    /// <summary>
    /// <b>«Si no existe o está vacante»</b>: el puesto superior sin nadie que lo ocupe manda al
    /// respaldo de sede, <b>y se dice que estaba vacante</b>.
    /// </summary>
    [Fact]
    public void Con_el_superior_vacante_va_al_respaldo_de_sede()
    {
        var destino = ReglasDelEscalamiento.Resolver(
            Nery, Delegado, Estructura(),
            Ocupan((Nery, Delegado), (Marlon, RespaldoSede)),
            Respaldos, Hoy);

        Assert.Equal(SaltoDelEscalamiento.RespaldoDeSede, destino.Salto);
        Assert.Equal(RespaldoSede, destino.Puesto);
        Assert.Contains("está vacante", destino.PorQueNoAntes);
    }

    /// <summary>
    /// <b>Escalar a quien quedó bloqueado es un callejón sin salida disfrazado de bandeja.</b>
    ///
    /// Y ocurre de verdad: §5.4 describe la delegación donde una persona ocupa varios puestos.
    /// Devolverle el acto le devolvería el mismo bloqueo, y el sistema se leería como roto.
    /// </summary>
    [Fact]
    public void No_se_escala_a_la_misma_persona_que_quedo_bloqueada()
    {
        // Nery ocupa el puesto Y su superior: el caso de la delegación chica.
        var destino = ReglasDelEscalamiento.Resolver(
            Nery, Delegado, Estructura(),
            Ocupan((Nery, Delegado), (Nery, JefeRegional), (Marlon, RespaldoSede)),
            Respaldos, Hoy);

        Assert.Equal(SaltoDelEscalamiento.RespaldoDeSede, destino.Salto);
        Assert.DoesNotContain(Nery, destino.Ocupantes);
    }

    // ── Salto 3 ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Sin superior útil y sin respaldo ocupado, <b>Gerencia Administrativa</b> — y el mensaje
    /// enumera los dos motivos, que es lo que convierte el último recurso en información.
    /// </summary>
    [Fact]
    public void Sin_superior_ni_respaldo_va_a_gerencia_y_dice_los_dos_motivos()
    {
        var destino = ReglasDelEscalamiento.Resolver(
            Nery, Delegado, Estructura(), Ocupan((Nery, Delegado)), Respaldos, Hoy);

        Assert.Equal(SaltoDelEscalamiento.GerenciaAdministrativa, destino.Salto);
        Assert.Null(destino.Puesto);
        Assert.Contains("vacante", destino.PorQueNoAntes);
        Assert.Contains("respaldo de sede", destino.PorQueNoAntes);
    }

    /// <summary>
    /// <b>Sin jerarquía en el espejo, el primer salto ni se intenta — y se dice.</b>
    ///
    /// Es el estado en que estuvo el sistema hasta hoy, y decirlo distingue «la organización no
    /// tiene superior» de «la integración no trae el dato».
    /// </summary>
    [Fact]
    public void Sin_jerarquia_en_el_espejo_lo_declara()
    {
        var destino = ReglasDelEscalamiento.Resolver(
            Nery, Delegado, EstructuraDePuestos.Vacia, Ocupan((Nery, Delegado)), [], Hoy);

        Assert.Equal(SaltoDelEscalamiento.GerenciaAdministrativa, destino.Salto);
        Assert.Contains("no trae la jerarquía", destino.PorQueNoAntes);
    }

    /// <summary>
    /// Un puesto de sede <b>no tiene respaldo de delegación</b>, y decirlo evita que el salto se
    /// lea como un hueco de configuración que alguien tendría que ir a llenar.
    /// </summary>
    [Fact]
    public void Un_puesto_de_sede_no_busca_respaldo_de_delegacion()
    {
        var destino = ReglasDelEscalamiento.Resolver(
            Marlon, RespaldoSede, Estructura(), Ocupan((Marlon, RespaldoSede)), Respaldos, Hoy);

        Assert.Equal(SaltoDelEscalamiento.GerenciaAdministrativa, destino.Salto);
        Assert.Contains("no pertenece a ninguna delegación", destino.PorQueNoAntes);
    }

    /// <summary>
    /// La delegación <b>sin respaldo designado</b> se distingue de la que lo tiene vacante: la
    /// primera se resuelve designándolo, la segunda nombrando a alguien.
    /// </summary>
    [Fact]
    public void Sin_respaldo_designado_lo_dice_distinto_de_vacante()
    {
        var destino = ReglasDelEscalamiento.Resolver(
            Nery, Delegado, Estructura(), Ocupan((Nery, Delegado)), [], Hoy);

        Assert.Contains("no tiene respaldo de sede designado", destino.PorQueNoAntes);
    }

    // ── El texto que lee quien quedó bloqueado ──────────────────────────────

    /// <summary>
    /// El mensaje <b>nombra el puesto y a quién</b>, no un identificador. §5.3.B.1: un mensaje
    /// preciso produce la acción correcta.
    /// </summary>
    [Fact]
    public void El_texto_nombra_el_puesto_y_a_quien()
    {
        var estructura = Estructura();

        var destino = ReglasDelEscalamiento.Resolver(
            Nery, Delegado, estructura,
            Ocupan((Nery, Delegado), (Karla, JefeRegional)), Respaldos, Hoy);

        var texto = ReglasDelEscalamiento.EnPalabras(destino, estructura);

        Assert.Contains("Jefe Regional", texto);
        Assert.Contains("P-KARLA", texto);
        Assert.Contains("puesto superior", texto);
    }

    /// <summary>
    /// Cuando cae al último recurso, el texto <b>explica por qué</b>. Sin eso, «va a Gerencia
    /// Administrativa» se lee como que la jerarquía no sirve.
    /// </summary>
    [Fact]
    public void El_texto_del_ultimo_recurso_explica_por_que()
    {
        var estructura = Estructura();

        var destino = ReglasDelEscalamiento.Resolver(
            Nery, Delegado, estructura, Ocupan((Nery, Delegado)), Respaldos, Hoy);

        var texto = ReglasDelEscalamiento.EnPalabras(destino, estructura);

        Assert.Contains("último recurso", texto);
        Assert.Contains("Los saltos anteriores no aplicaron", texto);
    }
}
