using Sigti.Dominio.M03_Flota;

namespace Sigti.Dominio.M04_Documentacion;

/// <summary>
/// Qué hace un vehículo durante el feriado largo — <b>las tres situaciones posibles, y sólo
/// tres</b>.
/// </summary>
public enum SituacionEnElPeriodo
{
    /// <summary>Tiene misión y permiso propuesto: circula, y la máxima autoridad lo firma.</summary>
    ConPermisoPropuesto,

    /// <summary>
    /// No circula. <b>Tiene que quedar resguardado</b>, y alguien tiene que confirmar dónde.
    /// </summary>
    AResguardar,

    /// <summary>
    /// Servicio exceptuado vigente — `RN-24`: ambulancia, emergencia, seguridad.
    ///
    /// <b>Se lista aparte y sin permiso a firmar.</b> Meterlo entre los que se firman haría que
    /// la máxima autoridad firmara permisos que la regla dice que no hacen falta.
    /// </summary>
    Exceptuado,
}

/// <summary>El estado del resguardo de un vehículo que no circula.</summary>
public enum EstadoDelResguardo
{
    /// <summary>Alguien confirmó dónde está, con evidencia fechada.</summary>
    Confirmado,

    /// <summary>
    /// <b>Nadie confirmó dónde está.</b>
    ///
    /// ⚠️ No es lo mismo que «está perdido»: es que <b>nadie fue a mirar</b>. Y es exactamente
    /// lo que un operativo encuentra.
    /// </summary>
    NoConfirmado,
}

/// <param name="Resguardo">
/// Sólo para los que no circulan. <b>Nulo en los demás</b> — un vehículo que sale no tiene
/// resguardo que confirmar, y pedirlo produciría una tarea imposible.
/// </param>
/// <param name="PorQueNoSeFirma">
/// Por qué este permiso no se puede firmar todavía. <b>Nulo es que sí se puede.</b>
/// </param>
/// <param name="Firmado">
/// Si el permiso <b>ya está firmado</b>.
///
/// ── ⚠️ Va aparte de <c>PorQueNoSeFirma</c>, y no es redundante ──────────────
/// Los dos dicen «no se puede firmar», y son cosas <b>opuestas</b>: uno es el problema y el
/// otro es el resultado. Una pantalla que sólo mire el motivo pinta de rojo, con un mensaje de
/// bloqueo, justamente el permiso que la máxima autoridad ya resolvió.
/// </param>
public sealed record VehiculoEnElPeriodo(
    Ulid Vehiculo,
    string Identificacion,
    SituacionEnElPeriodo Situacion,
    Ulid? Permiso,
    string? FolioDelPermiso,
    string? Mision,
    EstadoDelResguardo? Resguardo,
    DateOnly? ConfirmadoEl,
    string? Predio,
    string? PorQueNoSeFirma,
    bool Firmado = false);

/// <param name="CorteDeConocimiento">
/// A qué momento está hecho el reporte — `RN-94`. <b>Va en el reporte y no al margen</b>: una
/// consulta con la misma fecha de corte tiene que reproducir el mismo resultado, y sin
/// declararla nadie puede saber contra qué se comparó.
/// </param>
public sealed record ReporteDelPeriodo(
    DateOnly Desde,
    DateOnly Hasta,
    DateTimeOffset CorteDeConocimiento,
    IReadOnlyList<VehiculoEnElPeriodo> Vehiculos,
    IReadOnlyList<string> TramosInhabiles)
{
    public IReadOnlyList<VehiculoEnElPeriodo> Circulan =>
        [.. Vehiculos.Where(v => v.Situacion == SituacionEnElPeriodo.ConPermisoPropuesto)];

    /// <summary>
    /// Los que no circulan, <b>con los no confirmados primero</b>.
    ///
    /// El orden es la mitad del valor: un reporte que lista dieciocho vehículos en orden
    /// alfabético obliga a buscar los tres que importan.
    /// </summary>
    public IReadOnlyList<VehiculoEnElPeriodo> Resguardados =>
    [
        .. Vehiculos
            .Where(v => v.Situacion == SituacionEnElPeriodo.AResguardar)
            .OrderBy(v => v.Resguardo == EstadoDelResguardo.Confirmado)
            .ThenBy(v => v.Identificacion),
    ];

    public IReadOnlyList<VehiculoEnElPeriodo> Exceptuados =>
        [.. Vehiculos.Where(v => v.Situacion == SituacionEnElPeriodo.Exceptuado)];

    /// <summary>
    /// Los que nadie fue a mirar. <b>Es la cifra que se lee primero.</b>
    /// </summary>
    public int SinConfirmar =>
        Vehiculos.Count(v => v.Resguardo == EstadoDelResguardo.NoConfirmado);

    /// <summary>
    /// Cuántos permisos se pueden firmar hoy, de los propuestos.
    ///
    /// Va aparte del total porque <b>la máxima autoridad necesita saber cuántos va a resolver</b>
    /// antes de sentarse: «cinco propuestos» y «cinco firmables» no son lo mismo.
    ///
    /// ⚠️ <b>El firmado no vuelve a contar.</b> Es la mitad que se olvida: sin descontarlo, la
    /// cifra no baja al firmar y la sesión de firma no termina nunca — quien firma vuelve a
    /// abrirla creyendo que quedaron permisos pendientes.
    /// </summary>
    public int Firmables => Circulan.Count(v => !v.Firmado && v.PorQueNoSeFirma is null);
}

/// <summary>
/// `HU-020` — el reporte previo al feriado largo y la firma en lote.
///
/// ── Por qué esta pantalla existe ────────────────────────────────────────────
/// El Tribunal Superior de Cuentas hace operativos de fiscalización vehicular <b>específicamente
/// en Semana Santa</b> `[V]`. Es el pico anual de riesgo de la institución — y es
/// <b>predecible</b>, lo que lo vuelve el caso más fácil de resolver bien y el más caro de
/// resolver mal.
///
/// Un flujo que le exige a la máxima autoridad abrir veinte expedientes uno por uno a las cinco
/// de la tarde del jueves santo produce una de dos cosas: <b>permisos que no se firman y
/// misiones que salen sin amparo, o la clave prestada a un asistente</b>.
///
/// ── Y por qué las tres listas tienen que sumar la flota ─────────────────────
/// Un reporte que liste sólo los que circulan deja al resto invisible, y <b>un vehículo del que
/// nadie confirmó dónde está es exactamente lo que un operativo encuentra</b>. La suma es la
/// propiedad que hace útil el reporte, no un detalle de presentación.
/// </summary>
public static class ReglasDelReporteDelPeriodo
{
    /// <summary>La clave de cuántos días antes se anticipa el período.</summary>
    public const string ClaveDeAnticipacion = "anticipacion_reporte_feriado_largo_dias";

    /// <summary>
    /// Si el vehículo <b>entra al reporte</b>.
    ///
    /// ── ⚠️ Los dos terminales de §10.2 quedan fuera ─────────────────────────
    /// Un vehículo <b>dado de baja</b> o <b>retirado de flota</b> ya no es flota: pedirle a
    /// alguien que vaya a confirmar dónde quedó resguardado es mandarlo a hacer una tarea que
    /// puede ser imposible —el bien se descargó, se devolvió, se remató—, y la confirmación
    /// nunca llegaría.
    ///
    /// El daño no es la tarea de más. Es que <b>cada uno de esos infla «sin confirmar»</b>, y
    /// en una institución con años de historia son decenas: los tres que de verdad nadie fue a
    /// mirar quedan enterrados entre ellos. Es exactamente el defecto que el orden de la lista
    /// existe para evitar.
    ///
    /// ── Y lo que NO queda fuera ─────────────────────────────────────────────
    /// <b>Prestado sigue siendo bien nuestro</b> y devenga responsabilidad patrimonial: dónde
    /// está durante el feriado es una pregunta legítima. <b>En taller también</b>: el taller es
    /// un lugar, y un vehículo que nadie ubica no deja de estar perdido porque haya una orden
    /// de trabajo abierta.
    ///
    /// <b>Nulo entra.</b> «Nunca se declaró estado» no es «no es flota» — es un vehículo del
    /// que se sabe todavía menos, y esconderlo sería lo contrario de lo que el reporte hace.
    /// </summary>
    public static bool EstaEnLaFlota(EstadoOperativo? estado) =>
        estado is not (EstadoOperativo.DadoDeBaja or EstadoOperativo.RetiradoDeFlota);


    /// <summary>
    /// En qué situación queda un vehículo.
    /// </summary>
    /// <param name="excepcion">
    /// `RN-24` — la excepción es atributo del <b>vehículo</b>. Se evalúa primero: un exceptuado
    /// no circula «con permiso» ni queda «a resguardar», y clasificarlo en cualquiera de las
    /// otras dos produciría una firma que no hace falta o una confirmación que nadie debe.
    /// </param>
    public static SituacionEnElPeriodo Situar(
        ServicioExceptuado? excepcion, DateOnly inicioDelPeriodo, bool tieneMisionEnElPeriodo)
    {
        if (excepcion is { } e && e.VigenteAl(inicioDelPeriodo))
            return SituacionEnElPeriodo.Exceptuado;

        return tieneMisionEnElPeriodo
            ? SituacionEnElPeriodo.ConPermisoPropuesto
            : SituacionEnElPeriodo.AResguardar;
    }

    /// <summary>
    /// La comprobación que hace que el reporte signifique algo: <b>ningún vehículo queda fuera,
    /// y ninguno aparece dos veces</b>.
    ///
    /// Devuelve el motivo cuando no cuadra. <b>Nulo es que cuadra.</b>
    /// </summary>
    public static string? PorQueNoCuadra(ReporteDelPeriodo reporte, int vehiculosDeLaFlota)
    {
        var suma = reporte.Circulan.Count + reporte.Resguardados.Count + reporte.Exceptuados.Count;

        if (suma != vehiculosDeLaFlota)
        {
            return $"Las tres listas suman {suma} y la flota tiene {vehiculosDeLaFlota} " +
                   "vehículos. Un reporte que no cuadra deja vehículos invisibles, y uno del " +
                   "que nadie confirmó dónde está es exactamente lo que un operativo encuentra.";
        }

        var repetidos = reporte.Vehiculos
            .GroupBy(v => v.Vehiculo)
            .Where(g => g.Count() > 1)
            .Select(g => g.First().Identificacion)
            .ToList();

        return repetidos.Count == 0
            ? null
            : $"Estos vehículos aparecen en más de una lista: {string.Join(", ", repetidos)}. " +
              "Las tres situaciones son excluyentes.";
    }

    /// <summary>
    /// Por qué no se acepta esta confirmación de resguardo.
    ///
    /// <b>Con evidencia fechada o no vale.</b> `RN-18` ya fija la disciplina para la
    /// constatación de rotulación, y acá es la misma razón: sin evidencia lo único que queda
    /// registrado es que alguien dijo que el vehículo estaba ahí.
    /// </summary>
    public static string? PorQueNoSeConfirma(bool tieneEvidencia, string? predio) =>
        !tieneEvidencia
            ? "La confirmación de resguardo necesita evidencia fechada. Sin ella lo único que " +
              "queda registrado es que alguien dijo que el vehículo estaba ahí, y eso es lo " +
              "que un operativo viene a discutir."
            : string.IsNullOrWhiteSpace(predio)
                ? "Diga dónde queda resguardado. «Confirmado» sin lugar no contesta la pregunta " +
                  "que el reporte hace, que es dónde está cada vehículo."
                : null;
}
