using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M09_Combustible;

/// <summary>
/// De dónde salió el combustible que entró al tanque — `RN-83`.
///
/// ── Por qué es un enum y no una tabla, si la regla lo llama configurable ────
/// Porque <b>el comportamiento cambia por fuente</b>, y un catálogo puro no puede llevar
/// comportamiento: lo del fondo entra al cuadre de `RN-29`, lo del peculio genera obligación de
/// reintegro, y lo demás no toca el fondo pero sí el denominador de `RN-30`. Un valor nuevo
/// cargado por pantalla no sabría a cuál de esos tres grupos pertenece.
///
/// Lo configurable es <b>cuáles de éstas usa la institución</b>. Añadir una séptima es un cambio
/// de código, y queda dicho acá en vez de prometer una configuración que no existe.
/// </summary>
public enum FuenteDeAbastecimiento
{
    /// <summary>
    /// El vale de la misión. <b>La única que entra al cuadre del fondo</b> (`RN-29`).
    /// </summary>
    FondoDeLaMision,

    /// <summary>
    /// El tanque de la sede. <b>No pasa por ningún folio</b>, y por eso hasta hoy no existía
    /// para el sistema — que es exactamente lo que produce un rendimiento imposiblemente bueno:
    /// el vehículo recorrió 900 km con 20 galones registrados porque los otros 40 salieron de
    /// acá.
    /// </summary>
    TanqueInstitucional,

    OtraDependencia,

    /// <summary>Sin monto si no lo hay. <b>Un galón sin precio sigue siendo un galón.</b></summary>
    Donacion,

    /// <summary>
    /// Lo pagó el servidor de su bolsillo. Genera <b>obligación de reintegro</b> a su favor
    /// (`RN-86`) y <b>no afecta el cuadre del fondo</b> — que de otro modo mentiría en los dos
    /// lados a la vez.
    ///
    /// `[C]` <b>Si la institución reintegra no está confirmado</b> — insumo #37. Mientras no se
    /// decida, el abastecimiento se registra y el reintegro queda pendiente sin acto: la
    /// práctica ocurre igual, y hoy quedaría fuera de todo registro.
    /// </summary>
    PeculioDelServidor,

    TerceroEnApoyo,
}

/// <summary>
/// La escala con que se leyó el tanque. <b>Se registra</b> — `RN-83` punto 2: <i>«un octavo de
/// tanque no es lo mismo en un pickup que en un bus»</i>.
/// </summary>
public enum EscalaDeNivel
{
    /// <summary>Fracción del indicador: octavos, cuartos. Lo que la aguja permite leer.</summary>
    FraccionDelIndicador,

    /// <summary>Galones, cuando el instrumento los da.</summary>
    Galones,
}

/// <summary>
/// El nivel del tanque a la salida o al retorno — <b>dato obligatorio de bitácora</b> por
/// `RN-83`.
///
/// ── Por qué hace falta, y qué se cae sin él ─────────────────────────────────
/// Sin nivel, <i>«salió lleno y volvió vacío»</i> no se distingue de un faltante, y la
/// conciliación de una misión corta con tanque grande no significa nada. `RN-30` lo menciona
/// como caso límite y lo atribuía a `RN-22`, que trata de custodia: <b>ninguna regla lo
/// obligaba</b>.
/// </summary>
/// <param name="Valor">
/// En la escala declarada. Con <c>FraccionDelIndicador</c> va de 0 a 1 — <c>0.125m</c> es un
/// octavo.
/// </param>
public sealed record NivelDeTanque(EscalaDeNivel Escala, decimal Valor)
{
    /// <summary>
    /// Cuánto se movió el nivel entre dos lecturas, cuando <b>las dos usan la misma escala</b>.
    ///
    /// Nulo cuando no se pueden comparar: un octavo de indicador y quince galones no se restan,
    /// y convertir uno en otro exige la capacidad del tanque, que la ficha no declara.
    /// </summary>
    public decimal? DiferenciaCon(NivelDeTanque otro) =>
        Escala == otro.Escala ? otro.Valor - Valor : null;

    /// <summary>
    /// Si el tanque volvió a un nivel <b>muy distinto</b> del que salió — lo que vuelve no
    /// concluyente la conciliación de `RN-30`.
    ///
    /// ⚠️ <b>El umbral es una decisión de esta implementación, no de la norma.</b> `RN-83`
    /// habla de un nivel <i>«muy distinto al inicial»</i> sin fijar cuánto, y la institución no
    /// lo ha declarado — queda `[C]`. Un cuarto es lo que una aguja permite leer sin discutir.
    ///
    /// ── Y devuelve nulo cuando no se puede saber ────────────────────────────
    /// Escalas distintas no se restan sin la capacidad del tanque, que la ficha técnica no
    /// declara. <b>Nulo es «no se puede comparar»</b>, y no se disfraza de «no hay diferencia»:
    /// dar por parejo lo que no se midió es justo lo que `RN-80` prohíbe al decir que el campo
    /// no consignado no se estima.
    /// </summary>
    public bool? MuyDistintoDe(NivelDeTanque retorno, decimal tolerancia = 0.25m)
    {
        if (DiferenciaCon(retorno) is not { } diferencia) return null;

        return Escala is EscalaDeNivel.FraccionDelIndicador
            ? Math.Abs(diferencia) > tolerancia
            // En galones no hay fracción del indicador de la que hablar: la referencia es lo
            // que llevaba al salir, y perder más de un cuarto de eso es la misma idea.
            : Valor > 0 && Math.Abs(diferencia) > Valor * tolerancia;
    }
}

/// <summary>
/// Un ingreso de combustible al tanque — `RN-83`.
///
/// ── Qué cambia respecto de lo que había ─────────────────────────────────────
/// Antes el sistema sólo conocía el consumo <b>del fondo</b>, porque el único registro vivía en
/// el vale. Las siete reglas de `M-09` modelan el consumo del fondo, y `RN-83` es la que las
/// desborda: <i>«un despacho desde el tanque de la institución no pasa por ningún folio y por eso
/// no existe para el sistema»</i>.
///
/// El efecto de esa ausencia es peor que un dato faltante: `RN-30` <b>detecta una desviación y
/// señala un síntoma cuya causa el sistema no puede registrar</b>. El conciliador busca un fraude
/// donde hay un procedimiento no modelado, y cuando el patrón se repite deja de mirar el
/// indicador.
///
/// ── Por qué cuelga del VEHÍCULO y no de la misión ───────────────────────────
/// `RN-83` aplica <i>«a todo vehículo de la flota, en misión o fuera de ella»</i>. Un
/// reabastecimiento de rutina en el predio no tiene misión, y colgarlo de una obligaría a
/// inventar el expediente al que pertenece.
/// </summary>
public sealed class Abastecimiento
{
    private Abastecimiento(
        Ulid id, Ulid vehiculo, DateTimeOffset ocurridoEn, decimal galones, int odometro,
        FuenteDeAbastecimiento fuente, IdPersona registra)
    {
        Id = id;
        Vehiculo = vehiculo;
        OcurridoEn = ocurridoEn;
        Galones = galones;
        Odometro = odometro;
        Fuente = fuente;
        Registra = registra;
    }

    public Ulid Id { get; }

    public Ulid Vehiculo { get; }

    /// <summary>La fecha del <b>hecho</b>, no la de captura — P-4.</summary>
    public DateTimeOffset OcurridoEn { get; }

    public decimal Galones { get; }

    /// <summary>El odómetro del momento: lo que ancla el galón a un tramo recorrido.</summary>
    public int Odometro { get; }

    public FuenteDeAbastecimiento Fuente { get; }

    public IdPersona Registra { get; private init; }

    /// <summary>La misión a la que sirvió. Nula en el reabastecimiento de rutina sin misión.</summary>
    public Ulid? Mision { get; private init; }

    /// <summary>
    /// El vale del que salió. <b>Sólo con fuente `FondoDeLaMision`</b>, y es lo que impide que
    /// el galón se cuente dos veces: el diario del vale y este registro son <b>el mismo hecho</b>
    /// visto desde dos lados, no dos hechos.
    /// </summary>
    public Ulid? Asignacion { get; private init; }

    /// <summary>Nulo cuando la fuente no lo tiene — una donación no trae precio.</summary>
    public decimal? Monto { get; private init; }

    public string? Estacion { get; private init; }

    public string? Comprobante { get; private init; }

    /// <summary>Por qué no lo hay — `RN-85`. Obligatoria cuando la fuente sí debería traerlo.</summary>
    public string? CausaSinComprobante { get; private init; }

    /// <summary>
    /// El consumo que <b>excede el fondo asignado</b> — `RN-83` punto 6.
    ///
    /// Se registra igual y se marca: <i>«su cobertura se resuelve en la liquidación, nunca
    /// omitiendo el registro»</i>. Omitirlo dejaría el galón fuera del denominador de `RN-30`, que
    /// es donde más falta hace.
    /// </summary>
    public bool Excedido { get; private init; }

    /// <summary>
    /// Si este galón entra al cuadre del fondo — `RN-29`.
    ///
    /// <b>Sólo el del fondo de la misión.</b> Lo demás entra al denominador de `RN-30` y no al
    /// cuadre, hasta que exista el acto que corresponda.
    /// </summary>
    public bool EntraAlCuadreDelFondo => Fuente is FuenteDeAbastecimiento.FondoDeLaMision;

    /// <summary>
    /// Si genera obligación de reintegro a favor de quien pagó — `RN-86`.
    ///
    /// `[C]` insumo #37. Se marca igual: la práctica ocurre, y hoy quedaría fuera de todo
    /// registro.
    /// </summary>
    public bool GeneraReintegro => Fuente is FuenteDeAbastecimiento.PeculioDelServidor;

    /// <summary>
    /// Registra el ingreso. <b>Nunca se rechaza por falta de papel</b> — `RN-85`.
    /// </summary>
    public static Abastecimiento Registrar(
        Ulid id,
        Ulid vehiculo,
        DateTimeOffset ocurridoEn,
        decimal galones,
        int odometro,
        FuenteDeAbastecimiento fuente,
        IdPersona registra,
        Ulid? mision = null,
        Ulid? asignacion = null,
        decimal? monto = null,
        string? estacion = null,
        string? comprobante = null,
        string? causaSinComprobante = null,
        bool excedido = false)
    {
        ReglasDeAbastecimiento.ExigirDatosDelHecho(galones, odometro);
        ReglasDeAbastecimiento.ExigirRespaldoSegunLaFuente(fuente, comprobante, causaSinComprobante);
        ReglasDeAbastecimiento.ExigirVinculoCoherente(fuente, asignacion);

        return new Abastecimiento(id, vehiculo, ocurridoEn, galones, odometro, fuente, registra)
        {
            Mision = mision,
            Asignacion = asignacion,
            Monto = monto,
            Estacion = estacion?.Trim(),
            Comprobante = comprobante?.Trim(),
            CausaSinComprobante = causaSinComprobante?.Trim(),
            Excedido = excedido,
        };
    }

    /// <summary>Rehidrata desde la base, sin volver a juzgar lo que ya se registró.</summary>
    public static Abastecimiento Reconstruir(
        Ulid id, Ulid vehiculo, DateTimeOffset ocurridoEn, decimal galones, int odometro,
        FuenteDeAbastecimiento fuente, IdPersona registra, Ulid? mision, Ulid? asignacion,
        decimal? monto, string? estacion, string? comprobante, string? causaSinComprobante,
        bool excedido) =>
        new(id, vehiculo, ocurridoEn, galones, odometro, fuente, registra)
        {
            Mision = mision,
            Asignacion = asignacion,
            Monto = monto,
            Estacion = estacion,
            Comprobante = comprobante,
            CausaSinComprobante = causaSinComprobante,
            Excedido = excedido,
        };

    /// <summary>Lo que va al asiento y al reporte de conciliación de `NRM-01`.</summary>
    public string Descripcion =>
        $"{Galones:N2} gal {Texto(Fuente)}, odómetro {Odometro:N0} km" +
        (Monto is { } m ? $", {m:N2}" : ", sin monto") +
        (Estacion is null ? "" : $", en {Estacion}") +
        (Comprobante is null
            ? $" · SIN COMPROBANTE (`RN-85`): {CausaSinComprobante ?? "la fuente no lo genera"}"
            : $", comprobante {Comprobante}") +
        (Excedido ? " · EXCEDE el fondo asignado" : "");

    internal static string Texto(FuenteDeAbastecimiento fuente) => fuente switch
    {
        FuenteDeAbastecimiento.FondoDeLaMision => "del fondo de la misión",
        FuenteDeAbastecimiento.TanqueInstitucional => "del tanque institucional",
        FuenteDeAbastecimiento.OtraDependencia => "de otra dependencia",
        FuenteDeAbastecimiento.Donacion => "de donación",
        FuenteDeAbastecimiento.PeculioDelServidor => "del peculio del servidor",
        FuenteDeAbastecimiento.TerceroEnApoyo => "de un tercero en apoyo",
        _ => fuente.ToString(),
    };
}

/// <summary>
/// Lo que `RN-83` exige de todo ingreso de combustible, venga de donde venga.
/// </summary>
public static class ReglasDeAbastecimiento
{
    /// <summary>
    /// `RN-83` punto 1 — galones y <b>odómetro del momento</b>.
    ///
    /// El odómetro es el único que ancla el galón a un tramo. Sin él la conciliación de `RN-30`
    /// compara un total contra otro total y no puede decir <b>dónde</b> se fue la diferencia.
    /// </summary>
    public static void ExigirDatosDelHecho(decimal galones, int odometro)
    {
        if (galones <= 0)
            throw new BloqueoDuro("RN-83",
                "Un abastecimiento de cero galones no es un abastecimiento.");

        if (odometro <= 0)
            throw new BloqueoDuro("RN-83",
                "El abastecimiento exige el odómetro del momento. Sin él el galón no queda " +
                "anclado a ningún tramo.");
    }

    /// <summary>
    /// `RN-85` — la ausencia de comprobante se registra, <b>con causa</b>.
    ///
    /// ── Y no se le pide papel a quien no lo tiene ───────────────────────────
    /// Una donación y el despacho del tanque de la sede <b>no generan factura</b>, así que
    /// exigirles causa obligaría a escribir «no aplica» en cada registro — y una casilla que
    /// siempre dice lo mismo deja de leerse. La causa se exige donde <b>debería haber papel</b>:
    /// una compra en estación.
    /// </summary>
    public static void ExigirRespaldoSegunLaFuente(
        FuenteDeAbastecimiento fuente, string? comprobante, string? causa)
    {
        if (!string.IsNullOrWhiteSpace(comprobante)) return;

        if (!DeberiaTraerComprobante(fuente)) return;

        if (string.IsNullOrWhiteSpace(causa))
            throw new BloqueoDuro("RN-85",
                $"Un abastecimiento {Abastecimiento.Texto(fuente)} normalmente trae " +
                "comprobante. Sin él hay que declarar por qué: el registro no se omite nunca por " +
                "falta de papel, pero tampoco se disimula.");
    }

    /// <summary>
    /// Sólo el del fondo cuelga de un vale. Vincular una donación a un folio haría que el
    /// cuadre de `RN-29` contara un galón que nadie pagó con dinero del fondo.
    /// </summary>
    public static void ExigirVinculoCoherente(FuenteDeAbastecimiento fuente, Ulid? asignacion)
    {
        if (fuente is FuenteDeAbastecimiento.FondoDeLaMision && asignacion is null)
            throw new BloqueoDuro("RN-83",
                "Un abastecimiento con cargo al fondo tiene que decir de qué vale salió. Sin el " +
                "folio no se puede contestar de qué fondo salió este galón.");

        if (fuente is not FuenteDeAbastecimiento.FondoDeLaMision && asignacion is not null)
            throw new BloqueoDuro("RN-83",
                $"Un abastecimiento {Abastecimiento.Texto(fuente)} no sale de un vale. " +
                "Vincularlo a uno lo metería en el cuadre del fondo, que no lo pagó.");
    }

    /// <summary>
    /// Qué fuentes deberían traer factura. Las otras no la generan, y pedirla sería papeleo
    /// inventado.
    /// </summary>
    public static bool DeberiaTraerComprobante(FuenteDeAbastecimiento fuente) =>
        fuente is FuenteDeAbastecimiento.FondoDeLaMision
                or FuenteDeAbastecimiento.PeculioDelServidor;
}
