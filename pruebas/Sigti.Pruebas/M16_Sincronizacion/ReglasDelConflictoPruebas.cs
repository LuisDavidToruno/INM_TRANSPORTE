using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M16_Sincronizacion;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M16_Sincronizacion;

/// <summary>
/// `RN-45` y mapa §7.1 — la cola de conflictos.
///
/// <i>«Es la pantalla más difícil del sistema y la que nadie diseña hasta que ya duele.»</i>
/// </summary>
public class ReglasDelConflictoPruebas
{
    private static readonly DateTimeOffset Mayo16 = new(2026, 5, 16, 17, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Mayo28 = new(2026, 5, 28, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Ahora = new(2026, 5, 30, 8, 0, 0, TimeSpan.Zero);

    private static ConflictoDeSincronizacion Conflicto(
        string campo = "odometroRetorno",
        string delServidor = "93061",
        string deCampo = "93610",
        DateTimeOffset? registradoDeCampo = null,
        EstadoDelConflicto estado = EstadoDelConflicto.Pendiente,
        ResolucionDelConflicto? resolucion = null) =>
        new(Ulid.NewUlid(), Ulid.NewUlid(), "T-18", campo,

            // La delegación digitó del papel doce días después, con foto del original.
            new VersionEnConflicto(delServidor, new IdPersona("P-CHOLUTECA"),
                Mayo16, Mayo28, "PC-DELEGACION", Ulid.NewUlid()),

            // El motorista anotó en el momento, sin señal, con foto del tablero.
            new VersionEnConflicto(deCampo, new IdPersona("motorista-1"),
                Mayo16, registradoDeCampo ?? Mayo28, "TAB-01", Ulid.NewUlid()),

            estado, resolucion);

    // ── El caso de los 549 kilómetros ───────────────────────────────────────

    [Fact]
    public void El_odometro_es_de_alto_impacto()
    {
        // «Los dos son de buena fe. Uno de los dos está mal, y la diferencia son 549 kilómetros
        // que van a entrar en una conciliación de combustible.»
        Assert.Equal(ImpactoDelConflicto.Alto, Conflicto().Impacto);
    }

    [Theory]
    [InlineData("odometro")]
    [InlineData("odometroSalida")]
    [InlineData("monto")]
    [InlineData("galones")]
    [InlineData("autorizacion")]
    public void Los_campos_que_entran_en_una_conciliacion_son_de_alto_impacto(string campo)
    {
        Assert.Equal(ImpactoDelConflicto.Alto, ReglasDelConflicto.ImpactoDe(campo));
    }

    [Theory]
    [InlineData("horaDeArribo")]
    [InlineData("observaciones")]
    [InlineData("ubicacion")]
    public void Lo_demas_es_impacto_normal(string campo)
    {
        Assert.Equal(ImpactoDelConflicto.Normal, ReglasDelConflicto.ImpactoDe(campo));
    }

    [Fact]
    public void Las_dos_versiones_conservan_quien_cuando_paso_y_cuando_se_registro()
    {
        // §7.1 punto 2: son **tres datos distintos**, y la distinción entre los dos últimos es
        // «exactamente lo que permite decidir». Una versión anotada en el momento pesa distinto
        // que una digitada del papel doce días después.
        var c = Conflicto();

        Assert.Equal(Mayo16, c.DeCampo.OcurrioEl);
        Assert.Equal(Mayo28, c.DelServidor.RegistradoEl);
        Assert.NotEqual(c.DelServidor.CapturadaPor, c.DeCampo.CapturadaPor);

        // Y las dos fotos existen: la del tablero contra la del original es lo que en la
        // práctica resuelve el conflicto.
        Assert.NotNull(c.DelServidor.Foto);
        Assert.NotNull(c.DeCampo.Foto);
    }

    // ── El orden de la cola ─────────────────────────────────────────────────

    [Fact]
    public void La_cola_ordena_por_impacto_y_despues_por_antiguedad()
    {
        // §7.1 punto 10. La antigüedad sola pondría una observación de hace un mes por encima
        // de un odómetro de ayer que está frenando una liquidación.
        var viejoNormal = Conflicto("observaciones", registradoDeCampo: Mayo16);
        var nuevoAlto = Conflicto("odometroRetorno", registradoDeCampo: Mayo28);

        var orden = ReglasDelConflicto.Ordenar([viejoNormal, nuevoAlto]);

        Assert.Equal(nuevoAlto.Campo, orden[0].Campo);
    }

    [Fact]
    public void Entre_dos_del_mismo_impacto_manda_el_mas_viejo()
    {
        var viejo = Conflicto("observaciones", registradoDeCampo: Mayo16);
        var nuevo = Conflicto("ubicacion", registradoDeCampo: Mayo28);

        var orden = ReglasDelConflicto.Ordenar([nuevo, viejo]);

        Assert.Equal("observaciones", orden[0].Campo);
    }

    [Fact]
    public void Los_dias_esperando_no_son_negativos()
    {
        // Un conflicto registrado con el reloj adelantado daría días negativos, y la cola
        // ordenada por antigüedad lo pondría primero — el menos urgente arriba de todo.
        var futuro = Conflicto(registradoDeCampo: Ahora.AddDays(3));

        Assert.Equal(0, futuro.DiasEsperando(Ahora));
    }

    // ── El lote, y lo que nunca entra en él ─────────────────────────────────

    [Fact]
    public void El_lote_excluye_siempre_el_odometro_el_monto_y_la_autorizacion()
    {
        // §7.1 punto 9, literal. Resolver en bloque los tres campos que entran en una
        // conciliación contable es sobrescritura silenciosa con un paso de más.
        var reparto = ReglasDelConflicto.Repartir([
            Conflicto("observaciones"),
            Conflicto("odometroRetorno"),
            Conflicto("monto"),
            Conflicto("horaDeArribo"),
        ]);

        Assert.Equal(2, reparto.EnElLote.Count);
        Assert.Equal(2, reparto.FueraDelLote.Count);
        Assert.All(reparto.EnElLote, c => Assert.Equal(ImpactoDelConflicto.Normal, c.Impacto));
    }

    [Fact]
    public void Lo_que_queda_fuera_del_lote_viene_enumerado_y_ordenado()
    {
        // «3 conflictos de alto impacto quedan fuera del lote y se resuelven uno por uno.» Un
        // lote que dice haber resuelto «todo» sin mencionar las exclusiones hace creer que la
        // cola quedó vacía — y los que frenan liquidaciones siguen ahí sin que nadie los mire.
        var reparto = ReglasDelConflicto.Repartir([
            Conflicto("monto", registradoDeCampo: Mayo28),
            Conflicto("odometro", registradoDeCampo: Mayo16),
        ]);

        Assert.Empty(reparto.EnElLote);
        Assert.Equal(2, reparto.FueraDelLote.Count);
        Assert.Equal("odometro", reparto.FueraDelLote[0].Campo);
    }

    [Fact]
    public void El_lote_no_toca_lo_ya_resuelto()
    {
        var resuelto = Conflicto("observaciones", estado: EstadoDelConflicto.Resuelto,
            resolucion: new ResolucionDelConflicto(
                OrigenElegido.Campo, "la foto del tablero es legible",
                new IdPersona("P-TRANSPORTE"), Ahora, null));

        var reparto = ReglasDelConflicto.Repartir([resuelto, Conflicto("ubicacion")]);

        Assert.Single(reparto.EnElLote);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("porque")]
    public void El_lote_exige_declarar_su_criterio(string? criterio)
    {
        // «Hacerlo sin declarar el criterio es sobrescritura con más pasos.»
        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelConflicto.ExigirCriterioDelLote(criterio));

        Assert.Contains("criterio", e.Message);
    }

    [Fact]
    public void Un_criterio_declarado_pasa()
    {
        ReglasDelConflicto.ExigirCriterioDelLote(
            "aceptar la versión de campo para todos los registros de esta misión");
    }

    // ── La resolución ───────────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ok")]
    public void Resolver_exige_motivo_escrito(string? motivo)
    {
        var e = Assert.Throws<BloqueoDuro>(() => ReglasDelConflicto.ExigirMotivo(motivo));

        // El texto es el que la pantalla muestra, y dice para quién es: «la decisión queda en
        // el expediente y el auditor la va a leer».
        Assert.Contains("auditor", e.Message);
    }

    [Fact]
    public void Un_conflicto_ya_resuelto_no_se_vuelve_a_decidir()
    {
        // La segunda decisión pisaría a la primera sin dejar rastro — que es exactamente la
        // sobrescritura silenciosa que `RN-45` prohíbe, cometida desde la propia cola.
        var resuelto = Conflicto(estado: EstadoDelConflicto.Resuelto,
            resolucion: new ResolucionDelConflicto(
                OrigenElegido.Campo, "la foto del tablero es legible",
                new IdPersona("P-TRANSPORTE"), Ahora, null));

        var e = Assert.Throws<BloqueoDuro>(() => ReglasDelConflicto.ExigirPendiente(resuelto));

        Assert.Contains("asiento nuevo", e.Message);
    }

    [Fact]
    public void Un_conflicto_pendiente_se_puede_resolver()
    {
        ReglasDelConflicto.ExigirPendiente(Conflicto());
        ReglasDelConflicto.ExigirMotivo("la foto del tablero se lee sin dudas");
    }

    // ── Lo que la pantalla contesta ─────────────────────────────────────────

    [Fact]
    public void Hay_una_respuesta_para_quien_busca_el_boton_de_editar()
    {
        // §7.1 punto 5: **va a buscarlo**. `R-6` dice que ninguna pantalla edita un hecho
        // pasado, y es lo que hace difícil esta pantalla: no se le puede dar la salida fácil.
        Assert.Contains("No se edita", ReglasDelConflicto.PorQueNoSeEdita);
        Assert.Contains("asiento nuevo", ReglasDelConflicto.PorQueNoSeEdita);
    }

    [Fact]
    public void Y_otra_para_quien_quiere_combinar_las_dos()
    {
        // «Combinar sólo produciría un registro que nadie capturó.»
        Assert.Contains("nadie capturó", ReglasDelConflicto.PorQueNoSeCombina);
    }

    [Fact]
    public void Ningun_texto_de_la_cola_usa_lenguaje_de_datos()
    {
        // §7.1 punto 1 es un criterio de aceptación **literal y verificable** de `HU-068`:
        // «Nunca dice merge, timestamp, versión, hash divergente ni conflicto de escritura».
        // Quien la usa es el Jefe de Transporte, que «no entiende de sincronización y no tiene
        // por qué».
        string[] prohibidas =
            ["merge", "timestamp", "hash", "conflicto de escritura", "sobrescr"];

        var textos = new[]
        {
            ReglasDelConflicto.PorQueNoSeEdita,
            ReglasDelConflicto.PorQueNoSeCombina,
        };

        foreach (var texto in textos)
            foreach (var palabra in prohibidas)
                Assert.DoesNotContain(palabra, texto, StringComparison.OrdinalIgnoreCase);
    }
}
