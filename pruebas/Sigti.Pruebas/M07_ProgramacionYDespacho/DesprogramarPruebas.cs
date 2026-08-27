using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M07_ProgramacionYDespacho;

/// <summary>
/// `T-11` desprogramar y `T-13` anular una programada — las dos salidas de `PROGRAMADA`
/// que no son despachar.
///
/// ── Lo que faltaba, y por qué era grave ──────────────────────────────────────
/// Hasta ahora una misión programada <b>no se podía deshacer</b>: ni devolverla a la cola
/// ni matarla. Un vehículo asignado por error quedaba tomado hasta que alguien lo
/// despachara. Y sin `T-11` la cuarta salida de un conflicto de `BD-11` —escalar la
/// prioridad— no tenía por dónde ejecutarse: `EF-01` exige que desplazar a una misión pase
/// <b>por devolverla explícitamente a la cola</b>, nunca por quitarle el vehículo en
/// silencio, porque eso se descubre el día de la salida, en el predio.
///
/// ── La diferencia entre las dos ──────────────────────────────────────────────
/// `T-11` devuelve la misión a `APROBADA`: sigue queriendo salir, y conserva su aprobación
/// original. `T-13` la mata, y es irreversible. Confundirlas deja una misión viva sin
/// vehículo o una muerta ocupando flota.
/// </summary>
public class DesprogramarPruebas
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
    public void Desprogramar_devuelve_la_mision_a_aprobada_conservando_su_aprobacion()
    {
        // «La solicitud vuelve a la cola de programación **conservando su aprobación
        // original**». Por eso vuelve a APROBADA y no a SOLICITADA: obligar a la jefatura a
        // firmar de nuevo por un problema de flota es castigar a quien pidió.
        var expediente = Programada();

        expediente.Desprogramar(Transporte, "Desplazada por prioridad superior", Momento);

        Assert.Equal(EstadoDeMision.Aprobada, expediente.Estado);
    }

    [Fact]
    public void Desprogramar_sin_motivo_no_se_puede()
    {
        // La dependencia pierde un vehículo que ya tenía asignado. `EF-01` exige
        // notificarla, y una notificación sin razón no es una notificación.
        var expediente = Programada();

        var bloqueo = Assert.Throws<BloqueoDuro>(
            () => expediente.Desprogramar(Transporte, "   ", Momento));

        Assert.Equal("T-11", bloqueo.Precondicion);
        // Y no quedó a medias: sigue programada, con su vehículo.
        Assert.Equal(EstadoDeMision.Programada, expediente.Estado);
    }

    [Fact]
    public void La_transicion_que_reservo_sigue_en_el_diario_despues_de_desprogramar()
    {
        // P-3: nada se deshace. La reserva de `T-08` **permanece** — deja de contar porque
        // el estado ya no la sostiene, no porque se haya borrado. Es lo que permite que un
        // auditor reconstruya que ese vehículo estuvo comprometido y por qué se liberó.
        var expediente = Programada();

        expediente.Desprogramar(Transporte, "El vehículo entró a taller", Momento);

        var reserva = expediente.Diario.Single(t => t.Id == "T-08");
        Assert.NotNull(reserva.Recursos);
        Assert.Equal("T-11", expediente.Diario[^1].Id);
    }

    [Fact]
    public void Se_puede_reprogramar_despues_de_desprogramar_y_la_reserva_es_la_nueva()
    {
        // `T-11` es reversible vía `T-08` — es lo que hace que desprogramar sirva para algo.
        // Y el diario queda con **dos** reservas: la que vale es la última, que es
        // exactamente lo que la consulta de ocupación proyecta.
        var expediente = Programada();
        var otroVehiculo = Ulid.NewUlid();
        var otroConductor = Ulid.NewUlid();

        expediente.Desprogramar(Transporte, "Se reasigna a un vehículo con remolque", Momento);

        expediente.Programar(Transporte, Asignacion.Valida(), Asignacion.Matriz,
                             PoliticaDeDocumentacion.PorDefecto, Momento,
                             new RecursosTomados(otroVehiculo, otroConductor), []);

        Assert.Equal(EstadoDeMision.Programada, expediente.Estado);

        var reservas = expediente.Diario.Where(t => t.Recursos is not null).ToList();
        Assert.Equal(2, reservas.Count);
        Assert.Equal(otroVehiculo, reservas[^1].Recursos!.Vehiculo);
    }

    [Fact]
    public void Reprogramar_despues_del_inicio_de_la_ventana_ya_no_se_puede()
    {
        // El recíproco que evita el abuso: desprogramar no es una forma de resucitar una
        // aprobación caducada. Si la ventana ya inició, `T-08` sigue bloqueando igual.
        var expediente = Programada();
        expediente.Desprogramar(Transporte, "Motivo cualquiera", Momento);

        var salida = Asignacion.Ventana.Salida;

        Assert.Throws<AprobacionCaducada>(
            () => expediente.Programar(Transporte, Asignacion.Valida(), Asignacion.Matriz,
                                       PoliticaDeDocumentacion.PorDefecto,
                                       new DateTimeOffset(salida.ToDateTime(TimeOnly.MinValue),
                                                          TimeSpan.FromHours(-6))));
    }

    [Fact]
    public void Anular_una_programada_la_mata_con_motivo_tipificado()
    {
        var expediente = Programada();

        expediente.AnularProgramada(Transporte, MotivoDeAnulacion.CausaExterna,
                                    "Cierre de carretera por derrumbe", Momento);

        Assert.Equal(EstadoDeMision.Anulada, expediente.Estado);
        Assert.Contains("CausaExterna", expediente.Diario[^1].Motivo);
        Assert.Contains("derrumbe", expediente.Diario[^1].Motivo);
    }

    [Fact]
    public void De_una_anulada_no_se_vuelve()
    {
        // La tabla de transiciones marca `T-13` **irreversible**. Quien quiera el viaje
        // presenta una solicitud nueva; una anulada que se pudiera reprogramar dejaría el
        // indicador de déficit contando misiones que sí se hicieron.
        var expediente = Programada();
        expediente.AnularProgramada(Transporte, MotivoDeAnulacion.SinFlotaDisponible, null, Momento);

        Assert.Throws<TransicionInvalida>(
            () => expediente.Desprogramar(Transporte, "Me arrepentí", Momento));

        Assert.Throws<TransicionInvalida>(
            () => expediente.Programar(Transporte, Asignacion.Valida(), Asignacion.Matriz,
                                       PoliticaDeDocumentacion.PorDefecto, Momento));
    }

    [Fact]
    public void Una_aprobada_sin_programar_no_se_desprograma()
    {
        // No hay recursos que liberar. Aceptarlo dejaría un `T-11` en el diario que sugiere
        // que hubo una reserva donde nunca la hubo.
        var expediente = OrdenDeMision.Crear(Ulid.NewUlid(), Asistente, Jefe, Solicitud, Momento);
        expediente.Enviar(Asistente, Momento);
        expediente.Aprobar(Jefatura, Momento);

        Assert.Throws<TransicionInvalida>(
            () => expediente.Desprogramar(Transporte, "Motivo cualquiera", Momento));
    }

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
