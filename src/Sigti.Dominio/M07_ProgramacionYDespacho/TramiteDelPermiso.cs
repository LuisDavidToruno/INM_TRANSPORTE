using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M07_ProgramacionYDespacho;

/// <summary>
/// En qué va el permiso. <b>Dos estados, y la diferencia es todo `BD-04`.</b>
///
/// Un trámite abierto no ampara nada: es una petición. Si <see cref="Firmado"/> y
/// <see cref="Solicitado"/> se trataran igual, cualquiera podría destrabar el despacho de un
/// domingo abriendo un trámite y despachando sin esperar la firma — que es exactamente lo que
/// el permiso existe para impedir.
/// </summary>
public enum EstadoDelPermiso
{
    /// <summary>Encaminado a la máxima autoridad. <b>No ampara nada todavía.</b></summary>
    Solicitado,

    /// <summary>Firmado por `ACT-09`. Es el único que `BD-04` mira.</summary>
    Firmado,

    /// <summary>
    /// Retirado antes de firmarse — la misión se reprogramó a franja hábil, o se anuló.
    ///
    /// No se borra: <b>que alguien haya pedido circular un domingo es un hecho</b>, y que se
    /// haya desistido también. Un trámite desaparecido y uno que nunca existió se ven iguales.
    /// </summary>
    Desistido,
}

/// <summary>
/// Lo que se necesita para juzgar si se puede <b>abrir</b> un trámite de permiso.
/// </summary>
/// <param name="Excepcion">
/// Nula cuando el vehículo no tiene servicio exceptuado, que es el caso normal. `RN-24`: es
/// atributo del vehículo, <b>no del viaje</b>.
/// </param>
/// <param name="Existentes">
/// Los trámites y permisos ya registrados para el expediente. Los desistidos entran: la regla
/// necesita distinguirlos para no confundir un trámite retirado con uno vivo.
/// </param>
public sealed record AperturaDelPermiso(
    Ulid Expediente,
    string Destino,
    DateOnly Desde,
    DateOnly Hasta,
    ServicioExceptuado? Excepcion,
    IReadOnlyList<PermisoEnTramite> Existentes);

/// <summary>
/// Un permiso en cualquiera de sus estados.
/// </summary>
/// <param name="Vehiculo">
/// <b>Nulo mientras la misión no esté programada.</b> `RN-23` dice dos cosas que sólo se
/// cumplen a la vez si se separa abrir de firmar: el permiso no exige que la misión esté
/// programada, y el permiso es nominativo. Se abre sin vehículo; <b>no se firma sin él</b>.
/// </param>
/// <param name="TramosInhabiles">
/// Los días y franjas que el permiso viene a cubrir, enumerados. Van en el documento porque
/// <b>el agente en carretera lee el papel</b>: un permiso que dice «ampara del 1 al 5» sin
/// decir qué días de esos eran inhábiles no le permite verificar nada.
/// </param>
public sealed record PermisoEnTramite(
    Ulid Id,
    string Folio,
    EstadoDelPermiso Estado,
    Ulid? Vehiculo,
    Ulid? Motorista,
    string Destino,
    DateOnly Desde,
    DateOnly Hasta,
    string Justificacion,
    IReadOnlyList<string> TramosInhabiles,
    IdPersona Solicita,
    IdPersona? FirmadoPor)
{
    /// <summary>
    /// Si este trámite cubre lo mismo que se está por pedir. <b>Sólo cuenta lo vivo</b>: un
    /// desistido no estorba, porque desistir es justamente decir que ya no se pide.
    /// </summary>
    public bool Cubre(string destino, DateOnly desde, DateOnly hasta) =>
        Estado != EstadoDelPermiso.Desistido
        && string.Equals(Destino, destino, StringComparison.OrdinalIgnoreCase)
        && Desde <= desde
        && Hasta >= hasta;
}

/// <summary>Por qué no hizo falta abrir el trámite. Nulo cuando sí hizo falta.</summary>
public sealed record NoHaceFalta(string Motivo, string Detalle);

/// <summary>
/// `HU-016` — el trámite y la firma del permiso de circulación en día u hora inhábil.
///
/// ── Por qué abrir y firmar son dos actos ────────────────────────────────────
/// [`RN-23`](RN-23) dice que el permiso <b>no requiere que la misión esté programada</b>, y a la
/// vez que es <b>nominativo</b> sobre vehículo, ruta y ventana. Las dos cosas no se cumplen a la
/// vez sobre un solo acto: si el permiso naciera firmado habría que exigir el vehículo desde el
/// principio, y el trámite no podría adelantarse a la programación — que es justo lo que hay que
/// poder hacer un viernes por la tarde para una salida del sábado.
///
/// Resolución `HCU-05` de `CU-03`: se separan. Se abre sin vehículo; <b>no se firma sin él</b>.
///
/// ── Y por qué la firma es indelegable ───────────────────────────────────────
/// `[C]` insumo #29. Hasta que la institución confirme lo contrario, el sistema <b>no la
/// permite</b>. No es una omisión conservadora por comodidad: si se habilitara por defecto y
/// después resultara indelegable, cada permiso firmado por delegación sería un vehículo del
/// Estado que circuló un domingo sin amparo válido, y no habría forma de repararlo hacia atrás.
/// Al revés sí se repara — se habilita y se sigue.
/// </summary>
public static class ReglasDelPermiso
{
    /// <summary>
    /// ¿Hace falta abrir el trámite? Devuelve el motivo cuando <b>no</b> hace falta.
    ///
    /// Se contesta antes de bloquear nada: decirle a alguien «no puede» cuando la respuesta es
    /// «no le hace falta» lo manda a resolver un problema que no tiene.
    /// </summary>
    public static NoHaceFalta? PorQueNoHaceFalta(AperturaDelPermiso apertura)
    {
        // `RN-24` — la excepción es atributo del VEHÍCULO. Una ambulancia con excepción vigente
        // no tramita nada: sale, y la Orden de Misión registra la excepción con su fundamento.
        if (apertura.Excepcion is { } e && e.VigenteAl(apertura.Desde))
        {
            return new NoHaceFalta(
                "SERVICIO_EXCEPTUADO",
                $"El vehículo tiene excepción de servicio exceptuado vigente" +
                (e.Hasta is { } hasta ? $" hasta el {hasta:dd/MM/yyyy}" : " sin fecha de cierre") +
                ". No requiere permiso (RN-24).");
        }

        if (apertura.Existentes.FirstOrDefault(
                p => p.Cubre(apertura.Destino, apertura.Desde, apertura.Hasta)) is { } ya)
        {
            return new NoHaceFalta(
                "YA_EXISTE",
                $"Ya existe el permiso {ya.Folio} para ese destino y esa ventana " +
                $"({ya.Desde:dd/MM/yyyy} al {ya.Hasta:dd/MM/yyyy}), en estado " +
                $"{Texto(ya.Estado)}. Dos permisos para una misma circulación rompen la " +
                "conciliación.");
        }

        return null;
    }

    /// <summary>
    /// Por qué no se puede firmar. <b>Nulo es que sí se puede.</b>
    ///
    /// Devuelve el motivo en lugar de lanzar porque <b>el intento se registra igual</b>: que
    /// alguien que no es la máxima autoridad haya intentado firmar un permiso de circulación es
    /// precisamente lo que un control interno quiere poder ver.
    /// </summary>
    /// <param name="rolesDeQuienFirma">
    /// Los roles vigentes de quien firma, a la fecha del hecho. <b>Vacío no es «no tiene
    /// permiso»</b>: es que no se pudo resolver su puesto, y eso también bloquea — pero por otra
    /// razón, y el mensaje lo dice.
    /// </param>
    public static string? PorQueNoSeFirma(
        PermisoEnTramite permiso, IReadOnlyCollection<Rol> rolesDeQuienFirma)
    {
        if (permiso.Estado == EstadoDelPermiso.Firmado)
            return $"El permiso {permiso.Folio} ya está firmado por " +
                   $"{permiso.FirmadoPor?.Valor}. Una segunda firma no agrega amparo: " +
                   "duplica el documento que el agente en carretera compara.";

        if (permiso.Estado == EstadoDelPermiso.Desistido)
            return $"El permiso {permiso.Folio} fue desistido. Abra un trámite nuevo si la " +
                   "misión volvió a caer en franja inhábil.";

        if (rolesDeQuienFirma.Count == 0)
            return "No se pudo resolver el puesto de quien firma, así que no se puede " +
                   "comprobar que sea la máxima autoridad. La firma del permiso de circulación " +
                   "no se concede sin esa comprobación.";

        // ⚠️ `RN-07` **no está habilitada para esta facultad** — insumo #29. Se comprueba el rol
        // propio, no una delegación: una delegación para autorizar solicitudes no alcanza acá,
        // y ése es un escenario propio de `HU-016`.
        if (!rolesDeQuienFirma.Contains(Rol.MaximaAutoridad))
            return "Esta facultad es de la máxima autoridad y se trata como indelegable " +
                   "mientras no se confirme lo contrario. Las salidas son reprogramar la " +
                   "ventana a franja hábil o esperar la firma (RN-23, insumo #29).";

        // El nominativo. Es lo último porque es lo único que quien firma puede mandar a
        // arreglar: los tres anteriores no dependen de la misión.
        if (permiso.Vehiculo is null || permiso.Motorista is null)
            return "El permiso es nominativo sobre vehículo, ruta y ventana. Programe la " +
                   "misión antes de firmar (RN-23).";

        return null;
    }

    public static string Texto(EstadoDelPermiso estado) => estado switch
    {
        EstadoDelPermiso.Solicitado => "solicitado",
        EstadoDelPermiso.Firmado => "firmado",
        EstadoDelPermiso.Desistido => "desistido",
        _ => estado.ToString(),
    };
}
