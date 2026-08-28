using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.Organizacion;
using Sigti.Pruebas.M07_ProgramacionYDespacho;

namespace Sigti.Pruebas.M09_Combustible;

/// <summary>
/// El acoplamiento entre el vale y la Orden de Misión — §10.1, «Reglas de acoplamiento».
///
/// ── Las cuatro reglas que se ejercen acá ─────────────────────────────────────
/// | Regla | Qué impide |
/// |---|---|
/// | `T-15` con consumo | Anular sería <b>borrar un hecho económico</b> |
/// | `T-15` con vales sin devolver | Cerrar el expediente con dinero en la calle |
/// | `T-19` (`INV-34`) | Declarar cuadrado un viaje cuyo dinero nadie cuadró |
/// | `T-21`/`T-22` | Cerrar sin la única comprobación cruzada que el sistema tiene |
/// </summary>
public class CombustibleEnLaMisionPruebas
{
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 16, 9, 0, 0, TimeSpan.FromHours(-6));

    private static readonly IdPersona Asistente = new("P-ASISTENTE");
    private static readonly IdPersona Jefatura = new("P-JEFATURA");
    private static readonly IdPersona Transporte = new("P-TRANSPORTE");
    private static readonly IdPersona Gerencia = new("P-GERENCIA");
    private static readonly IdPersona Despacho = new("P-DESPACHO");

    private static readonly DatosDeLaSolicitud Solicitud = new(
        Dependencia: "Delegación de Choluteca",
        ObjetoDelTraslado: "Traslado de personal y equipo",
        Destino: "Choluteca",
        Ventana: Asignacion.Ventana);

    /// <summary>Un expediente hasta `DESPACHADA` — el vehículo con las llaves, sin salir.</summary>
    private static OrdenDeMision Despachada()
    {
        var e = OrdenDeMision.Crear(Ulid.NewUlid(), Asistente, Asistente, Solicitud, Momento);
        e.Enviar(Asistente, Momento);
        e.Aprobar(Jefatura, Momento, motivo: null);
        e.Programar(Transporte, Asignacion.Valida(),
            Asignacion.Matriz, PoliticaDeDocumentacion.PorDefecto, Momento);
        e.Despachar(Despacho, Asignacion.Valida(), Asignacion.Matriz,
            PoliticaDeDocumentacion.PorDefecto, Momento, Asignacion.Custodiado,
            Asignacion.SinDiasInhabiles(),
            // `INV-17`: el acta de entrega lleva odómetro, y es contra ésta que `T-15` y `T-16`
            // prueban que el vehículo nunca salió.
            odometroDeEntrega: 10_000);
        return e;
    }

    private static OrdenDeMision Retornada()
    {
        var e = Despachada();
        e.IniciarRuta(new IdPersona("P-MOTORISTA"), Momento, Asignacion.Sale);
        e.Retornar(new IdPersona("P-MOTORISTA"), Momento, Asignacion.Vuelve);
        return e;
    }

    // ── T-15 · anular con devolución íntegra ────────────────────────────────

    [Fact]
    public void T15_anula_la_mision_despachada_cuando_todo_volvio_integro()
    {
        var e = Despachada();

        e.AnularDespachada(Transporte, Momento,
            "Suspendida por orden de la máxima autoridad. Acta 2026-051.",
            odometroDeRetorno: 10_000, toleranciaKm: 5,
            combustible: new RecuentoDeAsignaciones(
                Total: 2, SinLiquidar: 0, SinConciliar: 0, ConConsumo: 0, EntregadasSinDevolver: 0));

        Assert.Equal(EstadoDeMision.Anulada, e.Estado);
        Assert.Contains("devueltas o anuladas íntegras", e.Diario[^1].Motivo);
    }

    [Fact]
    public void Con_UN_SOLO_consumo_T15_deja_de_existir_y_el_mensaje_manda_a_T16()
    {
        // §10.1: «si hubo cualquier consumo, aunque sea parcial». Es la bifurcación completa
        // del circuito, y el mensaje tiene que decir por dónde se sigue — quedarse en «no se
        // puede» deja a quien despacha con un expediente atorado y sin salida.
        var e = Despachada();

        var fallo = Assert.Throws<BloqueoDuro>(() => e.AnularDespachada(
            Transporte, Momento, "Suspendida.", 10_000, 5,
            new RecuentoDeAsignaciones(1, 1, 1, ConConsumo: 1, EntregadasSinDevolver: 0)));

        Assert.Contains("borrar un hecho económico", fallo.Message);
        Assert.Contains("`T-16`", fallo.Message);
        // Y sigue DESPACHADA: el bloqueo no dejó el expediente a medio camino.
        Assert.Equal(EstadoDeMision.Despachada, e.Estado);
    }

    [Fact]
    public void Con_vales_entregados_sin_devolver_la_mision_SIGUE_despachada()
    {
        // La autoridad decidió no crear un estado «anulación en trámite»: «el control real es
        // la lista de devoluciones pendientes, no un nombre de estado». Que la misión se quede
        // donde está ES el mecanismo, no un efecto secundario.
        var e = Despachada();

        var fallo = Assert.Throws<BloqueoDuro>(() => e.AnularDespachada(
            Transporte, Momento, "Suspendida.", 10_000, 5,
            new RecuentoDeAsignaciones(2, 2, 2, 0, EntregadasSinDevolver: 2)));

        Assert.Contains("Faltan 2 devolución(es)", fallo.Message);
        Assert.Equal(EstadoDeMision.Despachada, e.Estado);
    }

    [Fact]
    public void Si_el_vehiculo_SALIO_no_hay_anulacion_sino_mision_ejecutada()
    {
        // El odómetro es lo que distingue «nunca salió» de «salió y volvió». Sin esta
        // comprobación, `T-15` sería la puerta para hacer desaparecer un viaje que ocurrió.
        var e = Despachada();

        var fallo = Assert.Throws<BloqueoDuro>(() => e.AnularDespachada(
            Transporte, Momento, "Suspendida.",
            odometroDeRetorno: 10_420, toleranciaKm: 5,
            combustible: RecuentoDeAsignaciones.Ninguna));

        Assert.Contains("420 km desde la entrega", fallo.Message);
        Assert.Contains("`T-18`", fallo.Message);
    }

    [Fact]
    public void La_tolerancia_cubre_el_movimiento_dentro_del_predio()
    {
        // Mover el vehículo del predio al surtidor suma kilómetros reales. Sin tolerancia, la
        // regla bloquearía la operación normal.
        var e = Despachada();

        e.AnularDespachada(Transporte, Momento, "Suspendida.", 10_003, 5,
            RecuentoDeAsignaciones.Ninguna);

        Assert.Equal(EstadoDeMision.Anulada, e.Estado);
    }

    [Fact]
    public void Sin_recuento_de_combustible_T15_procede_y_el_diario_lo_DICE()
    {
        // Hay expedientes anteriores a `M-09`. Lo que no se puede hacer se declara: dentro de
        // dos años nadie puede confundir «se verificó y estaba devuelto» con «no se consultó».
        var e = Despachada();

        e.AnularDespachada(Transporte, Momento, "Suspendida.", 10_000, 5, combustible: null);

        Assert.Contains("NO verificada", e.Diario[^1].Motivo);
    }

    [Fact]
    public void T15_exige_motivo()
    {
        var e = Despachada();

        Assert.Throws<BloqueoDuro>(() => e.AnularDespachada(
            Transporte, Momento, "   ", 10_000, 5, RecuentoDeAsignaciones.Ninguna));
    }

    // ── T-16 · misión no ejecutada con consumo ──────────────────────────────

    [Fact]
    public void T16_retorna_la_mision_sin_ejecutar_y_la_marca_para_que_no_contamine()
    {
        // Una misión de cero kilómetros con treinta galones consumidos destruiría el promedio
        // de la flota y haría que `RN-30` señalara al vehículo equivocado.
        var e = Despachada();

        e.RegistrarNoEjecutadaConConsumo(Transporte, Momento,
            "El motorista llenó el tanque la tarde anterior y la misión se suspendió esa noche.",
            odometroDeRetorno: 10_000, toleranciaKm: 5,
            combustible: new RecuentoDeAsignaciones(1, 1, 1, ConConsumo: 1, EntregadasSinDevolver: 1));

        Assert.Equal(EstadoDeMision.Retornada, e.Estado);
        Assert.Contains("NO EJECUTADA", e.Diario[^1].Motivo);
        Assert.Contains("no computa para indicadores", e.Diario[^1].Motivo);
    }

    [Fact]
    public void T16_admite_tambien_lo_entregado_que_no_es_devolvible()
    {
        // La autoridad admite los dos casos en la misma frase: «hubo consumo O parte de lo
        // entregado no es devolvible». Exigir consumo dejaría sin salida al segundo, que es un
        // hecho económico igual de real y tampoco cabe en `T-15`.
        var e = Despachada();

        e.RegistrarNoEjecutadaConConsumo(Transporte, Momento,
            "Orden de pago ya endosada al proveedor, no reversible.",
            10_000, 5,
            new RecuentoDeAsignaciones(1, 1, 1, ConConsumo: 0, EntregadasSinDevolver: 1));

        Assert.Equal(EstadoDeMision.Retornada, e.Estado);
    }

    [Fact]
    public void T16_tampoco_admite_un_vehiculo_que_salio()
    {
        var e = Despachada();

        Assert.Throws<BloqueoDuro>(() => e.RegistrarNoEjecutadaConConsumo(
            Transporte, Momento, "Suspendida.", 10_900, 5, RecuentoDeAsignaciones.Ninguna));
    }

    // ── T-19 · INV-34 ───────────────────────────────────────────────────────

    [Fact]
    public void La_mision_NO_se_liquida_con_vales_vivos()
    {
        var e = Retornada();

        var fallo = Assert.Throws<BloqueoDuro>(() => e.Liquidar(
            Transporte, Momento,
            new RecuentoDeAsignaciones(3, SinLiquidar: 2, SinConciliar: 3, ConConsumo: 3, EntregadasSinDevolver: 0)));

        Assert.Equal("INV-34", fallo.Precondicion);
        Assert.Contains("2 asignación(es)", fallo.Message);
        Assert.Equal(EstadoDeMision.Retornada, e.Estado);
    }

    [Fact]
    public void Con_todo_liquidado_la_mision_liquida()
    {
        var e = Retornada();

        e.Liquidar(Transporte, Momento,
            new RecuentoDeAsignaciones(3, 0, SinConciliar: 3, ConConsumo: 3, EntregadasSinDevolver: 0));

        Assert.Equal(EstadoDeMision.Liquidada, e.Estado);
        Assert.Contains("3 asignación(es) de combustible, todas liquidadas", e.Diario[^1].Motivo);
    }

    [Fact]
    public void Una_mision_sin_combustible_asignado_liquida_y_el_diario_lo_distingue()
    {
        // Cero es un dato: el vehículo salió con el tanque lleno. Que el diario lo diga es lo
        // que impide leer un renglón en blanco como «no se revisó».
        var e = Retornada();

        e.Liquidar(Transporte, Momento, RecuentoDeAsignaciones.Ninguna);

        Assert.Contains("sin combustible asignado", e.Diario[^1].Motivo);
    }

    [Fact]
    public void Sin_recuento_la_liquidacion_procede_y_el_diario_dice_que_no_se_evaluo()
    {
        var e = Retornada();

        e.Liquidar(Transporte, Momento, combustible: null);

        Assert.Contains("INV-34 NO evaluada", e.Diario[^1].Motivo);
    }

    // ── T-21 y T-22 · conciliación ──────────────────────────────────────────

    [Fact]
    public void La_mision_NO_cierra_con_vales_sin_conciliar()
    {
        var e = Retornada();
        e.Liquidar(Transporte, Momento, new RecuentoDeAsignaciones(2, 0, 2, 2, 0));

        var fallo = Assert.Throws<BloqueoDuro>(() => e.Cerrar(
            Gerencia, Momento, criterios: [], justificacion: null,
            combustible: new RecuentoDeAsignaciones(2, 0, SinConciliar: 2, ConConsumo: 2, EntregadasSinDevolver: 0)));

        Assert.Contains("sin conciliar", fallo.Message);
        Assert.Equal(EstadoDeMision.Liquidada, e.Estado);
    }

    [Fact]
    public void Una_desviacion_ya_conciliada_NO_impide_cerrar()
    {
        // `CONCILIADA_CON_DESVIACION` cuenta como conciliada: §10.1 dice «en cualquiera de las
        // dos formas». Lo que bloquea es el vale que nadie contrastó, no el que se contrastó y
        // salió mal — ése es un cierre con hallazgo, que es un cierre.
        var e = Retornada();
        e.Liquidar(Transporte, Momento, new RecuentoDeAsignaciones(2, 0, 2, 2, 0));

        e.Cerrar(Gerencia, Momento,
            criterios: [new HallazgoDetectado("H-01", "Rendimiento 9 km/gal contra 17 esperado")],
            justificacion: "Ruta de montaña. Causa tipificada por el conciliador.",
            combustible: new RecuentoDeAsignaciones(2, 0, SinConciliar: 0, ConConsumo: 2, EntregadasSinDevolver: 0));

        Assert.Equal(EstadoDeMision.CerradaConHallazgo, e.Estado);
    }
}
