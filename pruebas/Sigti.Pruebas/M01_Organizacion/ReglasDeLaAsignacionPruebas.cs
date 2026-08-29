using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Pruebas.M01_Organizacion;

/// <summary>
/// La segregación de funciones — §5 de <c>actores-y-roles.md</c>.
///
/// Lo que el sistema <b>hace</b> con la tabla: el control preventivo de §5.3.A al otorgar un
/// rol, y la unión de competencias sobre la persona. La tabla en sí —su transcripción y el
/// puente rol→función— vive en <see cref="IncompatibilidadesPruebas"/>.
/// </summary>
public class ReglasDeLaAsignacionPruebas
{
    // ── El control preventivo de §5.3.A ─────────────────────────────────────

    /// <summary>
    /// <b>`I-12` rechaza la asignación</b>, sin esperar a que haya un expediente: *«la
    /// independencia de la auditoría no admite excepción»*.
    /// </summary>
    [Fact]
    public void Auditor_mas_cualquier_rol_ejecutor_se_rechaza()
    {
        var efecto = ReglasDeLaAsignacion.Evaluar([Rol.AuditorInterno, Rol.EncargadoDeDespacho]);

        Assert.True(efecto.SeRechaza);
        Assert.Contains("I-12", efecto.Rechazan.Select(p => p.Id));

        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaAsignacion.Exigir(efecto, "Karla Zavala", Rol.EncargadoDeDespacho));

        Assert.Equal("I-12", error.Precondicion);
        Assert.Contains("núcleo irreductible", error.Message);
        Assert.Contains("no se levanta", error.Message.ToLowerInvariant());
    }

    /// <summary>
    /// <b>`I-13`</b>: el administrador podría otorgarse la facultad y borrar el rastro.
    /// </summary>
    [Fact]
    public void Administrador_mas_facultad_de_aprobar_fondo_se_rechaza()
    {
        var efecto = ReglasDeLaAsignacion.Evaluar([Rol.Administrador, Rol.GerenciaAdministrativa]);

        Assert.True(efecto.SeRechaza);
        Assert.Contains("I-13", efecto.Rechazan.Select(p => p.Id));
    }

    /// <summary>
    /// <b>El administrador solo NO se rechaza.</b> Sin este recíproco, una regla que rechazara
    /// siempre a `ACT-01` pasaría la prueba anterior y nadie podría administrar el sistema.
    /// </summary>
    [Fact]
    public void El_administrador_solo_pasa()
    {
        Assert.False(ReglasDeLaAsignacion.Evaluar([Rol.Administrador]).SeRechaza);
    }

    /// <summary>
    /// <b>La acumulación por expediente NO se prohíbe: se vigila.</b>
    ///
    /// §5.3.A: *«no se puede prohibir de entrada que el Encargado de Delegación sea también
    /// Solicitante: sería inoperante»*. El bloqueo real llega al ejecutar el acto sobre una
    /// misión concreta.
    /// </summary>
    [Fact]
    public void La_delegacion_que_acumula_pasa_pero_queda_vigilada()
    {
        var efecto = ReglasDeLaAsignacion.Evaluar([Rol.EncargadoDeDelegacion, Rol.Solicitante]);

        Assert.False(efecto.SeRechaza);
        Assert.True(efecto.QuedaVigilada);

        // Los dos extremos de la cadena de control, en una sola persona.
        Assert.Contains("I-07", efecto.Vigilados.Select(p => p.Id));

        // Y no tira: la asignación se guarda.
        ReglasDeLaAsignacion.Exigir(efecto, "Nery Alvarado", Rol.Solicitante);
    }

    /// <summary>
    /// Un puesto con un solo rol corriente <b>no queda vigilado</b>. Sin este recíproco, una
    /// implementación que marcara todo como vigilado pasaría la prueba anterior y el tablero
    /// de `ACT-08` sería inútil por estar siempre lleno.
    /// </summary>
    [Fact]
    public void Un_rol_solo_no_queda_vigilado()
    {
        var efecto = ReglasDeLaAsignacion.Evaluar([Rol.Solicitante]);

        Assert.False(efecto.SeRechaza);
        Assert.False(efecto.QuedaVigilada);
    }

    /// <summary>
    /// El par que <b>ninguna delegación puede levantar</b>: quien entrega el dinero no declara
    /// en qué se gastó.
    /// </summary>
    [Fact]
    public void Entregar_el_fondo_y_liquidar_queda_vigilado_como_nucleo()
    {
        var efecto = ReglasDeLaAsignacion.Evaluar(
            [Rol.EncargadoDeCombustible, Rol.JefeDeTransporte]);

        var i10 = efecto.Vigilados.Single(p => p.Id == "I-10");
        Assert.Equal(NivelDeIncompatibilidad.NucleoIrreductible, i10.Nivel);
    }

    // ── La unión sobre la persona, no sobre el puesto ───────────────────────

    /// <summary>
    /// <b>Las incompatibilidades se evalúan sobre la persona, nunca sobre el puesto</b> —§5.2.
    ///
    /// Dos puestos inofensivos por separado pueden ser incompatibles juntos, y mirar puesto por
    /// puesto es exactamente cómo se cuela la acumulación que la segregación existe para
    /// impedir.
    /// </summary>
    [Fact]
    public void Dos_puestos_inofensivos_por_separado_pueden_serlo_juntos()
    {
        var auditoria = new IdPuesto("PUE-AUDITORIA");
        var despacho = new IdPuesto("PUE-DESPACHO");
        var persona = new Sigti.Dominio.Organizacion.IdPersona("P-NERY");
        var hoy = new DateOnly(2026, 8, 29);

        // Cada puesto, por su cuenta, es intachable.
        Assert.False(ReglasDeLaAsignacion.Evaluar([Rol.AuditorInterno]).SeRechaza);
        Assert.False(ReglasDeLaAsignacion.Evaluar([Rol.EncargadoDeDespacho]).SeRechaza);

        var organigrama = new Organigrama(
        [
            new AsignacionDePuesto(persona, auditoria, new DateOnly(2026, 1, 1), null),
            new AsignacionDePuesto(persona, despacho, new DateOnly(2026, 1, 1), null),
        ]);

        var tabla = new TablaDeCompetencias(
        [
            new CompetenciaDelPuesto(auditoria, Rol.AuditorInterno, AlcanceDeDatos.Institucion,
                new DateOnly(2026, 1, 1), null),
            new CompetenciaDelPuesto(despacho, Rol.EncargadoDeDespacho, AlcanceDeDatos.Dependencia,
                new DateOnly(2026, 1, 1), null),
        ]);

        var suyas = tabla.De(persona, organigrama, hoy);

        Assert.Equal(2, suyas.Puestos.Count);
        Assert.True(ReglasDeLaAsignacion.Evaluar(suyas.Roles).SeRechaza);
    }

    /// <summary>
    /// Todo se resuelve <b>a la fecha del hecho</b> (`P-4`, `RN-46`). Una competencia que ya
    /// venció no cuenta, y una que todavía no empezaba tampoco.
    /// </summary>
    [Fact]
    public void La_competencia_vencida_no_cuenta()
    {
        var puesto = new IdPuesto("PUE-01");
        var persona = new Sigti.Dominio.Organizacion.IdPersona("P-MARLON");

        var organigrama = new Organigrama(
            [new AsignacionDePuesto(persona, puesto, new DateOnly(2026, 1, 1), null)]);

        var tabla = new TablaDeCompetencias(
        [
            new CompetenciaDelPuesto(puesto, Rol.EncargadoDeDespacho, AlcanceDeDatos.Dependencia,
                new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30)),
        ]);

        Assert.True(tabla.De(persona, organigrama, new DateOnly(2026, 6, 30)).Tiene(Rol.EncargadoDeDespacho));
        Assert.False(tabla.De(persona, organigrama, new DateOnly(2026, 7, 1)).Tiene(Rol.EncargadoDeDespacho));

        // Y el 1 de julio la persona ocupa el puesto pero **no tiene ninguna competencia**,
        // que no es lo mismo que no ocupar nada.
        var despues = tabla.De(persona, organigrama, new DateOnly(2026, 7, 1));
        Assert.Single(despues.Puestos);
        Assert.True(despues.SinCompetencia);
    }

    /// <summary>
    /// <b>Una persona sin puesto vigente es un usuario sin permisos</b> —§2.3—, y eso no es lo
    /// mismo que una persona que el espejo no conoce.
    /// </summary>
    [Fact]
    public void Sin_puesto_no_hay_permiso_y_el_alcance_es_nulo()
    {
        var sin = CompetenciasDeLaPersona.SinPuesto(
            new Sigti.Dominio.Organizacion.IdPersona("P-QUIENSEA"), new DateOnly(2026, 8, 29));

        Assert.True(sin.SinCompetencia);
        Assert.Empty(sin.Roles);

        // Nulo es «no tiene alcance», que no es `Propio`: `Propio` ya es un permiso.
        Assert.Null(sin.AlcanceMaximo);
    }

    /// <summary>
    /// El alcance es <b>el más amplio de los puestos</b>: quien ocupa uno de sede con alcance
    /// institución no deja de verlo todo porque además ocupe uno regional.
    /// </summary>
    [Fact]
    public void El_alcance_es_el_mas_amplio_de_los_puestos()
    {
        var sede = new IdPuesto("PUE-SEDE");
        var regional = new IdPuesto("PUE-REGIONAL");
        var persona = new Sigti.Dominio.Organizacion.IdPersona("P-KARLA");
        var hoy = new DateOnly(2026, 8, 29);

        var organigrama = new Organigrama(
        [
            new AsignacionDePuesto(persona, sede, new DateOnly(2026, 1, 1), null),
            new AsignacionDePuesto(persona, regional, new DateOnly(2026, 1, 1), null),
        ]);

        var tabla = new TablaDeCompetencias(
        [
            new CompetenciaDelPuesto(sede, Rol.GerenciaAdministrativa, AlcanceDeDatos.Institucion,
                new DateOnly(2026, 1, 1), null),
            new CompetenciaDelPuesto(regional, Rol.Solicitante, AlcanceDeDatos.Propio,
                new DateOnly(2026, 1, 1), null),
        ]);

        Assert.Equal(AlcanceDeDatos.Institucion, tabla.De(persona, organigrama, hoy).AlcanceMaximo);
    }
}
