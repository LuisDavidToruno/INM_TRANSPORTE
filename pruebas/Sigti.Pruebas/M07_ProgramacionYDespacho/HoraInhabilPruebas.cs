using Sigti.Dominio.M11_Mantenimiento;
using Sigti.Dominio.M02_Parametros;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M07_ProgramacionYDespacho;

/// <summary>
/// La <b>hora</b> inhábil de `BD-04` — la mitad que faltaba.
///
/// ── Hacen falta los dos lados ────────────────────────────────────────────────
/// Que la <b>misión</b> declare a qué hora sale y vuelve, y que la <b>institución</b> declare
/// su horario hábil. Falta cualquiera de los dos y la hora no se juzga — y el diario dice
/// <b>cuál</b> faltó, en vez de dejar creer que se verificó.
///
/// Hoy falta el segundo: el horario oficial es el insumo #1, `[C]`, y el calendario
/// provisional lo lleva en nulo a propósito.
///
/// ── Sólo los dos extremos, y es una decisión acotada ─────────────────────────
/// Se evalúan la hora de salida y la de retorno, <b>no las noches intermedias</b>. Una misión
/// de cuatro días está fuera del horario todas sus madrugadas, y evaluarlas haría que toda
/// misión de más de un día exigiera permiso — con lo cual la mitad del <i>día</i> de `BD-04`
/// quedaría sin sentido. `[C]` si la institución entiende otra cosa.
/// </summary>
public class HoraInhabilPruebas
{
    private static readonly IdPersona Asistente = new("P-ASISTENTE");
    private static readonly IdPersona Jefe = new("P-JEFE");
    private static readonly IdPersona Jefatura = new("P-JEFATURA");
    private static readonly IdPersona Transporte = new("P-TRANSPORTE");
    private static readonly IdPersona Encargado = new("P-ENCARGADO");
    private static readonly IdPersona MaximaAutoridad = new("P-MAXIMA-AUTORIDAD");

    private static readonly Ulid Vehiculo = Ulid.NewUlid();
    private static readonly Ulid Motorista = Ulid.NewUlid();

    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

    private static readonly HashSet<DayOfWeek> DeLunesAViernes =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
        DayOfWeek.Thursday, DayOfWeek.Friday,
    ];

    /// <summary>Con jornada declarada: de 8 de la mañana a 5 de la tarde.</summary>
    private static CalendarioDeDiasHabiles ConJornada() =>
        new("PRUEBA-CON-JORNADA", DeLunesAViernes, new HashSet<DateOnly>(),
            new HorarioHabil(new TimeOnly(8, 0), new TimeOnly(17, 0)));

    /// <summary>Sin jornada declarada — el estado real del sistema hoy.</summary>
    private static CalendarioDeDiasHabiles SinJornada() =>
        new("PRUEBA-SIN-JORNADA", DeLunesAViernes, new HashSet<DateOnly>());

    [Fact]
    public void Salir_antes_de_la_jornada_en_dia_habil_exige_permiso()
    {
        // **El caso que motivó agregar la hora.** Lunes a miércoles: ningún día inhábil. Pero
        // sale a las 5:30 de la mañana, que es la ráfaga del despachador y está fuera de la
        // jornada. Antes de esto, `BD-04` la dejaba pasar sin salvoconducto.
        var expediente = Programada(new TimeOnly(5, 30), new TimeOnly(16, 0));

        var bloqueo = Assert.Throws<BloqueoDuro>(() => Despachar(expediente, ConJornada(), null));

        Assert.Equal("BD-04", bloqueo.Precondicion);
        Assert.Contains("fuera del horario hábil", bloqueo.Message);
        Assert.Contains("salida 05:30", bloqueo.Message);
        // Y no menciona días inhábiles, porque no los hay: el motivo tiene que ser el real.
        Assert.DoesNotContain("días inhábiles", bloqueo.Message);
    }

    [Fact]
    public void Volver_despues_de_la_jornada_tambien()
    {
        var expediente = Programada(new TimeOnly(8, 0), new TimeOnly(21, 45));

        var bloqueo = Assert.Throws<BloqueoDuro>(() => Despachar(expediente, ConJornada(), null));

        Assert.Contains("retorno 21:45", bloqueo.Message);
    }

    [Fact]
    public void Dentro_de_la_jornada_y_en_dia_habil_no_exige_nada()
    {
        // El recíproco. Sin él, la evaluación podría estar bloqueando toda hora declarada y
        // las otras pruebas seguirían en verde.
        var expediente = Programada(new TimeOnly(8, 0), new TimeOnly(16, 30));

        Despachar(expediente, ConJornada(), null);

        Assert.Equal(EstadoDeMision.Despachada, expediente.Estado);
        Assert.Contains("BD-04 no aplica", expediente.Diario[^1].Motivo);
        // Y NO dice que algo quedó sin evaluar: acá se miraron las dos mitades.
        Assert.DoesNotContain("NO evaluada", expediente.Diario[^1].Motivo!);
    }

    [Fact]
    public void Salir_exactamente_a_la_hora_de_apertura_es_estar_en_horario()
    {
        // Los extremos son inclusivos, como toda vigencia del sistema. La alternativa —abrir
        // a las 08:00:01— no la entiende nadie que lea el mensaje del bloqueo.
        var expediente = Programada(new TimeOnly(8, 0), new TimeOnly(17, 0));

        Despachar(expediente, ConJornada(), null);

        Assert.Equal(EstadoDeMision.Despachada, expediente.Estado);
    }

    [Fact]
    public void Sin_horario_declarado_la_hora_NO_se_evalua_y_el_diario_lo_dice()
    {
        // **El estado real del sistema hoy.** La misión declara 5:30, pero la institución no
        // declaró su jornada: no hay contra qué contrastar.
        //
        // Y el asiento lo dice. Un «BD-04 no aplica» a secas es indistinguible de uno que
        // verificó las dos mitades, y dentro de dos años nadie podría saber cuál fue.
        var expediente = Programada(new TimeOnly(5, 30), new TimeOnly(16, 0));

        Despachar(expediente, SinJornada(), null);

        Assert.Equal(EstadoDeMision.Despachada, expediente.Estado);
        Assert.Contains("hora NO evaluada", expediente.Diario[^1].Motivo);
        Assert.Contains("no declaró horario hábil", expediente.Diario[^1].Motivo);
    }

    [Fact]
    public void Sin_horas_en_la_mision_tampoco_y_el_diario_distingue_cual_falto()
    {
        // Los expedientes creados antes de que el campo existiera. La causa es otra —falta el
        // dato de la misión, no el de la institución— y el asiento tiene que distinguirlas:
        // una se arregla cargando un parámetro, la otra no se arregla.
        var expediente = Programada(horaDeSalida: null, horaDeRetorno: null);

        Despachar(expediente, ConJornada(), null);

        Assert.Contains("la misión no declara horas", expediente.Diario[^1].Motivo);
    }

    [Fact]
    public void Con_permiso_de_la_maxima_autoridad_la_hora_inhabil_se_despacha()
    {
        // La salida existe y es la misma que para el día inhábil: el salvoconducto.
        var expediente = Programada(new TimeOnly(5, 30), new TimeOnly(16, 0));

        var permiso = new PermisoDeCirculacion(
            "SC-000077", MaximaAutoridad, Vehiculo, Motorista, "Choluteca",
            new DateOnly(2026, 3, 16), new DateOnly(2026, 3, 18));

        Despachar(expediente, ConJornada(), permiso);

        Assert.Equal(EstadoDeMision.Despachada, expediente.Estado);
        Assert.Contains("BD-04 verificada", expediente.Diario[^1].Motivo);
        Assert.Contains("SC-000077", expediente.Diario[^1].Motivo);
    }

    [Fact]
    public void Una_mision_de_varios_dias_NO_exige_permiso_por_sus_noches()
    {
        // **La decisión acotada, ejercida.** Del lunes al miércoles, saliendo y volviendo en
        // horario. Las dos noches intermedias están fuera de la jornada, y si se evaluaran,
        // toda misión de más de un día exigiría salvoconducto — con lo cual el control dejaría
        // de distinguir nada.
        var expediente = Programada(new TimeOnly(9, 0), new TimeOnly(15, 0));

        Despachar(expediente, ConJornada(), null);

        Assert.Equal(EstadoDeMision.Despachada, expediente.Estado);
        Assert.Contains("BD-04 no aplica", expediente.Diario[^1].Motivo);
    }

    private static void Despachar(
        OrdenDeMision expediente,
        CalendarioDeDiasHabiles calendario,
        PermisoDeCirculacion? permiso) =>
        expediente.Despachar(Encargado, Asignacion.Valida(), Asignacion.Matriz,
                             PoliticaDeDocumentacion.PorDefecto, Momento,
                             Asignacion.Custodiado,
                             new CirculacionEnDiaInhabil(
                                 calendario, Vehiculo, Motorista, null,
                                 permiso is null ? [] : [permiso]),
                             ConflictoPorIndisponibilidad.Ninguno);

    /// <summary>Del lunes 16 al miércoles 18 de marzo de 2026 — sin ningún día inhábil.</summary>
    private static OrdenDeMision Programada(TimeOnly? horaDeSalida, TimeOnly? horaDeRetorno)
    {
        var ventana = new VentanaDeMision(
            new DateOnly(2026, 3, 16), new DateOnly(2026, 3, 18), HolguraDias: 0,
            horaDeSalida, horaDeRetorno);

        var solicitud = new DatosDeLaSolicitud(
            "Delegacion de Choluteca", "Traslado de personal", "Choluteca", ventana);

        var expediente = OrdenDeMision.Crear(Ulid.NewUlid(), Asistente, Jefe, solicitud, Momento);
        expediente.Enviar(Asistente, Momento);
        expediente.Aprobar(Jefatura, Momento);
        expediente.Programar(Transporte, Asignacion.Valida(), Asignacion.Matriz,
                             PoliticaDeDocumentacion.PorDefecto, Momento,
                             new RecursosTomados(Vehiculo, Motorista), []);
        return expediente;
    }
}
