using System.Security.Cryptography;
using System.Text;

namespace Sigti.Dominio.M06_Solicitudes;

/// <summary>
/// El congelamiento del contenido al enviar a autorización — `HU-004`.
///
/// ── Qué problema resuelve ───────────────────────────────────────────────────
/// <i>«En papel, el expediente que firma la jefatura es el que tiene enfrente. En un sistema sin
/// congelamiento, el solicitante puede cambiar el destino, la carga o la fecha después de que la
/// firma quedó registrada, y la autorización pasa a amparar algo que nunca se autorizó.»</i>
///
/// Y no hace falta mala fe: basta con corregir una fecha «para que quede bien». El expediente
/// resultante es indistinguible de uno legítimo, y <b>eso es exactamente lo que un auditor
/// busca</b>.
///
/// ── Por qué una huella y no una copia ───────────────────────────────────────
/// Guardar una copia del contenido también funcionaría, y duplicaría el dato: dos versiones que
/// hay que mantener y que alguien va a poder editar. La huella <b>no se puede editar para que
/// coincida</b> sin conocer el contenido original, y ocupa 64 caracteres.
/// </summary>
public static class ReglasDelCongelamiento
{
    /// <summary>
    /// La huella del contenido que se somete a autorización.
    ///
    /// ── Determinista, y por eso el orden es fijo ────────────────────────────
    /// La misma solicitud tiene que dar siempre la misma huella, en cualquier máquina y en
    /// cualquier momento. Por eso los campos van en un orden declarado —no el de un diccionario
    /// ni el de la reflexión, que cambian— y las fechas en formato invariante: en una máquina
    /// con configuración regional distinta, «03/05/2026» y «05/03/2026» darían huellas
    /// distintas para el mismo expediente, y el sistema reportaría una alteración que no ocurrió.
    /// </summary>
    public static string Huella(ContenidoSometido contenido)
    {
        // El separador es un carácter que no puede aparecer en los campos. Con un separador
        // corriente, «Choluteca|Danlí» y «Choluteca» + «Danlí» darían la misma huella, y se
        // podría mover texto de un campo al siguiente sin que la huella cambie.
        const char separador = '';

        var canonico = string.Join(separador,
            contenido.Dependencia,
            contenido.ObjetoDelTraslado,
            contenido.Destino,
            contenido.SolicitanteDeDerecho,
            contenido.Salida.ToString("yyyy-MM-dd"),
            contenido.Retorno.ToString("yyyy-MM-dd"),
            contenido.HoraDeSalida?.ToString("HH:mm") ?? "",
            contenido.HoraDeRetorno?.ToString("HH:mm") ?? "",
            contenido.HolguraDias.ToString());

        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(canonico)));
    }

    /// <summary>
    /// Si el contenido sigue siendo el que se congeló.
    ///
    /// <b>Nulo en la huella guardada no es «coincide»</b>: es un expediente que se envió antes
    /// de que existiera el congelamiento, y sobre ése no se puede afirmar nada. Devolverlo como
    /// íntegro sería certificar algo que nadie verificó.
    /// </summary>
    public static ResultadoDelCotejo Cotejar(string? huellaCongelada, ContenidoSometido actual)
    {
        if (huellaCongelada is null)
            return new ResultadoDelCotejo(null, Huella(actual), Veredicto.SinCongelar,
                "Este expediente se envió sin congelar su contenido, así que no hay contra qué " +
                "cotejar. No se puede afirmar que lo autorizado sea lo que hoy se ve.");

        var ahora = Huella(actual);

        return huellaCongelada == ahora
            ? new ResultadoDelCotejo(huellaCongelada, ahora, Veredicto.Intacto,
                "El contenido es el mismo que se sometió a autorización.")
            : new ResultadoDelCotejo(huellaCongelada, ahora, Veredicto.Alterado,
                "⚠️ El contenido cambió después de enviarse a autorización. Lo que se autorizó " +
                "no es lo que hoy está en el expediente.");
    }

    /// <summary>
    /// Exige que el contenido no haya cambiado. Se llama <b>antes de autorizar</b>.
    ///
    /// Un expediente sin congelar <b>no bloquea</b>: son los anteriores al congelamiento y
    /// negarles la autorización dejaría trabajo legítimo detenido por una función que no
    /// existía cuando se capturó. Se autoriza y el cotejo lo declara — que es lo que el auditor
    /// necesita ver.
    /// </summary>
    public static void ExigirIntacto(string? huellaCongelada, ContenidoSometido actual)
    {
        var cotejo = Cotejar(huellaCongelada, actual);

        if (cotejo.Veredicto == Veredicto.Alterado)
            throw new M07_ProgramacionYDespacho.BloqueoDuro("RN-04",
                cotejo.PorQue + " Autorizar así ampararía un contenido que nunca se sometió. " +
                "Lo que corresponde es devolver el expediente para corrección, que lo regresa a " +
                "borrador y obliga a enviarlo de nuevo — con su huella nueva.");
    }
}

/// <summary>
/// Lo que se somete a autorización, y por lo tanto lo que se congela.
///
/// <b>No incluye el estado ni el diario</b>: esos cambian por definición cuando el expediente
/// avanza, y meterlos haría que la huella no coincidiera nunca. Lo que se congela es lo que la
/// jefatura leyó para decidir.
/// </summary>
public sealed record ContenidoSometido(
    string Dependencia,
    string ObjetoDelTraslado,
    string Destino,
    string SolicitanteDeDerecho,
    DateOnly Salida,
    DateOnly Retorno,
    TimeOnly? HoraDeSalida,
    TimeOnly? HoraDeRetorno,
    int HolguraDias);

public enum Veredicto
{
    Intacto,

    /// <summary>El contenido cambió después del envío. Es el hallazgo.</summary>
    Alterado,

    /// <summary>
    /// Se envió antes de que existiera el congelamiento. <b>No es «intacto»</b>: es que no hay
    /// con qué comprobarlo.
    /// </summary>
    SinCongelar,
}

public sealed record ResultadoDelCotejo(
    string? Congelada, string Actual, Veredicto Veredicto, string PorQue);
