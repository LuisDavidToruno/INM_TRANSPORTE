using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M07_ProgramacionYDespacho;

/// <summary>
/// `BD-11` — sin solapamiento de reserva.
///
/// ── Por qué la aritmética del solape vive en el dominio ──────────────────────
/// Porque es <b>la regla</b>, no una optimización de consulta. Puesta en el `WHERE` de un
/// SQL, no se puede ejercer sin base de datos y los casos de borde —dos misiones que se
/// tocan por un día— se prueban a través de tres capas o no se prueban. Acá se ejercen
/// directamente, que es lo que permite escribir la prueba del caso que importa.
///
/// ── El caso que importa ──────────────────────────────────────────────────────
/// Los extremos son <b>inclusivos de los dos lados</b>. Una misión que retorna el jueves y
/// otra que sale el jueves <b>chocan</b>: el vehículo no puede estar volviendo de Danlí y
/// saliendo a Juticalpa el mismo día. Con extremo exclusivo el sistema las aceptaría, y el
/// error se descubriría en el predio a las cinco de la mañana.
/// </summary>
public class SolapamientoDeReservaPruebas
{
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

    private static readonly IdPersona Asistente = new("P-ASISTENTE");
    private static readonly IdPersona Jefe = new("P-JEFE");
    private static readonly IdPersona Jefatura = new("P-JEFATURA");
    private static readonly IdPersona Transporte = new("P-TRANSPORTE");

    /// <summary>La misión bajo prueba ocupa del 20 al 22, más un día de holgura: hasta el 23.</summary>
    private static readonly DatosDeLaSolicitud Solicitud = new(
        "Delegacion de Choluteca", "Traslado de personal", "Choluteca", Asignacion.Ventana);

    [Fact]
    public void Un_vehiculo_ya_tomado_en_la_franja_bloquea_y_nombra_al_titular()
    {
        // `EF-01`: hay que mostrar **qué misión, de qué dependencia, en qué franja**. Las
        // cuatro salidas que la regla ofrece —consolidar, otro recurso, reprogramar,
        // escalar— empiezan todas por saber a quién llamar.
        var expediente = Aprobada();

        var bloqueo = Assert.Throws<BloqueoDuro>(() => Programar(expediente, [
            Reserva(new DateOnly(2026, 3, 21), new DateOnly(2026, 3, 21)),
        ]));

        Assert.Equal("BD-11", bloqueo.Precondicion);
        Assert.Contains("PROV-000042", bloqueo.Message);
        Assert.Contains("Delegacion de Danli", bloqueo.Message);
        Assert.Contains("2026-03-21", bloqueo.Message);
        Assert.Equal(EstadoDeMision.Aprobada, expediente.Estado);
    }

    [Fact]
    public void Dos_misiones_que_se_tocan_por_un_dia_se_solapan()
    {
        // **El caso que decide si la regla sirve.** La otra misión termina el 20; ésta sale
        // el 20. El vehículo no puede estar volviendo y saliendo el mismo día, y un extremo
        // exclusivo las dejaría pasar a las dos.
        var expediente = Aprobada();

        var bloqueo = Assert.Throws<BloqueoDuro>(() => Programar(expediente, [
            Reserva(new DateOnly(2026, 3, 17), new DateOnly(2026, 3, 20)),
        ]));

        Assert.Equal("BD-11", bloqueo.Precondicion);
    }

    [Fact]
    public void La_holgura_posterior_tambien_esta_reservada()
    {
        // La misión retorna el 22, pero la holgura la extiende al 23 — el último día en que
        // el motorista podría estar conduciendo. Reservar sólo hasta el retorno ofrecería
        // libre un vehículo que todavía no volvió al predio.
        var expediente = Aprobada();

        var bloqueo = Assert.Throws<BloqueoDuro>(() => Programar(expediente, [
            Reserva(new DateOnly(2026, 3, 23), new DateOnly(2026, 3, 25)),
        ]));

        Assert.Equal("BD-11", bloqueo.Precondicion);
        // El mensaje declara la franja propia, no sólo la ajena: sin eso, quien programa no
        // entiende por qué choca una reserva que empieza un día después del retorno.
        Assert.Contains("al 2026-03-23", bloqueo.Message);
    }

    [Fact]
    public void Una_reserva_que_termina_la_vispera_no_estorba()
    {
        // El recíproco de lo anterior, y hace falta: una regla que bloquea siempre pasa
        // todas las pruebas de bloqueo. Sin este caso, `SeSolapaCon` podría devolver
        // `true` incondicionalmente y las otras tres seguirían en verde.
        var expediente = Aprobada();

        Programar(expediente, [Reserva(new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 19))]);

        Assert.Equal(EstadoDeMision.Programada, expediente.Estado);
    }

    [Fact]
    public void Se_deja_constancia_de_haber_verificado_contra_las_reservas()
    {
        // Un control que no deja rastro es un control que no se puede auditar. El diario
        // dice **contra cuántas** reservas se verificó, no un «BD-11 verificada» a secas.
        var expediente = Aprobada();

        Programar(expediente, [Reserva(new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 19))]);

        Assert.Contains("BD-11 verificada contra 1 reserva", expediente.Diario[^1].Motivo);
    }

    [Fact]
    public void Sin_reservas_consultadas_NO_se_declara_verificada()
    {
        // Nulo y lista vacía no son lo mismo: vacío es «se consultó y está libre», nulo es
        // «nadie consultó». Declarar el control en el segundo caso dejaría en un expediente
        // auditable la constancia de una verificación que no ocurrió.
        var expediente = Aprobada();

        expediente.Programar(Transporte, Asignacion.Valida(), Asignacion.Matriz,
                             PoliticaDeDocumentacion.PorDefecto, Momento);

        Assert.DoesNotContain("BD-11", expediente.Diario[^1].Motivo!);
    }

    [Fact]
    public void Con_el_recurso_libre_se_declara_verificada_igual()
    {
        var expediente = Aprobada();

        Programar(expediente, []);

        Assert.Contains("BD-11 verificada contra 0 reserva", expediente.Diario[^1].Motivo);
    }

    /// <summary>
    /// Una reserva ajena sobre el vehículo. Los datos son los que `EF-01` exige mostrar.
    /// </summary>
    private static ReservaDeRecurso Reserva(DateOnly desde, DateOnly hasta) => new(
        Ulid.NewUlid(), "PROV-000042", "Delegacion de Danli", desde, hasta,
        Vehiculo: true, Conductor: false);

    private static void Programar(OrdenDeMision expediente, IReadOnlyList<ReservaDeRecurso> reservas) =>
        expediente.Programar(Transporte, Asignacion.Valida(), Asignacion.Matriz,
                             PoliticaDeDocumentacion.PorDefecto, Momento, reservas: reservas);

    private static OrdenDeMision Aprobada()
    {
        var expediente = OrdenDeMision.Crear(Ulid.NewUlid(), Asistente, Jefe, Solicitud, Momento);
        expediente.Enviar(Asistente, Momento);
        expediente.Aprobar(Jefatura, Momento);
        return expediente;
    }
}
