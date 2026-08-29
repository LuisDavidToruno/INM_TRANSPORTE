namespace Sigti.Dominio.M09_Combustible;

/**
 * Nada acá lee el reloj ni la base: es aritmética sobre dos lecturas.
 */

/// <summary>
/// Qué hace la institución con el combustible que quedó en el tanque — `CE-07`.
///
/// ⚠️ <b>`[C]` — la institución no lo ha declarado.</b> `CE-07` nombra las tres salidas y deja
/// abierta cuál rige: <i>«si el remanente se abona al fondo, si se le imputa a la siguiente
/// misión de ese vehículo, o si simplemente se documenta»</i>. Se cruza con las decisiones
/// abiertas de `PROP-01`, insumo #7.
///
/// <b>Lo que no puede pasar</b>, dice el caso, <b>es que un tanque lleno pagado con fondo de
/// esta misión desaparezca del expediente.</b> Por eso el remanente se calcula y se muestra
/// aunque su destino contable siga sin decidirse: documentarlo es la única de las tres que se
/// puede hacer sin saber cuál rige.
/// </summary>
public enum DestinoDelRemanente
{
    /// <summary>Vuelve al saldo del fondo que lo pagó.</summary>
    SeAbonaAlFondo,

    /// <summary>Se le carga a la próxima misión de ese vehículo, que sale con el tanque servido.</summary>
    SeImputaALaSiguienteMision,

    /// <summary>Queda en el expediente y no mueve ningún cuadre.</summary>
    SoloSeDocumenta,
}

/// <summary>
/// Cuánto combustible quedó —o faltó— en el tanque respecto de como salió.
/// </summary>
/// <param name="Galones">
/// <b>Positivo</b>: volvió con más de lo que llevaba, así que parte de lo abastecido no lo
/// gastó esta misión. <b>Negativo</b>: volvió con menos, así que gastó combustible que ya
/// estaba en el tanque al salir — el caso que `RN-30` nombra como <i>«sale lleno y retorna
/// vacío: los galones consumidos exceden a los cargados»</i>.
///
/// <b>Nulo es «no se pudo calcular»</b>, y no cero: un cero diría que el tanque volvió exacto.
/// </param>
/// <param name="Explicacion">
/// Por qué se pudo o no. Va siempre, porque un remanente ausente sin razón se lee como un
/// tanque que no se movió.
/// </param>
public sealed record Remanente(decimal? Galones, string Explicacion)
{
    public bool EsCalculable => Galones is not null;
}

/// <summary>
/// El remanente en tanque — `RN-83` punto 3 y `CE-07`.
///
/// ── La fórmula, tal como la escribe `CE-07` ─────────────────────────────────
/// <c>consumido por la misión = entregado − devuelto en vales − remanente en tanque
/// atribuible</c>.
///
/// Traducido a lo que el sistema tiene: <b>lo que la misión quemó es lo que entró al tanque
/// menos lo que quedó de más</b>. Sin esa resta, un vehículo que vuelve con el tanque servido
/// aparece consumiendo de más, y `RN-30` lo marca como desviación — de un combustible que
/// sigue en el tanque, a la vista de cualquiera que abra la tapa.
///
/// ── Por qué esto no se puede estimar ────────────────────────────────────────
/// Porque la conversión de fracción a galones necesita la <b>capacidad del tanque</b>, y la
/// ficha técnica puede no declararla. Inventarla produciría un remanente que después nadie
/// distinguiría de uno medido — y ese número entra directo al denominador del rendimiento.
/// </summary>
public static class ReglasDelRemanente
{
    /// <param name="capacidadDeTanqueGalones">
    /// De la ficha técnica del vehículo. <b>Nula cuando no está declarada</b>, y entonces las
    /// lecturas en fracción del indicador no se pueden convertir.
    /// </param>
    public static Remanente Calcular(
        NivelDeTanque? salida,
        NivelDeTanque? retorno,
        decimal? capacidadDeTanqueGalones)
    {
        if (salida is null || retorno is null)
            return new Remanente(null,
                "el nivel del tanque no se consignó en la salida o en el retorno, y `RN-80` " +
                "prohíbe estimarlo: sin las dos lecturas no hay diferencia que medir");

        if (salida.Escala != retorno.Escala)
            return new Remanente(null,
                $"las dos lecturas usan escalas distintas ({salida.Escala} y {retorno.Escala}), " +
                "y convertir una en otra exige la capacidad del tanque");

        var diferencia = retorno.Valor - salida.Valor;

        if (salida.Escala is EscalaDeNivel.Galones)
            return new Remanente(diferencia, Texto(diferencia, "galones leídos directamente"));

        // Fracción del indicador. Sin capacidad no hay conversión, y un octavo de tanque no
        // significa lo mismo en un pickup que en un bus — la fracción sola no es una cantidad.
        if (capacidadDeTanqueGalones is not { } capacidad || capacidad <= 0)
            return new Remanente(null,
                "las lecturas están en fracción del indicador y la ficha técnica del vehículo " +
                "no declara la capacidad del tanque: un octavo no es una cantidad hasta saber " +
                "de qué tanque");

        var enGalones = diferencia * capacidad;

        return new Remanente(enGalones,
            Texto(enGalones, $"{diferencia:P0} de un tanque de {capacidad:N0} galones"));
    }

    /// <summary>
    /// Lo que la misión <b>quemó</b>: lo abastecido menos lo que quedó de más.
    ///
    /// Cuando el remanente no se puede calcular, devuelve lo abastecido y <b>eso es lo mejor
    /// que se puede afirmar</b>. No es lo mismo que decir que el remanente fue cero: la
    /// diferencia queda dicha en la explicación, y quien concilia decide si el número le sirve.
    /// </summary>
    public static decimal ConsumidoPorLaMision(decimal abastecido, Remanente remanente) =>
        remanente.Galones is { } galones ? abastecido - galones : abastecido;

    private static string Texto(decimal galones, string comoSeSupo) =>
        galones switch
        {
            > 0 => $"quedaron {galones:N2} galones de más en el tanque ({comoSeSupo}): " +
                   "no los gastó esta misión",
            < 0 => $"faltan {Math.Abs(galones):N2} galones respecto de como salió ({comoSeSupo}): " +
                   "la misión gastó combustible que ya estaba en el tanque",
            _ => $"el tanque volvió al mismo nivel con que salió ({comoSeSupo})",
        };
}
