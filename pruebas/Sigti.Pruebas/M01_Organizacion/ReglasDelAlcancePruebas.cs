using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M01_Organizacion;

/// <summary>
/// `actores-y-roles` §3 — el alcance de datos, aplicado.
///
/// Lo que estas pruebas defienden es que <b>falle cerrado</b>. Un control de acceso que ante la
/// duda abre no es un control: funciona mientras nada falle, y lo que falla es el espejo de
/// `ACT-16`, que viene de otro sistema.
/// </summary>
public class ReglasDelAlcancePruebas
{
    private static readonly IdPuesto Transporte = new("PUE-JEFE-TRANSPORTE");
    private static readonly IdPuesto Despacho = new("PUE-DESPACHO-SEDE");
    private static readonly IdPuesto Choluteca = new("PUE-DELEGACION-CHOLUTECA");
    private static readonly IdPuesto Gerencia = new("PUE-GERENCIA-ADMIN");

    /// <summary>El espejo real de desarrollo, con su jerarquía.</summary>
    private static readonly List<Puesto> Espejo =
    [
        new(Transporte, "Jefe de Transporte", "Unidad de Transporte", null, null),
        new(Despacho, "Encargado de Despacho", "Unidad de Transporte", Transporte, null),
        new(new IdPuesto("PUE-JEFE-REGIONAL"), "Jefe Regional", "Delegacion de Choluteca",
            Transporte, "Choluteca"),
        new(Choluteca, "Encargado de Delegación", "Delegacion de Choluteca",
            new IdPuesto("PUE-JEFE-REGIONAL"), "Choluteca"),
        new(Gerencia, "Gerencia Administrativa", "Gerencia Administrativa", null, null),
    ];

    private static ExpedienteParaAlcance Exp(
        string? dependencia = "Unidad de Transporte",
        string? delegacion = null,
        string captura = "P-ASISTENTE",
        string solicita = "P-JEFATURA") =>
        new("EXP-1", dependencia, delegacion, new IdPersona(captura), new IdPersona(solicita),
            [], []);

    // ── Falla cerrado ───────────────────────────────────────────────────────

    [Fact]
    public void Un_puesto_que_no_esta_en_el_espejo_no_alcanza_nada()
    {
        var r = ReglasDelAlcance.Resolver(
            new IdPuesto("PUE-QUE-NO-EXISTE"), AlcanceDeDatos.Dependencia, Espejo);

        // El defecto que esta prueba impide: devolver «sin restricción» cuando no se pudo
        // resolver. Ahí una falla de la integración con ARGOS se convierte en un permiso, y
        // nadie se entera — la lista se ve llena, que es como se ve cuando todo está bien.
        Assert.False(r.SePudoResolver);
        Assert.False(ReglasDelAlcance.Alcanza(r, Exp()));
        Assert.Contains("no está en el espejo", r.PorQueNo);
    }

    [Fact]
    public void No_poder_resolver_y_no_tener_permiso_se_distinguen()
    {
        var irresoluble = ReglasDelAlcance.Resolver(
            new IdPuesto("PUE-QUE-NO-EXISTE"), AlcanceDeDatos.Dependencia, Espejo);
        var resuelto = ReglasDelAlcance.Resolver(Despacho, AlcanceDeDatos.Dependencia, Espejo);

        // Las dos muestran una lista vacía y sólo una es correcta. Por eso viaja el porqué.
        Assert.NotNull(irresoluble.PorQueNo);
        Assert.Null(resuelto.PorQueNo);
        Assert.True(resuelto.SePudoResolver);
    }

    [Fact]
    public void Un_puesto_de_sede_con_alcance_de_delegacion_no_alcanza_todo()
    {
        // `Delegacion` nula es SEDE, no dato faltante. La competencia está mal otorgada, y
        // taparlo con un permiso haría invisible el error de configuración.
        var r = ReglasDelAlcance.Resolver(Transporte, AlcanceDeDatos.Delegacion, Espejo);

        Assert.False(r.SePudoResolver);
        Assert.Contains("puesto de sede", r.PorQueNo);
    }

    // ── Los cuatro niveles ──────────────────────────────────────────────────

    [Fact]
    public void Institucion_ve_todo_y_no_necesita_el_espejo()
    {
        // Es el único nivel que se resuelve sin mirar el espejo: `ACT-08`, `ACT-09` y `ACT-12`
        // ven todo por definición, y hacerlos depender de la integración los dejaría ciegos
        // cuando el espejo falle.
        var r = ReglasDelAlcance.Resolver(new IdPuesto("PUE-CUALQUIERA"),
            AlcanceDeDatos.Institucion, []);

        Assert.True(r.SePudoResolver);
        Assert.True(ReglasDelAlcance.Alcanza(r, Exp(dependencia: "Cualquier cosa")));
    }

    [Fact]
    public void Dependencia_alcanza_la_unidad_propia_y_la_de_los_puestos_subordinados()
    {
        // §3.1: «la unidad organizativa del puesto **y sus unidades descendientes**».
        var unidades = ReglasDelAlcance.UnidadesAlcanzadas(Transporte, Espejo);

        Assert.Contains("Unidad de Transporte", unidades);
        Assert.Contains("Delegacion de Choluteca", unidades);
        Assert.DoesNotContain("Gerencia Administrativa", unidades);
    }

    [Fact]
    public void Un_puesto_subordinado_no_alcanza_hacia_arriba()
    {
        // El despacho cuelga de transporte: ve su unidad, no la delegación que cuelga del otro
        // lado del árbol. Si alcanzara hacia arriba, todo puesto vería toda la institución.
        var unidades = ReglasDelAlcance.UnidadesAlcanzadas(Despacho, Espejo);

        Assert.Equal(["Unidad de Transporte"], unidades);
    }

    [Fact]
    public void Un_ciclo_en_el_espejo_no_cuelga_el_recorrido()
    {
        // El espejo viene de otro sistema: no se puede suponer que siempre sea un árbol. Un
        // recorrido recursivo ingenuo se colgaría, y el cuelgue aparecería en producción.
        var a = new IdPuesto("A");
        var b = new IdPuesto("B");
        List<Puesto> ciclo =
        [
            new(a, "A", "Unidad A", b, null),
            new(b, "B", "Unidad B", a, null),
        ];

        var unidades = ReglasDelAlcance.UnidadesAlcanzadas(a, ciclo);

        Assert.Equal(2, unidades.Count);
    }

    [Fact]
    public void Delegacion_atraviesa_dependencias()
    {
        // §3.1: es un corte territorial, no jerárquico. Los dos ejes coexisten.
        var r = ReglasDelAlcance.Resolver(Choluteca, AlcanceDeDatos.Delegacion, Espejo);

        Assert.True(ReglasDelAlcance.Alcanza(r, Exp(dependencia: "Otra cosa", delegacion: "Choluteca")));
        Assert.False(ReglasDelAlcance.Alcanza(r, Exp(delegacion: "Copán")));
    }

    [Fact]
    public void Propio_es_autor_o_solicitante_de_derecho()
    {
        // Es el nivel que hace que un motorista no vea las misiones de sus compañeros.
        var r = ReglasDelAlcance
            .Resolver(Transporte, AlcanceDeDatos.Propio, Espejo)
            .De(new IdPersona("P-ASISTENTE"));

        Assert.True(ReglasDelAlcance.Alcanza(r, Exp(captura: "P-ASISTENTE")));
        Assert.False(ReglasDelAlcance.Alcanza(r, Exp(captura: "P-OTRO", solicita: "P-OTRO")));
    }

    [Fact]
    public void Propio_alcanza_lo_que_uno_pidio_aunque_lo_haya_capturado_otro()
    {
        // Con frecuencia captura la asistente por encargo de la jefatura: quien es solicitante
        // de derecho tiene que ver su propia solicitud aunque no la haya escrito.
        var r = ReglasDelAlcance
            .Resolver(Transporte, AlcanceDeDatos.Propio, Espejo)
            .De(new IdPersona("P-JEFATURA"));

        Assert.True(ReglasDelAlcance.Alcanza(r, Exp(captura: "P-ASISTENTE", solicita: "P-JEFATURA")));
    }

    // ── Los datos sucios ────────────────────────────────────────────────────

    [Fact]
    public void Un_expediente_sin_dependencia_no_entra_en_ningun_alcance_de_dependencia()
    {
        // Hay expedientes sembrados con la dependencia vacía. Tratar el vacío como comodín los
        // mostraría a todo el mundo; tratarlo como una unidad más los mostraría a nadie. Lo
        // segundo es lo correcto: un expediente sin dependencia es un dato incompleto, no un
        // expediente de todos.
        var r = ReglasDelAlcance.Resolver(Transporte, AlcanceDeDatos.Dependencia, Espejo);

        Assert.False(ReglasDelAlcance.Alcanza(r, Exp(dependencia: "")));
        Assert.False(ReglasDelAlcance.Alcanza(r, Exp(dependencia: null)));
    }

    [Fact]
    public void La_comparacion_de_unidad_ignora_mayusculas_pero_no_adivina()
    {
        var r = ReglasDelAlcance.Resolver(Transporte, AlcanceDeDatos.Dependencia, Espejo);

        Assert.True(ReglasDelAlcance.Alcanza(r, Exp(dependencia: "UNIDAD DE TRANSPORTE")));

        // ⚠️ Debilidad conocida y anotada: «Delegacion Choluteca» y «Delegacion de Choluteca»
        // son la misma unidad en la realidad y dos para esta comparación. Normalizar de más
        // adivinaría, y adivinar en un control de acceso abre lo que no debía abrirse.
        Assert.False(ReglasDelAlcance.Alcanza(r, Exp(dependencia: "Unidad Transporte")));
    }
}
