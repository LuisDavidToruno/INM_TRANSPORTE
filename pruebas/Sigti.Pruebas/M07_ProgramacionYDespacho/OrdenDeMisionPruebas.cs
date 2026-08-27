using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M07_ProgramacionYDespacho;

/// <summary>
/// La máquina de estados del expediente de misión. La autoridad sobre transiciones,
/// precondiciones e invariantes es docs/03-arquitectura/estados/orden-de-mision.md;
/// estas pruebas la ejercen, no la redefinen.
/// </summary>
public class OrdenDeMisionPruebas
{
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

    private static readonly IdPersona Asistente = new("P-ASISTENTE");
    private static readonly IdPersona Jefe = new("P-JEFE");
    private static readonly IdPersona Jefatura = new("P-JEFATURA");
    private static readonly IdPersona Transporte = new("P-TRANSPORTE");
    private static readonly IdPersona Encargado = new("P-ENCARGADO");
    private static readonly IdPersona Motorista = new("P-MOTORISTA");

    /// <summary>Lo que se pidió movilizar. Lo mismo para todas: no es lo que estas pruebas ejercen.</summary>
    private static readonly DatosDeLaSolicitud Solicitud = new(
        Dependencia: "Delegación de Choluteca",
        ObjetoDelTraslado: "Traslado de personal y equipo",
        Destino: "Choluteca",
        Ventana: Asignacion.Ventana);

    [Fact]
    public void El_expediente_conserva_lo_que_se_pidio_movilizar()
    {
        // El sistema no gestiona «viajes de personas»: gestiona movilizaciones de
        // recursos. Sin el objeto del traslado y la ventana, el expediente es una
        // máquina de estados sin nada que autorizar — y `BD-09` no tendría contra qué
        // verificar la compatibilidad.
        var solicitud = new DatosDeLaSolicitud(
            Dependencia: "Delegación de Choluteca",
            ObjetoDelTraslado: "Traslado de 3 servidores y equipo de cómputo",
            Destino: "Choluteca",
            Ventana: new VentanaDeMision(new DateOnly(2026, 3, 20), new DateOnly(2026, 3, 21), 1));

        var expediente = OrdenDeMision.Crear(Ulid.NewUlid(), Asistente, Jefe, solicitud, Momento);

        Assert.Equal(solicitud, expediente.Solicitud);
    }

    [Fact]
    public void Un_expediente_recien_creado_esta_en_borrador()
    {
        var expediente = OrdenDeMision.Crear(
            id: Ulid.NewUlid(),
            capturadaPor: Asistente,
            solicitanteDeDerecho: Jefe,
            solicitud: Solicitud,
            momento: Momento);

        Assert.Equal(EstadoDeMision.Borrador, expediente.Estado);
    }

    [Fact]
    public void El_solicitante_de_derecho_no_puede_autorizar_su_propia_solicitud()
    {
        // El caso cotidiano que BD-01 no cubría antes del hallazgo HB3-01: la asistente
        // captura la solicitud para su jefe. Formalmente el jefe no creó ni envió nada
        // — pero es el solicitante, y la incompatibilidad I-01 sí se está violando.
        var expediente = OrdenDeMision.Crear(Ulid.NewUlid(), Asistente, Jefe, Solicitud, Momento);
        expediente.Enviar(Asistente, Momento);

        var bloqueo = Assert.Throws<BloqueoDuro>(() => expediente.Aprobar(Jefe, Momento));

        Assert.Equal("BD-01", bloqueo.Precondicion);
        Assert.Equal(EstadoDeMision.Solicitada, expediente.Estado);
    }

    [Fact]
    public void El_motivo_de_la_autorizacion_queda_en_el_diario()
    {
        // `HU-009` exige que la constancia diga SOBRE QUÉ DATO se autorizó, y esa
        // constancia se imprime en la orden. Un motivo que el sistema recibe y descarta
        // deja a la jefatura respondiendo por una decisión cuya justificación no existe.
        const string motivo =
            "Autorizo con estructura de 98 horas de antigüedad. Verificada la compatibilidad del pick-up.";

        var expediente = OrdenDeMision.Crear(Ulid.NewUlid(), Asistente, Jefe, Solicitud, Momento);
        expediente.Enviar(Asistente, Momento);
        expediente.Aprobar(Jefatura, Momento, motivo);

        Assert.Equal(motivo, expediente.Diario.Single(t => t.Id == "T-05").Motivo);
    }

    [Fact]
    public void Una_aprobacion_caducada_no_se_puede_programar()
    {
        // «Se calcula la fecha de caducidad de la aprobación: si no se programa antes del
        // INICIO de la ventana solicitada, caduca.» Una cola de aprobadas que nadie
        // depura oculta el déficit real de flota, que es el indicador que la institución
        // necesita.
        var expediente = Aprobada();
        var salida = Asignacion.Ventana.Salida;

        var bloqueo = Assert.Throws<AprobacionCaducada>(
            () => expediente.Programar(Transporte, Asignacion.Valida(), Asignacion.Matriz,
                                       PoliticaDeDocumentacion.PorDefecto, EnLaFecha(salida)));

        Assert.Equal(salida, bloqueo.InicioDeLaVentana);
        Assert.Equal(EstadoDeMision.Aprobada, expediente.Estado);
    }

    [Fact]
    public void Programar_el_dia_anterior_al_inicio_todavia_se_puede()
    {
        // El límite es el inicio, no el fin: reservar un vehículo para un viaje que ya
        // debía haber salido no es programar, es tapar un hueco.
        var expediente = Aprobada();
        var vispera = Asignacion.Ventana.Salida.AddDays(-1);

        expediente.Programar(Transporte, Asignacion.Valida(), Asignacion.Matriz,
                             PoliticaDeDocumentacion.PorDefecto, EnLaFecha(vispera));

        Assert.Equal(EstadoDeMision.Programada, expediente.Estado);
    }

    [Fact]
    public void Anular_exige_un_motivo_del_catalogo_y_lo_deja_en_el_diario()
    {
        // `T-09`: el motivo tipificado ES el indicador de déficit de flota. Un motivo de
        // texto libre no produce ningún indicador — por eso el comentario es complemento
        // y no sustituto.
        var expediente = Aprobada();

        expediente.Anular(
            Transporte,
            MotivoDeAnulacion.CaducadaPorFaltaDeProgramacion,
            comentario: "No hubo pick-up disponible en la ventana.",
            momento: Momento);

        var anulacion = expediente.Diario.Single(t => t.Id == "T-09");

        Assert.Equal(EstadoDeMision.Anulada, expediente.Estado);
        Assert.Contains("CaducadaPorFaltaDeProgramacion", anulacion.Motivo);
        Assert.Contains("No hubo pick-up disponible", anulacion.Motivo);
    }

    private static DateTimeOffset EnLaFecha(DateOnly fecha) =>
        new(fecha.ToDateTime(new TimeOnly(9, 0)), TimeSpan.FromHours(-6));

    [Fact]
    public void No_se_puede_despachar_una_mision_que_no_fue_programada()
    {
        // §3.4: APROBADA → DESPACHADA no existe. Sin programación no hay verificación de
        // licencia, documentación ni reserva.
        var expediente = Aprobada();

        var fallo = Assert.Throws<TransicionInvalida>(
            () => expediente.Despachar(Encargado, Asignacion.Valida(), Asignacion.Matriz,
                                       PoliticaDeDocumentacion.PorDefecto, Momento));

        Assert.Equal(EstadoDeMision.Programada, fallo.EstadoRequerido);
        Assert.Equal(EstadoDeMision.Aprobada, expediente.Estado);
    }

    [Fact]
    public void No_se_programa_con_una_licencia_que_vence_dentro_del_rango()
    {
        // BD-02 se evalúa en T-08. Es la precondición que traslada responsabilidad
        // directa a quien autorizó, y no admite excepción configurable.
        var expediente = Aprobada();
        var asignacion = Asignacion.ConLicenciaHasta(new DateOnly(2026, 3, 13));

        var bloqueo = Assert.Throws<BloqueoDuro>(
            () => expediente.Programar(Transporte, asignacion, Asignacion.Matriz,
                                       PoliticaDeDocumentacion.PorDefecto, Momento));

        Assert.Equal("BD-02", bloqueo.Precondicion);
        Assert.Equal(EstadoDeMision.Aprobada, expediente.Estado);
    }

    [Fact]
    public void Programar_deja_la_evidencia_de_BD_02_en_el_diario()
    {
        // «Guardar solo "verificado" no defiende a nadie.» La evidencia va al diario,
        // que es lo que sobrevive para una auditoría.
        var expediente = Aprobada();
        expediente.Programar(Transporte, Asignacion.Valida(), Asignacion.Matriz,
                             PoliticaDeDocumentacion.PorDefecto, Momento);

        var programacion = expediente.Diario.Single(t => t.Id == "T-08");

        Assert.NotNull(programacion.Motivo);
        Assert.Contains("0801-1990-01234", programacion.Motivo);
        Assert.Contains("PRUEBA-01", programacion.Motivo);
    }

    [Fact]
    public void El_estado_se_reconstruye_desde_el_diario()
    {
        // P-1: dos dispositivos no negocian «el estado», intercambian transiciones. Si el
        // estado no se puede reconstruir del diario, la sincronización desconectada no
        // tiene solución.
        var original = HiloCompleto();

        var reconstruido = OrdenDeMision.Reconstruir(
            original.Id,
            original.CapturadaPor,
            original.SolicitanteDeDerecho,
            original.Solicitud,
            original.Diario);

        Assert.Equal(original.Id, reconstruido.Id);

        Assert.Equal(EstadoDeMision.Liquidada, original.Estado);
        Assert.Equal(original.Estado, reconstruido.Estado);
        Assert.Equal(original.Diario, reconstruido.Diario);
    }

    /// <summary>El hilo completo: solicitud → despacho → ejecución → liquidación.</summary>
    private static OrdenDeMision HiloCompleto()
    {
        var expediente = Aprobada();
        expediente.Programar(Transporte, Asignacion.Valida(), Asignacion.Matriz,
                             PoliticaDeDocumentacion.PorDefecto, Momento);
        expediente.Despachar(Encargado, Asignacion.Valida(), Asignacion.Matriz,
                             PoliticaDeDocumentacion.PorDefecto, Momento);
        expediente.IniciarRuta(Motorista, Momento);
        expediente.Retornar(Motorista, Momento);
        expediente.Liquidar(Transporte, Momento);
        return expediente;
    }

    /// <summary>Un expediente aprobado por la jefatura, que no es ni capturador ni solicitante.</summary>
    private static OrdenDeMision Aprobada()
    {
        var expediente = OrdenDeMision.Crear(Ulid.NewUlid(), Asistente, Jefe, Solicitud, Momento);
        expediente.Enviar(Asistente, Momento);
        expediente.Aprobar(Jefatura, Momento);
        return expediente;
    }
}
