namespace Sigti.Dominio.M03_Flota;

/// <summary>
/// La excepción de circulación en día inhábil — `RN-24`.
///
/// ── Es atributo del VEHÍCULO, no del viaje ───────────────────────────────────
/// `RN-24` lo dice y explica por qué: <i>«si la excepción se declarara por viaje, cualquier
/// misión podría autoexceptuarse alegando urgencia, y el control de `RN-23` se vaciaría en
/// una semana»</i>. Una ambulancia está exceptuada siempre; una misión no se vuelve urgente
/// porque quien la pide lo diga.
///
/// ── Y por eso lleva vigencia y fundamento ────────────────────────────────────
/// `NRM-02` `[V]` reconoce los servicios esenciales —emergencia, seguridad, defensa, salud,
/// CONAPREMM—, pero <b>que este vehículo concreto esté bajo alguno es `[C]`</b> de cada
/// institución. La excepción sin fundamento documental y sin rango explícito es una casilla
/// marcada, y una casilla marcada no se sostiene ante el Tribunal Superior de Cuentas.
/// </summary>
/// <param name="Tipo">
/// Del catálogo `tipo_servicio_exceptuado`. ⚠️ <b>Hoy es texto libre</b>: el catálogo con
/// vigencia que `RN-24` pide es de `M-02` y no está cargado. Se guarda igual porque el dato
/// existe en la realidad antes que su catálogo.
/// </param>
/// <param name="Fundamento">
/// Qué documento sostiene la excepción. El <b>adjunto</b> que `RN-24` exige va aparte, en el
/// almacén de `M-16`; esto es la referencia.
/// </param>
public sealed record ServicioExceptuado(
    string Tipo,
    string Fundamento,
    DateOnly Desde,
    DateOnly? Hasta)
{
    /// <summary>Los dos extremos inclusivos, como toda vigencia del sistema.</summary>
    public bool VigenteAl(DateOnly fecha) =>
        fecha >= Desde && (Hasta is null || fecha <= Hasta);
}
