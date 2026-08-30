using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M07_ProgramacionYDespacho;

/// <summary>
/// `HU-016` — el trámite y la firma del permiso de circulación en día u hora inhábil.
///
/// ── Lo que estas pruebas protegen ───────────────────────────────────────────
/// `BD-04` bloquea el despacho de toda misión que circule en franja inhábil sin permiso de la
/// máxima autoridad. Ese bloqueo estaba escrito, probado y operando, y <b>nadie podía emitir el
/// permiso que lo levanta</b>: cualquier misión que tocara un sábado era indespachable.
///
/// La llave que se le puso al bloqueo tiene que abrir <b>sólo</b> lo que corresponde.
/// </summary>
public class ReglasDelPermisoPruebas
{
    private static readonly Ulid Expediente = Ulid.NewUlid();
    private static readonly Ulid Vehiculo = Ulid.NewUlid();
    private static readonly Ulid Motorista = Ulid.NewUlid();

    private static readonly IdPersona Jefe = new("P-JEFE");
    private static readonly IdPersona Doris = new("P-DORIS");
    private static readonly IdPersona Elsa = new("P-ELSA");

    private static readonly DateOnly Salida = new(2026, 3, 20);
    private static readonly DateOnly Retorno = new(2026, 3, 21);

    // ── Abrir el trámite ────────────────────────────────────────────────────

    [Fact]
    public void Sin_excepcion_ni_permiso_previo_el_tramite_hace_falta()
    {
        Assert.Null(ReglasDelPermiso.PorQueNoHaceFalta(Apertura()));
    }

    /// <summary>
    /// `RN-24` — <b>la excepción es atributo del vehículo, no del viaje.</b>
    ///
    /// Una ambulancia con excepción vigente sale un domingo sin tramitar nada. Si esta regla no
    /// operara, el trámite se abriría igual y quedaría esperando una firma que nadie tiene por
    /// qué dar — y la ambulancia esperando con él.
    /// </summary>
    [Fact]
    public void El_vehiculo_de_servicio_exceptuado_no_tramita_permiso()
    {
        var apertura = Apertura() with
        {
            Excepcion = new ServicioExceptuado(
                "AMBULANCIA", "Acuerdo 44-2019", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)),
        };

        var porQue = ReglasDelPermiso.PorQueNoHaceFalta(apertura);

        Assert.NotNull(porQue);
        Assert.Equal("SERVICIO_EXCEPTUADO", porQue.Motivo);
        Assert.Contains("31/12/2026", porQue.Detalle);
        Assert.Contains("RN-24", porQue.Detalle);
    }

    /// <summary>Una excepción vencida al día de la salida no exime — `P-4`.</summary>
    [Fact]
    public void La_excepcion_vencida_a_la_fecha_de_la_salida_no_exime()
    {
        var apertura = Apertura() with
        {
            Excepcion = new ServicioExceptuado(
                "AMBULANCIA", "Acuerdo 44-2019", new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31)),
        };

        Assert.Null(ReglasDelPermiso.PorQueNoHaceFalta(apertura));
    }

    [Fact]
    public void No_se_abre_un_segundo_tramite_que_cubre_lo_mismo()
    {
        var apertura = Apertura() with { Existentes = [EnTramite()] };

        var porQue = ReglasDelPermiso.PorQueNoHaceFalta(apertura);

        Assert.NotNull(porQue);
        Assert.Equal("YA_EXISTE", porQue.Motivo);
        Assert.Contains("rompen la conciliación", porQue.Detalle);
    }

    /// <summary>
    /// Desistir es decir que ya no se pide. Si un trámite retirado siguiera estorbando, la
    /// misión reprogramada a un domingo distinto no podría volver a tramitar nada.
    /// </summary>
    [Fact]
    public void Un_tramite_desistido_no_estorba_al_siguiente()
    {
        var apertura = Apertura() with
        {
            Existentes = [EnTramite() with { Estado = EstadoDelPermiso.Desistido }],
        };

        Assert.Null(ReglasDelPermiso.PorQueNoHaceFalta(apertura));
    }

    /// <summary>
    /// El permiso tiene que contener la ventana <b>entera</b>. Uno que cubre tres de los cinco
    /// días deja al agente que revise el cuarto con un vehículo del Estado sin respaldo.
    /// </summary>
    [Fact]
    public void Un_permiso_que_cubre_solo_parte_de_la_ventana_no_cuenta_como_existente()
    {
        var apertura = Apertura() with
        {
            Existentes = [EnTramite() with { Hasta = Salida }],   // le falta el retorno
        };

        Assert.Null(ReglasDelPermiso.PorQueNoHaceFalta(apertura));
    }

    // ── Firmar ──────────────────────────────────────────────────────────────

    [Fact]
    public void La_maxima_autoridad_firma_un_tramite_completo()
    {
        var permiso = EnTramite() with { Vehiculo = Vehiculo, Motorista = Motorista };

        Assert.Null(ReglasDelPermiso.PorQueNoSeFirma(permiso, [Rol.MaximaAutoridad]));
    }

    /// <summary>
    /// `RN-23` — el permiso es <b>nominativo</b>. El salvoconducto lo lee un agente que compara
    /// el nombre del papel con quien va al volante: uno sin motorista no sirve para lo único
    /// para lo que existe.
    /// </summary>
    [Fact]
    public void No_se_firma_un_permiso_sin_vehiculo_ni_motorista()
    {
        var motivo = ReglasDelPermiso.PorQueNoSeFirma(EnTramite(), [Rol.MaximaAutoridad]);

        Assert.NotNull(motivo);
        Assert.Contains("nominativo", motivo);
        Assert.Contains("Programe la misión antes de firmar", motivo);
    }

    /// <summary>Falta el motorista solo: el vehículo no alcanza.</summary>
    [Fact]
    public void No_se_firma_con_vehiculo_pero_sin_motorista()
    {
        var permiso = EnTramite() with { Vehiculo = Vehiculo, Motorista = null };

        Assert.NotNull(ReglasDelPermiso.PorQueNoSeFirma(permiso, [Rol.MaximaAutoridad]));
    }

    /// <summary>
    /// `RN-07` <b>no está habilitada para esta facultad</b> — insumo #29.
    ///
    /// La Gerencia Administrativa autoriza solicitudes y no firma esto. Se comprueba el rol
    /// propio y no una delegación: una delegación para autorizar solicitudes no alcanza.
    /// </summary>
    [Fact]
    public void Quien_no_es_la_maxima_autoridad_no_firma_aunque_autorice_solicitudes()
    {
        var permiso = EnTramite() with { Vehiculo = Vehiculo, Motorista = Motorista };

        var motivo = ReglasDelPermiso.PorQueNoSeFirma(
            permiso, [Rol.GerenciaAdministrativa, Rol.JefaturaInmediata]);

        Assert.NotNull(motivo);
        Assert.Contains("indelegable", motivo);
        Assert.Contains("insumo #29", motivo);

        // Y dice qué se puede hacer en su lugar: un bloqueo sin salida deja la misión varada.
        Assert.Contains("reprogramar la ventana", motivo);
    }

    /// <summary>
    /// Sin puesto resuelto <b>no se firma</b>, y el motivo es otro: no es que la persona no
    /// pueda, es que no se pudo comprobar. Confundirlos mandaría a buscar una firma distinta
    /// cuando el problema es el espejo del organigrama.
    /// </summary>
    [Fact]
    public void Sin_roles_resueltos_no_se_firma_y_lo_dice_distinto()
    {
        var permiso = EnTramite() with { Vehiculo = Vehiculo, Motorista = Motorista };

        var motivo = ReglasDelPermiso.PorQueNoSeFirma(permiso, []);

        Assert.NotNull(motivo);
        Assert.Contains("No se pudo resolver el puesto", motivo);
        Assert.DoesNotContain("indelegable", motivo);
    }

    [Fact]
    public void Un_permiso_ya_firmado_no_se_vuelve_a_firmar()
    {
        var permiso = EnTramite() with
        {
            Estado = EstadoDelPermiso.Firmado,
            Vehiculo = Vehiculo,
            Motorista = Motorista,
            FirmadoPor = Doris,
        };

        var motivo = ReglasDelPermiso.PorQueNoSeFirma(permiso, [Rol.MaximaAutoridad]);

        Assert.NotNull(motivo);
        Assert.Contains(Doris.Valor, motivo);
    }

    [Fact]
    public void Un_permiso_desistido_no_se_firma()
    {
        var permiso = EnTramite() with
        {
            Estado = EstadoDelPermiso.Desistido,
            Vehiculo = Vehiculo,
            Motorista = Motorista,
        };

        var motivo = ReglasDelPermiso.PorQueNoSeFirma(permiso, [Rol.MaximaAutoridad]);

        Assert.NotNull(motivo);
        Assert.Contains("desistido", motivo);
    }

    /// <summary>
    /// El orden de los rechazos: <b>el estado antes que el rol</b>.
    ///
    /// Decirle a la Gerencia «usted no puede firmar» sobre un permiso que ya está firmado la
    /// manda a buscar a la máxima autoridad para un acto que no hace falta.
    /// </summary>
    [Fact]
    public void Sobre_un_permiso_ya_firmado_se_reporta_eso_y_no_el_rol()
    {
        var permiso = EnTramite() with
        {
            Estado = EstadoDelPermiso.Firmado,
            Vehiculo = Vehiculo,
            Motorista = Motorista,
            FirmadoPor = Doris,
        };

        var motivo = ReglasDelPermiso.PorQueNoSeFirma(permiso, [Rol.GerenciaAdministrativa]);

        Assert.NotNull(motivo);
        Assert.Contains("ya está firmado", motivo);
    }

    // ── ¿Todavía cubre? — `HU-018`, `PT-024` ────────────────────────────────

    [Fact]
    public void Un_permiso_firmado_sobre_lo_mismo_sigue_cubriendo()
    {
        Assert.Null(YaNoCubre(Firmado(), Vehiculo, Motorista, "Choluteca", Salida, Retorno));
    }

    /// <summary>
    /// El vehículo entra a taller la víspera y se sustituye. <b>El mensaje nombra los dos
    /// vehículos</b>: sin eso, quien lo lee tiene que ir a comparar cuál es cuál.
    /// </summary>
    [Fact]
    public void Sustituir_el_vehiculo_deja_de_cubrir_y_lo_dice_nombrando_ambos()
    {
        var otro = Ulid.NewUlid();

        var porQue = ReglasDelPermiso.PorQueYaNoCubre(
            Firmado(), otro, Motorista, "Choluteca", Salida, Retorno,
            nombreDeLoAmparado: "INS-P-014", nombreDeLoDeHoy: "INS-P-021", enRuta: false);

        Assert.NotNull(porQue);
        Assert.Contains("INS-P-014", porQue.Detalle);
        Assert.Contains("INS-P-021", porQue.Detalle);
        Assert.Contains("RN-23", porQue.Detalle);
    }

    /// <summary>
    /// <b>La firma anterior no se arrastra.</b> Lo que la máxima autoridad firmó fue otro
    /// motorista, y el salvoconducto lo lee un agente que compara el nombre del papel con quien
    /// va al volante.
    /// </summary>
    [Fact]
    public void Relevar_al_motorista_antes_de_salir_deja_de_cubrir()
    {
        var porQue = YaNoCubre(Firmado(), Vehiculo, Ulid.NewUlid(), "Choluteca", Salida, Retorno);

        Assert.NotNull(porQue);
        Assert.Contains("nominativo sobre el motorista", porQue.Detalle);
        Assert.Contains("La firma anterior no se arrastra", porQue.Detalle);
    }

    [Fact]
    public void Cambiar_el_destino_deja_de_cubrir()
    {
        var porQue = YaNoCubre(Firmado(), Vehiculo, Motorista, "Danlí", Salida, Retorno);

        Assert.NotNull(porQue);
        Assert.Contains("Choluteca", porQue.Detalle);
        Assert.Contains("Danlí", porQue.Detalle);
    }

    /// <summary>La vigencia <b>no se traslada</b>: es lo que el fiscalizador compara primero.</summary>
    [Fact]
    public void Correr_la_ventana_deja_de_cubrir()
    {
        var porQue = YaNoCubre(
            Firmado(), Vehiculo, Motorista, "Choluteca",
            Salida.AddDays(7), Retorno.AddDays(7));

        Assert.NotNull(porQue);
        Assert.Contains("la vigencia no se traslada", porQue.Detalle);
    }

    /// <summary>
    /// Una ventana que <b>se acorta</b> sigue cubierta: el permiso contiene el rango entero. Si
    /// esto diera «ya no cubre», adelantar un retorno obligaría a reemitir un papel que ampara
    /// de sobra.
    /// </summary>
    [Fact]
    public void Acortar_la_ventana_dentro_del_permiso_sigue_cubierto()
    {
        Assert.Null(YaNoCubre(
            Firmado(), Vehiculo, Motorista, "Choluteca", Salida, Retorno.AddDays(-1)));
    }

    /// <summary>
    /// ⚠️ <b>La excepción deliberada de `HU-018`.</b>
    ///
    /// Un relevo documentado en ruta <b>no invalida</b> el permiso de la misión ya iniciada. El
    /// vehículo está en la carretera: declarar el papel inválido no lo devuelve, y sí dejaría al
    /// motorista relevado circulando sin nada. Lo que sí exige permiso reemitido es la
    /// circulación en franja inhábil <b>posterior</b> al traspaso, que es de `M-08`.
    /// </summary>
    [Fact]
    public void Un_relevo_en_ruta_no_invalida_el_permiso_de_la_mision_ya_iniciada()
    {
        var porQue = ReglasDelPermiso.PorQueYaNoCubre(
            Firmado(), Vehiculo, Ulid.NewUlid(), "Choluteca", Salida, Retorno,
            "INS-P-014", "INS-P-014", enRuta: true);

        Assert.Null(porQue);
    }

    /// <summary>
    /// Un trámite sin firmar <b>no puede dejar de cubrir</b>: nunca cubrió nada. Reportarlo
    /// como caduco mandaría a reemitir algo que sólo hay que firmar.
    /// </summary>
    [Fact]
    public void Un_tramite_sin_firmar_no_reporta_que_dejo_de_cubrir()
    {
        Assert.Null(YaNoCubre(EnTramite(), Ulid.NewUlid(), Ulid.NewUlid(), "Danlí", Salida, Retorno));
    }

    /// <summary>
    /// ⚠️ Una misión desprogramada <b>no ordena reemitir</b>, y la diferencia es cara.
    ///
    /// Si se reprograma con el mismo vehículo y el mismo motorista, el permiso sigue amparando.
    /// Reemitirlo quemaría un folio —que no se recicla— y exigiría otra firma de la máxima
    /// autoridad para nada. El mensaje tiene que decir <b>de qué depende</b>, no dar una orden
    /// que puede ser innecesaria.
    /// </summary>
    [Fact]
    public void Sin_vehiculo_asignado_se_dice_de_que_depende_y_no_se_ordena_reemitir()
    {
        var porQue = YaNoCubre(Firmado(), null, null, "Choluteca", Salida, Retorno);

        Assert.NotNull(porQue);
        Assert.Contains("el permiso vuelve a amparar", porQue.Detalle);
        Assert.Contains("si cambia alguno de los dos, reemítalo", porQue.Detalle);

        // ⚠️ **Y no exige reemisión.** Es la mitad que la pantalla usa para decidir si ofrece
        // el botón: sin esto, cada reprogramación ofrecería reemitir, quemando un folio —que
        // no se recicla— y pidiendo otra firma de la máxima autoridad para nada.
        Assert.False(porQue.ExigeReemision);
    }

    /// <summary>
    /// Lo contrario: un elemento que <b>de verdad cambió</b> sí exige reemisión. Falso significa
    /// espere; verdadero significa actúe, y confundirlos deja al motorista saliendo con un papel
    /// que no lo ampara.
    /// </summary>
    [Theory]
    [InlineData("vehículo")]
    [InlineData("motorista")]
    [InlineData("destino")]
    [InlineData("ventana")]
    public void Un_elemento_que_cambio_de_verdad_exige_reemision(string elemento)
    {
        var porQue = elemento switch
        {
            "vehículo" => YaNoCubre(Firmado(), Ulid.NewUlid(), Motorista, "Choluteca", Salida, Retorno),
            "motorista" => YaNoCubre(Firmado(), Vehiculo, Ulid.NewUlid(), "Choluteca", Salida, Retorno),
            "destino" => YaNoCubre(Firmado(), Vehiculo, Motorista, "Danlí", Salida, Retorno),
            _ => YaNoCubre(
                Firmado(), Vehiculo, Motorista, "Choluteca",
                Salida.AddDays(7), Retorno.AddDays(7)),
        };

        Assert.NotNull(porQue);
        Assert.True(porQue.ExigeReemision);
    }

    // ── Reemitir ────────────────────────────────────────────────────────────

    [Fact]
    public void Un_permiso_firmado_con_motivo_se_reemite()
    {
        Assert.Null(ReglasDelPermiso.PorQueNoSeReemite(
            Firmado(), "Sustitución de vehículo INS-P-014 por INS-P-021."));
    }

    [Fact]
    public void No_se_reemite_sin_decir_que_cambio()
    {
        var porQue = ReglasDelPermiso.PorQueNoSeReemite(Firmado(), "  ");

        Assert.NotNull(porQue);
        Assert.Contains("por qué hay dos folios", porQue);
    }

    /// <summary>
    /// Lo que no llegó a amparar nada <b>no se reemplaza, se retira</b>. Reemitir un trámite sin
    /// firmar dejaría dos peticiones vivas para la misma circulación.
    /// </summary>
    [Fact]
    public void Un_tramite_sin_firmar_no_se_reemite_se_desiste()
    {
        var porQue = ReglasDelPermiso.PorQueNoSeReemite(EnTramite(), "Cambió el vehículo.");

        Assert.NotNull(porQue);
        Assert.Contains("se retira", porQue);
    }

    private static PermisoEnTramite Firmado() => EnTramite() with
    {
        Estado = EstadoDelPermiso.Firmado,
        Vehiculo = Vehiculo,
        Motorista = Motorista,
        FirmadoPor = Doris,
    };

    private static NoCubre? YaNoCubre(
        PermisoEnTramite permiso, Ulid? vehiculo, Ulid? motorista,
        string destino, DateOnly desde, DateOnly hasta) =>
        ReglasDelPermiso.PorQueYaNoCubre(
            permiso, vehiculo, motorista, destino, desde, hasta,
            "INS-P-014", "INS-P-021", enRuta: false);

    // ── Andamios ────────────────────────────────────────────────────────────

    private static AperturaDelPermiso Apertura() => new(
        Expediente, "Choluteca", Salida, Retorno, Excepcion: null, Existentes: []);

    private static PermisoEnTramite EnTramite() => new(
        Ulid.NewUlid(),
        "PC-PROV-ABCD1234",
        EstadoDelPermiso.Solicitado,
        Vehiculo: null,
        Motorista: null,
        "Choluteca",
        Salida,
        Retorno,
        "Operativo migratorio de fin de semana coordinado con la Policía Nacional.",
        ["21/03/2026"],
        Jefe,
        FirmadoPor: null);
}
