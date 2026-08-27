using Sigti.Dominio.M03_Flota;

namespace Sigti.Dominio.M07_ProgramacionYDespacho;

/// <summary>
/// Una reserva que <b>otra</b> misión ya tiene sobre el vehículo o sobre quien conduce.
///
/// ── Qué trae y por qué esos campos y no menos ────────────────────────────────
/// `EF-01` es explícito sobre lo que hay que mostrar cuando aparece un conflicto: <i>«qué
/// misión tiene tomado el recurso, <b>de qué dependencia</b>, en qué franja»</i>. Los tres
/// datos están acá porque un bloqueo que dice «el vehículo está ocupado» y nada más obliga
/// a quien programa a salir a buscar con quién negociar — y las cuatro salidas que `EF-01`
/// ofrece (consolidar, otro recurso, reprogramar, escalar) <b>empiezan todas por saber a
/// qué dependencia llamar</b>.
/// </summary>
/// <param name="Vehiculo">
/// Si el choque es por el vehículo. <b>Puede ser cierto junto con <paramref name="Conductor"/></b>:
/// la misma misión reserva los dos, y si se reintenta con el mismo par choca por ambos.
/// </param>
public sealed record ReservaDeRecurso(
    Ulid Mision,
    string Folio,
    string Dependencia,
    DateOnly Desde,
    DateOnly Hasta,
    bool Vehiculo,
    bool Conductor)
{
    /// <summary>
    /// ¿Se solapa con esta ventana?
    ///
    /// <b>Los dos extremos son inclusivos, de los dos lados.</b> Dos misiones que se tocan
    /// —una termina el jueves y la otra empieza el jueves— <b>se solapan</b>: el vehículo no
    /// puede estar volviendo de Danlí y saliendo a Juticalpa el mismo día. Tratar el
    /// extremo como exclusivo dejaría pasar exactamente el caso que `EF-01` describe: el
    /// servidor público esperando en la puerta.
    /// </summary>
    public bool SeSolapaCon(VentanaDeMision ventana) =>
        Desde <= ventana.FinDelRango && Hasta >= ventana.Salida;
}
