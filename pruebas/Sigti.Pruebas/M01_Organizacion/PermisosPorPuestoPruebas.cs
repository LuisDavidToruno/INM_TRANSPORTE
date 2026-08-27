using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M01_Organizacion;

/// <summary>
/// `RN-100` — los permisos se conceden al <b>puesto</b>, nunca a la persona.
///
/// ── El problema que esto resuelve, y por qué es de este país ─────────────────
/// `NRM-09` `[V]`: la rotación de personal en el sector público es alta, y Honduras
/// cambió de gobierno en enero de 2026. Cuando el permiso cuelga de la persona, cada
/// rotación obliga a reconstruir a mano quién puede hacer qué — y lo que ocurre en la
/// práctica es conocido: se copian los permisos del saliente al entrante *«para que
/// pueda trabajar»*, y con ellos toda la acumulación indebida que el saliente había
/// juntado en años. <b>La segregación de `RN-01` se pierde sin que nadie decida
/// perderla.</b>
///
/// ── Y su recíproco, que es la mitad que se olvida ────────────────────────────
/// La <b>autoría</b> de un acto es de la persona y no se reasigna jamás. Cuando el
/// auditor pregunta *«¿quién autorizó esto y con qué competencia?»*, el nombre solo no
/// responde: la competencia estaba en el puesto, y el puesto pudo cambiar de manos tres
/// veces desde entonces. Por eso se guardan los dos, congelados en el asiento.
/// </summary>
public class PermisosPorPuestoPruebas
{
    private static readonly IdPersona Saliente = new("P-SALIENTE");
    private static readonly IdPersona Entrante = new("P-ENTRANTE");
    private static readonly IdPuesto Jefatura = new("PU-JEFATURA-CHOLUTECA");

    [Fact]
    public void Quien_dejo_el_puesto_pierde_sus_permisos_el_dia_que_lo_dejo()
    {
        // El caso de todos los meses. Si esto fallara, un servidor trasladado seguiría
        // autorizando misiones de una delegación que ya no es la suya.
        var organigrama = new Organigrama([
            new AsignacionDePuesto(Saliente, Jefatura,
                Desde: new DateOnly(2025, 1, 15),
                Hasta: new DateOnly(2026, 2, 28)),
        ]);

        Assert.True(organigrama.Ocupa(Saliente, Jefatura, new DateOnly(2026, 2, 28)));
        Assert.False(organigrama.Ocupa(Saliente, Jefatura, new DateOnly(2026, 3, 1)));
    }

    [Fact]
    public void Los_permisos_se_resuelven_a_la_FECHA_DEL_HECHO_no_a_la_de_consulta()
    {
        // `P-4` y `RN-46`. Un acto de febrero se juzga con la ocupación de puesto vigente
        // en febrero — aunque hoy sea abril y el puesto tenga otro dueño.
        //
        // Sin esto, reevaluar un expediente viejo diría que quien lo autorizó no tenía
        // competencia, y el expediente quedaría indefendible por un artefacto del sistema.
        var organigrama = new Organigrama([
            new AsignacionDePuesto(Saliente, Jefatura,
                Desde: new DateOnly(2025, 1, 15), Hasta: new DateOnly(2026, 2, 28)),
            new AsignacionDePuesto(Entrante, Jefatura,
                Desde: new DateOnly(2026, 3, 1), Hasta: null),
        ]);

        var enFebrero = new DateOnly(2026, 2, 10);

        Assert.True(organigrama.Ocupa(Saliente, Jefatura, enFebrero));
        Assert.False(organigrama.Ocupa(Entrante, Jefatura, enFebrero));
    }

    [Fact]
    public void Un_usuario_sin_asignacion_vigente_no_tiene_NINGUN_permiso()
    {
        // «Un usuario sin asignación de puesto vigente no tiene ningún permiso, aunque
        // exista, esté activo y tenga contraseña.» Es la afirmación completa de `RN-100`,
        // y sin ella una cuenta olvidada sigue pudiendo autorizar.
        var organigrama = new Organigrama([]);

        Assert.Empty(organigrama.PuestosDe(Saliente, new DateOnly(2026, 3, 12)));
    }

    [Fact]
    public void Dos_personas_pueden_ocupar_el_mismo_puesto_durante_el_traspaso()
    {
        // Coocupación acotada — `actores-y-roles` §2. El solape existe porque el traspaso
        // real dura días, y negarlo obligaría a dejar el puesto vacante justo cuando hay
        // más trabajo. Ambas tienen los permisos; **cada acto queda a nombre de quien lo
        // hizo**, que es lo que impide que el solape borre la responsabilidad.
        var organigrama = new Organigrama([
            new AsignacionDePuesto(Saliente, Jefatura,
                Desde: new DateOnly(2025, 1, 15), Hasta: new DateOnly(2026, 3, 15)),
            new AsignacionDePuesto(Entrante, Jefatura,
                Desde: new DateOnly(2026, 3, 1), Hasta: null),
        ]);

        var enElSolape = new DateOnly(2026, 3, 10);

        Assert.True(organigrama.Ocupa(Saliente, Jefatura, enElSolape));
        Assert.True(organigrama.Ocupa(Entrante, Jefatura, enElSolape));
    }

    [Fact]
    public void Una_persona_puede_ocupar_dos_puestos_a_la_vez()
    {
        // Frecuente en delegaciones. Sus permisos son la unión de ambos — y `RN-01` sigue
        // bloqueando **por identidad de persona**, así que acumular puestos no es una vía
        // para levantar incompatibilidades.
        var despacho = new IdPuesto("PU-DESPACHO-CHOLUTECA");

        var organigrama = new Organigrama([
            new AsignacionDePuesto(Saliente, Jefatura, new DateOnly(2025, 1, 15), null),
            new AsignacionDePuesto(Saliente, despacho, new DateOnly(2026, 1, 1), null),
        ]);

        var puestos = organigrama.PuestosDe(Saliente, new DateOnly(2026, 3, 12));

        Assert.Equal(2, puestos.Count);
    }
}
