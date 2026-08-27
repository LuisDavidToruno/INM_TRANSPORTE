using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M01_Organizacion;

/// <summary>
/// La otra mitad de `RN-100`: <b>la autoría es de la persona y no se reasigna jamás</b>.
///
/// ── La pregunta que esto responde ────────────────────────────────────────────
/// El auditor no pregunta «¿quién firmó?». Pregunta <b>«¿quién autorizó esto, y con qué
/// competencia?»</b>. El nombre solo no responde: la competencia estaba en el puesto, y
/// el puesto pudo haber cambiado de manos tres veces desde entonces.
///
/// Por eso el asiento guarda <b>los dos, congelados</b>. Guardar solo la persona deja el
/// acto sin fundamento; guardar solo el puesto lo deja sin responsable.
/// </summary>
public class AutoriaCongeladaPruebas
{
    private static readonly IdPersona Jefa = new("P-JEFA");
    private static readonly IdPuesto Jefatura = new("PU-JEFATURA-CHOLUTECA");

    [Fact]
    public void La_autoria_conserva_persona_Y_puesto_del_momento_del_acto()
    {
        var autoria = Autoria.De(Jefa, Jefatura, new DateOnly(2026, 2, 10));

        Assert.Equal(Jefa, autoria.Persona);
        Assert.Equal(Jefatura, autoria.Puesto);
        Assert.Equal(new DateOnly(2026, 2, 10), autoria.FechaDelHecho);
    }

    [Fact]
    public void La_autoria_no_se_puede_reasignar_ni_por_descuido()
    {
        // Es `record` inmutable a propósito, y esta prueba lo fija: el día que alguien
        // quiera «corregir» la autoría de un asiento porque el puesto se renombró, el
        // compilador se lo va a impedir. `P-3` — nada se sobrescribe.
        var autoria = Autoria.De(Jefa, Jefatura, new DateOnly(2026, 2, 10));
        var otra = autoria with { Persona = new IdPersona("P-OTRO") };

        // `with` produce una copia; el original no cambió. Reasignar exige crear un
        // asiento nuevo, que es exactamente lo que `RN-04` obliga.
        Assert.Equal(Jefa, autoria.Persona);
        Assert.NotEqual(autoria, otra);
    }

    [Fact]
    public void Un_puesto_suprimido_sigue_explicando_los_actos_que_autorizo()
    {
        // Una reestructuración renombra o suprime puestos. Los asientos históricos
        // conservan el puesto **tal como se llamaba entonces**, y por eso el catálogo de
        // puestos se cierra con vigencia en vez de borrarse: los actos que autorizó siguen
        // existiendo y tienen que poder explicarse.
        var suprimido = new IdPuesto("PU-SUBJEFATURA-2025");
        var autoria = Autoria.De(Jefa, suprimido, new DateOnly(2025, 11, 3));

        Assert.Equal("PU-SUBJEFATURA-2025", autoria.Puesto.Valor);
    }
}
