using Sigti.Dominio.M02_Parametros;
using Sigti.Dominio.M03_Flota;

namespace Sigti.Dominio.M07_ProgramacionYDespacho;

/// <summary>
/// Todo lo que hace falta para juzgar `BD-04`, junto.
///
/// ── Por qué van en un solo objeto y no como cuatro parámetros ────────────────
/// Porque son <b>una sola pregunta</b> —«¿puede este vehículo, con este motorista, salir en
/// estos días?»— y separarlos deja que un llamador conteste tres cuartos: pasar el
/// calendario y olvidar los permisos daría un bloqueo perfectamente ejecutado contra datos
/// incompletos. Agrupados, el compilador exige la respuesta entera.
///
/// ── Los identificadores son los del recurso ASIGNADO ─────────────────────────
/// No los que trae el permiso. El permiso se compara <b>contra</b> ellos: si el motorista
/// cambió por un relevo, el permiso deja de amparar, que es exactamente lo que `RN-23`
/// prescribe.
/// </summary>
/// <param name="Excepcion">
/// Nula cuando el vehículo <b>no</b> tiene servicio exceptuado, que es el caso normal. `RN-24`:
/// es atributo del vehículo, no del viaje.
/// </param>
/// <param name="Permisos">
/// Los emitidos para esta misión. Vacía significa que no hay ninguno — y el mensaje del
/// bloqueo distingue eso de «hay permisos pero ninguno ampara», porque son dos problemas
/// con dos arreglos distintos.
/// </param>
public sealed record CirculacionEnDiaInhabil(
    CalendarioDeDiasHabiles Calendario,
    Ulid Vehiculo,
    Ulid Motorista,
    ServicioExceptuado? Excepcion,
    IReadOnlyList<PermisoDeCirculacion> Permisos,

    /// <summary>
    /// Si el salvoconducto del permiso <b>está emitido y en la mano del motorista</b>.
    ///
    /// ── ⚠️ La otra mitad de `INV-19`, que faltaba ──────────────────────────
    /// El invariante de `DESPACHADA` dice: <i>«existe el permiso de la máxima autoridad <b>y su
    /// salvoconducto impreso</b> — `BD-04`»</i>. `BD-04` comprobaba el permiso y nada más, así
    /// que una misión podía salir en franja inhábil con la firma registrada en el sistema y
    /// <b>sin papel en la guantera</b> — que es lo único que un agente en carretera puede pedir.
    ///
    /// <c>false</c> cuando no se emitió, o cuando se emitió y nadie firmó su recepción: emitir
    /// es un acto de oficina, y entre la impresora y el vehículo el papel se pierde.
    /// </summary>
    bool SalvoconductoEnLaMano = true);
