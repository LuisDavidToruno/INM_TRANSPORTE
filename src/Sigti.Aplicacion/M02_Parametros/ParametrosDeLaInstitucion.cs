using Sigti.Dominio.M02_Parametros;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M05_Motoristas;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Aplicacion.M02_Parametros;

/// <summary>
/// Resuelve los parámetros normativos vigentes <b>a la fecha del hecho</b>, no a la de
/// captura (`P-4`, `RNF-05`).
/// </summary>
public interface IParametrosDeLaInstitucion
{
    MatrizDeLicencias MatrizVigenteAl(DateOnly fecha);
    PoliticaDeDocumentacion PoliticaVigenteAl(DateOnly fecha);
    CalendarioDeDiasHabiles CalendarioVigenteAl(DateOnly fecha);

    /// <summary>
    /// `RN-32` — <c>estado_minimo_orden_para_emitir_combustible</c>.
    ///
    /// Configurable por institución, con <b>piso en `PROGRAMADA`</b> que la regla impone y el
    /// parámetro no puede bajar: antes de programar no hay vehículo ni motorista contra los
    /// que evaluar al receptor.
    /// </summary>
    EstadoDeMision EstadoMinimoParaEmitirCombustible { get; }

    /// <summary>
    /// `RN-26` — <c>tolerancia_sobregiro</c>, con <b>valor inicial cero</b>.
    ///
    /// La regla es explícita: <i>«Con `tolerancia_sobregiro` en cero —su valor inicial— no hay
    /// excepción»</i>. Que sea configurable no la vuelve opcional: cero significa que el fondo
    /// no se puede exceder en un centavo, y la salida es la ampliación.
    /// </summary>
    decimal ToleranciaDeSobregiro { get; }
}

/// <summary>
/// La matriz licencia↔vehículo del <b>Artículo 4 del Acuerdo 1012-2021</b> `[V]`,
/// La Gaceta No. 35,661 del 19 de julio de 2021.
///
/// Fuente en el repositorio:
/// `docs/01-negocio/normativa/fuentes/acuerdo-1012-2021-permisos-de-conducir.pdf`.
///
/// <b>Sigue siendo provisional en un sentido:</b> los valores ya son normativos, pero
/// están escritos acá en lugar de cargarse por el circuito de `HU-144` con su doble
/// control. Cuando se carguen por ahí, esta clase se borra.
/// </summary>
public sealed class ParametrosProvisionales : IParametrosDeLaInstitucion
{
    private static readonly DateTimeOffset Publicacion =
        new(2021, 7, 19, 0, 0, 0, TimeSpan.FromHours(-6));

    private static readonly DateOnly EnVigencia = new(2021, 7, 19);

    /// <summary>
    /// Las nueve categorías del Artículo 4. Ninguna lleva umbral inventado: donde el
    /// Acuerdo no fija techo, la entrada no lo fija tampoco y el límite real lo pone la
    /// ficha técnica del vehículo.
    /// </summary>
    private static readonly MatrizDeLicencias Matriz = MatrizDeLicencias.Con("ACUERDO-1012-2021-ART-4",
    [
        // TIPO A: ciclomotores y motocicletas, de motor o eléctricas. La norma no fija
        // masa ni pasajeros: la clase es todo el criterio.
        Entrada(CategoriaDeLicencia.A, ClaseNormativa.Motocicleta),

        // TIPO B1: todo tipo de triciclos y cuadriciclos de motor. Igual que A.
        Entrada(CategoriaDeLicencia.B1, ClaseNormativa.TricicloCuadriciclo),

        // TIPO B: livianos, masa máxima autorizada ≤ 3,500 kg, diseñados para no más de
        // ocho (8) personas además del conductor. «No comprendidos en la categoría A y B1».
        Entrada(CategoriaDeLicencia.B, ClaseNormativa.Automovil, kg: 3_500, pasajeros: 8),

        // TIPO BE: automóviles de la categoría B enganchados a un remolque.
        Entrada(CategoriaDeLicencia.BE, ClaseNormativa.Automovil, kg: 3_500, pasajeros: 8, remolque: true),

        // TIPO C1: no comprendidos en B, masa máxima autorizada ≤ 7,500 kg.
        Entrada(CategoriaDeLicencia.C1, ClaseNormativa.Camion, kg: 7_500),

        // TIPO C: vehículos de carga superiores a 7,500 kg, no articulados.
        Entrada(CategoriaDeLicencia.C, ClaseNormativa.Camion),

        // TIPO CE: categoría C enganchada a remolque o semirremolque (cisternas,
        // plataformas, furgones).
        Entrada(CategoriaDeLicencia.CE, ClaseNormativa.Camion, remolque: true),

        // TIPO D1: autobuses hasta 25 pasajeros. TIPO D: superiores a 26.
        Entrada(CategoriaDeLicencia.D1, ClaseNormativa.Autobus, pasajeros: 25),
        Entrada(CategoriaDeLicencia.D, ClaseNormativa.Autobus)
    ]);

    public MatrizDeLicencias MatrizVigenteAl(DateOnly fecha) => Matriz;

    /// <summary>
    /// ⚠️ <b>Calendario provisional, y SUBDECLARA lo inhábil.</b>
    ///
    /// ── Lo que declara ──────────────────────────────────────────────────────
    /// Lunes a viernes hábiles. Es `[C]` —decisión institucional, insumo #1— y no una
    /// afirmación normativa: se pone como omisión razonable para que `BD-04` pueda operar
    /// sobre fines de semana, que es el caso mayoritario.
    ///
    /// ── Lo que NO declara, y es lo grave ────────────────────────────────────
    /// <b>La lista de feriados está vacía.</b> Con esto, una misión que sale el 15 de
    /// septiembre —Día de la Independencia— <b>pasa `BD-04` sin permiso</b>, y ese es
    /// exactamente el día en que un vehículo del Estado circulando sin salvoconducto es más
    /// visible.
    ///
    /// No se cargan porque la máquina de estados lo prohíbe: <i>«nunca se cablean los
    /// feriados. El Art. 339 del Código del Trabajo fija los nacionales, pero existe
    /// legislación posterior sobre los feriados de octubre que no se pudo verificar»</i> —
    /// insumo #14, `[C]`. Inventar el articulado bloquearía misiones legítimas contra fechas
    /// que nadie verificó, y ese error es peor que el que se comete acá.
    ///
    /// <b>Esta clase se borra</b> cuando los feriados se carguen por el circuito de `HU-144`
    /// con su doble control, igual que la matriz.
    /// </summary>
    public CalendarioDeDiasHabiles CalendarioVigenteAl(DateOnly fecha) => CalendarioProvisional;

    private static readonly CalendarioDeDiasHabiles CalendarioProvisional = new(
        Version: "PROVISIONAL-SIN-FERIADOS",
        DiasHabiles: new HashSet<DayOfWeek>
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday,
        },
        // Vacía a propósito. La versión lo dice en su nombre para que aparezca en el diario
        // de cada despacho: un asiento que cita `PROVISIONAL-SIN-FERIADOS` se puede auditar;
        // uno que dice «calendario vigente» a secas, no.
        Feriados: new HashSet<DateOnly>(),

        // ⚠️ NULO, y no un 08:00–17:00 razonable. El horario hábil oficial de la institución
        // es el insumo #1, `[C]`, y esta es la mitad de `BD-04` que decide si una salida a las
        // cinco de la mañana exige salvoconducto. Inventarlo bloquearía --o dejaría pasar--
        // salidas contra una jornada que nadie declaró.
        //
        // El código que lo usa ya distingue «no se sabe» de «todo es hábil», así que el día
        // que la institución lo cargue, la hora empieza a evaluarse sin tocar una línea.
        Horario: null);

    /// <summary>Póliza y revisión apagadas: no son obligatorias por ley vigente (`DP-001, D-13`).</summary>
    public PoliticaDeDocumentacion PoliticaVigenteAl(DateOnly fecha) => PoliticaDeDocumentacion.PorDefecto;

    /// <summary>
    /// El valor inicial que `RN-32` fija tras la corrección `HB1-06`, y que además es su piso:
    /// no se puede configurar más abajo.
    /// </summary>
    public EstadoDeMision EstadoMinimoParaEmitirCombustible => EstadoDeMision.Programada;

    /// <summary>
    /// <b>Cero, que es el valor inicial de `RN-26`</b> — y a diferencia del horario hábil o de
    /// los feriados, éste <b>no</b> es un `[C]` sin resolver: la regla lo declara. Un centavo
    /// sobre el saldo bloquea, y la salida es la ampliación del fondo por el mismo circuito.
    /// </summary>
    public decimal ToleranciaDeSobregiro => 0m;

    /// <summary>
    /// Omitir <c>kg</c> o <c>pasajeros</c> significa <b>que el Acuerdo no fija ese
    /// techo</b>, no que sea infinito por descuido. El límite real lo pone la ficha
    /// técnica del vehículo que se asigne.
    /// </summary>
    private static EntradaDeMatriz Entrada(
        CategoriaDeLicencia categoria,
        ClaseNormativa clase,
        int kg = int.MaxValue,
        int pasajeros = int.MaxValue,
        bool remolque = false) =>
        new(categoria,
            Clase: clase,
            PesoBrutoMaximoKg: kg,
            CapacidadMaximaPasajeros: pasajeros,
            PermiteRemolque: remolque,
            VigenteDesde: EnVigencia,
            VigenteHasta: null,
            RegistradoDesde: Publicacion,
            RegistradoHasta: null);
}
