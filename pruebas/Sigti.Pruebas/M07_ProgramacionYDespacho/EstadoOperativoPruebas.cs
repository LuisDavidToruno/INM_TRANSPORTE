using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M07_ProgramacionYDespacho;

/// <summary>
/// `BD-07`, la mitad del <b>estado operativo</b> — §10.2.
///
/// ── Por qué el estado es un enum y no un booleano ────────────────────────────
/// Porque las causas de no poder asignar <b>no son intercambiables</b>: de `EN_TALLER` se sale
/// esperando, de `PRESTADO` se sale con acta, de `DADO_DE_BAJA` no se sale. Colapsarlas en
/// «disponible sí/no» borra la única información con la que se planifica — y con la que se
/// decide si vale la pena volver mañana.
///
/// ── La otra mitad de `BD-07` no se evalúa ────────────────────────────────────
/// La <b>compatibilidad</b> entre lo que se mueve y el tipo de vehículo necesita la matriz de
/// `M-02`, que no existe, y el objeto del traslado es texto libre: no hay nada estructurado
/// contra lo que contrastarla.
/// </summary>
public class EstadoOperativoPruebas
{
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

    private static readonly IdPersona Asistente = new("P-ASISTENTE");
    private static readonly IdPersona Jefe = new("P-JEFE");
    private static readonly IdPersona Jefatura = new("P-JEFATURA");
    private static readonly IdPersona Transporte = new("P-TRANSPORTE");

    private static readonly DatosDeLaSolicitud Solicitud = new(
        "Delegacion de Choluteca", "Traslado de personal", "Choluteca", Asignacion.Ventana);

    [Fact]
    public void Un_vehiculo_DISPONIBLE_se_programa_y_queda_constancia()
    {
        var expediente = Aprobada();

        Programar(expediente, EstadoOperativo.Disponible);

        Assert.Equal(EstadoDeMision.Programada, expediente.Estado);
        Assert.Contains("BD-07 verificada", expediente.Diario[^1].Motivo);
    }

    [Theory]
    [InlineData(EstadoOperativo.EnTaller)]
    [InlineData(EstadoOperativo.NoDisponible)]
    [InlineData(EstadoOperativo.Prestado)]
    public void Un_vehiculo_inutilizable_bloquea_y_el_mensaje_dice_por_que(EstadoOperativo estado)
    {
        // Los tres que vuelven al vehículo inutilizable. El mensaje nombra el estado real:
        // «no está disponible» a secas no dice si esperar sirve.
        var expediente = Aprobada();

        var bloqueo = Assert.Throws<BloqueoDuro>(() => Programar(expediente, estado));

        Assert.Equal("BD-07", bloqueo.Precondicion);
        Assert.Contains(estado.ToString(), bloqueo.Message);
        Assert.Contains("Elija otro vehículo o espere", bloqueo.Message);
        Assert.Equal(EstadoDeMision.Aprobada, expediente.Estado);
    }

    [Theory]
    [InlineData(EstadoOperativo.DadoDeBaja)]
    [InlineData(EstadoOperativo.RetiradoDeFlota)]
    public void Un_estado_terminal_lo_dice_en_vez_de_invitar_a_esperar(EstadoOperativo estado)
    {
        // De `EN_TALLER` se sale; de éstos no. Decirle a quien programa que «espere a que
        // vuelva a estar disponible» un vehículo descargado lo manda a esperar para siempre.
        var expediente = Aprobada();

        var bloqueo = Assert.Throws<BloqueoDuro>(() => Programar(expediente, estado));

        Assert.Contains("terminal", bloqueo.Message);
        Assert.Contains("ya no vuelve a la flota", bloqueo.Message);
    }

    [Theory]
    [InlineData(EstadoOperativo.Asignado)]
    [InlineData(EstadoOperativo.EnMision)]
    public void Un_vehiculo_COMPROMETIDO_si_se_puede_programar_para_otra_ventana(EstadoOperativo estado)
    {
        // **§10.2 dice lo contrario, y leído al pie de la letra rompe la operación normal.**
        // Un vehículo comprometido a una misión de diciembre queda `ASIGNADO` desde hoy, y
        // bloquearía programar una de marzo con la que no se solapa en nada.
        //
        // Todo el sistema está construido sobre lo contrario: `EF-01` reserva por ventana,
        // `BD-11` compara ventanas, y el cronograma dibuja **varias barras por carril**
        // justamente porque un vehículo tiene varias misiones a lo largo del mes.
        //
        // El solape lo decide `BD-11`, que además nombra al titular. Si `ASIGNADO` bloqueara,
        // taparía a `BD-11` con un mensaje mucho peor.
        var expediente = Aprobada();

        Programar(expediente, estado);

        Assert.Equal(EstadoDeMision.Programada, expediente.Estado);
        Assert.Contains($"BD-07 verificada: vehículo {estado}", expediente.Diario[^1].Motivo);
    }

    [Fact]
    public void Sin_estado_declarado_NO_se_da_por_disponible_y_el_diario_lo_dice()
    {
        // **La decisión que más importa acá.** §10.2 lista «alta reciente sin habilitar» entre
        // las causas de `NO_DISPONIBLE`: tratar el nulo como disponible haría que **el alta
        // habilitara sola**, que es lo contrario.
        //
        // No bloquea —hay expedientes anteriores al estado operativo— pero **deja dicho que no
        // se verificó**, que es distinto de haber verificado y pasado.
        var expediente = Aprobada();

        Programar(expediente, estado: null);

        Assert.Equal(EstadoDeMision.Programada, expediente.Estado);
        Assert.Contains("BD-07 NO evaluada", expediente.Diario[^1].Motivo);
        Assert.DoesNotContain("BD-07 verificada", expediente.Diario[^1].Motivo!);
    }

    private static void Programar(OrdenDeMision expediente, EstadoOperativo? estado) =>
        expediente.Programar(Transporte, Asignacion.Valida(), Asignacion.Matriz,
                             PoliticaDeDocumentacion.PorDefecto, Momento,
                             new RecursosTomados(Ulid.NewUlid(), Ulid.NewUlid()), [],
                             estado);

    private static OrdenDeMision Aprobada()
    {
        var expediente = OrdenDeMision.Crear(Ulid.NewUlid(), Asistente, Jefe, Solicitud, Momento);
        expediente.Enviar(Asistente, Momento);
        expediente.Aprobar(Jefatura, Momento);
        return expediente;
    }
}
