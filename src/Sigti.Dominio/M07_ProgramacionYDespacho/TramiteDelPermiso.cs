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
    /// A qué permiso reemplaza. <b>Nulo en el primero</b>, que es el caso normal.
    ///
    /// La referencia cruzada de `RN-04`. Sin ella hay dos permisos sueltos para una misma misión
    /// y nada dice cuál superó a cuál: un auditor ve dos folios, dos firmas y dos salvoconductos,
    /// y tiene que reconstruir el orden por las fechas.
    /// </summary>
    public Ulid? Reemplaza { get; init; }

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
/// Por qué un permiso firmado dejó de cubrir la misión.
/// </summary>
/// <param name="ExigeReemision">
/// ⚠️ <b>No todo lo que deja de cubrir hay que reemitirlo.</b>
///
/// Una misión desprogramada no ampara nada <b>en este momento</b>, y si se reprograma con el
/// mismo vehículo y el mismo motorista el permiso vuelve a amparar solo. Ofrecer «reemitir» ahí
/// quemaría un folio —que no se recicla— y exigiría otra firma de la máxima autoridad para nada.
///
/// Falso significa <b>espere</b>; verdadero significa <b>actúe</b>. Un aviso que no distingue
/// las dos cosas convierte cada reprogramación en un trámite.
/// </param>
public sealed record NoCubre(string Detalle, bool ExigeReemision);

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

    /// <summary>
    /// Por qué este permiso firmado <b>ya no cubre</b> la misión. Nulo es que sigue cubriendo.
    ///
    /// ── ⚠️ Por qué dice QUÉ cambió y no sólo que cambió ─────────────────────
    /// «El permiso ya no cubre la misión» manda a alguien a comparar cuatro cosas a mano
    /// contra un papel. Cada elemento tiene su propio arreglo y su propia urgencia: una ventana
    /// corrida se reprograma, un vehículo en taller se sustituye, un motorista incapacitado se
    /// releva. El mensaje que no nombra el elemento convierte una acción en una investigación.
    ///
    /// ── Y por qué no es lo mismo que <see cref="PermisoDeCirculacion.Ampara"/> ──
    /// `Ampara` contesta sí o no en el camino crítico del despacho, y `BD-04` lo usa para
    /// bloquear. Esto contesta <b>por qué no</b>, antes de llegar ahí, para que nadie descubra
    /// el problema el sábado por la mañana con el vehículo cargado.
    /// </summary>
    /// <param name="enRuta">
    /// Si la misión <b>ya salió</b>.
    ///
    /// ⚠️ Excepción deliberada de `HU-018`: <b>un relevo documentado en ruta no invalida el
    /// permiso de la misión ya iniciada</b>. El vehículo está en la carretera; declarar el papel
    /// inválido no lo devuelve, y sí dejaría al motorista relevado circulando sin nada. El
    /// traspaso consta en acta con corte de odómetro (`RN-71`), y lo que sí exige permiso
    /// reemitido es la circulación en franja inhábil <b>posterior</b> al traspaso — que es de
    /// `M-08` y no se bloquea.
    /// </param>
    public static NoCubre? PorQueYaNoCubre(
        PermisoEnTramite permiso,
        Ulid? vehiculoDeHoy,
        Ulid? motoristaDeHoy,
        string destinoDeHoy,
        DateOnly desdeDeHoy,
        DateOnly hastaDeHoy,
        string nombreDeLoAmparado,
        string nombreDeLoDeHoy,
        bool enRuta)
    {
        if (permiso.Estado != EstadoDelPermiso.Firmado) return null;

        if (enRuta) return null;

        // Se comparan en el orden en que se arreglan: lo que exige otra persona primero.
        if (permiso.Vehiculo != vehiculoDeHoy)
        {
            // ⚠️ Sin vehículo asignado **no se ordena reemitir**, y la diferencia es cara: si la
            // misión se reprograma con el mismo vehículo y el mismo motorista, el permiso sigue
            // amparando. Reemitirlo quemaría un folio y exigiría otra firma de la máxima
            // autoridad para nada.
            return vehiculoDeHoy is null
                ? new NoCubre(
                    "La misión no tiene vehículo asignado en este momento, así que el permiso " +
                    "no ampara nada. Si se reprograma con el mismo vehículo y el mismo " +
                    "motorista, el permiso vuelve a amparar; si cambia alguno de los dos, " +
                    "reemítalo.",
                    ExigeReemision: false)
                : new NoCubre(
                    $"El permiso ampara el vehículo {nombreDeLoAmparado} y la misión se " +
                    $"ejecutará con {nombreDeLoDeHoy}. El permiso debe reemitirse y firmarse " +
                    "de nuevo (RN-23).",
                    ExigeReemision: true);
        }

        if (permiso.Motorista != motoristaDeHoy)
        {
            return new NoCubre(
                "El permiso es nominativo sobre el motorista y el de la misión cambió. " +
                "La firma anterior no se arrastra: reemita el permiso.",
                ExigeReemision: true);
        }

        if (!string.Equals(permiso.Destino, destinoDeHoy, StringComparison.OrdinalIgnoreCase))
        {
            return new NoCubre(
                $"El permiso ampara el destino {permiso.Destino} y la misión se ejecutará a " +
                $"{destinoDeHoy}. Reemita el permiso.",
                ExigeReemision: true);
        }

        // La ventana tiene que estar contenida ENTERA. Un permiso que cubre tres de los cinco
        // días no ampara los otros dos, y el agente que revise el cuarto tiene un vehículo del
        // Estado circulando sin respaldo.
        if (permiso.Desde > desdeDeHoy || permiso.Hasta < hastaDeHoy)
        {
            return new NoCubre(
                $"El permiso ampara del {permiso.Desde:dd/MM/yyyy} al " +
                $"{permiso.Hasta:dd/MM/yyyy} y la misión se ejecutará del " +
                $"{desdeDeHoy:dd/MM/yyyy} al {hastaDeHoy:dd/MM/yyyy}. Reemita el permiso; la " +
                "vigencia no se traslada.",
                ExigeReemision: true);
        }

        return null;
    }

    /// <summary>
    /// Por qué no se puede reemitir. <b>Nulo es que sí.</b>
    ///
    /// La reemisión abre un trámite nuevo <b>sin firma</b> — `HU-018`: <i>«el permiso nuevo
    /// requiere firma nueva, no la firma anterior»</i>. Arrastrarla convertiría el acto de la
    /// máxima autoridad en una casilla que se hereda, y lo que firmó fue <b>otro</b> vehículo
    /// con <b>otro</b> motorista.
    /// </summary>
    public static string? PorQueNoSeReemite(PermisoEnTramite permiso, string? motivo)
    {
        if (permiso.Estado != EstadoDelPermiso.Firmado)
        {
            return $"El permiso {permiso.Folio} está {Texto(permiso.Estado)}. Sólo se reemite " +
                   "uno firmado: lo que no llegó a amparar nada no se reemplaza, se retira.";
        }

        return string.IsNullOrWhiteSpace(motivo)
            ? "Diga qué cambió. El motivo queda en el asiento de anulación del salvoconducto " +
              "anterior, y sin él nadie puede reconstruir por qué hay dos folios."
            : null;
    }

    public static string Texto(EstadoDelPermiso estado) => estado switch
    {
        EstadoDelPermiso.Solicitado => "solicitado",
        EstadoDelPermiso.Firmado => "firmado",
        EstadoDelPermiso.Desistido => "desistido",
        _ => estado.ToString(),
    };
}
