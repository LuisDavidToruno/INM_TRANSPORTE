using System.Security.Cryptography;
using System.Text;

using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M15_Formatos;

/// <summary>
/// Qué dice el punto de verificación cuando alguien escanea el QR.
///
/// ── ⚠️ Por qué son tres y no dos ────────────────────────────────────────────
/// «Vigente o anulado» no alcanza, y `RN-25` lo dice explícitamente: el documento se imprime
/// <b>antes</b> de salir —una delegación sin cobertura lo emite por anticipado— y la misión
/// puede cambiar después. El papel que el motorista lleva en la mano deja de corresponder sin
/// que nadie lo anule.
///
/// Un verificador que consulte y lea «vigente» sobre un papel que ampara a otro motorista está
/// recibiendo una respuesta correcta a la pregunta equivocada.
/// </summary>
public enum EstadoDelSalvoconducto
{
    /// <summary>Lo impreso sigue coincidiendo con lo que la misión tiene hoy.</summary>
    Vigente,

    /// <summary>
    /// <b>El papel ya no corresponde.</b> Nadie lo anuló: la misión cambió debajo de él —un
    /// relevo de motorista, una sustitución de vehículo, una ventana corrida.
    ///
    /// No es lo mismo que anulado, y la diferencia importa en carretera: anulado significa que
    /// el documento nunca debió usarse; desactualizado, que ampara algo que ya no es el viaje.
    /// </summary>
    Desactualizado,

    /// <summary>El permiso se retiró o el expediente se anuló.</summary>
    Anulado,
}

/// <summary>
/// Lo que va impreso en el papel, congelado al emitir.
///
/// ── Se congela, no se deriva ────────────────────────────────────────────────
/// El documento es lo que dice el papel, y el papel no cambia cuando cambia la base. Derivar
/// estos campos al reimprimir produciría dos impresiones <b>del mismo folio con contenidos
/// distintos</b>, que es exactamente lo que la huella existe para hacer imposible.
/// </summary>
/// <param name="Vehiculo">
/// Cómo se identifica en el papel. <b>El correlativo institucional primero</b>: `RN-15` y
/// `CE-17` — hay vehículos del Estado circulando sin lámina metálica por el desabastecimiento
/// nacional, y un documento que sólo diga la placa no identifica a esos.
/// </param>
/// <param name="TramosInhabiles">
/// Qué días y franjas ampara. Va impreso porque <b>el agente compara el papel con el día del
/// control</b>: un permiso que dice «del 4 al 7» sin decir cuáles de esos eran inhábiles no le
/// deja verificar que el control de hoy esté cubierto.
/// </param>
public sealed record ContenidoDelSalvoconducto(
    string FolioDelPermiso,
    string Vehiculo,
    string Motorista,
    string Destino,
    DateOnly Desde,
    DateOnly Hasta,
    IReadOnlyList<string> TramosInhabiles,
    string Justificacion,
    IdPersona FirmadoPor,
    DateTimeOffset FirmadoEn);

/// <summary>
/// `HU-017` y `RN-25` — el salvoconducto impreso: folio, QR, huella y vigencia explícita.
///
/// ── Por qué este documento existe ───────────────────────────────────────────
/// <b>El control en carretera es físico.</b> El destinatario del papel no se autentica, no tiene
/// usuario y no verá nunca el expediente: el agente del TSC o de la DNVT en un operativo no va a
/// consultar un sistema, va a pedir un papel. Y un papel que no se puede verificar vale lo mismo
/// que uno falsificado.
///
/// ── Y por qué lleva DOS mecanismos de verificación ──────────────────────────
/// El QR resuelve a un punto de verificación, y <b>en zona sin señal no se puede escanear</b>.
/// `RN-25` obliga por eso a un <b>código corto</b> legible y consultable después, más datos
/// suficientes para el control visual. La verificación en línea no puede ser el único mecanismo
/// en un país con la conectividad que documenta `NRM-09`.
/// </summary>
public static class ReglasDelSalvoconducto
{
    /// <summary>
    /// Por qué no se puede emitir. <b>Nulo es que sí.</b>
    /// </summary>
    /// <param name="permisoFirmado">
    /// Si el permiso que este documento materializa está firmado por la máxima autoridad.
    /// <b>Sin firma no hay nada que imprimir</b>: el salvoconducto no autoriza, materializa una
    /// autorización que ya ocurrió.
    /// </param>
    public static string? PorQueNoSeEmite(bool permisoFirmado, bool yaEmitido) =>
        !permisoFirmado
            ? "No existe permiso firmado por la máxima autoridad. El salvoconducto materializa " +
              "un permiso; sin firma no hay documento que emitir."
            : yaEmitido
                ? "Este permiso ya tiene salvoconducto emitido. Para volver a tenerlo en la " +
                  "mano, reimprima: dos folios para un mismo permiso rompen la conciliación " +
                  "(RN-04)."
                : null;

    /// <summary>
    /// La huella del documento electrónico — `RN-25` punto 3.
    ///
    /// ── El separador no es cosmético ────────────────────────────────────────
    /// Es un carácter <b>no imprimible</b> (<c></c>, separador de unidad) por la misma
    /// razón que en el congelamiento de la solicitud: con un separador tecleable, dos
    /// contenidos distintos pueden producir la misma cadena. «Choluteca|2026» y
    /// «Choluteca|2026» compuestos de piezas diferentes colisionarían, y la huella dejaría de
    /// distinguir dos documentos que no son el mismo.
    /// </summary>
    public static string Huella(ContenidoDelSalvoconducto c)
    {
        const char sep = '';

        var canonico = string.Join(sep,
            c.FolioDelPermiso,
            c.Vehiculo,
            c.Motorista,
            c.Destino,
            c.Desde.ToString("yyyy-MM-dd"),
            c.Hasta.ToString("yyyy-MM-dd"),
            string.Join(',', c.TramosInhabiles),
            c.Justificacion,
            c.FirmadoPor.Valor,
            c.FirmadoEn.ToString("O"));

        return "sha256:" + Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(canonico)));
    }

    /// <summary>
    /// El código corto que se lee en voz alta y se teclea — <b>para cuando no hay señal</b>.
    ///
    /// ── Por qué existe, y por qué es corto ──────────────────────────────────
    /// El agente en una carretera sin cobertura no puede escanear el QR. Se lleva el código
    /// anotado y lo consulta al volver. Una huella de 64 caracteres hexadecimales no se copia a
    /// mano sin equivocarse; ocho sí.
    ///
    /// ── ⚠️ Y por qué NO reemplaza a la huella ───────────────────────────────
    /// Ocho caracteres son un <b>localizador</b>, no una prueba criptográfica: sirven para
    /// encontrar el documento y compararlo, no para demostrar que el papel no fue alterado. Eso
    /// lo hace la huella completa, que también va impresa.
    ///
    /// Se excluyen las letras que se confunden al dictar o al escribir a mano —<c>I</c> con
    /// <c>1</c>, <c>O</c> con <c>0</c>— porque el código se dicta por teléfono.
    /// </summary>
    public static string CodigoCorto(string huella)
    {
        const string cifras = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

        // Del hexadecimal de la huella, no del texto completo: es lo que ya está calculado.
        var hex = huella[(huella.IndexOf(':') + 1)..];
        var salida = new StringBuilder(9);

        for (var i = 0; i < 8; i++)
        {
            var pareja = Convert.ToInt32(hex.Substring(i * 2, 2), 16);
            salida.Append(cifras[pareja % cifras.Length]);
            if (i == 3) salida.Append('-');
        }

        return salida.ToString();
    }

    /// <summary>
    /// En qué estado responde el punto de verificación.
    ///
    /// <b>Se compara lo impreso contra lo que la misión tiene hoy</b>, no se lee una columna:
    /// «desactualizado» no es algo que alguien declare — es lo que pasa solo cuando la misión
    /// cambia debajo de un papel ya impreso, y nadie está ahí para marcarlo.
    /// </summary>
    /// <param name="ahora">
    /// Lo que la misión tiene hoy. <b>Nulo cuando ya no hay asignación</b> —la misión se
    /// desprogramó— y eso también desactualiza el papel.
    /// </param>
    public static EstadoDelSalvoconducto Estado(
        ContenidoDelSalvoconducto impreso, ContenidoDelSalvoconducto? ahora, bool anulado)
    {
        if (anulado) return EstadoDelSalvoconducto.Anulado;
        if (ahora is null) return EstadoDelSalvoconducto.Desactualizado;

        // Los cuatro elementos que el permiso ampara, y sólo esos: la justificación puede
        // corregirse sin que el papel deje de corresponder al viaje.
        var mismo =
            impreso.Vehiculo == ahora.Vehiculo
            && impreso.Motorista == ahora.Motorista
            && string.Equals(impreso.Destino, ahora.Destino, StringComparison.OrdinalIgnoreCase)
            && impreso.Desde == ahora.Desde
            && impreso.Hasta == ahora.Hasta;

        return mismo ? EstadoDelSalvoconducto.Vigente : EstadoDelSalvoconducto.Desactualizado;
    }

    /// <summary>
    /// Qué se le dice a quien verifica, en palabras que sirvan en la carretera.
    ///
    /// El estado por sí solo no basta: «desactualizado» no le dice a un agente si puede dejar
    /// pasar el vehículo. La frase tiene que decir <b>qué hacer</b>.
    /// </summary>
    public static string Veredicto(EstadoDelSalvoconducto estado) => estado switch
    {
        EstadoDelSalvoconducto.Vigente =>
            "Documento válido. Ampara al vehículo, al motorista, al destino y a la ventana " +
            "que están impresos. Compare los cuatro con lo que tiene enfrente.",

        EstadoDelSalvoconducto.Desactualizado =>
            "⚠️ El documento fue emitido válidamente y YA NO CORRESPONDE: la misión cambió " +
            "después de imprimirlo. No ampara la circulación de hoy. Consulte con la " +
            "institución emisora antes de dejar circular.",

        EstadoDelSalvoconducto.Anulado =>
            "⚠️ Documento anulado. No ampara ninguna circulación.",

        _ => "Estado desconocido.",
    };

    /// <summary>
    /// Por qué no se puede reimprimir. <b>Nulo es que sí.</b>
    ///
    /// La reimpresión <b>conserva el folio, el contenido y la huella</b> — `RN-04`: el folio no
    /// se recicla y dos folios para un mismo permiso rompen la conciliación. Lo único que se
    /// agrega es el asiento de quién reimprimió, cuándo y por qué.
    /// </summary>
    public static string? PorQueNoSeReimprime(string? motivo) =>
        string.IsNullOrWhiteSpace(motivo)
            ? "Diga por qué se reimprime. Una reimpresión sin motivo es indistinguible de una " +
              "copia de más, y el conteo de impresiones deja de significar algo."
            : null;
}
