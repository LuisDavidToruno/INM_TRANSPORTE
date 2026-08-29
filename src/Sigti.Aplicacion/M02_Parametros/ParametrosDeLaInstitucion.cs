using Sigti.Dominio.M02_Parametros;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M05_Motoristas;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M09_Combustible;

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

    /// <summary>
    /// `RN-30` — <c>rendimiento_esperado</c> del vehículo, vigente a la fecha del hecho.
    ///
    /// <b>Nulo cuando la institución no lo fijó.</b> No hay valor por omisión razonable: un
    /// pick-up y un bus no se parecen en nada, y suponer uno produciría hallazgos falsos que
    /// en tres meses nadie miraría — que es como muere un control.
    /// </summary>
    RendimientoEsperado? RendimientoEsperadoDe(Ulid vehiculo, DateOnly fecha);

    /// <summary>
    /// `RN-30` — <c>umbral_desviacion_rendimiento</c>, superior e inferior, <b>independientes</b>.
    ///
    /// Nulos por la misma razón que el esperado: sin ellos cualquier diferencia sería hallazgo
    /// o ninguna lo sería, y las dos cosas son falsas.
    /// </summary>
    UmbralesDeDesviacion? UmbralesVigentesAl(DateOnly fecha);

    /// <summary>
    /// `RN-86` — <c>plazo_devolucion_saldo</c>, <b>en días hábiles</b>, contado desde la
    /// fecha del hecho del retorno.
    ///
    /// <b>Nulo mientras la institución no lo defina</b> — `[C]`, insumo #32. Y nulo no es
    /// cero: con cero, todo saldo estaría vencido el mismo día del retorno y el bloqueo de
    /// nueva asignación caería sobre la flota entera por un dato que nadie entregó.
    ///
    /// Lo que cuesta no tenerlo lo dice la propia regla: <i>«sin plazo definido, el sistema
    /// no puede decir si el dinero estuvo afuera dos días o dos meses, que es exactamente lo
    /// que el arqueo necesita»</i>. El saldo se ve igual; lo que no se puede es declararlo
    /// vencido.
    /// </summary>
    int? PlazoDeDevolucionDeSaldoEnDiasHabiles { get; }

    /// <summary>
    /// `RN-37` — <c>velocidad_media_maxima_por_tipo_vehiculo</c>, en km/h.
    ///
    /// Es lo que decide si el intervalo entre dos casetas es viable. <b>Nula mientras la
    /// institución no la fije</b>: sin velocidad declarada, cualquier intervalo se podría
    /// llamar imposible y ninguno se podría defender.
    ///
    /// ⚠️ La regla la quiere <b>por tipo de vehículo</b> —un bus y una moto no van igual—, y
    /// esto todavía es una sola cifra. Se dice acá en vez de fingir que ya está resuelto.
    /// </summary>
    int? VelocidadMediaMaximaKmH { get; }
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
    /// ⚠️ <b>NULO, y es la respuesta honesta.</b> `RN-86` lo declara configurable con
    /// vigencia y `HU-078` lo marca `[C]` — insumo #32, junto con la tolerancia de
    /// liquidación.
    ///
    /// Mientras siga nulo, <b>el arqueo muestra quién tiene cuánto y desde cuándo</b> —que es
    /// la primera pregunta y hoy no la contesta nadie— pero <b>no bloquea por saldo</b>,
    /// porque no hay contra qué decir que se venció. La otra mitad del bloqueo, la de las
    /// obligaciones nominadas, funciona sin este parámetro.
    /// </summary>
    public int? PlazoDeDevolucionDeSaldoEnDiasHabiles => null;

    /// <summary>
    /// ⚠️ <b>NULA, y es la respuesta honesta.</b> `RN-37` la declara configurable por tipo de
    /// vehículo y la institución no la ha fijado.
    ///
    /// Mientras siga nula, las otras tres dimensiones de `RN-37` se evalúan igual y el
    /// dictamen <b>dice</b> que la temporal no. Poner un número razonable produciría
    /// intervalos «imposibles» que nadie podría defender contra una cifra que nadie declaró.
    /// </summary>
    public int? VelocidadMediaMaximaKmH => null;

    /// <summary>
    /// ⚠️ <b>NULO, y esa es la respuesta honesta.</b> `RN-30` punto 1 lo declara `[C]`: <i>«la
    /// institución debe fijarlo»</i>, ajustable por tipo de terreno o de ruta.
    ///
    /// ── Por qué no se pone un valor «razonable» ─────────────────────────────
    /// Porque un pick-up y un bus no se parecen en nada, y porque la propia regla advierte lo
    /// que pasa al inventarlo: <i>«el sistema producirá hallazgos falsos y en tres meses nadie
    /// los mirará»</i>. Un control que la gente aprendió a ignorar es peor que uno apagado.
    ///
    /// ── Lo que SÍ hay mientras tanto ────────────────────────────────────────
    /// La <b>propuesta del histórico</b> del propio vehículo, que `RN-30` autoriza
    /// expresamente y que el servicio calcula. Va marcada como propuesta y su origen viaja
    /// hasta el asiento: nadie puede confundirla con el número de la institución.
    /// </summary>
    public RendimientoEsperado? RendimientoEsperadoDe(Ulid vehiculo, DateOnly fecha) => null;

    /// <summary>
    /// Los umbrales asimétricos <b>sí</b> se declaran, y no contradice lo anterior.
    ///
    /// La diferencia es qué tipo de dato es cada uno. El <b>esperado</b> es un hecho sobre un
    /// vehículo concreto que sólo la institución conoce; los <b>umbrales</b> son cuánta
    /// desviación se tolera antes de mirar, y ahí `RN-30` sí fija la forma: <i>«un exceso de
    /// consumo del 20% y un ahorro del 20% no significan lo mismo»</i>.
    ///
    /// ⚠️ <b>Los números siguen siendo `[C]`</b> —insumos #1 y #19—, y por eso la versión lo
    /// dice: un asiento que cite `PROVISIONAL` se puede auditar; uno que diga «umbral vigente»
    /// a secas, no. Se aprieta más arriba que abajo porque un exceso puede ser montaña y un
    /// ahorro imposible casi siempre es un despacho que nadie registró.
    /// </summary>
    public UmbralesDeDesviacion? UmbralesVigentesAl(DateOnly fecha) =>
        new(ToleranciaInferior: 0.25m, ToleranciaSuperior: 0.15m);

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
