using Sigti.Dominio.M01_Organizacion;

namespace Sigti.Pruebas.M01_Organizacion;

/// <summary>
/// La tabla de incompatibilidades — §5.2 de <c>actores-y-roles.md</c>, que es la autoridad.
///
/// ── Dos cosas distintas se defienden acá ────────────────────────────────────
/// Que la <b>tabla</b> está bien transcrita, y que el <b>puente rol→función</b> está bien
/// tendido. La segunda es la que se rompe en silencio: la tabla puede estar perfecta y el
/// puente mal, y entonces el sistema bloquea lo que no debe y deja pasar lo que sí — sin que
/// ninguna prueba de la tabla lo note, porque la tabla estaría bien.
///
/// Lo que hace el sistema <b>con</b> la tabla vive en <see cref="ReglasDeLaAsignacionPruebas"/>.
/// </summary>
public class IncompatibilidadesPruebas
{

    // ── La tabla, contra el documento ───────────────────────────────────────

    /// <summary>
    /// Los diecinueve identificadores están, y <b>ninguno se reciclò</b>.
    ///
    /// `I-11`, `I-12` y `I-13` aparecen varias veces —son uno contra varios— y por eso se
    /// cuentan identificadores distintos y no filas.
    /// </summary>
    [Fact]
    public void Estan_los_diecinueve_pares()
    {
        var ids = Incompatibilidades.Tabla.Select(p => p.Id).Distinct().Order().ToList();

        Assert.Equal(19, ids.Count);
        Assert.Equal("I-01", ids[0]);
        Assert.Equal("I-19", ids[^1]);
    }

    /// <summary>
    /// **El núcleo irreductible son exactamente cinco**, y son los que el documento nombra:
    /// <i>«I-07, I-10, I-11, I-12, I-13 no se levanta nunca»</i>.
    ///
    /// Se verifica por lista y no por cantidad: contar cinco no dice que sean los cinco.
    /// </summary>
    [Fact]
    public void El_nucleo_irreductible_son_esos_cinco_y_no_otros()
    {
        var nucleo = Incompatibilidades.Tabla
            .Where(p => p.Nivel == NivelDeIncompatibilidad.NucleoIrreductible)
            .Select(p => p.Id)
            .Distinct()
            .Order()
            .ToList();

        Assert.Equal(["I-07", "I-10", "I-11", "I-12", "I-13"], nucleo);
    }

    /// <summary>
    /// `I-14` es <b>configurable y está apagado</b>: no está en la enumeración del MARCI.
    /// Tratarlo como bloqueo dejaría sin operar a `ACT-04`, que emite y liquida por diseño.
    /// </summary>
    [Fact]
    public void I14_es_configurable_y_no_bloquea()
    {
        var i14 = Incompatibilidades.Tabla.Single(p => p.Id == "I-14");

        Assert.Equal(NivelDeIncompatibilidad.Configurable, i14.Nivel);

        // Y por eso NO entra en lo vigilado: vigilar por algo apagado es ruido.
        var vigilados = Incompatibilidades
            .VigiladosQueActiva([Rol.JefeDeTransporte, Rol.GerenciaAdministrativa])
            .Select(p => p.Id);

        Assert.DoesNotContain("I-14", vigilados);
    }

    /// <summary>
    /// `I-15` e `I-16` son <b>advertencia</b>: se continúa con motivo escrito. Convertirlas en
    /// bloqueo inventaría una norma — las dos son práctica de control marcada `[I]`.
    /// </summary>
    [Theory]
    [InlineData("I-15")]
    [InlineData("I-16")]
    public void Las_advertencias_no_son_bloqueo(string id)
    {
        var par = Incompatibilidades.Tabla.Single(p => p.Id == id);
        Assert.Equal(NivelDeIncompatibilidad.Advertencia, par.Nivel);
    }

    /// <summary>
    /// Sólo `I-12` e `I-13` son <b>absolutos</b>. El resto es por expediente, y eso es lo que
    /// permite que el Encargado de Delegación exista: su acumulación se vigila, no se prohíbe.
    /// </summary>
    [Fact]
    public void Solo_I12_e_I13_son_absolutos()
    {
        var absolutos = Incompatibilidades.Tabla
            .Where(p => p.Alcance == AlcanceDelPar.Absoluto)
            .Select(p => p.Id)
            .Distinct()
            .Order()
            .ToList();

        Assert.Equal(["I-12", "I-13"], absolutos);
    }

    /// <summary>
    /// `I-19` <b>no es una copia de `I-01`</b>.
    ///
    /// Existe porque el par *solicita fondo × aprueba fondo* se caía entre `RN-01` —que razona
    /// por Orden de Misión— y el fondo, que es objeto **de período**. Si las dos funciones no
    /// fueran propias, este par sería literalmente `I-01` y el hueco del hallazgo `HB3-06`
    /// seguiría abierto con una fila que aparenta cubrirlo.
    /// </summary>
    [Fact]
    public void I19_tiene_funciones_propias_y_no_repite_a_I01()
    {
        var i01 = Incompatibilidades.Tabla.Single(p => p.Id == "I-01");
        var i19 = Incompatibilidades.Tabla.Single(p => p.Id == "I-19");

        Assert.NotEqual((i01.Una, i01.Otra), (i19.Una, i19.Otra));
        Assert.Equal(Funcion.SolicitaFondo, i19.Una);
        Assert.Equal(Funcion.ApruebaFondo, i19.Otra);
    }

    // ── El puente rol→función ───────────────────────────────────────────────

    /// <summary>
    /// <b>`ACT-04` NO autoriza.</b> Su ficha lo pone como límite expreso: *«no autoriza la
    /// necesidad (`ACT-03`), no despacha físicamente (`ACT-05`), no entrega el fondo
    /// (`ACT-07`), no cierra el expediente (`ACT-08`)»*.
    ///
    /// Si tuviera `Autoriza`, el solo hecho de ser Jefe de Transporte activaría `I-07`
    /// —núcleo irreductible— contra sí mismo, y el rol quedaría inoperable.
    /// </summary>
    [Fact]
    public void El_jefe_de_transporte_no_autoriza_ni_despacha_ni_entrega_fondo()
    {
        var suyas = Incompatibilidades.Funciones(Rol.JefeDeTransporte);

        Assert.DoesNotContain(Funcion.Autoriza, suyas);
        Assert.DoesNotContain(Funcion.Despacha, suyas);
        Assert.DoesNotContain(Funcion.EntregaFondo, suyas);

        // Lo que sí hace, según la misma ficha.
        Assert.Contains(Funcion.SolicitaFondo, suyas);
        Assert.Contains(Funcion.Liquida, suyas);
        Assert.Contains(Funcion.HabilitaLicencia, suyas);
    }

    /// <summary>
    /// <b>El custodio no conduce.</b> Responde patrimonialmente por el bien, que es otra cosa.
    /// Confundirlos activaría `I-11` —núcleo irreductible— sobre alguien que nunca se sube al
    /// vehículo, y dejaría sin liquidar a cualquier jefe que además custodia una unidad.
    /// </summary>
    [Fact]
    public void El_custodio_no_conduce()
    {
        Assert.DoesNotContain(Funcion.Conduce, Incompatibilidades.Funciones(Rol.CustodioDelVehiculo));
        Assert.Contains(Funcion.Custodia, Incompatibilidades.Funciones(Rol.CustodioDelVehiculo));
    }

    /// <summary>
    /// <b>El auditor no produce ningún acto de negocio.</b> *«Sólo lectura y exportación. Sin
    /// excepciones y sin régimen de excepción que lo levante»*.
    /// </summary>
    [Fact]
    public void El_auditor_solo_audita()
    {
        Assert.Equal([Funcion.Audita], Incompatibilidades.Funciones(Rol.AuditorInterno));
    }

    /// <summary>
    /// El Encargado de Delegación <b>acumula por diseño</b>: *«todo lo que en sede producen
    /// `ACT-03`, `ACT-04`, `ACT-05` y `ACT-07`»*. Es el caso de §5.4, y el sistema tiene que
    /// verlo en vez de disimularlo.
    /// </summary>
    [Fact]
    public void El_encargado_de_delegacion_acumula_las_cinco_funciones()
    {
        var suyas = Incompatibilidades.Funciones(Rol.EncargadoDeDelegacion);

        foreach (var f in new[]
                 { Funcion.Solicita, Funcion.Autoriza, Funcion.Despacha, Funcion.EntregaFondo, Funcion.Liquida })
        {
            Assert.Contains(f, suyas);
        }
    }

    /// <summary>
    /// Todos los roles tienen entrada en el puente, aunque sea vacía.
    ///
    /// <b>Un rol sin entrada no falla: pasa desapercibido</b>, porque no activa ningún par y
    /// el sistema lo trata como inofensivo. Es exactamente cómo se abre un hueco de
    /// segregación sin que ninguna prueba de la tabla lo note.
    /// </summary>
    [Fact]
    public void Ningun_rol_queda_fuera_del_puente()
    {
        foreach (var rol in Enum.GetValues<Rol>())
        {
            Assert.True(
                Incompatibilidades.FuncionesDe.ContainsKey(rol),
                $"El rol {rol} no tiene funciones declaradas. Sin entrada no activa ningún par, " +
                "y el sistema lo trata como inofensivo.");
        }
    }

}
