using Sigti.Dominio.M11_Mantenimiento;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M07_ProgramacionYDespacho;

/// <summary>
/// `BD-13` — un vehículo sin custodio vigente no se despacha.
///
/// ── Por qué es bloqueo y no una formalidad ───────────────────────────────────
/// No es que falte un papel: es que <b>la operación que `T-12` describe no se puede
/// ejecutar</b>. <i>«Trasladar una custodia que no existe no es posible: si nadie responde
/// hoy por el bien, tampoco hay de quién recibirlo ni a quién devolverlo, y el acta de
/// entrega queda sin una de sus dos firmas.»</i>
///
/// `RN-22` sabe que incomoda y lo sostiene igual: <i>«vehículo asignado a una delegación sin
/// custodio designado: bloqueo del despacho. Es incómodo y es correcto — un vehículo del
/// Estado sin responsable identificado es un hallazgo esperando ocurrir»</i>.
///
/// ── La pregunta que contesta la cadena de custodia ───────────────────────────
/// La que aparece cuando algo falta o algo se daña: <b>¿quién tenía el vehículo en ese
/// momento?</b> Sin ella, la deducción de responsabilidad no tiene sobre quién recaer, y el
/// hallazgo del Tribunal Superior de Cuentas queda sin responsable — <b>lo que agrava, no
/// atenúa</b>.
/// </summary>
public class CustodiaAlDespacharPruebas
{
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

    private static readonly IdPersona Asistente = new("P-ASISTENTE");
    private static readonly IdPersona Jefe = new("P-JEFE");
    private static readonly IdPersona Jefatura = new("P-JEFATURA");
    private static readonly IdPersona Transporte = new("P-TRANSPORTE");
    private static readonly IdPersona Encargado = new("P-ENCARGADO");
    private static readonly IdPersona Custodio = new("P-CUSTODIO");

    private static readonly DatosDeLaSolicitud Solicitud = new(
        "Delegacion de Choluteca", "Traslado de personal", "Choluteca", Asignacion.Ventana);

    /// <summary>Custodia abierta: `Hasta` nulo es <b>vigente</b>, no eterno.</summary>
    private static readonly CustodiaDelVehiculo Vigente =
        new(Custodio, new DateOnly(2025, 6, 1), null);

    [Fact]
    public void Sin_ninguna_custodia_registrada_no_se_despacha()
    {
        var expediente = Programada();

        var bloqueo = Assert.Throws<BloqueoDuro>(() => Despachar(expediente, []));

        Assert.Equal("BD-13", bloqueo.Precondicion);
        // El mensaje distingue «nunca tuvo custodio» de «la custodia cesó»: son dos
        // problemas con dos arreglos distintos, y quien despacha necesita saber cuál tiene.
        Assert.Contains("no tiene ninguna custodia registrada", bloqueo.Message);
        Assert.Equal(EstadoDeMision.Programada, expediente.Estado);
    }

    [Fact]
    public void Con_la_custodia_cesada_tampoco_y_el_mensaje_lo_distingue()
    {
        // El caso de `RN-22`: «custodio que cesa en el cargo dejando el vehículo asignado».
        // Que haya habido custodio no es lo mismo que haberlo hoy.
        var expediente = Programada();

        var cesada = new CustodiaDelVehiculo(Custodio, new DateOnly(2025, 6, 1), new DateOnly(2026, 2, 28));

        var bloqueo = Assert.Throws<BloqueoDuro>(() => Despachar(expediente, [cesada]));

        Assert.Equal("BD-13", bloqueo.Precondicion);
        Assert.Contains("ninguna vigente a esa fecha", bloqueo.Message);
        // Y dice a qué fecha se evaluó: sin eso, quien despacha no sabe si esperar sirve.
        Assert.Contains("2026-03-12", bloqueo.Message);
    }

    [Fact]
    public void Con_custodio_vigente_se_despacha_y_queda_constancia_de_quien_es()
    {
        // «¿Quién tenía el vehículo en ese momento?» El diario tiene que poder contestarlo
        // años después, y por eso el nombre va al asiento y no sólo a un «BD-13 verificada».
        var expediente = Programada();

        Despachar(expediente, [Vigente]);

        Assert.Equal(EstadoDeMision.Despachada, expediente.Estado);
        Assert.Contains("BD-13 verificada", expediente.Diario[^1].Motivo);
        Assert.Contains("P-CUSTODIO", expediente.Diario[^1].Motivo);
    }

    [Fact]
    public void La_vigencia_se_juzga_a_la_fecha_del_HECHO_y_no_a_la_de_captura()
    {
        // **P-4.** Un despacho capturado en campo y sincronizado días después se juzga con
        // el custodio que había el día en que el vehículo salió. Acá la custodia cesó DESPUÉS
        // del despacho: el despacho fue correcto cuando ocurrió, y una rotación posterior no
        // puede invalidarlo retroactivamente.
        var expediente = Programada();

        var cesoDespues = new CustodiaDelVehiculo(
            Custodio, new DateOnly(2025, 6, 1), new DateOnly(2026, 3, 12));

        Despachar(expediente, [cesoDespues]);

        Assert.Equal(EstadoDeMision.Despachada, expediente.Estado);
    }

    [Fact]
    public void Una_custodia_que_todavia_no_empieza_no_habilita()
    {
        // El recíproco, y hace falta: sin él, `VigenteAl` podría ignorar `Desde` y las otras
        // pruebas seguirían en verde. Firmar hoy la tarjeta de responsabilidad de la semana
        // que viene no pone a nadie a responder por el bien esta semana.
        var expediente = Programada();

        var futura = new CustodiaDelVehiculo(Custodio, new DateOnly(2026, 4, 1), null);

        var bloqueo = Assert.Throws<BloqueoDuro>(() => Despachar(expediente, [futura]));

        Assert.Equal("BD-13", bloqueo.Precondicion);
    }

    [Fact]
    public void Entre_varias_custodias_responde_la_vigente()
    {
        // Un vehículo con historial: dos custodios anteriores y el actual. `BD-13` no cuenta
        // filas, resuelve a la fecha.
        var expediente = Programada();
        var anterior = new IdPersona("P-ANTERIOR");

        Despachar(expediente, [
            new CustodiaDelVehiculo(anterior, new DateOnly(2023, 1, 10), new DateOnly(2024, 5, 31)),
            new CustodiaDelVehiculo(anterior, new DateOnly(2024, 6, 1), new DateOnly(2025, 5, 31)),
            Vigente,
        ]);

        Assert.Contains("P-CUSTODIO", expediente.Diario[^1].Motivo);
        Assert.DoesNotContain("P-ANTERIOR", expediente.Diario[^1].Motivo);
    }

    /// <summary>
    /// Con organigrama VACIO: estas pruebas van de `BD-13`, no de la custodia vacante. Un
    /// espejo que no conoce a nadie no puede afirmar que alguien ceso.
    /// </summary>
    private static void Despachar(OrdenDeMision expediente, IReadOnlyList<CustodiaDelVehiculo> custodias) =>
        expediente.Despachar(Encargado, Asignacion.Valida(), Asignacion.Matriz,
                             PoliticaDeDocumentacion.PorDefecto, Momento,
                             new CustodiaAlDespachar(custodias, new Organigrama([])),
                             Asignacion.SinDiasInhabiles(), ConflictoPorIndisponibilidad.Ninguno);

    private static OrdenDeMision Programada()
    {
        var expediente = OrdenDeMision.Crear(Ulid.NewUlid(), Asistente, Jefe, Solicitud, Momento);
        expediente.Enviar(Asistente, Momento);
        expediente.Aprobar(Jefatura, Momento);
        expediente.Programar(Transporte, Asignacion.Valida(), Asignacion.Matriz,
                             PoliticaDeDocumentacion.PorDefecto, Momento,
                             new RecursosTomados(Ulid.NewUlid(), Ulid.NewUlid()), []);
        return expediente;
    }
}
