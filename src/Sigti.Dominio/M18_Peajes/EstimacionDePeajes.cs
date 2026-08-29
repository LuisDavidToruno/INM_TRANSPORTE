using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Reglas;

namespace Sigti.Dominio.M18_Peajes;

/// <summary>
/// Una exoneración — `RN-38`.
///
/// ── El valor por defecto es PAGA ────────────────────────────────────────────
/// `NRM-10` llega a una conclusión marcada `[I]` y explícitamente no verificada: <i>«la
/// exoneración se perfila como funcional (emergencia y rescate), no institucional (por ser del
/// Estado)... <b>Un pickup institucional en misión administrativa PAGA peaje</b>»</i>.
///
/// La suposición contraria —«somos del Estado, no pagamos»— es la más probable y la más
/// costosa: produce estimados en cero, faltante de efectivo en ruta, y un motorista pagando de
/// su bolsillo en Zambrano. Por eso <b>ninguna exoneración se carga por defecto</b> y ninguna se
/// admite como configuración global sin fundamento por vehículo.
///
/// ⚠️ `[C]` <b>insumo #22</b> — la lista oficial de exoneraciones no está publicada en ninguna
/// fuente consultable. `NRM-10` la califica como <i>«lo que decide cómo se construye M-18»</i>.
/// </summary>
/// <param name="Punto">
/// Nulo significa <b>todos los puntos de un operador</b>, que es como se otorgan. No significa
/// «todos los puntos del país»: eso exigiría un acuerdo con cada concesionario.
/// </param>
/// <param name="Fundamento">
/// Obligatorio, con adjunto. Es un acto autorizado y registrado (`RN-03`): una exoneración es
/// una excepción permanente al pago, y exige vigilancia proporcional.
/// </param>
public sealed record ExoneracionDePeaje(
    Ulid Id,
    Ulid Vehiculo,
    Ulid? Punto,
    string? Operador,
    string Fundamento,
    DateOnly VigenteDesde,
    DateOnly? VigenteHasta,
    DateTimeOffset RegistradoDesde,
    DateTimeOffset? RegistradoHasta = null) : IConVigencia
{
    public bool Cubre(PuntoDePeaje punto) =>
        Punto == punto.Id ||
        (Punto is null && Operador is not null &&
         string.Equals(Operador, punto.Operador, StringComparison.OrdinalIgnoreCase));
}

/// <summary>
/// Una fila del desglose — `RN-35` punto 2.
///
/// ── Por qué el desglose y no un total ──────────────────────────────────────
/// `NRM-10`: <i>«presentar el estimado desglosado por punto, no como total opaco. Quien
/// autoriza tiene que poder verificar el cálculo»</i>. Un viaje Tegucigalpa → San Pedro Sula
/// atraviesa las tres estaciones del Corredor Logístico; ida y vuelta son <b>6 cruces</b> `[V]`.
/// Sin desglose, el autorizador no puede distinguir un estimado correcto de uno que duplicó un
/// cruce, y el estimado deja de ser un control para volverse un trámite.
/// </summary>
/// <param name="Cruces">
/// <b>Cruces, no puntos distintos.</b> El conteo es lo que más se equivoca al hacerlo a mano, y
/// es la razón entera del desglose.
/// </param>
/// <param name="Subtotal">
/// Nulo cuando la línea <b>no se pudo valorar</b>. Nunca cero por omisión: un cero
/// indistinguible de un error es peor que la ausencia declarada.
/// </param>
/// <param name="Fundamento">
/// Por qué esta línea vale lo que vale. Va siempre, y en las líneas en cero es obligatorio: un
/// cero sin explicación es indistinguible de un error de cálculo.
/// </param>
public sealed record LineaDeEstimacion(
    Ulid Punto,
    string NombreDelPunto,
    int Cruces,
    CategoriaDePeaje? Categoria,
    BaseDeLaCategoria Base,
    decimal? TarifaUnitaria,
    Ulid? IdDeLaTarifa,
    decimal? Subtotal,
    string Fundamento)
{
    public bool SeValoro => Subtotal is not null;
}

/// <summary>
/// El estimado de peajes de una misión — `RN-35`.
///
/// ── No bloquea la aprobación, y eso es la regla ─────────────────────────────
/// `RN-35`: <i>«si el estimado no se puede calcular, la Orden se puede aprobar igual, con el
/// estimado marcado de forma visible como no disponible y su causa»</i>. El sistema arranca
/// <b>sin tarifas cargadas</b> (insumo #21) y detener toda aprobación por eso pararía la
/// institución por un dato de catálogo.
///
/// Lo que sí bloquea es <b>programar</b> un vehículo sin categoría resuelta — `BD-07`.
/// </summary>
public sealed record Estimacion(
    IReadOnlyList<LineaDeEstimacion> Lineas,
    BaseDeLaCategoria Base,
    bool Provisional)
{
    /// <summary>
    /// Suma sólo lo valorado. <b>Nulo cuando ninguna línea se pudo valorar</b>: un total de cero
    /// sobre líneas no valoradas diría que la misión no cuesta peaje.
    /// </summary>
    public decimal? Total =>
        Lineas.Count == 0
            ? 0m
            : Lineas.All(l => !l.SeValoro)
                ? null
                : Lineas.Sum(l => l.Subtotal ?? 0m);

    /// <summary>
    /// Si alguna línea quedó sin valorar. <b>Se dice aunque el total exista</b>: un total
    /// parcial presentado como completo subestima el costo y produce faltante en ruta.
    /// </summary>
    public bool Parcial => Lineas.Any(l => !l.SeValoro) && Lineas.Any(l => l.SeValoro);

    public bool Disponible => Lineas.Count == 0 || Lineas.Any(l => l.SeValoro);

    /// <summary>Las causas de lo que no se pudo valorar, para mostrarlas juntas.</summary>
    public IReadOnlyList<string> Faltantes =>
        [.. Lineas.Where(l => !l.SeValoro).Select(l => $"{l.NombreDelPunto}: {l.Fundamento}")];
}

/// <summary>
/// Cómo se arma el estimado — `RN-35` y `RN-38`.
/// </summary>
public static class ReglasDeEstimacionDePeajes
{
    /// <summary>
    /// Arma el desglose de una ruta.
    ///
    /// ── El orden de las tres preguntas importa ──────────────────────────────
    /// Primero el <b>estado del punto</b>: un punto cerrado no cobra a nadie, exonerado o no.
    /// Después la <b>exoneración</b>: `RN-38` — no confundirla con el estado del punto, porque
    /// al reactivarse el cobro el sistema seguiría estimando cero. Y al final la <b>tarifa</b>.
    ///
    /// Invertirlas produciría un «no hay tarifa» sobre una caseta cerrada, que es un faltante
    /// falso en el tablero de parámetros.
    /// </summary>
    public static Estimacion Armar(
        IReadOnlyList<CruceDeRuta> ruta,
        CategoriaResuelta categoria,
        IEnumerable<TarifaDePeaje> tarifas,
        IEnumerable<VigenciaDelPunto> estadosDePuntos,
        IEnumerable<ExoneracionDePeaje> exoneraciones,
        Ulid? vehiculo,
        DateOnly fechaPrevista,
        DateTimeOffset conocidoAl)
    {
        var lineas = new List<LineaDeEstimacion>();

        foreach (var cruce in ruta)
        {
            lineas.Add(Valorar(
                cruce, categoria, tarifas, estadosDePuntos, exoneraciones,
                vehiculo, fechaPrevista, conocidoAl));
        }

        return new Estimacion(lineas, categoria.Base, categoria.Provisional);
    }

    private static LineaDeEstimacion Valorar(
        CruceDeRuta cruce,
        CategoriaResuelta categoria,
        IEnumerable<TarifaDePeaje> tarifas,
        IEnumerable<VigenciaDelPunto> estadosDePuntos,
        IEnumerable<ExoneracionDePeaje> exoneraciones,
        Ulid? vehiculo,
        DateOnly fecha,
        DateTimeOffset conocidoAl)
    {
        var punto = cruce.Punto;

        // 1. El estado del punto. Sin vigencia declarada no se supone activo.
        var estado = ReglasDeTarifaDePeaje.EstadoA(estadosDePuntos, punto.Id, fecha, conocidoAl);

        if (estado is null)
            return Sin(cruce, categoria,
                "El punto no tiene estado operativo declarado a esa fecha. Suponerlo activo " +
                "estimaría de más sobre una caseta que quizá cerró; suponerlo cerrado, de " +
                "menos, y eso es un faltante de efectivo en ruta.");

        if (estado.Estado is not EstadoDelPunto.Activo)
            return new LineaDeEstimacion(
                punto.Id, punto.Nombre, cruce.Cruces, categoria.Categoria, categoria.Base,
                TarifaUnitaria: 0m, IdDeLaTarifa: null, Subtotal: 0m,
                $"El punto está {estado.Estado.ToString().ToUpperInvariant()} a esa fecha. " +
                $"{estado.Fundamento}. No es exoneración del vehículo: es estado del punto, y " +
                "cuando se reactive el cobro esta línea vuelve a valer.");

        // 2. La exoneración del vehículo. `RN-38`: distinta del estado del punto.
        if (vehiculo is { } idVehiculo)
        {
            var exonerado = ReglasDeVigencia
                .TodasLasVigentesA(
                    exoneraciones.Where(e => e.Vehiculo == idVehiculo), fecha, conocidoAl)
                .FirstOrDefault(e => e.Cubre(punto));

            if (exonerado is not null)
                return new LineaDeEstimacion(
                    punto.Id, punto.Nombre, cruce.Cruces, categoria.Categoria, categoria.Base,
                    TarifaUnitaria: 0m, IdDeLaTarifa: null, Subtotal: 0m,
                    // El fundamento va en la línea. Un cero sin explicación es indistinguible
                    // de un error de cálculo — `RN-35` punto 3.
                    $"Exonerado: {exonerado.Fundamento}. Vigente desde el " +
                    $"{exonerado.VigenteDesde:dd/MM/yyyy}. El paso se registra igual: `RN-38` " +
                    "punto 5 lo necesita para la coherencia de la secuencia.");
        }

        // 3. La tarifa. Sin categoría no hay contra qué buscarla.
        if (categoria.Categoria is not { } cat)
            return Sin(cruce, categoria, categoria.Explicacion);

        var tarifa = ReglasDeTarifaDePeaje.Resolver(tarifas, punto.Id, cat, fecha, conocidoAl);

        if (tarifa is null)
            return Sin(cruce, categoria,
                $"No hay tarifa vigente para el punto «{punto.Nombre}», categoría " +
                $"«{cat.Nombre}», a la fecha {fecha:dd/MM/yyyy}. Solicite a la Gerencia " +
                "Administrativa que registre la tabla vigente.");

        return new LineaDeEstimacion(
            punto.Id, punto.Nombre, cruce.Cruces, cat, categoria.Base,
            tarifa.Monto, tarifa.Id, tarifa.Monto * cruce.Cruces,
            $"{cruce.Cruces} cruce(s) × {tarifa.Monto:N2}. Tarifa vigente desde el " +
            $"{tarifa.VigenteDesde:dd/MM/yyyy}, fuente {tarifa.Fuente}, verificada el " +
            $"{tarifa.FechaDeVerificacion:dd/MM/yyyy}" +
            (tarifa.SinRevisarHaceMasDeUnAnio(fecha)
                ? " — ⚠️ SIN REVISAR HACE MÁS DE UN AÑO. La tarifa cambia al menos una vez al " +
                  "año, en enero."
                : "."));
    }

    private static LineaDeEstimacion Sin(
        CruceDeRuta cruce, CategoriaResuelta categoria, string porQue) =>
        new(cruce.Punto.Id, cruce.Punto.Nombre, cruce.Cruces, categoria.Categoria,
            categoria.Base, null, null, null, porQue);

    /// <summary>
    /// `RN-35` — <b>la diferencia contra lo autorizado exige reautorización</b>, y es
    /// precondición del despacho.
    ///
    /// Lo autorizado tenía un costo y ese costo cambió: quien autorizó la misión autorizó un
    /// número, no una intención. El umbral es configurable porque una diferencia de dos lempiras
    /// por redondeo de tarifa no es una decisión nueva.
    /// </summary>
    /// <param name="umbral">
    /// Proporción. <b>Nulo es «no configurado» y no bloquea</b>: sin umbral declarado, exigir
    /// reautorización por cualquier diferencia detendría toda misión cuyo estimado se afinó al
    /// asignar el vehículo, que es lo que se espera que pase.
    /// </param>
    public static void ExigirReautorizacionSiSeDesvio(
        decimal? congelado, decimal? recalculado, decimal? umbral, bool hayReautorizacion)
    {
        if (hayReautorizacion) return;
        if (congelado is not { } antes || recalculado is not { } ahora) return;
        if (umbral is not { } tope) return;

        // Un estimado congelado en cero con un recálculo positivo es una desviación infinita en
        // proporción. Se juzga por el hecho —apareció un costo que nadie autorizó— y no por una
        // división que no existe.
        if (antes == 0m)
        {
            if (ahora == 0m) return;

            throw new BloqueoDuro("RN-35",
                $"Lo autorizado no contemplaba peajes y el recálculo da {ahora:N2}. Es un costo " +
                "que nadie autorizó: exige nueva autorización antes de despachar.");
        }

        var desviacion = Math.Abs(ahora - antes) / antes;

        if (desviacion <= tope) return;

        throw new BloqueoDuro("RN-35",
            $"El estimado de peajes pasó de {antes:N2} a {ahora:N2} — {desviacion:P1} de " +
            $"desviación sobre un umbral de {tope:P1}. Lo que se autorizó tenía un costo y ese " +
            "costo cambió: exige nueva autorización antes de despachar (`T-12`).");
    }
}

/// <summary>
/// Un punto de la ruta y cuántas veces se cruza.
///
/// ── Cruces, no puntos ───────────────────────────────────────────────────────
/// Es la distinción que `RN-35` insiste en hacer: <i>«el sistema debe contar cruces, no puntos
/// distintos»</i>. Un paso repetido por la misma caseta en una misión multi-destino es el error
/// más frecuente del cálculo a mano.
/// </summary>
public sealed record CruceDeRuta(PuntoDePeaje Punto, int Cruces);
