using Sigti.Dominio.M13_Cierre;

namespace Sigti.Pruebas.M13_Cierre;

/// <summary>
/// `RN-08` — la cadena de trazabilidad para cerrar.
///
/// ── Lo que el auditor pide ──────────────────────────────────────────────────
/// `NRM-01` exige vincular en cadena trazable cada eslabón <i>«con su documento y su
/// firmante»</i>. El auditor del TSC no pide comprobantes sueltos: pide <b>poder recorrer la
/// cadena de una punta a la otra</b> sobre un expediente concreto.
///
/// ── ⚠️ Y las dos distinciones que hacen que esto sirva ──────────────────────
/// <b>«No aplicable» no es «presente»</b> — `RN-08`: <i>«lo que no se admite es cerrarlo como
/// presente con consumo cero»</i>. Y <b>«en camino» no es «ausente»</b>: marcar de hallazgo un
/// expediente cuya bitácora viaja en el teléfono de un motorista acusa de una omisión que no
/// ocurrió, y el cierre es inmutable.
///
/// ── Nivel ───────────────────────────────────────────────────────────────────
/// `RN-08` es <b>`[I]`</b>: la cadena de eslabones es implicación de requerimiento del equipo
/// sobre `NRM-01`, no articulado citable (`HN1-06`).
/// </summary>
public class ReglasDeLaCadenaPruebas
{
    // ── La cadena completa ──────────────────────────────────────────────────

    [Fact]
    public void Con_todos_los_eslabones_la_cadena_esta_completa()
    {
        var cadena = ReglasDeLaCadena.Evaluar(Completa());

        Assert.True(cadena.Completa);
        Assert.Empty(cadena.Faltantes);
    }

    /// <summary>Los ocho de `RN-08` aparecen, y <b>en el orden de la regla</b>.</summary>
    [Fact]
    public void Los_ocho_eslabones_aparecen_en_el_orden_de_la_regla()
    {
        var cadena = ReglasDeLaCadena.Evaluar(Completa());

        Assert.Equal(
            new[]
            {
                EslabonDeLaCadena.Solicitud,
                EslabonDeLaCadena.Autorizacion,
                EslabonDeLaCadena.OrdenDeMision,
                EslabonDeLaCadena.AsignacionDeVehiculoYMotorista,
                EslabonDeLaCadena.BitacoraConOdometros,
                EslabonDeLaCadena.Combustible,
                EslabonDeLaCadena.Peajes,
                EslabonDeLaCadena.Liquidacion,
            },
            cadena.Eslabones.Select(e => e.Eslabon));
    }

    // ── Los eslabones que faltan ────────────────────────────────────────────

    /// <summary>
    /// ⚠️ <b>Ejecutada sin autorización previa: ausente y no subsanable.</b> `RN-08` lo dice
    /// literal — no se fabrica autorización retroactiva, y el mensaje tiene que decirlo o alguien
    /// va a salir a buscar el papel.
    /// </summary>
    [Fact]
    public void Sin_autorizacion_el_eslabon_falta_y_se_dice_que_no_es_subsanable()
    {
        var cadena = ReglasDeLaCadena.Evaluar(Completa() with { Autorizada = false });

        var eslabon = De(cadena, EslabonDeLaCadena.Autorizacion);

        Assert.Equal(EstadoDelEslabon.Ausente, eslabon.Estado);
        Assert.Contains("no se fabrica autorización retroactiva", eslabon.Detalle);
        Assert.False(cadena.Completa);
    }

    /// <summary>
    /// La bitácora exige <b>las dos lecturas</b>: una sola no produce kilometraje, y el
    /// kilometraje es el ancla de toda la conciliación.
    /// </summary>
    [Theory]
    [InlineData(null, 10_450, "la lectura de salida")]
    [InlineData(10_000, null, "la lectura de retorno")]
    [InlineData(null, null, "las dos lecturas")]
    public void Con_un_odometro_a_medias_falta_la_bitacora(int? salida, int? retorno, string dice)
    {
        var cadena = ReglasDeLaCadena.Evaluar(
            Completa() with { OdometroDeSalida = salida, OdometroDeRetorno = retorno });

        var eslabon = De(cadena, EslabonDeLaCadena.BitacoraConOdometros);

        Assert.Equal(EstadoDelEslabon.Ausente, eslabon.Estado);
        Assert.Contains(dice, eslabon.Detalle);
    }

    /// <summary>
    /// ⚠️ <b>El folio provisional NO es eslabón ausente</b> — y esto se corrigió al ver la
    /// prueba de punta a punta ponerse roja.
    ///
    /// Hoy ninguna delegación tiene rango de folios asignado —configuración pendiente, insumo
    /// #34— y el sistema decidió a propósito que eso no bloquee. Marcarlo faltante habría puesto
    /// un hallazgo en <b>todos</b> los expedientes por una tabla que nadie cargó, y un control
    /// que produce hallazgos falsos en masa muere en tres meses.
    ///
    /// Lo que sí hace es <b>decirlo</b>: el documento existe, su numeración oficial espera esa
    /// configuración, y quien lea el expediente lo sabe.
    /// </summary>
    [Fact]
    public void El_folio_provisional_no_rompe_la_cadena_pero_se_declara()
    {
        var cadena = ReglasDeLaCadena.Evaluar(
            Completa() with { Folio = "PROV-78BMFT", FolioOficial = false });

        var eslabon = De(cadena, EslabonDeLaCadena.OrdenDeMision);

        Assert.Equal(EstadoDelEslabon.Presente, eslabon.Estado);
        Assert.Contains("provisional", eslabon.Detalle);
        Assert.Contains("rango de folios", eslabon.Detalle);
        Assert.True(cadena.Completa);
    }

    // ── ⚠️ «No aplicable» no es «presente» ──────────────────────────────────

    /// <summary>
    /// `RN-08` nombra el caso: <i>«misión de cortesía sin combustible ni peaje… el eslabón se
    /// marca no aplicable <b>con fundamento</b>; lo que no se admite es cerrarlo como presente
    /// con consumo cero»</i>.
    /// </summary>
    [Fact]
    public void Sin_vales_el_combustible_no_aplica_y_no_se_da_por_cumplido()
    {
        var cadena = ReglasDeLaCadena.Evaluar(Completa() with { ValesDeLaMision = 0 });

        var eslabon = De(cadena, EslabonDeLaCadena.Combustible);

        Assert.Equal(EstadoDelEslabon.NoAplicable, eslabon.Estado);
        Assert.NotEqual(EstadoDelEslabon.Presente, eslabon.Estado);

        // Con fundamento: sin él es indistinguible de una omisión.
        Assert.Contains("no se da por cumplido", eslabon.Detalle);

        // Y no rompe la cadena — pero sólo porque lleva su fundamento.
        Assert.True(cadena.Completa);
    }

    /// <summary>
    /// El fundamento del «no aplica» de peajes sale de la <b>ruta autorizada</b>, no de que nadie
    /// registrara pasos: deducirlo de la ausencia de pasos haría que una misión que cruzó tres
    /// casetas sin registrar ninguna se declarara sola como ruta sin peajes.
    /// </summary>
    [Fact]
    public void Sin_cruces_autorizados_los_peajes_no_aplican()
    {
        var cadena = ReglasDeLaCadena.Evaluar(
            Completa() with { CrucesAutorizados = 0, PasosRegistrados = 0 });

        Assert.Equal(EstadoDelEslabon.NoAplicable, De(cadena, EslabonDeLaCadena.Peajes).Estado);
        Assert.True(cadena.Completa);
    }

    /// <summary>⚠️ Con cruces autorizados y <b>ningún paso registrado</b>, el eslabón falta.</summary>
    [Fact]
    public void Con_cruces_autorizados_y_sin_pasos_falta_el_eslabon_de_peajes()
    {
        var cadena = ReglasDeLaCadena.Evaluar(
            Completa() with { CrucesAutorizados = 3, PasosRegistrados = 0 });

        var eslabon = De(cadena, EslabonDeLaCadena.Peajes);

        Assert.Equal(EstadoDelEslabon.Ausente, eslabon.Estado);
        Assert.False(cadena.Completa);
    }

    // ── ⚠️ «En camino» no es «ausente» ──────────────────────────────────────

    /// <summary>
    /// <b>La distinción que evita acusar a un inocente.</b> `RN-08`: <i>«no se cierra con
    /// hallazgo por falta de datos que están en camino»</i>. La bitácora de una misión larga
    /// viaja en el teléfono del motorista, y el cierre es inmutable — el hallazgo quedaría para
    /// siempre.
    /// </summary>
    [Fact]
    public void Con_hechos_sin_sincronizar_la_bitacora_esta_en_camino_y_no_ausente()
    {
        var cadena = ReglasDeLaCadena.Evaluar(Completa() with
        {
            OdometroDeSalida = null,
            OdometroDeRetorno = null,
            HechosSinSincronizar = 2,
        });

        var eslabon = De(cadena, EslabonDeLaCadena.BitacoraConOdometros);

        Assert.Equal(EstadoDelEslabon.PendienteDeSincronizacion, eslabon.Estado);

        // Y por eso **no cuenta como faltante**: no hay omisión que reprochar.
        Assert.Empty(cadena.Faltantes);
        Assert.Single(cadena.EnCamino);

        // Pero la cadena tampoco está completa: no se cierra todavía.
        Assert.False(cadena.Completa);
    }

    /// <summary>
    /// Lo que está en camino <b>tiñe los eslabones de campo, no todos</b>. La solicitud y la
    /// autorización nacieron en la oficina: declararlas «en camino» porque un teléfono no
    /// sincronizó mueve el problema a donde no está.
    /// </summary>
    [Fact]
    public void Lo_pendiente_de_sincronizar_no_tine_los_eslabones_de_oficina()
    {
        var cadena = ReglasDeLaCadena.Evaluar(
            Completa() with { Autorizada = false, HechosSinSincronizar = 5 });

        Assert.Equal(EstadoDelEslabon.Ausente, De(cadena, EslabonDeLaCadena.Autorizacion).Estado);
    }

    // ── Andamios ────────────────────────────────────────────────────────────

    /// <summary>Un expediente con la cadena entera y nada en camino.</summary>
    private static HechosDeLaCadena Completa() => new(
        Autorizada: true,
        Folio: "OM-CHO-2026-000031",
        FolioOficial: true,
        ConVehiculoYMotorista: true,
        OdometroDeSalida: 10_000,
        OdometroDeRetorno: 10_450,
        ValesDeLaMision: 1,
        CrucesAutorizados: 2,
        PasosRegistrados: 2,
        Liquidada: true,
        HechosSinSincronizar: 0);

    private static EslabonEvaluado De(CadenaDeTrazabilidad cadena, EslabonDeLaCadena cual) =>
        cadena.Eslabones.Single(e => e.Eslabon == cual);
}
