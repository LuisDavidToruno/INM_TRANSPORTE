namespace Sigti.Dominio.M19_Seguimiento;

/// <summary>
/// Cuán viejo es lo último que sabemos — `HU-057`, `RN-50`, `RN-76`.
///
/// ── Por qué esto es una regla y no un formato de pantalla ───────────────────
/// «Última posición conocida hace 10 h 40 min» y «última posición conocida» son dos frases que
/// llevan al Jefe de Transporte a decisiones distintas. La segunda <b>afirma algo falso sin
/// decir ninguna mentira</b>, y por eso la antigüedad no puede quedar a criterio de quien
/// dibuje la pantalla: se calcula acá y viaja con el dato.
/// </summary>
public static class ReglasDeLaFrescura
{
    /// <summary>Clave del umbral de degradación de `RN-50`. `[C]` insumo #68 — hoy sin fijar.</summary>
    public const string ClaveDelUmbral = "seguimiento.umbral_degradacion_horas";

    /// <param name="ultimoHecho">
    /// El momento del <b>hecho</b> del último reporte, nunca el de su recepción (`RN-46`).
    /// <b>Nulo es «nunca declaró nada»</b>, que no es lo mismo que «declaró hace mucho».
    /// </param>
    /// <param name="umbral">
    /// De `RN-50`. <b>Nulo es «la institución no lo fijó»</b> y obliga a decirlo: cablear un
    /// «razonable» de doce horas produciría un tablero que degrada o no degrada según un número
    /// que nadie decidió y que nadie puede rastrear. Va como parámetro y no como valor por
    /// omisión para que ningún llamador pueda omitir la pregunta.
    /// </param>
    public static Frescura Evaluar(
        DateTimeOffset? ultimoHecho, DateTimeOffset ahora, TimeSpan? umbral)
    {
        if (ultimoHecho is null)
            return new Frescura(null, GradoDeFrescura.NuncaHuboDato,
                "No hay ninguna declaración del motorista. El silencio no dice dónde está el " +
                "vehículo, ni que se haya detenido, ni que haya pasado algo.");

        var antiguedad = ahora - ultimoHecho.Value;

        // ── El reloj adelantado ─────────────────────────────────────────────
        // Aplastar la antigüedad negativa a cero es la salida cómoda y hace el peor daño
        // posible: un dispositivo con el reloj roto se vería «recién actualizado» — el dato
        // más fresco del tablero sería justo el menos confiable.
        if (antiguedad < TimeSpan.Zero)
            return new Frescura(antiguedad, GradoDeFrescura.RelojAdelantado,
                $"El dispositivo declaró un hecho {EnPalabras(-antiguedad)} en el futuro. " +
                "La hora del equipo está mal puesta y ningún tiempo calculado con ella sirve.");

        if (umbral is null)
            return new Frescura(antiguedad, GradoDeFrescura.NoSeClasifica,
                $"El dato tiene {EnPalabras(antiguedad)}. No se puede decir si eso es mucho: " +
                $"el umbral `{ClaveDelUmbral}` no está fijado (insumo #68).");

        return antiguedad <= umbral.Value
            ? new Frescura(antiguedad, GradoDeFrescura.Fresco,
                $"Declarado hace {EnPalabras(antiguedad)}.")
            : new Frescura(antiguedad, GradoDeFrescura.Degradado,
                $"Sin datos nuevos desde hace {EnPalabras(antiguedad)}. En zonas sin cobertura " +
                "esto es esperable y no indica que haya pasado nada.");
    }

    /// <summary>
    /// La forma en que `HU-057` pide que se lea: «hace 10 horas 40 minutos». No se redondea a
    /// «hace un día», que borra justamente la diferencia que se quería ver.
    /// </summary>
    public static string EnPalabras(TimeSpan d)
    {
        if (d < TimeSpan.FromMinutes(1)) return "menos de un minuto";

        var partes = new List<string>();
        if (d.Days > 0) partes.Add($"{d.Days} día{(d.Days == 1 ? "" : "s")}");
        if (d.Hours > 0) partes.Add($"{d.Hours} hora{(d.Hours == 1 ? "" : "s")}");
        if (d.Days == 0 && d.Minutes > 0)
            partes.Add($"{d.Minutes} minuto{(d.Minutes == 1 ? "" : "s")}");

        return string.Join(" ", partes);
    }
}

/// <param name="Antiguedad">
/// <b>Nula cuando nunca hubo dato</b>, y negativa cuando el reloj del dispositivo va adelantado.
/// Las dos cosas se muestran como son.
/// </param>
public sealed record Frescura(TimeSpan? Antiguedad, GradoDeFrescura Grado, string PorQue);

public enum GradoDeFrescura
{
    /// <summary>Dentro del umbral que la institución fijó.</summary>
    Fresco,

    /// <summary>Fuera del umbral. <b>No es una alarma</b>: en Honduras el silencio es lo normal.</summary>
    Degradado,

    /// <summary>Hay dato y no hay umbral con qué juzgarlo. La antigüedad se muestra igual.</summary>
    NoSeClasifica,

    /// <summary>El hecho está fechado en el futuro: la hora del dispositivo está mal.</summary>
    RelojAdelantado,

    /// <summary>Ninguna declaración. <b>Distinto de una declaración muy vieja.</b></summary>
    NuncaHuboDato,
}
