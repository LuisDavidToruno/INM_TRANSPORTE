using Sigti.Dominio.M02_Parametros;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M07_ProgramacionYDespacho;

/// <summary>
/// `BD-04` — circular en día inhábil exige permiso de la máxima autoridad.
///
/// ── Por qué el permiso ampara cuatro cosas y no dos ──────────────────────────
/// **Vehículo, motorista, ruta y ventana.** El hallazgo `HB3-07` encontró tres redacciones
/// distintas conviviendo, y se resolvió por la más exigente. La razón no es formal: <i>«el
/// salvoconducto lo lee un agente en carretera que compara el nombre del papel con quien va
/// al volante. Si no coinciden, el documento no sirve para lo único que existe»</i>.
///
/// De ahí sale, sin regla aparte, que **un relevo de motorista invalide el permiso**.
///
/// ── Y por qué la excepción es del vehículo ───────────────────────────────────
/// `HB3-08`: el bloqueo no la contemplaba, de modo que **una ambulancia con excepción
/// vigente no podía despacharse un domingo**. `RN-24` la hace atributo del vehículo con
/// vigencia — si fuera del viaje, <i>«cualquier misión podría autoexceptuarse alegando
/// urgencia, y el control se vaciaría en una semana»</i>.
/// </summary>
public class DiaInhabilPruebas
{
    private static readonly IdPersona Asistente = new("P-ASISTENTE");
    private static readonly IdPersona Jefe = new("P-JEFE");
    private static readonly IdPersona Jefatura = new("P-JEFATURA");
    private static readonly IdPersona Transporte = new("P-TRANSPORTE");
    private static readonly IdPersona Encargado = new("P-ENCARGADO");
    private static readonly IdPersona MaximaAutoridad = new("P-MAXIMA-AUTORIDAD");

    private static readonly Ulid Vehiculo = Ulid.NewUlid();
    private static readonly Ulid Motorista = Ulid.NewUlid();

    /// <summary>Lunes a viernes hábiles, sin feriados — el mismo caso del calendario provisional.</summary>
    private static readonly CalendarioDeDiasHabiles Calendario = new(
        "PRUEBA-01",
        new HashSet<DayOfWeek>
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday,
        },
        new HashSet<DateOnly>());

    /// <summary>
    /// Del <b>lunes 16</b> al <b>miércoles 18</b> de marzo de 2026, sin holgura. No toca fin de
    /// semana: es la ventana que <b>no</b> exige permiso.
    /// </summary>
    private static readonly VentanaDeMision EntreSemana =
        new(new DateOnly(2026, 3, 16), new DateOnly(2026, 3, 18), HolguraDias: 0);

    /// <summary>Del <b>viernes 20</b> al <b>lunes 23</b>: cruza sábado y domingo.</summary>
    private static readonly VentanaDeMision CruzaFinDeSemana =
        new(new DateOnly(2026, 3, 20), new DateOnly(2026, 3, 23), HolguraDias: 0);

    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

    [Fact]
    public void Una_mision_entre_semana_no_exige_permiso_y_deja_dicho_el_calendario()
    {
        // Dentro de dos años, reconstruir por qué esta salida no exigió permiso requiere
        // saber contra qué calendario se juzgó. Un «BD-04 no aplica» a secas no se audita.
        var expediente = Programada(EntreSemana);

        Despachar(expediente, EntreSemana, Sin());

        Assert.Equal(EstadoDeMision.Despachada, expediente.Estado);
        Assert.Contains("BD-04 no aplica", expediente.Diario[^1].Motivo);
        Assert.Contains("PRUEBA-01", expediente.Diario[^1].Motivo);
    }

    [Fact]
    public void Cruzar_el_fin_de_semana_sin_permiso_bloquea_y_dice_que_dias()
    {
        var expediente = Programada(CruzaFinDeSemana);

        var bloqueo = Assert.Throws<BloqueoDuro>(
            () => Despachar(expediente, CruzaFinDeSemana, Sin()));

        Assert.Equal("BD-04", bloqueo.Precondicion);
        // Qué días, no «hay días inhábiles»: quien despacha necesita saber si mover la
        // salida un día resuelve el problema o no.
        Assert.Contains("2026-03-21", bloqueo.Message);
        Assert.Contains("2026-03-22", bloqueo.Message);
        Assert.Contains("No hay ningún permiso registrado", bloqueo.Message);
        Assert.Equal(EstadoDeMision.Programada, expediente.Estado);
    }

    [Fact]
    public void La_holgura_tambien_cuenta_para_lo_inhabil()
    {
        // «Cualquier parte de la ventana de la misión.» Una misión que retorna el viernes
        // con un día de holgura sigue afuera el sábado, y ese es justamente el día que nadie
        // previó. Evaluar sólo hasta el retorno dejaría pasar el caso.
        var conHolgura = new VentanaDeMision(new DateOnly(2026, 3, 18), new DateOnly(2026, 3, 20), 1);
        var expediente = Programada(conHolgura);

        var bloqueo = Assert.Throws<BloqueoDuro>(
            () => Despachar(expediente, conHolgura, Sin()));

        Assert.Equal("BD-04", bloqueo.Precondicion);
        Assert.Contains("2026-03-21", bloqueo.Message);
    }

    [Fact]
    public void Con_permiso_que_ampara_las_cuatro_cosas_se_despacha()
    {
        var expediente = Programada(CruzaFinDeSemana);

        Despachar(expediente, CruzaFinDeSemana, Con(Permiso()));

        Assert.Equal(EstadoDeMision.Despachada, expediente.Estado);
        Assert.Contains("BD-04 verificada", expediente.Diario[^1].Motivo);
        // Qué permiso y quién lo firmó: es lo que se muestra si alguien pregunta.
        Assert.Contains("SC-000001", expediente.Diario[^1].Motivo);
        Assert.Contains("P-MAXIMA-AUTORIDAD", expediente.Diario[^1].Motivo);
    }

    [Fact]
    public void Un_relevo_de_motorista_invalida_el_permiso()
    {
        // **El caso que decide si el permiso sirve.** El agente en carretera compara el
        // nombre del papel con quien va al volante; si no coinciden, el documento no sirve
        // para lo único que existe. Un permiso a nombre de otro no ampara.
        var expediente = Programada(CruzaFinDeSemana);
        var relevo = Ulid.NewUlid();

        var bloqueo = Assert.Throws<BloqueoDuro>(() => expediente.Despachar(
            Encargado, Asignacion.Valida(), Asignacion.Matriz,
            PoliticaDeDocumentacion.PorDefecto, Momento, Asignacion.Custodiado,
            new CirculacionEnDiaInhabil(Calendario, Vehiculo, relevo, null, [Permiso()])));

        Assert.Equal("BD-04", bloqueo.Precondicion);
        // Y el mensaje distingue este caso del de no tener ninguno: son dos arreglos
        // distintos — reemitir el permiso, o pedirlo por primera vez.
        Assert.Contains("ninguno que ampare", bloqueo.Message);
        Assert.Contains("relevo de motorista invalida", bloqueo.Message);
    }

    [Fact]
    public void Un_permiso_que_no_cubre_la_ventana_entera_no_ampara()
    {
        // Tres de los cuatro días no son cuatro. El agente que revise el día que quedó fuera
        // tiene un vehículo del Estado circulando sin respaldo.
        var expediente = Programada(CruzaFinDeSemana);

        var corto = Permiso() with { Hasta = new DateOnly(2026, 3, 22) };

        var bloqueo = Assert.Throws<BloqueoDuro>(
            () => Despachar(expediente, CruzaFinDeSemana, Con(corto)));

        Assert.Equal("BD-04", bloqueo.Precondicion);
    }

    [Fact]
    public void Una_ambulancia_con_excepcion_vigente_se_despacha_un_domingo()
    {
        // **El hallazgo `HB3-08`.** El bloqueo no contemplaba la excepción, y con eso una
        // ambulancia con excepción vigente no podía salir un domingo — que es precisamente
        // cuando hace falta.
        var expediente = Programada(CruzaFinDeSemana);

        var ambulancia = new ServicioExceptuado(
            "Salud", "Acuerdo institucional 04-2025", new DateOnly(2025, 1, 1), null);

        Despachar(expediente, CruzaFinDeSemana, Con(excepcion: ambulancia));

        Assert.Equal(EstadoDeMision.Despachada, expediente.Estado);
        // **El uso de la excepción queda registrado**, que es lo que `BD-04` exige. Una
        // excepción que se usa sin dejar rastro es una excepción que nadie puede auditar.
        Assert.Contains("BD-04 exceptuada", expediente.Diario[^1].Motivo);
        Assert.Contains("Salud", expediente.Diario[^1].Motivo);
    }

    [Fact]
    public void Una_excepcion_vencida_no_exceptua()
    {
        // El recíproco. `RN-24` exige rango de vigencia explícito justamente para esto: una
        // excepción sin caducidad es una excepción permanente que nadie revisa.
        var expediente = Programada(CruzaFinDeSemana);

        var vencida = new ServicioExceptuado(
            "Salud", "Acuerdo institucional 04-2020",
            new DateOnly(2020, 1, 1), new DateOnly(2024, 12, 31));

        var bloqueo = Assert.Throws<BloqueoDuro>(
            () => Despachar(expediente, CruzaFinDeSemana, Con(excepcion: vencida)));

        Assert.Equal("BD-04", bloqueo.Precondicion);
    }

    [Fact]
    public void Un_feriado_entre_semana_tambien_exige_permiso()
    {
        // El calendario no es sólo el fin de semana. Este calendario de prueba SÍ lleva
        // feriados — el provisional del sistema no, y por eso subdeclara.
        var conFeriado = new CalendarioDeDiasHabiles(
            "PRUEBA-CON-FERIADO", Calendario.DiasHabiles,
            new HashSet<DateOnly> { new(2026, 3, 17) });

        var expediente = Programada(EntreSemana);

        var bloqueo = Assert.Throws<BloqueoDuro>(() => expediente.Despachar(
            Encargado, Asignacion.Valida(), Asignacion.Matriz,
            PoliticaDeDocumentacion.PorDefecto, Momento, Asignacion.Custodiado,
            new CirculacionEnDiaInhabil(conFeriado, Vehiculo, Motorista, null, [])));

        Assert.Equal("BD-04", bloqueo.Precondicion);
        Assert.Contains("2026-03-17", bloqueo.Message);
    }

    private static PermisoDeCirculacion Permiso() => new(
        "SC-000001", MaximaAutoridad, Vehiculo, Motorista, "Choluteca",
        new DateOnly(2026, 3, 20), new DateOnly(2026, 3, 23));

    private static CirculacionEnDiaInhabil Sin() =>
        new(Calendario, Vehiculo, Motorista, null, []);

    private static CirculacionEnDiaInhabil Con(
        PermisoDeCirculacion? permiso = null,
        ServicioExceptuado? excepcion = null) =>
        new(Calendario, Vehiculo, Motorista, excepcion, permiso is null ? [] : [permiso]);

    private static void Despachar(
        OrdenDeMision expediente, VentanaDeMision _, CirculacionEnDiaInhabil circulacion) =>
        expediente.Despachar(Encargado, Asignacion.Valida(), Asignacion.Matriz,
                             PoliticaDeDocumentacion.PorDefecto, Momento,
                             Asignacion.Custodiado, circulacion);

    private static OrdenDeMision Programada(VentanaDeMision ventana)
    {
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
