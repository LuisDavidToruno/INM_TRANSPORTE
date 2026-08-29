using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M01_Organizacion;

/// <summary>
/// Un acto ya ejercido sobre un expediente, y quién lo ejerció.
/// </summary>
/// <param name="Referencia">
/// Cómo nombrar el acto en el mensaje del bloqueo. §5.3.B.1 exige precisión: *«Usted registró
/// la solicitud SOL-2026-00417 el 03/08/2026»*. <b>Un mensaje genérico produce una llamada a
/// soporte; uno preciso produce la acción correcta.</b>
/// </param>
public sealed record ActoDelExpediente(
    Funcion Funcion,
    IdPersona Quien,
    string Referencia,
    DateOnly Fecha);

/// <summary>
/// Lo que ya se hizo sobre un expediente — <b>la entrada del control bloqueante de §5.3.B</b>.
///
/// ── Por qué se recibe y no se deriva ────────────────────────────────────────
/// Las cinco funciones viven en módulos distintos: solicitar y autorizar están en el diario de
/// la Orden de Misión, conducir está en la asignación, y <b>entregar el fondo está en `M-09`</b>,
/// que la misión no conoce. Una clase que fuera a buscarlas tendría que conocer los cinco
/// módulos, y sería la única del dominio que los conoce a todos.
///
/// ── Y por qué es obligatorio, no opcional ───────────────────────────────────
/// Mismo criterio que <c>CustodiaAlDespachar</c> y <c>ConflictoPorIndisponibilidad</c>: es la
/// diferencia entre <b>«nadie más actuó» y «nadie preguntó»</b>, y en un bloqueo duro las dos
/// no pueden verse igual. El compilador obliga a que todo llamador conteste.
/// </summary>
public sealed record ActosDelExpediente(IReadOnlyList<ActoDelExpediente> Actos)
{
    /// <summary>
    /// El expediente sobre el que todavía no actuó nadie más.
    ///
    /// <b>Es una afirmación, no un valor por omisión.</b> Se escribe cuando consta que no hay
    /// actos previos, y por eso tiene nombre en vez de ser una lista vacía anónima.
    /// </summary>
    public static ActosDelExpediente Ninguno { get; } = new([]);

    public IReadOnlyList<ActoDelExpediente> De(Funcion funcion) =>
        [.. Actos.Where(a => a.Funcion == funcion)];
}

/// <summary>
/// Un intento bloqueado — <b>§5.3.B.2</b>.
///
/// <i>«Se registra el intento en la pista de auditoría: persona, puesto, acción pretendida,
/// misión, par de incompatibilidad detectado, marca de tiempo y origen. El intento bloqueado es
/// información de control, no ruido. Un mismo usuario intentando quince veces autorizar sus
/// propias solicitudes es exactamente lo que Auditoría Interna quiere ver»</i>.
/// </summary>
public sealed record IntentoBloqueado(
    IdPersona Quien,
    Funcion Pretendia,
    string Expediente,
    string Par,
    Funcion ChocaCon,
    IdPersona QuienLaEjercio,
    string Referencia,
    DateTimeOffset Momento);

/// <summary>
/// El bloqueo por segregación, con todo lo que §5.3.B exige poder decir.
/// </summary>
public sealed class SegregacionIncompatible(
    string par,
    string mensaje,
    IntentoBloqueado intento) : Exception(mensaje)
{
    /// <summary>El par de §5.2 que se activó. Va en la respuesta y en la pista.</summary>
    public string Par { get; } = par;

    /// <summary>Lo que se registra en la pista de auditoría, ya armado.</summary>
    public IntentoBloqueado Intento { get; } = intento;
}

/// <summary>
/// Una advertencia de `I-15` o `I-16`: se puede continuar <b>exigiendo motivo escrito</b>.
/// </summary>
public sealed record AdvertenciaDeSegregacion(
    string Par, Funcion ChocaCon, string Referencia, string PorQue);

/// <summary>
/// <b>El control bloqueante de §5.3.B</b> — impedir el acto sobre un expediente concreto.
///
/// ── Los dos controles no son el mismo, y hacen falta los dos ────────────────
/// El <b>preventivo</b> (§5.3.A, <see cref="ReglasDeLaAsignacion"/>) mira la acumulación de
/// roles al otorgarlos y sólo puede rechazar lo absoluto: prohibir de entrada que el Encargado
/// de Delegación sea también Solicitante sería inoperante. El <b>bloqueante</b> es donde se
/// decide de verdad: *«es aquí donde se decide de verdad»*, porque acá sí hay un expediente y
/// se puede comparar persona contra persona.
///
/// ── Qué NO hace esta clase ──────────────────────────────────────────────────
/// <b>No busca los actos previos</b> —se los pasan— ni escribe en la pista de auditoría: arma
/// el asiento y quien la llama lo persiste. Escribir desde el dominio la obligaría a conocer la
/// base, y el bloqueo dejaría de ser probable sin ella.
///
/// ── La relación con `BD-01`, que ya existía ─────────────────────────────────
/// `BD-01` es la expresión que §10.2 le da a este control <b>sobre la autorización de una
/// misión</b>, y sigue donde está: es el identificador que la máquina de estados publica, y
/// cambiarlo rompería la trazabilidad de todo lo que lo cita. Esta clase cubre <b>los pares que
/// `BD-01` no alcanza</b> —despacho, fondo, liquidación, conducción— que hasta ahora no
/// bloqueaban en ninguna parte.
/// </summary>
public static class ReglasDeSegregacion
{
    /// <summary>
    /// Impide consumar el acto si choca con algo ya ejercido en el mismo expediente.
    ///
    /// <b>No se guarda nada</b> (§5.3.B.1). Lanza <see cref="SegregacionIncompatible"/> con el
    /// par, el mensaje preciso y el asiento de auditoría ya armado.
    /// </summary>
    /// <param name="i14Activo">
    /// `I-14` —emitir la Orden de Misión y liquidarla— <b>está apagado por omisión</b>: no está
    /// en la enumeración del MARCI. Se recibe explícito para que activarlo sea una decisión de
    /// la institución y no un valor escondido acá.
    /// </param>
    /// <returns>
    /// Las advertencias de `I-15` e `I-16`, que <b>no impiden</b> pero exigen motivo escrito.
    /// Se devuelven en vez de lanzarse porque son otra cosa: *«para las advertencias sí se
    /// permite continuar»*.
    /// </returns>
    public static IReadOnlyList<AdvertenciaDeSegregacion> Exigir(
        IdPersona quienPretende,
        Funcion pretende,
        ActosDelExpediente previos,
        string expediente,
        DateTimeOffset momento,
        bool i14Activo = false)
    {
        var advertencias = new List<AdvertenciaDeSegregacion>();

        foreach (var par in Incompatibilidades.Tabla)
        {
            // El par se activa en cualquiera de los dos sentidos: quien autoriza no puede
            // liquidar, y quien liquida no puede autorizar. Es el mismo par leído al revés.
            var otra = par.Una == pretende ? par.Otra
                : par.Otra == pretende ? par.Una
                : (Funcion?)null;

            if (otra is not { } choca) continue;

            // Los absolutos ya se rechazaron al asignar el rol. Acá se comparan los actos de un
            // expediente, y un par absoluto no habla de actos: habla de acumulación de roles.
            if (par.Alcance == AlcanceDelPar.Absoluto) continue;

            if (par.Nivel == NivelDeIncompatibilidad.Configurable && !i14Activo) continue;

            // **La misma persona.** `BD-01` es explícito: un mismo servidor con dos cuentas
            // sigue siendo la misma persona, y la comparación es por identidad de persona.
            var propio = previos.De(choca).FirstOrDefault(a => a.Quien == quienPretende);

            if (propio is null) continue;

            if (par.Nivel == NivelDeIncompatibilidad.Advertencia)
            {
                advertencias.Add(new AdvertenciaDeSegregacion(
                    par.Id, choca, propio.Referencia, par.PorQue));

                continue;
            }

            var intento = new IntentoBloqueado(
                quienPretende, pretende, expediente, par.Id, choca, propio.Quien,
                propio.Referencia, momento);

            throw new SegregacionIncompatible(par.Id, Mensaje(par, pretende, propio), intento);
        }

        return advertencias;
    }

    /// <summary>
    /// El mensaje de §5.3.B.1.
    ///
    /// <b>Nombra el acto concreto, su fecha y el par.</b> El ejemplo del documento es literal:
    /// *«Usted registró la solicitud SOL-2026-00417 el 03/08/2026. No puede autorizarla»*. Lo
    /// que falta —a qué puesto corresponde— exige la jerarquía de puestos, que el espejo
    /// todavía no trae; se dice qué falta en vez de inventar un destinatario.
    /// </summary>
    private static string Mensaje(ParIncompatible par, Funcion pretende, ActoDelExpediente propio)
    {
        var nucleo = par.Nivel == NivelDeIncompatibilidad.NucleoIrreductible
            ? " Es del núcleo irreductible: no se levanta por régimen de excepción, ni por " +
              "delegación, ni por emergencia, ni por resolución de la máxima autoridad."
            : string.Empty;

        // **No dice «sobre este expediente».** El control opera igual sobre una Orden de
        // Misión y sobre un fondo de combustible —que es objeto de período y no un
        // expediente—, y nombrar mal el objeto en el mensaje del bloqueo lo vuelve
        // sospechoso justo cuando tiene que ser creíble.
        return
            $"Usted ya ejerció {EnPalabras(propio.Funcion)} " +
            $"({propio.Referencia}, el {propio.Fecha:dd/MM/yyyy}). No puede además " +
            $"{EnPalabras(pretende)}: es la incompatibilidad {par.Id}. {par.PorQue}{nucleo} " +
            "Lo que corresponde es que el acto lo ejerza otra persona, o que se escale al " +
            "puesto superior.";
    }

    /// <summary>
    /// La función dicha como la diría quien opera.
    ///
    /// El identificador del enum va en la pista de auditoría; <b>el mensaje al usuario no lleva
    /// nombres de tipos</b>, porque quien lo lee está tratando de resolver un trámite.
    /// </summary>
    private static string EnPalabras(Funcion funcion) => funcion switch
    {
        Funcion.Solicita => "la solicitud",
        Funcion.Autoriza => "la autorización",
        Funcion.Despacha => "el despacho",
        Funcion.EntregaFondo => "la entrega del fondo",
        Funcion.Liquida => "la liquidación",
        Funcion.Conduce => "la conducción",
        Funcion.SolicitaFondo => "la solicitud del fondo",
        Funcion.ApruebaFondo => "la aprobación del fondo",
        Funcion.HabilitaLicencia => "la habilitación de la licencia",
        Funcion.Custodia => "la custodia del vehículo",
        Funcion.ProponeDescargo => "la propuesta de descargo",
        Funcion.ApruebaDescargo => "la aprobación del descargo",
        Funcion.OrdenaMantenimiento => "la orden de mantenimiento",
        Funcion.RecibeConforme => "la recepción conforme",
        Funcion.EmiteOrdenDeMision => "la emisión de la Orden de Misión",
        Funcion.Audita => "la auditoría",
        Funcion.Administra => "la administración del sistema",

        // Una función nueva sin traducir se ve rara; una escondida deja el mensaje sin sujeto.
        _ => funcion.ToString(),
    };

    /// <summary>
    /// A quién se escala — <b>§5.3.B.3, parcialmente</b>.
    ///
    /// ⚠️ El documento pide tres saltos: *«primero, al puesto superior del que intentó actuar,
    /// dentro de la misma unidad; si no existe o está vacante, al puesto de sede central
    /// designado como respaldo de esa delegación; si tampoco, a `ACT-08`»*.
    ///
    /// <b>Los dos primeros exigen la jerarquía de puestos y el respaldo designado</b>, y el
    /// espejo del organigrama hoy sólo trae persona↔puesto: no trae puesto superior ni unidad.
    /// Inventar un destinatario sería peor que decir que falta, porque la misión quedaría
    /// «visiblemente pendiente» en una bandeja equivocada.
    ///
    /// Lo que sí se puede hoy es el tercer salto, que el documento fija como último recurso.
    /// </summary>
    public static string DestinoDelEscalamiento(bool hayJerarquiaEnElEspejo) =>
        hayJerarquiaEnElEspejo
            ? "el puesto superior dentro de la misma unidad"
            : "Gerencia Administrativa (ACT-08). El espejo del organigrama no trae todavía el " +
              "puesto superior ni el respaldo de sede, así que los dos primeros saltos del " +
              "escalamiento no se pueden resolver: se va directo al último recurso.";
}
