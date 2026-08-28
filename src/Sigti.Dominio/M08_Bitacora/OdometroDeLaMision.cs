using Sigti.Dominio.M09_Combustible;

namespace Sigti.Dominio.M08_Bitacora;

/// <summary>
/// Cómo se registró el retorno — `T-18` y su subtipo.
///
/// ── Por qué la distinción existe, y qué corrigió ─────────────────────────────
/// El hallazgo `HB3-04`: `BD-05` bloqueaba una lectura de retorno menor a la de salida
/// <b>siempre</b>, y eso era incompatible con `RN-79`, que establece que la constatación
/// física del retorno <i>«se registra tal cual, se marca la inconsistencia y el vehículo se
/// libera igual»</i>.
/// </summary>
public enum SubtipoDeRetorno
{
    /// <summary>
    /// El motorista registra su propio retorno, con el vehículo delante.
    ///
    /// Acá una lectura menor a la de salida <b>es error de digitación</b> y se corrige en el
    /// momento: hay alguien con el tablero a la vista. Por eso bloquea.
    /// </summary>
    Ordinario,

    /// <summary>
    /// Un tercero verifica que el vehículo está de vuelta sin que el motorista haya podido
    /// cerrar la bitácora — incapacitado, sin dispositivo, incomunicado.
    ///
    /// Acá <b>bloquear no arregla nada</b>: el vehículo ya está en el predio, y negarse a
    /// registrarlo lo deja secuestrado por un trámite mientras la delegación se queda sin
    /// unidad. Se registra tal cual y se marca la inconsistencia.
    /// </summary>
    Constatado,
}

/// <summary>
/// El acta de sustitución o reinicio de odómetro — <b>la única salida al bloqueo duro</b> de
/// `BD-05` en el `T-18` ordinario.
///
/// ── No es un permiso para saltarse la validación ─────────────────────────────
/// Es <b>un hecho mecánico que hay que poder registrar</b>. Un odómetro se rompe y se cambia,
/// y el instalado empieza en cero o en la lectura que traiga. Sin poder declararlo, el
/// sistema obligaría a mentir en el número o dejaría el vehículo sin poder retornar.
///
/// La levanta `ACT-11` Encargado de Mantenimiento <b>antes de la salida</b>, con la lectura
/// del odómetro retirado y la del instalado.
///
/// ⚠️ <b>El circuito que la produce es de `M-11`, que no existe.</b> Este tipo es la forma en
/// que el dominio la recibe; hoy no hay pantalla ni tabla que la genere. Se modela igual
/// porque sin ella `BD-05` sería un bloqueo sin salida — y un bloqueo sin salida es el
/// hallazgo `HB3-02`, que ya se corrigió una vez en este documento.
/// </summary>
/// <param name="LecturaDelRetirado">Lo que marcaba el odómetro que se quitó.</param>
/// <param name="LecturaDelInstalado">Con cuánto arranca el nuevo. Puede ser cero.</param>
public sealed record ActaDeSustitucionDeOdometro(
    string Folio,
    DateOnly Fecha,
    int LecturaDelRetirado,
    int LecturaDelInstalado);

/// <summary>
/// Lo que hace falta para juzgar `BD-05` al <b>salir</b> — `T-14`.
/// </summary>
/// <param name="UltimaConocida">
/// La última lectura registrada para este vehículo. <b>Nula sólo cuando el vehículo no tiene
/// ninguna</b> — su primera misión.
///
/// El parámetro <b>no tiene valor por omisión</b>: el compilador obliga a que todo llamador
/// conteste, porque «no hay lectura previa» y «nadie consultó» no pueden verse igual en un
/// bloqueo duro.
/// </param>
/// <param name="Nivel">
/// El nivel del tanque a la salida — <b>dato obligatorio de bitácora</b> por `RN-83`.
///
/// ⚠️ <b>Nulo es «no consignado», no cero.</b> `RN-80` es explícita sobre la hoja de papel:
/// un campo que no se llenó se declara como no consignado y <b>no se estima</b>. Un cero
/// diría que el vehículo salió con el tanque vacío.
/// </param>
/// <param name="RazonSinNivel">
/// Por qué no se leyó el tanque. <b>Va con la lectura porque <i>es</i> la lectura</b>, en su
/// forma ausente: `RN-80` manda declarar el campo no consignado, y declararlo sin decir por
/// qué deja la ausencia sin nada que reclamar — no se sabe si faltó porque el indicador
/// estaba averiado o porque nadie se acordó.
/// </param>
public sealed record OdometroAlSalir(
    int Lectura, int? UltimaConocida, NivelDeTanque? Nivel = null,
    string? RazonSinNivel = null);

/// <summary>
/// Lo que hace falta para juzgar `BD-05` al <b>retornar</b> — `T-18`.
/// </summary>
/// <param name="Justificacion">
/// Obligatoria cuando la lectura de retorno <b>iguala</b> la de salida en una misión que se
/// ejecutó. No bloquea, pero no se pasa en silencio: <i>«es el patrón de la misión que nunca
/// se hizo»</i>, y ese patrón es lo que el Tribunal Superior de Cuentas busca.
/// </param>
/// <param name="Nivel">
/// El nivel al retorno. Con el de la salida es lo que permite separar el <b>remanente en
/// tanque</b> del consumo de la misión: sin los dos, <i>«salió lleno y volvió vacío»</i> no se
/// distingue de un faltante, y la conciliación de una misión corta con tanque grande no
/// significa nada.
/// </param>
public sealed record OdometroAlRetornar(
    int Lectura,
    SubtipoDeRetorno Subtipo,
    string? Justificacion = null,
    ActaDeSustitucionDeOdometro? Acta = null,
    NivelDeTanque? Nivel = null,
    string? RazonSinNivel = null);
