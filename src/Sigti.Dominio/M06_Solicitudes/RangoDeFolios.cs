using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Dominio.M06_Solicitudes;

/// <summary>
/// Los rangos de folio pre-asignados por delegación — `RN-44`, `RNF-21`, `ADR-005`.
///
/// ── Por qué el folio no lo puede dar el servidor ────────────────────────────
/// <i>«Si el folio lo asigna el servidor, no hay documento imprimible antes de salir — y el
/// control en carretera es físico.»</i> Una delegación que lleva cuatro días sin enlace tiene
/// que poder emitir su orden de misión igual, y por eso el rango se reserva por adelantado.
///
/// ── Lo que hay que garantizar, y no es obvio ────────────────────────────────
/// `RNF-21` fija cuatro ceros: cero duplicados a nivel institución, cero folios reciclados,
/// cero colisiones entre dispositivos, y <b>cero huecos sin explicación</b> — un salto sin
/// explicar es exactamente lo que el auditor busca.
///
/// Y nombra la prueba que <i>«realmente rompe»</i>: <b>tres dispositivos de la misma
/// delegación</b>, los tres desconectados, emitiendo el mismo tipo de documento. La delegación
/// no alcanza como unidad de reserva — dos dispositivos con el mismo rango emiten el mismo
/// folio y ninguno se entera hasta que sincronizan.
/// </summary>
public static class ReglasDelFolio
{
    /// <summary>
    /// Cuándo avisar de que el rango se está acabando. `[C]` insumo #34 — <b>nulo cuando la
    /// institución no lo fijó</b>, y entonces no se avisa y se dice que no se avisa.
    /// </summary>
    public const string ClaveDelUmbral = "folio.umbral_agotamiento";

    /// <summary>
    /// La plantilla del folio. `[C]` insumo #34: `RNF-21` dice que el formato del correlativo
    /// es <b>configurable y «no se decide por inferencia»</b>.
    /// </summary>
    public const string ClaveDelFormato = "folio.formato";

    /// <summary>
    /// Dos rangos del mismo tipo de documento no pueden solaparse. <b>Bloqueo duro.</b>
    ///
    /// Es la única garantía real de la unicidad institucional: si los rangos no se solapan,
    /// ningún par de dispositivos puede emitir el mismo folio aunque no se vean entre sí
    /// durante semanas. Comprobarlo al sincronizar sería tarde — el papel ya se imprimió y se
    /// entregó en una caseta de peaje.
    /// </summary>
    public static void ExigirSinSolape(RangoDeFolios nuevo, IEnumerable<RangoDeFolios> existentes)
    {
        if (nuevo.Desde > nuevo.Hasta)
            throw new BloqueoDuro("RN-44",
                $"El rango {nuevo.Desde}–{nuevo.Hasta} está invertido.");

        if (nuevo.Desde < 1)
            throw new BloqueoDuro("RN-44", "Los folios empiezan en 1.");

        var choque = existentes.FirstOrDefault(r =>
            r.Id != nuevo.Id &&
            r.TipoDeDocumento == nuevo.TipoDeDocumento &&
            r.Desde <= nuevo.Hasta && nuevo.Desde <= r.Hasta);

        if (choque is not null)
            throw new BloqueoDuro("RN-44",
                $"El rango {nuevo.Desde}–{nuevo.Hasta} se solapa con el {choque.Desde}–" +
                $"{choque.Hasta}, ya asignado a «{choque.Delegacion}»" +
                (choque.Dispositivo is null ? "" : $" · dispositivo {choque.Dispositivo}") +
                $" para {nuevo.TipoDeDocumento}. Dos rangos solapados producen el mismo folio " +
                "en dos lugares, y eso destruye la trazabilidad documental del expediente.");
    }

    /// <summary>
    /// El siguiente folio del rango. <b>No se recicla nunca</b>, ni el de un documento anulado.
    ///
    /// Por eso el contador avanza sobre lo <b>emitido</b> y no sobre lo vigente: una anulación
    /// deja un hueco, el hueco se explica con su asiento reverso, y el número no vuelve. Un
    /// correlativo con huecos es normal; <b>uno reutilizado es un expediente que sustituye a
    /// otro</b>.
    /// </summary>
    public static int Siguiente(RangoDeFolios rango)
    {
        if (rango.Agotado)
            throw new BloqueoDuro("RN-44",
                $"El rango de «{rango.Delegacion}» para {rango.TipoDeDocumento} está agotado: " +
                $"{rango.Desde}–{rango.Hasta}, {rango.Emitidos} emitidos. Reponer el rango " +
                "exige conectividad con la sede — por eso el aviso tiene que llegar antes.");

        return rango.Desde + rango.Emitidos;
    }

    /// <summary>
    /// Si conviene avisar de que el rango se agota.
    ///
    /// <b>Nulo en el umbral es «no se fijó»</b>, y entonces no hay aviso — pero quien muestre
    /// esto tiene que decir que no lo hay. `RNF-21` exige <i>cero agotamientos sin aviso
    /// previo</i>, y un tablero silencioso porque falta un parámetro se ve igual que uno
    /// silencioso porque todo está bien.
    /// </summary>
    public static AvisoDeRango Evaluar(RangoDeFolios rango, decimal? umbralDeSaldo)
    {
        if (rango.Agotado)
            return new AvisoDeRango(GradoDelRango.Agotado, rango.Disponibles,
                $"El rango está agotado. No se pueden emitir más {rango.TipoDeDocumento} en " +
                $"«{rango.Delegacion}» hasta reponerlo, y reponer exige conectividad.");

        if (umbralDeSaldo is null)
            return new AvisoDeRango(GradoDelRango.NoSeEvalua, rango.Disponibles,
                $"Quedan {rango.Disponibles} folios. No se puede decir si eso es poco: el " +
                $"umbral `{ClaveDelUmbral}` no está fijado (insumo #34), así que **no habrá " +
                "aviso previo cuando se agote**.");

        return rango.Saldo <= umbralDeSaldo.Value
            ? new AvisoDeRango(GradoDelRango.PorAgotarse, rango.Disponibles,
                $"Quedan {rango.Disponibles} folios de {rango.Total}. Reponer exige " +
                "conectividad con la sede: conviene pedirlo mientras haya enlace.")
            : new AvisoDeRango(GradoDelRango.Suficiente, rango.Disponibles,
                $"Quedan {rango.Disponibles} folios de {rango.Total}.");
    }

    /// <summary>
    /// El folio impreso, según la plantilla configurada.
    ///
    /// ── Nulo cuando no hay formato, y no se inventa uno ─────────────────────
    /// `RNF-21` es explícita: el formato <b>«no se decide por inferencia»</b> (insumo #34).
    /// Componer un «OM-CHO-2026-000123» plausible produciría folios que la institución citaría
    /// en descargos y que no coinciden con su numeración oficial — y cambiarlos después
    /// obligaría a reemitir todo lo impreso.
    ///
    /// Devuelve nulo, y quien lo reciba sigue mostrando el provisional <b>diciendo que lo es</b>.
    /// </summary>
    /// <param name="plantilla">
    /// Con los marcadores <c>{delegacion}</c>, <c>{anio}</c>, <c>{tipo}</c> y <c>{numero}</c>.
    /// </param>
    public static string? Componer(
        string? plantilla, RangoDeFolios rango, int anio, int numero)
    {
        if (string.IsNullOrWhiteSpace(plantilla)) return null;

        return plantilla
            .Replace("{delegacion}", rango.Delegacion)
            .Replace("{anio}", anio.ToString())
            .Replace("{tipo}", rango.TipoDeDocumento)

            // Seis dígitos con ceros a la izquierda: un correlativo que se ordena como texto en
            // un reporte tiene que ordenarse igual que como número, o «10» aparece antes de «9».
            .Replace("{numero}", numero.ToString("D6"));
    }
}

/// <summary>
/// Un rango reservado a una delegación —y opcionalmente a un dispositivo— para un tipo de
/// documento.
/// </summary>
/// <param name="Dispositivo">
/// <b>Nulo es «toda la delegación»</b>, y sólo sirve cuando la delegación tiene un solo equipo
/// emitiendo. Con dos o más hay que dar un subrango a cada uno: es el caso que `RNF-21` llama
/// <i>«la que realmente rompe»</i>, porque los tres emiten sin verse y descubren la colisión al
/// sincronizar, con el papel ya entregado.
/// </param>
/// <param name="Emitidos">
/// Cuántos se sacaron del rango. <b>Incluye los anulados</b>: el folio de un documento anulado
/// no vuelve al rango, deja un hueco, y el hueco se explica con su asiento reverso.
/// </param>
public sealed record RangoDeFolios(
    Ulid Id,
    string Delegacion,
    string TipoDeDocumento,
    int Desde,
    int Hasta,
    int Emitidos,
    string? Dispositivo,
    string Asigna,
    DateOnly AsignadoEl)
{
    public int Total => Hasta - Desde + 1;

    public int Disponibles => Total - Emitidos;

    public bool Agotado => Disponibles <= 0;

    /// <summary>La fracción que queda, de 0 a 1. Es contra esto que se compara el umbral.</summary>
    public decimal Saldo => Total == 0 ? 0 : (decimal)Disponibles / Total;
}

public enum GradoDelRango
{
    Suficiente,
    PorAgotarse,
    Agotado,

    /// <summary>Hay saldo y no hay umbral con qué juzgarlo. <b>No habrá aviso previo.</b></summary>
    NoSeEvalua,
}

public sealed record AvisoDeRango(GradoDelRango Grado, int Disponibles, string PorQue);
