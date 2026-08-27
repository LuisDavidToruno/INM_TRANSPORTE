using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M07_ProgramacionYDespacho;

/// <summary>
/// `T-10` — cambiar el vehículo o quien conduce sin soltar la misión.
///
/// ── Por qué no basta con `T-11` + `T-08` ─────────────────────────────────────
/// Porque el rodeo pierde algo. Desprogramar devuelve la misión a la cola —donde otro
/// puede tomarle el vehículo entre medio— y `EF-02` <b>anula el folio reservado</b>. La
/// ficha de `T-10` es explícita: <i>«el folio reservado no cambia: es el mismo
/// expediente»</i>.
///
/// ── Lo que la vuelve el caso de borde de `BD-11` ─────────────────────────────
/// Acá la misión <b>está ocupando</b> mientras se evalúa, a diferencia de `T-08`, que sale
/// de `APROBADA`. Si el llamador no la excluye de las reservas, choca contra su propia
/// reserva y ningún cambio es posible — que es exactamente el fallo que la exclusión de
/// `ConsultaDeOcupacion.ReservasDeAsync` existe para evitar.
/// </summary>
public class ReasignarPruebas
{
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

    private static readonly IdPersona Asistente = new("P-ASISTENTE");
    private static readonly IdPersona Jefe = new("P-JEFE");
    private static readonly IdPersona Jefatura = new("P-JEFATURA");
    private static readonly IdPersona Transporte = new("P-TRANSPORTE");

    private static readonly DatosDeLaSolicitud Solicitud = new(
        "Delegacion de Choluteca", "Traslado de personal", "Choluteca", Asignacion.Ventana);

    private static readonly Ulid VehiculoOriginal = Ulid.NewUlid();
    private static readonly Ulid ConductorOriginal = Ulid.NewUlid();

    [Fact]
    public void Reasignar_deja_la_mision_programada_con_el_recurso_nuevo()
    {
        var expediente = Programada();
        var entrante = Ulid.NewUlid();

        expediente.Reasignar(Transporte, Asignacion.Valida(),
                             MotivoDeReasignacion.VehiculoATaller, "Falla de frenos",
                             Asignacion.Matriz, PoliticaDeDocumentacion.PorDefecto, Momento,
                             new RecursosTomados(entrante, ConductorOriginal), []);

        // Sigue PROGRAMADA: no pasó por un estado en que no tenía vehículo.
        Assert.Equal(EstadoDeMision.Programada, expediente.Estado);

        var vigente = expediente.Diario.Last(t => t.Recursos is not null);
        Assert.Equal(entrante, vigente.Recursos!.Vehiculo);
    }

    [Fact]
    public void La_asignacion_original_sigue_en_el_diario_con_el_motivo_del_cambio()
    {
        // `DP-001 D-07`: «el diario muestra a quién se había asignado, **por qué se cambió**
        // y a quién se asignó». Las tres cosas, no dos.
        var expediente = Programada();

        expediente.Reasignar(Transporte, Asignacion.Valida(),
                             MotivoDeReasignacion.MotoristaNoDisponible, "Incapacidad de tres días",
                             Asignacion.Matriz, PoliticaDeDocumentacion.PorDefecto, Momento,
                             new RecursosTomados(VehiculoOriginal, Ulid.NewUlid()), []);

        var reservas = expediente.Diario.Where(t => t.Recursos is not null).ToList();

        // A quién se había asignado: la reserva de `T-08` permanece.
        Assert.Equal(2, reservas.Count);
        Assert.Equal(ConductorOriginal, reservas[0].Recursos!.Conductor);

        // Por qué se cambió, y tipificado.
        Assert.Contains("MotoristaNoDisponible", expediente.Diario[^1].Motivo);
        Assert.Contains("Incapacidad", expediente.Diario[^1].Motivo);
    }

    [Fact]
    public void El_recurso_entrante_se_revalida_entero()
    {
        // «Todas las precondiciones de `T-08` para el recurso entrante». Que la misión ya
        // esté programada no exime al vehículo nuevo: la licencia tiene que habilitarlo a
        // él, no al que salió.
        var expediente = Programada();

        // Licencia que cubre hasta el retorno, pero no la holgura posterior.
        var asignacion = Asignacion.ConLicenciaHasta(Solicitud.Ventana.Retorno);

        var bloqueo = Assert.Throws<BloqueoDuro>(() => expediente.Reasignar(
            Transporte, asignacion, MotivoDeReasignacion.CambioDeRequerimiento, null,
            Asignacion.Matriz, PoliticaDeDocumentacion.PorDefecto, Momento,
            new RecursosTomados(Ulid.NewUlid(), Ulid.NewUlid()), []));

        Assert.Equal("BD-02", bloqueo.Precondicion);
        // Y el recurso original sigue siendo el vigente: no quedó a medias.
        Assert.Equal(VehiculoOriginal, expediente.Diario.Last(t => t.Recursos is not null).Recursos!.Vehiculo);
    }

    [Fact]
    public void Un_recurso_entrante_ya_tomado_por_otra_mision_bloquea()
    {
        // `BD-11` también acá: cambiar de vehículo no es excusa para tomar uno ocupado.
        var expediente = Programada();

        var bloqueo = Assert.Throws<BloqueoDuro>(() => expediente.Reasignar(
            Transporte, Asignacion.Valida(), MotivoDeReasignacion.VehiculoATaller, null,
            Asignacion.Matriz, PoliticaDeDocumentacion.PorDefecto, Momento,
            new RecursosTomados(Ulid.NewUlid(), Ulid.NewUlid()),
            [new ReservaDeRecurso(Ulid.NewUlid(), "PROV-000099", "Delegacion de Danli",
                                  new DateOnly(2026, 3, 21), new DateOnly(2026, 3, 21),
                                  Vehiculo: true, Conductor: false)]));

        Assert.Equal("BD-11", bloqueo.Precondicion);
        Assert.Contains("PROV-000099", bloqueo.Message);
    }

    [Fact]
    public void Reasignar_el_dia_de_la_salida_SI_se_puede()
    {
        // **El caso que decide si `T-10` sirve para algo.** `T-08` bloquea el día de salida
        // —programar entonces ya es tarde—, pero acá sería al revés: la misión ya está
        // programada y a punto de salir, y si el vehículo se avería esa mañana, cambiarlo es
        // la única maniobra que le queda a la institución. Revisar caducidad acá se la
        // quitaría justo el día que la necesita.
        var expediente = Programada();
        var salida = Asignacion.Ventana.Salida;

        expediente.Reasignar(Transporte, Asignacion.Valida(),
                             MotivoDeReasignacion.VehiculoATaller, "Se averió en el predio",
                             Asignacion.Matriz, PoliticaDeDocumentacion.PorDefecto,
                             new DateTimeOffset(salida.ToDateTime(TimeOnly.MinValue),
                                                TimeSpan.FromHours(-6)),
                             new RecursosTomados(Ulid.NewUlid(), ConductorOriginal), []);

        Assert.Equal(EstadoDeMision.Programada, expediente.Estado);
    }

    [Fact]
    public void Una_aprobada_sin_programar_no_se_reasigna()
    {
        // No hay nada que reasignar. Aceptarlo dejaría un `T-10` en el diario sugiriendo que
        // hubo un recurso anterior donde nunca lo hubo.
        var expediente = OrdenDeMision.Crear(Ulid.NewUlid(), Asistente, Jefe, Solicitud, Momento);
        expediente.Enviar(Asistente, Momento);
        expediente.Aprobar(Jefatura, Momento);

        Assert.Throws<TransicionInvalida>(() => expediente.Reasignar(
            Transporte, Asignacion.Valida(), MotivoDeReasignacion.Consolidacion, null,
            Asignacion.Matriz, PoliticaDeDocumentacion.PorDefecto, Momento,
            new RecursosTomados(Ulid.NewUlid(), Ulid.NewUlid()), []));
    }

    [Fact]
    public void Reasignar_sin_motivo_no_se_puede()
    {
        // El motivo tipificado **es** el indicador de fiabilidad de la flota: distingue un
        // vehículo que se avería seguido de uno que se cambió por consolidación. Sin él, un
        // `T-10` es un cambio de vehículo sin razón registrada.
        var expediente = Programada();

        var bloqueo = Assert.Throws<BloqueoDuro>(() => expediente.Reasignar(
            Transporte, Asignacion.Valida(), motivo: null, comentario: "Se cambió y ya",
            Asignacion.Matriz, PoliticaDeDocumentacion.PorDefecto, Momento,
            new RecursosTomados(Ulid.NewUlid(), Ulid.NewUlid()), []));

        Assert.Equal("T-10", bloqueo.Precondicion);
        // Y el recurso original sigue vigente: no quedó a medias.
        Assert.Equal(VehiculoOriginal,
                     expediente.Diario.Last(t => t.Recursos is not null).Recursos!.Vehiculo);
    }

    private static OrdenDeMision Programada()
    {
        var expediente = OrdenDeMision.Crear(Ulid.NewUlid(), Asistente, Jefe, Solicitud, Momento);
        expediente.Enviar(Asistente, Momento);
        expediente.Aprobar(Jefatura, Momento);
        expediente.Programar(Transporte, Asignacion.Valida(), Asignacion.Matriz,
                             PoliticaDeDocumentacion.PorDefecto, Momento,
                             new RecursosTomados(VehiculoOriginal, ConductorOriginal), []);
        return expediente;
    }
}
