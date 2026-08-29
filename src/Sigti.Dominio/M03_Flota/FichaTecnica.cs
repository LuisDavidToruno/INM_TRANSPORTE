namespace Sigti.Dominio.M03_Flota;

/// <summary>
/// La clase de vehículo del <b>Artículo 4 del Acuerdo 1012-2021</b> `[V]`. Conjunto
/// cerrado, y no ampliable por configuración: sale de la norma.
///
/// <b>No confundir con el tipo de vehículo del catálogo de `M-02`</b>, que es texto libre
/// de cada institución —«pick-up», «microbús», «cisterna»— y por eso no sirve para
/// resolver la matriz. `BD-02` es explícito en que la matriz no se resuelve por nombre
/// del tipo de vehículo.
///
/// Existe porque el Artículo 4 define `A` y `B1` <b>por clase</b> y no por umbral
/// numérico: con masa, pasajeros y remolque no se distingue una motocicleta de un
/// automóvil liviano, y sin esta distinción una licencia `A` no habilitaba nada.
/// </summary>
public enum ClaseNormativa
{
    /// <summary>Ciclomotores y motocicletas, de motor o eléctricas. Categoría `A`.</summary>
    Motocicleta,

    /// <summary>Triciclos y cuadriciclos de motor —mototaxi, cuatrimoto—. Categoría `B1`.</summary>
    TricicloCuadriciclo,

    /// <summary>Automóviles livianos no comprendidos en `A` ni `B1`. Categorías `B` y `BE`.</summary>
    Automovil,

    /// <summary>Vehículos de carga no articulados (camiones). Categorías `C1`, `C` y `CE`.</summary>
    Camion,

    /// <summary>Vehículos de transporte de pasajeros (autobuses). Categorías `D1` y `D`.</summary>
    Autobus
}

/// <summary>
/// Los atributos del vehículo contra los que se resuelve la matriz licencia↔vehículo.
///
/// `BD-02` es explícito en que la matriz <b>no se resuelve por número de ejes ni por
/// nombre del tipo de vehículo</b>, sino por estos atributos. El nombre del tipo se
/// conserva porque es el eje de compatibilidad de `BD-07`, que es otra pregunta.
/// </summary>
/// <param name="PesoBrutoKg">Peso bruto vehicular en kilogramos.</param>
/// <param name="LlevaRemolque">
/// Si la configuración va <b>enganchada a un remolque o acoplada a un semirremolque</b>.
///
/// Es el eje que separa `B` de `BE` y `C` de `CE` en el Artículo 4 del Acuerdo 1012-2021,
/// y <b>no es lo mismo que «articulado»</b>: un pick-up de 2,800 kg con una plataforma
/// enganchada requiere `BE` y no es articulado en ningún sentido. Confundirlos deja pasar
/// exactamente el caso que `BD-02` existe para impedir.
/// </param>
/// <param name="TipoDeVehiculo">
/// El tipo del catálogo institucional —«PICKUP», «MICROBÚS»—. Es el eje de compatibilidad
/// de `BD-07`, que es otra pregunta: <b>la matriz de licencias no lo usa</b>.
/// </param>
/// <param name="CapacidadDeTanqueGalones">
/// Cuánto le cabe al tanque. <b>Es dato del fabricante</b>, no de la institución, y por eso
/// vive en la ficha técnica y no en los parámetros.
///
/// ── Para qué hace falta ─────────────────────────────────────────────────
/// Para convertir la lectura del indicador en galones. Un octavo de tanque <b>no es una
/// cantidad</b> hasta saber de qué tanque, y sin la conversión el remanente de `RN-83` no se
/// puede separar del consumo de la misión.
///
/// ⚠️ <b>Nula cuando no está cargada</b>, y entonces las lecturas en fracción no se
/// convierten. Suponer una capacidad produciría un remanente que entra directo al
/// denominador del rendimiento y que después nadie distinguiría de uno medido.
/// </param>
public sealed record FichaTecnica(
    string TipoDeVehiculo,
    ClaseNormativa Clase,
    int PesoBrutoKg,
    int CapacidadPasajeros,
    bool LlevaRemolque,
    decimal? CapacidadDeTanqueGalones = null);

/// <summary>
/// La ventana de la misión. `BD-02` exige vigencia <b>durante todo el rango, incluida la
/// holgura posterior</b> — no basta que la licencia esté vigente el día de salida.
///
/// ── Por qué las horas son anulables ─────────────────────────────────────────
/// Porque hay expedientes creados antes de que existieran, y <b>fabricarles una hora sería
/// inventar el dato</b>: un «08:00» por omisión se ve idéntico a uno declarado, y sobre él
/// se juzgaría `BD-04` y se ordenaría el tablero del despachador.
///
/// La exigencia vive donde entra el dato nuevo —`POST /misiones` las pide—, no en el tipo.
/// Así lo viejo se lee como lo que es, <b>«no declarada»</b>, y lo nuevo está completo.
/// </summary>
/// <param name="HoraDeSalida">
/// A qué hora sale. La necesitan dos cosas que llegaron por caminos distintos: `BD-04`,
/// para juzgar la <b>hora</b> inhábil, y `PT-038`, para ordenar el día del despachador — la
/// ráfaga de las 5:30 con ocho salidas encimadas.
/// </param>
public sealed record VentanaDeMision(
    DateOnly Salida,
    DateOnly Retorno,
    int HolguraDias,
    TimeOnly? HoraDeSalida = null,
    TimeOnly? HoraDeRetorno = null)
{
    /// <summary>El último día en que el motorista podría estar conduciendo.</summary>
    public DateOnly FinDelRango => Retorno.AddDays(HolguraDias);

    /// <summary>
    /// ¿Declaró la misión sus horas? <b>Las dos o ninguna</b>: media ventana con hora es peor
    /// que ninguna, porque parece completa.
    /// </summary>
    public bool DeclaraHoras => HoraDeSalida is not null && HoraDeRetorno is not null;
}
