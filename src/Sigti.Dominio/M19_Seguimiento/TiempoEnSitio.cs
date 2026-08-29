namespace Sigti.Dominio.M19_Seguimiento;

/// <summary>
/// El tiempo en sitio, <b>derivado de arribo y salida</b> — `RN-76`, `CE-08`.
///
/// ── Nunca se le pide al motorista que lo cronometre ─────────────────────────
/// `RN-76` es explícita. Y no es comodidad: un tiempo digitado es un tiempo redondeado a la
/// media hora, siempre a favor de quien lo digita, y no serviría para atribuirle un costo a
/// nadie. Derivado de dos eventos con hora del hecho, sí sirve.
/// </summary>
public static class ReglasDeLaEstadia
{
    /// <summary>
    /// Arma las estadías de una misión.
    ///
    /// ── Se ordena por la hora del hecho ─────────────────────────────────────
    /// `HU-057` lo exige literalmente: al reconectar llegan de golpe los reportes acumulados, y
    /// el orden de recepción los pone al revés. Ordenar por captura —que es lo que sale de la
    /// base sin pedirlo— produciría estadías negativas y salidas antes de arribos.
    /// </summary>
    /// <param name="ahora">Para cerrar la estadía que sigue abierta. La duración queda en curso.</param>
    /// <param name="causasImproductivas">
    /// Del catálogo `causa_de_espera`. <b>Vacío significa que nada se puede clasificar</b>, y
    /// entonces ninguna estadía se declara productiva: dar todo por productivo con el catálogo
    /// sin poblar haría que el indicador reportara cero horas improductivas — la cifra más
    /// tranquilizadora posible, y falsa.
    /// </param>
    public static ResultadoDeEstadias Derivar(
        IEnumerable<ReporteDeCampo> reportes,
        DateTimeOffset ahora,
        IReadOnlySet<string> causasImproductivas)
    {
        var enOrden = reportes.OrderBy(r => r.MomentoDelHecho).ToList();

        var estadias = new List<EstadiaEnSitio>();
        var salidasHuerfanas = new List<SalidaSinArribo>();

        Abierta? abierta = null;

        foreach (var r in enOrden)
        {
            switch (r.Tipo)
            {
                case TipoDeReporte.Arribo:
                    // Dos arribos seguidos: el motorista no declaró la salida del primero. Se
                    // deriva del siguiente evento y queda marcada como derivada — `RN-76` pide
                    // que el registro señale que la salida no fue declarada.
                    if (abierta is not null)
                        estadias.Add(Cerrar(abierta, r.MomentoDelHecho,
                            ComoSeSupoLaSalida.DerivadaDelSiguienteEvento, causasImproductivas));

                    abierta = new Abierta(r.Destino!, r.MomentoDelHecho, null, null, null);
                    break;

                case TipoDeReporte.Salida:
                    if (abierta is null)
                    {
                        // No se inventa un arribo. Una salida sin arribo es un hueco, y un hueco
                        // se muestra: rellenarlo con la hora de la salida produciría una estadía
                        // de cero minutos que se leería como que no esperó nada.
                        salidasHuerfanas.Add(
                            new SalidaSinArribo(r.Destino ?? "sin destino", r.MomentoDelHecho));
                        break;
                    }

                    estadias.Add(Cerrar(
                        abierta with
                        {
                            Causa = r.CausaDeEspera ?? abierta.Causa,
                            Atribuida = r.SeAtribuyeA ?? abierta.Atribuida,
                            Motor = r.MotorEncendido ?? abierta.Motor,
                        },
                        r.MomentoDelHecho, ComoSeSupoLaSalida.Declarada, causasImproductivas));

                    abierta = null;
                    break;

                case TipoDeReporte.EstadoDeclarado:
                    // La espera se tipifica al declararla o al salir del sitio. Acá se recoge
                    // la primera de las dos vías.
                    if (abierta is not null)
                        abierta = abierta with
                        {
                            Causa = r.CausaDeEspera ?? abierta.Causa,
                            Atribuida = r.SeAtribuyeA ?? abierta.Atribuida,
                            Motor = r.MotorEncendido ?? abierta.Motor,
                        };
                    break;
            }
        }

        if (abierta is not null)
            estadias.Add(Cerrar(abierta, ahora, ComoSeSupoLaSalida.SinCerrar, causasImproductivas));

        return new ResultadoDeEstadias(estadias, salidasHuerfanas, causasImproductivas.Count == 0);
    }

    private static EstadiaEnSitio Cerrar(
        Abierta a, DateTimeOffset fin, ComoSeSupoLaSalida como,
        IReadOnlySet<string> causasImproductivas)
    {
        // Nulo, no falso: sin causa declarada no se sabe si la espera fue productiva, y
        // "no se sabe" no es "fue productiva". Con el catálogo vacío tampoco se sabe.
        bool? improductiva = a.Causa is null || causasImproductivas.Count == 0
            ? null
            : causasImproductivas.Contains(a.Causa);

        return new EstadiaEnSitio(
            a.Destino,
            a.Arribo,
            como == ComoSeSupoLaSalida.SinCerrar ? null : fin,
            como,
            fin - a.Arribo,
            a.Causa,
            a.Atribuida,
            a.Motor,
            improductiva);
    }

    private sealed record Abierta(
        string Destino, DateTimeOffset Arribo, string? Causa, string? Atribuida, bool? Motor);
}

/// <param name="Salida">
/// <b>Nula mientras el vehículo siga en el sitio.</b> La duración corre igual, pero no se
/// declara cerrada: dar por terminada una espera en curso subestimaría lo que se quiere medir.
/// </param>
/// <param name="EsImproductiva">
/// <b>Nulo es "no se pudo clasificar"</b> — sin causa declarada, o con el catálogo sin poblar.
/// Nunca se colapsa a falso: eso reportaría cero horas improductivas cuando lo que pasa es que
/// nadie las tipificó.
/// </param>
/// <param name="MotorEncendido">
/// Nulo es "no se preguntó". Entra en la conciliación galonaje–kilometraje (`RN-30`): una
/// desviación de consumo con espera y motor encendido registrados no produce hallazgo por sí sola.
/// </param>
public sealed record EstadiaEnSitio(
    string Destino,
    DateTimeOffset Arribo,
    DateTimeOffset? Salida,
    ComoSeSupoLaSalida Como,
    TimeSpan Duracion,
    string? Causa,
    string? SeAtribuyeA,
    bool? MotorEncendido,
    bool? EsImproductiva);

public sealed record SalidaSinArribo(string Destino, DateTimeOffset Momento);

public enum ComoSeSupoLaSalida
{
    /// <summary>El motorista la declaró.</summary>
    Declarada,

    /// <summary>Se dedujo del siguiente arribo. El registro lo señala: no fue declarada.</summary>
    DerivadaDelSiguienteEvento,

    /// <summary>Sigue en el sitio. El reloj corre.</summary>
    SinCerrar,
}

/// <param name="SalidasSinArribo">
/// Huecos visibles. No se rellenan con un arribo inventado.
/// </param>
/// <param name="SinCatalogoDeCausas">
/// Cuando es verdadero, <b>ninguna estadía está clasificada</b> y el total improductivo no se
/// puede calcular. Quien muestre esto tiene que decirlo, no reportar cero.
/// </param>
public sealed record ResultadoDeEstadias(
    IReadOnlyList<EstadiaEnSitio> Estadias,
    IReadOnlyList<SalidaSinArribo> SalidasSinArribo,
    bool SinCatalogoDeCausas)
{
    /// <summary>Sólo lo tipificado como improductivo. La carga y descarga es operación normal.</summary>
    public TimeSpan Improductivo =>
        Estadias.Where(e => e.EsImproductiva == true)
                .Aggregate(TimeSpan.Zero, (t, e) => t + e.Duracion);

    /// <summary>
    /// Cuántas estadías no se pudieron clasificar. <b>Va al lado del total siempre</b>: "4 horas
    /// improductivas" y "4 horas improductivas, con 3 estadías sin tipificar" sostienen
    /// conclusiones distintas.
    /// </summary>
    public int SinTipificar => Estadias.Count(e => e.EsImproductiva is null);
}
