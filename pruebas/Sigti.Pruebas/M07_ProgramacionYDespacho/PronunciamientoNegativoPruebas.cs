using Sigti.Dominio.M06_Solicitudes;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M07_ProgramacionYDespacho;

/// <summary>
/// Las salidas de `SOLICITADA` que no son aprobar — `T-04`, `T-06`, `T-07` — y `T-03`.
///
/// ── Lo que faltaba, y por qué era grave ──────────────────────────────────────
/// La jefatura podía aprobar y <b>nada más</b>. Una solicitud improcedente no se podía
/// rechazar, una incompleta no se podía devolver, y quien la pidió no podía retirarla: el
/// único camino era hacia adelante. La bandeja de `PT-013` ofrecía media función de
/// autoridad — la mitad que dice que sí.
///
/// ── Rechazar y devolver no son lo mismo, y confundirlas cuesta ───────────────
/// `T-06` dice <b>«no»</b> y es terminal. `T-04` dice <b>«así no»</b> y el expediente vuelve a
/// quien lo capturó. Con una sola de las dos, o una solicitud arreglable muere, o una
/// improcedente da vueltas para siempre.
/// </summary>
public class PronunciamientoNegativoPruebas
{
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

    private static readonly IdPersona Asistente = new("P-ASISTENTE");
    private static readonly IdPersona Jefe = new("P-JEFE");
    private static readonly IdPersona Jefatura = new("P-JEFATURA");

    private static readonly DatosDeLaSolicitud Solicitud = new(
        "Delegacion de Choluteca", "Traslado de personal", "Choluteca", Asignacion.Ventana);

    /// <summary>Los cuatro de `HU-014`, que la historia declara <b>de ejemplo</b>.</summary>
    private static readonly CatalogoDeMotivosDeRechazo Catalogo = new([
        "No corresponde a la función institucional",
        "Gasto no justificado",
        "Fecha no viable",
        "Duplica una misión ya autorizada",
    ]);

    [Fact]
    public void Rechazar_deja_el_expediente_en_rechazada_con_motivo_y_explicacion()
    {
        var expediente = Solicitada();

        expediente.Rechazar(Jefatura, "Gasto no justificado",
                            "El traslado se puede resolver con la unidad de la delegación.",
                            Catalogo, Momento);

        Assert.Equal(EstadoDeMision.Rechazada, expediente.Estado);
        Assert.Contains("Gasto no justificado", expediente.Diario[^1].Motivo);
        Assert.Contains("unidad de la delegación", expediente.Diario[^1].Motivo);
    }

    [Fact]
    public void De_rechazada_no_sale_ninguna_transicion()
    {
        // «La negativa queda documentada y **no se borra reabriendo el expediente**.» Un
        // rechazo que se puede deshacer no es un pronunciamiento, es un borrador. Quien
        // quiera insistir presenta una solicitud nueva.
        var expediente = Solicitada();
        expediente.Rechazar(Jefatura, "Fecha no viable", "La ventana choca con el cierre fiscal.",
                            Catalogo, Momento);

        Assert.Throws<TransicionInvalida>(() => expediente.Aprobar(Jefatura, Momento));
        Assert.Throws<TransicionInvalida>(
            () => expediente.DevolverParaCorreccion(Jefatura, "Mejor corríjala", Momento));
        Assert.Throws<TransicionInvalida>(() => expediente.Desistir(Asistente, "Ya no", Momento));
    }

    [Fact]
    public void El_solicitante_de_derecho_no_puede_rechazar_su_propia_solicitud()
    {
        // `BD-01`. Y el mensaje tiene que hablar de RECHAZAR: decir «no puede autorizarla» a
        // quien intentó rechazar manda a buscar el problema donde no está.
        var expediente = Solicitada();

        var bloqueo = Assert.Throws<BloqueoDuro>(
            () => expediente.Rechazar(Jefe, "Fecha no viable", "No conviene", Catalogo, Momento));

        Assert.Equal("BD-01", bloqueo.Precondicion);
        Assert.Contains("rechazarla", bloqueo.Message);
        Assert.Contains("solicitante de derecho", bloqueo.Message);
        Assert.Equal(EstadoDeMision.Solicitada, expediente.Estado);
    }

    [Fact]
    public void Un_motivo_fuera_del_catalogo_no_sirve_y_el_mensaje_dice_cuales_hay()
    {
        // «Seleccione un motivo del catálogo. El texto libre complementa el motivo
        // tipificado, no lo sustituye.» Sin esta regla el catálogo sería una sugerencia, y en
        // un mes habría cuatro redacciones del mismo rechazo que ningún reporte suma.
        var expediente = Solicitada();

        var bloqueo = Assert.Throws<BloqueoDuro>(
            () => expediente.Rechazar(Jefatura, "no me gusta", "Porque no", Catalogo, Momento));

        Assert.Equal("T-06", bloqueo.Precondicion);
        Assert.Contains("Gasto no justificado", bloqueo.Message);
    }

    [Fact]
    public void Un_rechazo_sin_explicacion_tampoco()
    {
        // El motivo tipificado dice qué se cuenta; el texto dice a la dependencia qué pasó.
        // Sin lo segundo, no sabe si vale la pena replantearlo.
        var expediente = Solicitada();

        var bloqueo = Assert.Throws<BloqueoDuro>(
            () => expediente.Rechazar(Jefatura, "Fecha no viable", "   ", Catalogo, Momento));

        Assert.Equal("T-06", bloqueo.Precondicion);
        Assert.Equal(EstadoDeMision.Solicitada, expediente.Estado);
    }

    [Fact]
    public void Devolver_para_correccion_regresa_a_borrador_y_se_puede_reenviar()
    {
        // **La diferencia con rechazar, ejercida entera.** El expediente vuelve a quien lo
        // capturó, se corrige, y se reenvía por `T-02` — sigue siendo el mismo expediente.
        var expediente = Solicitada();

        expediente.DevolverParaCorreccion(Jefatura, "Falta el detalle de la carga", Momento);
        Assert.Equal(EstadoDeMision.Borrador, expediente.Estado);

        expediente.Enviar(Asistente, Momento);
        Assert.Equal(EstadoDeMision.Solicitada, expediente.Estado);

        // Y el rastro de la devolución permanece: es lo que distingue un expediente que se
        // corrigió de uno que salió bien a la primera.
        Assert.Contains(expediente.Diario, t => t.Id == "T-04");
    }

    [Fact]
    public void Devolver_sin_decir_que_corregir_no_se_puede()
    {
        var expediente = Solicitada();

        var bloqueo = Assert.Throws<BloqueoDuro>(
            () => expediente.DevolverParaCorreccion(Jefatura, "", Momento));

        Assert.Equal("T-04", bloqueo.Precondicion);
    }

    [Fact]
    public void Desistir_NO_exige_segregacion_porque_no_es_un_pronunciamiento()
    {
        // **El caso que distingue `T-07` de `T-06`.** `BD-01` existe para que nadie autorice
        // lo que él mismo pidió; retirar lo propio es lo contrario. Exigir un tercero
        // obligaría a molestar a la jefatura para deshacer algo que no llegó a nada.
        var expediente = Solicitada();

        expediente.Desistir(Jefe, "La comisión se resolvió por videollamada", Momento);

        Assert.Equal(EstadoDeMision.Anulada, expediente.Estado);
    }

    [Fact]
    public void Un_borrador_que_nunca_se_envio_se_descarta_y_queda_el_rastro()
    {
        // «No hay asiento reverso porque no hubo transacción» — pero sí hay registro: un
        // borrador que desaparece sin rastro es indistinguible de uno que nunca existió.
        var expediente = OrdenDeMision.Crear(Ulid.NewUlid(), Asistente, Jefe, Solicitud, Momento);

        expediente.DescartarBorrador(Asistente, "Se capturó por error", Momento);

        Assert.Equal(EstadoDeMision.Anulada, expediente.Estado);
        Assert.Equal("T-03", expediente.Diario[^1].Id);
    }

    [Fact]
    public void Un_borrador_ya_enviado_no_se_descarta()
    {
        // `T-03` es sólo para lo que nunca entró al circuito de control. Una vez enviada, la
        // salida es `T-07` — y esa sí se retira de las bandejas de quienes iban a
        // pronunciarse.
        var expediente = Solicitada();

        Assert.Throws<TransicionInvalida>(
            () => expediente.DescartarBorrador(Asistente, "Me arrepentí", Momento));
    }

    private static OrdenDeMision Solicitada()
    {
        var expediente = OrdenDeMision.Crear(Ulid.NewUlid(), Asistente, Jefe, Solicitud, Momento);
        expediente.Enviar(Asistente, Momento);
        return expediente;
    }
}
