using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M08_Bitacora;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M07_ProgramacionYDespacho;

/// <summary>
/// `BD-05` — coherencia del odómetro, en `T-14` y `T-18`.
///
/// ── Por qué esto importa más que una validación de rango ─────────────────────
/// <i>«El hallazgo típico del TSC en flota es el incremento de consumo de combustible sin
/// relación con el uso habitual, y el odómetro es <b>el único ancla</b> que tiene el
/// sistema para detectarlo.»</i> Sin lectura no hay conciliación galonaje–kilometraje, y sin
/// conciliación el combustible no se controla.
///
/// ── La excepción a `P-2`, y por qué la autoridad la puso ─────────────────────
/// `P-2` dice que los hechos consumados se registran, no se bloquean. `BD-05` bloquea igual
/// en el `T-18` <b>ordinario</b>, y con razón: una lectura de retorno menor que la de salida
/// no es un hecho, es <b>un número mal tecleado</b>, y hay alguien con el tablero delante
/// que puede corregirlo.
///
/// En el subtipo <b>constatado</b> no bloquea — hallazgo `HB3-04`: ahí el vehículo ya está en
/// el predio y negarse a registrarlo <b>lo deja secuestrado por un trámite</b> mientras la
/// delegación se queda sin unidad.
/// </summary>
public class OdometroPruebas
{
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

    private static readonly IdPersona Asistente = new("P-ASISTENTE");
    private static readonly IdPersona Jefe = new("P-JEFE");
    private static readonly IdPersona Jefatura = new("P-JEFATURA");
    private static readonly IdPersona Transporte = new("P-TRANSPORTE");
    private static readonly IdPersona Encargado = new("P-ENCARGADO");
    private static readonly IdPersona Motorista = new("P-MOTORISTA");

    private static readonly DatosDeLaSolicitud Solicitud = new(
        "Delegacion de Choluteca", "Traslado de personal", "Choluteca", Asignacion.Ventana);

    [Fact]
    public void Salir_con_menos_kilometros_que_la_ultima_lectura_conocida_bloquea()
    {
        // O es error de digitación, o es retroceso de odómetro. Las dos se corrigen en el
        // momento, con el tablero delante — por eso bloquea en vez de marcar.
        var expediente = Despachada();

        var bloqueo = Assert.Throws<BloqueoDuro>(() => expediente.IniciarRuta(
            Motorista, Momento, new OdometroAlSalir(9_800, UltimaConocida: 10_000)));

        Assert.Equal("BD-05", bloqueo.Precondicion);
        // Los dos números en el mensaje: sin ellos, quien captura no sabe si se equivocó en un
        // dígito o si el vehículo trae un odómetro distinto.
        Assert.Contains("9,800", bloqueo.Message);
        Assert.Contains("10,000", bloqueo.Message);
        Assert.Equal(EstadoDeMision.Despachada, expediente.Estado);
    }

    [Fact]
    public void La_primera_mision_de_un_vehiculo_no_tiene_contra_que_comparar_y_lo_dice()
    {
        // «Sin lectura previa» y «no se verificó» son cosas distintas, y el diario tiene que
        // distinguirlas: la primera es normal, la segunda es un control que no ocurrió.
        var expediente = Despachada();

        expediente.IniciarRuta(Motorista, Momento, new OdometroAlSalir(10_000, UltimaConocida: null));

        Assert.Equal(EstadoDeMision.EnRuta, expediente.Estado);
        Assert.Contains("sin lectura previa registrada", expediente.Diario[^1].Motivo);
    }

    [Fact]
    public void Volver_con_menos_kilometros_que_al_salir_bloquea_en_el_T18_ordinario()
    {
        // Físicamente imposible. Y hay alguien con el vehículo delante: se corrige.
        var expediente = EnRuta(10_000);

        var bloqueo = Assert.Throws<BloqueoDuro>(() => expediente.Retornar(
            Motorista, Momento, new OdometroAlRetornar(9_500, SubtipoDeRetorno.Ordinario)));

        Assert.Equal("BD-05", bloqueo.Precondicion);
        // Y nombra la salida: el acta de sustitución de `M-11`.
        Assert.Contains("acta", bloqueo.Message);
        Assert.Equal(EstadoDeMision.EnRuta, expediente.Estado);
    }

    [Fact]
    public void En_el_retorno_CONSTATADO_no_bloquea_y_el_vehiculo_se_libera_igual()
    {
        // **El hallazgo `HB3-04`.** Un tercero verifica que el vehículo volvió sin que el
        // motorista haya podido cerrar la bitácora. Bloquear no arregla nada: el vehículo ya
        // está en el predio, y negarse a registrarlo lo deja secuestrado por un trámite.
        var expediente = EnRuta(10_000);

        expediente.Retornar(Motorista, Momento,
                            new OdometroAlRetornar(9_500, SubtipoDeRetorno.Constatado));

        Assert.Equal(EstadoDeMision.Retornada, expediente.Estado);
        // Se registra tal cual **y se marca**: liberar el vehículo no es dar por buena la
        // lectura, y el asiento tiene que dejar el rastro para la liquidación.
        Assert.Contains("INCONSISTENTE", expediente.Diario[^1].Motivo);
        Assert.Contains("RN-79", expediente.Diario[^1].Motivo);
    }

    [Fact]
    public void Volver_con_el_mismo_odometro_exige_justificacion()
    {
        // No bloquea por imposible —es posible—, pero no pasa en silencio: es el patrón de la
        // misión que nunca se hizo, y ése es justamente el que busca el TSC.
        var expediente = EnRuta(10_000);

        var bloqueo = Assert.Throws<BloqueoDuro>(() => expediente.Retornar(
            Motorista, Momento, new OdometroAlRetornar(10_000, SubtipoDeRetorno.Ordinario)));

        Assert.Equal("BD-05", bloqueo.Precondicion);
        Assert.Contains("nunca se hizo", bloqueo.Message);
    }

    [Fact]
    public void Con_justificacion_el_recorrido_cero_SI_se_registra()
    {
        // El recíproco: lo que se exige es la explicación, no que el kilometraje sea distinto
        // de cero. Una misión que se suspendió con el vehículo ya entregado es un hecho real.
        var expediente = EnRuta(10_000);

        expediente.Retornar(Motorista, Momento, new OdometroAlRetornar(
            10_000, SubtipoDeRetorno.Ordinario,
            Justificacion: "La comisión se suspendió con el vehículo ya entregado."));

        Assert.Equal(EstadoDeMision.Retornada, expediente.Estado);
        Assert.Contains("sin recorrido", expediente.Diario[^1].Motivo);
        Assert.Contains("se suspendió", expediente.Diario[^1].Motivo);
    }

    [Fact]
    public void El_acta_de_sustitucion_hace_la_comparacion_no_aplicable()
    {
        // **No es un permiso para saltarse la validación: es un hecho mecánico.** El odómetro
        // se cambió, el instalado arranca donde arranque, y comparar contra la lectura del
        // retirado no significa nada.
        var expediente = EnRuta(10_000);

        var acta = new ActaDeSustitucionDeOdometro(
            "ACT-2026-0031", new DateOnly(2026, 3, 13), LecturaDelRetirado: 10_120, LecturaDelInstalado: 0);

        expediente.Retornar(Motorista, Momento,
                            new OdometroAlRetornar(340, SubtipoDeRetorno.Ordinario, Acta: acta));

        Assert.Equal(EstadoDeMision.Retornada, expediente.Estado);
        Assert.Contains("no comparable", expediente.Diario[^1].Motivo);
        // Las dos lecturas van al asiento: es lo que permite recomponer el kilometraje
        // acumulado sumando tramos.
        Assert.Contains("10,120", expediente.Diario[^1].Motivo);
        Assert.Contains("ACT-2026-0031", expediente.Diario[^1].Motivo);
    }

    [Fact]
    public void Un_recorrido_normal_deja_el_kilometraje_en_el_diario()
    {
        var expediente = EnRuta(10_000);

        expediente.Retornar(Motorista, Momento, new OdometroAlRetornar(10_450, SubtipoDeRetorno.Ordinario));

        Assert.Contains("recorrido 450 km", expediente.Diario[^1].Motivo);
    }

    private static OrdenDeMision EnRuta(int odometroDeSalida)
    {
        var expediente = Despachada();
        expediente.IniciarRuta(Motorista, Momento, new OdometroAlSalir(odometroDeSalida, null));
        return expediente;
    }

    private static OrdenDeMision Despachada()
    {
        var expediente = OrdenDeMision.Crear(Ulid.NewUlid(), Asistente, Jefe, Solicitud, Momento);
        expediente.Enviar(Asistente, Momento);
        expediente.Aprobar(Jefatura, Momento);
        expediente.Programar(Transporte, Asignacion.Valida(), Asignacion.Matriz,
                             PoliticaDeDocumentacion.PorDefecto, Momento,
                             new RecursosTomados(Ulid.NewUlid(), Ulid.NewUlid()), []);
        expediente.Despachar(Encargado, Asignacion.Valida(), Asignacion.Matriz,
                             PoliticaDeDocumentacion.PorDefecto, Momento,
                             Asignacion.Custodiado, Asignacion.SinDiasInhabiles());
        return expediente;
    }
}
