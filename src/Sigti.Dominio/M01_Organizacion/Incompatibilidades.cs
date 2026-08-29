using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M01_Organizacion;

/// <summary>
/// Las cinco funciones que el MARCI exige en personas distintas — §5.1.
///
/// <b>Son funciones, no roles.</b> Un mismo `ACT-xx` puede ejercer varias, y varios roles
/// pueden ejercer la misma; separarlas es lo que permite que la tabla de incompatibilidades
/// hable de lo que la norma dice y no de nuestra nomenclatura.
/// </summary>
public enum Funcion
{
    /// <summary>Pedir el traslado — `ACT-02`, `ACT-10`.</summary>
    Solicita,
    /// <summary>Pronunciarse sobre la procedencia — `ACT-03`, `ACT-08`, `ACT-09`.</summary>
    Autoriza,
    /// <summary>El acto físico de salida y retorno — `ACT-05`, `ACT-10`.</summary>
    Despacha,
    /// <summary>Entregar el dinero del combustible — `ACT-07`, `ACT-10`.</summary>
    EntregaFondo,
    /// <summary>Elaborar el descargo conciliado de la misión — `ACT-04`.</summary>
    Liquida,
    /// <summary>Conducir — `ACT-06`. `I-11` lo cruza contra las cuatro anteriores.</summary>
    Conduce,
    /// <summary>Verificar. `I-12` lo hace incompatible con todo lo ejecutor — `ACT-12`.</summary>
    Audita,
    /// <summary>Administrar el sistema — `ACT-01`. `I-13`.</summary>
    Administra,

    // ── El fondo es objeto DE PERÍODO, no de misión ─────────────────────────
    // `I-19` existe porque este par se caía entre `RN-01` —que razona por Orden de Misión— y el
    // fondo, que es de período. Si estas dos funciones no fueran propias, `I-19` sería una copia
    // literal de `I-01` y el hueco que el hallazgo `HB3-06` destapó seguiría abierto.

    /// <summary>Pedir el fondo del período — `ACT-04`.</summary>
    SolicitaFondo,
    /// <summary>Aprobar el fondo contra cuota y partida — `ACT-08`.</summary>
    ApruebaFondo,

    /// <summary>Dar por buena una licencia en el padrón — `ACT-04`. `I-18`.</summary>
    HabilitaLicencia,
    /// <summary>Responder patrimonialmente por el bien — `ACT-13`. `I-15`.</summary>
    Custodia,
    /// <summary>Proponer la baja de un bien — `ACT-14`. `I-17`.</summary>
    ProponeDescargo,
    /// <summary>Aprobar la baja de un bien — `ACT-08`. `I-17`.</summary>
    ApruebaDescargo,
    /// <summary>Ordenar el mantenimiento — `ACT-11`. `I-16`.</summary>
    OrdenaMantenimiento,
    /// <summary>Recibir conforme el trabajo de taller — `I-16`.</summary>
    RecibeConforme,
}

/// <summary>Con qué fuerza opera un par incompatible — §5.2, columna «Nivel».</summary>
public enum NivelDeIncompatibilidad
{
    /// <summary>
    /// <b>Núcleo irreductible.</b> No se levanta nunca: ni por régimen de excepción, ni por
    /// delegación, ni por emergencia, ni por resolución de la máxima autoridad.
    /// </summary>
    NucleoIrreductible,
    /// <summary>Bloqueo duro. Sin botón de «continuar de todos modos».</summary>
    BloqueoDuro,
    /// <summary>Configurable, <b>apagado por omisión</b>. Sólo `I-14`.</summary>
    Configurable,
    /// <summary>Advertencia: se puede continuar <b>exigiendo motivo escrito</b>.</summary>
    Advertencia,
}

/// <summary>Sobre qué se compara el par — §5.2, columna «Alcance de la evaluación».</summary>
public enum AlcanceDelPar
{
    /// <summary>El mismo expediente. La acumulación en abstracto se vigila, no se prohíbe.</summary>
    MismoExpediente,
    /// <summary>
    /// <b>Absoluto y permanente.</b> La sola acumulación de los dos roles se rechaza al
    /// asignarlos, sin esperar a que haya un expediente.
    /// </summary>
    Absoluto,
}

/// <summary>Un par de la tabla de §5.2.</summary>
public sealed record ParIncompatible(
    string Id,
    Funcion Una,
    Funcion Otra,
    AlcanceDelPar Alcance,
    NivelDeIncompatibilidad Nivel,
    string PorQue);

/// <summary>
/// La segregación de funciones — §5 de <c>actores-y-roles.md</c>, <b>que es la autoridad</b>.
///
/// ── Por qué esta tabla se transcribe y no se deduce ─────────────────────────
/// <i>«Es la sección que hace o deshace este sistema. El MARCI la exige y el TSC la
/// verifica»</i>. Diecinueve pares, cada uno con su nivel y su alcance. Derivarlos de una
/// regla general —«las cinco funciones son mutuamente excluyentes»— perdería las tres cosas
/// que hacen útil a la tabla: que `I-14` es configurable y está apagado, que `I-15` y `I-16`
/// son advertencia y no bloqueo, y que cinco de ellos <b>no se levantan nunca</b>.
///
/// ── La aritmética que la sección dice en voz alta ───────────────────────────
/// Cinco funciones incompatibles ⇒ <b>cumplir la segregación completa exige cinco personas
/// distintas por misión</b>. Una delegación de tres no puede cumplirla localmente *«por
/// aritmética, no por falta de voluntad»*. Este archivo no resuelve eso —lo resuelve el
/// escalamiento a sede del Nivel 1— pero tampoco lo disimula.
///
/// ── Dónde NO vive la decisión ───────────────────────────────────────────────
/// Acá está la tabla y su lectura. <b>Quién ejerció qué función en un expediente concreto</b>
/// lo sabe cada módulo, y por eso el chequeo bloqueante recibe los actos ya resueltos en vez
/// de ir a buscarlos: esta clase no conoce misiones ni fondos.
/// </summary>
public static class Incompatibilidades
{
    /// <summary>
    /// Los diecinueve pares, en el orden del documento.
    ///
    /// <b>Los identificadores no se reciclan.</b> `I-18` e `I-19` se incorporaron tras los
    /// hallazgos `HB3-05` y `HB3-06`, y están marcadas `[C]`: la exigencia se deduce del
    /// principio de segregación del MARCI, no de articulado citable.
    /// </summary>
    public static readonly IReadOnlyList<ParIncompatible> Tabla =
    [
        new("I-01", Funcion.Solicita, Funcion.Autoriza, AlcanceDelPar.MismoExpediente,
            NivelDeIncompatibilidad.BloqueoDuro, "Quien pide no autoriza lo que pidió."),

        new("I-02", Funcion.Solicita, Funcion.Despacha, AlcanceDelPar.MismoExpediente,
            NivelDeIncompatibilidad.BloqueoDuro, "Quien pide no entrega el vehículo que pidió."),

        new("I-03", Funcion.Solicita, Funcion.EntregaFondo, AlcanceDelPar.MismoExpediente,
            NivelDeIncompatibilidad.BloqueoDuro, "Quien pide no entrega el dinero de lo que pidió."),

        new("I-04", Funcion.Solicita, Funcion.Liquida, AlcanceDelPar.MismoExpediente,
            NivelDeIncompatibilidad.BloqueoDuro, "Quien pide no declara cómo terminó lo que pidió."),

        new("I-05", Funcion.Autoriza, Funcion.Despacha, AlcanceDelPar.MismoExpediente,
            NivelDeIncompatibilidad.BloqueoDuro, "Quien autoriza no ejecuta su propia autorización."),

        new("I-06", Funcion.Autoriza, Funcion.EntregaFondo, AlcanceDelPar.MismoExpediente,
            NivelDeIncompatibilidad.BloqueoDuro, "Quien autoriza el gasto no entrega el dinero."),

        new("I-07", Funcion.Autoriza, Funcion.Liquida, AlcanceDelPar.MismoExpediente,
            NivelDeIncompatibilidad.NucleoIrreductible,
            "Son los dos extremos de la cadena de control."),

        new("I-08", Funcion.Despacha, Funcion.EntregaFondo, AlcanceDelPar.MismoExpediente,
            NivelDeIncompatibilidad.BloqueoDuro, "Quien entrega el vehículo no entrega el dinero."),

        new("I-09", Funcion.Despacha, Funcion.Liquida, AlcanceDelPar.MismoExpediente,
            NivelDeIncompatibilidad.BloqueoDuro, "Quien despacha no declara cómo volvió."),

        new("I-10", Funcion.EntregaFondo, Funcion.Liquida, AlcanceDelPar.MismoExpediente,
            NivelDeIncompatibilidad.NucleoIrreductible,
            "Quien entrega el dinero no puede declarar en qué se gastó."),

        // `I-11` es uno contra cuatro. Se enumera par a par para que el mensaje pueda nombrar
        // cuál se activó: «motorista × algo» no le dice a nadie qué hacer.
        new("I-11", Funcion.Conduce, Funcion.Autoriza, AlcanceDelPar.MismoExpediente,
            NivelDeIncompatibilidad.NucleoIrreductible,
            "Autoliquidación: el vector de fraude clásico en combustible."),
        new("I-11", Funcion.Conduce, Funcion.Despacha, AlcanceDelPar.MismoExpediente,
            NivelDeIncompatibilidad.NucleoIrreductible,
            "Autoliquidación: el vector de fraude clásico en combustible."),
        new("I-11", Funcion.Conduce, Funcion.EntregaFondo, AlcanceDelPar.MismoExpediente,
            NivelDeIncompatibilidad.NucleoIrreductible,
            "Autoliquidación: el vector de fraude clásico en combustible."),
        new("I-11", Funcion.Conduce, Funcion.Liquida, AlcanceDelPar.MismoExpediente,
            NivelDeIncompatibilidad.NucleoIrreductible,
            "Autoliquidación: el vector de fraude clásico en combustible."),

        // `I-12` y `I-13` son ABSOLUTOS: se rechazan al asignar el rol, sin esperar expediente.
        new("I-12", Funcion.Audita, Funcion.Solicita, AlcanceDelPar.Absoluto,
            NivelDeIncompatibilidad.NucleoIrreductible,
            "La independencia de la auditoría no admite excepción."),
        new("I-12", Funcion.Audita, Funcion.Autoriza, AlcanceDelPar.Absoluto,
            NivelDeIncompatibilidad.NucleoIrreductible,
            "La independencia de la auditoría no admite excepción."),
        new("I-12", Funcion.Audita, Funcion.Despacha, AlcanceDelPar.Absoluto,
            NivelDeIncompatibilidad.NucleoIrreductible,
            "La independencia de la auditoría no admite excepción."),
        new("I-12", Funcion.Audita, Funcion.EntregaFondo, AlcanceDelPar.Absoluto,
            NivelDeIncompatibilidad.NucleoIrreductible,
            "La independencia de la auditoría no admite excepción."),
        new("I-12", Funcion.Audita, Funcion.Liquida, AlcanceDelPar.Absoluto,
            NivelDeIncompatibilidad.NucleoIrreductible,
            "La independencia de la auditoría no admite excepción."),
        new("I-12", Funcion.Audita, Funcion.Conduce, AlcanceDelPar.Absoluto,
            NivelDeIncompatibilidad.NucleoIrreductible,
            "La independencia de la auditoría no admite excepción."),

        new("I-13", Funcion.Administra, Funcion.Autoriza, AlcanceDelPar.Absoluto,
            NivelDeIncompatibilidad.NucleoIrreductible,
            "Podría otorgarse a sí mismo la facultad y borrar el rastro."),
        new("I-13", Funcion.Administra, Funcion.ApruebaFondo, AlcanceDelPar.Absoluto,
            NivelDeIncompatibilidad.NucleoIrreductible,
            "Podría otorgarse a sí mismo la facultad y borrar el rastro."),
        new("I-13", Funcion.Administra, Funcion.EntregaFondo, AlcanceDelPar.Absoluto,
            NivelDeIncompatibilidad.NucleoIrreductible,
            "Podría otorgarse a sí mismo la facultad y borrar el rastro."),
        new("I-13", Funcion.Administra, Funcion.Liquida, AlcanceDelPar.Absoluto,
            NivelDeIncompatibilidad.NucleoIrreductible,
            "Podría otorgarse a sí mismo la facultad y borrar el rastro."),

        // `I-14` NO está en la enumeración del MARCI. Configurable y apagado por omisión.
        new("I-14", Funcion.Autoriza, Funcion.Liquida, AlcanceDelPar.MismoExpediente,
            NivelDeIncompatibilidad.Configurable,
            "Emitir la Orden de Misión y liquidarla. Activable para instituciones con " +
            "planilla suficiente."),

        // `I-15` y `I-16` son advertencia: se continúa con motivo escrito.
        new("I-15", Funcion.Custodia, Funcion.Autoriza, AlcanceDelPar.MismoExpediente,
            NivelDeIncompatibilidad.Advertencia,
            "El custodio autoriza la salida de su propio vehículo. Práctica de control, " +
            "sin norma expresa."),

        new("I-16", Funcion.OrdenaMantenimiento, Funcion.RecibeConforme,
            AlcanceDelPar.MismoExpediente, NivelDeIncompatibilidad.Advertencia,
            "Ordena el mantenimiento y recibe conforme el trabajo. Práctica de control."),

        new("I-17", Funcion.ProponeDescargo, Funcion.ApruebaDescargo,
            AlcanceDelPar.MismoExpediente, NivelDeIncompatibilidad.BloqueoDuro,
            "Quien propone la baja de un bien no la aprueba."),

        new("I-18", Funcion.HabilitaLicencia, Funcion.Conduce, AlcanceDelPar.MismoExpediente,
            NivelDeIncompatibilidad.BloqueoDuro,
            "Quien se habilita a sí mismo da por bueno el único dato del que depende BD-02."),

        new("I-19", Funcion.SolicitaFondo, Funcion.ApruebaFondo, AlcanceDelPar.MismoExpediente,
            NivelDeIncompatibilidad.BloqueoDuro,
            "Solicita el fondo y lo aprueba. RN-01 razona por Orden de Misión y el fondo es " +
            "objeto de período, así que este par se caía entre las dos."),
    ];

    /// <summary>
    /// Qué funciones ejerce un rol.
    ///
    /// ── Es la traducción, y es donde se puede meter la pata ─────────────────
    /// La tabla de §5.2 habla de <b>funciones</b> y el sistema asigna <b>roles</b>. Este mapa
    /// es el puente, y un rol de más acá abre un hueco de segregación que ninguna prueba de
    /// la tabla detecta — porque la tabla estaría bien y el puente mal.
    ///
    /// Sale de las fichas de actor de §1 y de la matriz de permisos de §4.
    /// </summary>
    public static readonly IReadOnlyDictionary<Rol, IReadOnlyList<Funcion>> FuncionesDe =
        new Dictionary<Rol, IReadOnlyList<Funcion>>
        {
            // «Sin acceso al contenido de negocio salvo diagnóstico registrado.»
            [Rol.Administrador] = [Funcion.Administra],

            [Rol.Solicitante] = [Funcion.Solicita],

            // «Pronunciarse sobre la procedencia de la necesidad. No decide sobre vehículo ni
            // motorista — eso es de Transporte.»
            [Rol.JefaturaInmediata] = [Funcion.Autoriza],

            // ⚠️ **NO autoriza.** Su ficha lo dice como límite expreso: «no autoriza la
            // necesidad (ACT-03), no despacha físicamente (ACT-05), no entrega el fondo
            // (ACT-07), no cierra el expediente (ACT-08)». Lo que sí hace: programar, solicitar
            // el fondo, liquidar y gestionar el padrón de motoristas.
            [Rol.JefeDeTransporte] =
                [Funcion.SolicitaFondo, Funcion.Liquida, Funcion.HabilitaLicencia],

            // «No programa la misión, no la autoriza y no la liquida.»
            [Rol.EncargadoDeDespacho] = [Funcion.Despacha],

            [Rol.Motorista] = [Funcion.Conduce],

            [Rol.EncargadoDeCombustible] = [Funcion.EntregaFondo],

            // «Aprobar el fondo […] resolver autorizaciones escaladas […] cerrar el expediente
            // […] aprobar cambios a catálogos y a parámetros normativos.»
            [Rol.GerenciaAdministrativa] =
                [Funcion.ApruebaFondo, Funcion.Autoriza, Funcion.ApruebaDescargo],

            // «Firmar el permiso de circulación […] resolver lo escalado.»
            [Rol.MaximaAutoridad] = [Funcion.Autoriza],

            // **Es el caso de §5.4, y acumula por diseño**: «todo lo que en sede producen
            // ACT-03, ACT-04, ACT-05 y ACT-07». Una delegación de tres personas no cumple la
            // segregación localmente —por aritmética, no por falta de voluntad—, y el sistema
            // tiene que poder verlo en vez de disimularlo.
            [Rol.EncargadoDeDelegacion] =
            [
                Funcion.Solicita, Funcion.Autoriza, Funcion.Despacha, Funcion.EntregaFondo,
                Funcion.SolicitaFondo, Funcion.Liquida,
            ],

            // «Declarar la indisponibilidad del vehículo y su reingreso al servicio.»
            [Rol.EncargadoDeMantenimiento] = [Funcion.OrdenaMantenimiento],

            // «Solo lectura y exportación. Sin excepciones y sin régimen de excepción que lo
            // levante. Un auditor con capacidad de ejecutar deja de ser auditor.»
            [Rol.AuditorInterno] = [Funcion.Audita],

            // Responde patrimonialmente por el bien. **No conduce**: son cosas distintas, y
            // confundirlas activaría I-11 sobre alguien que nunca se sube al vehículo.
            [Rol.CustodioDelVehiculo] = [Funcion.Custodia],

            // «El proceso de descargo o baja con acta y resolución» — propone, no aprueba.
            [Rol.EncargadoDeBienes] = [Funcion.ProponeDescargo],

            // Verifica en carretera. No ejecuta ningún acto del expediente.
            [Rol.VerificadorEnCarretera] = [],
        };

    public static IReadOnlyList<Funcion> Funciones(Rol rol) =>
        FuncionesDe.TryGetValue(rol, out var f) ? f : [];

    /// <summary>Las funciones que una acumulación de roles produce, sin repetir.</summary>
    public static IReadOnlyList<Funcion> FuncionesDeTodos(IEnumerable<Rol> roles) =>
        [.. roles.SelectMany(Funciones).Distinct().Order()];

    /// <summary>
    /// Los pares <b>absolutos</b> que una acumulación de roles activaría.
    ///
    /// Es el control preventivo de §5.3.A: *«si la asignación produce en una persona una
    /// acumulación incompatible de carácter absoluto (I-12, I-13), se rechaza la
    /// asignación»*.
    /// </summary>
    public static IReadOnlyList<ParIncompatible> AbsolutosQueActiva(IEnumerable<Rol> roles)
    {
        var funciones = FuncionesDeTodos(roles).ToHashSet();

        return
        [
            .. Tabla
                .Where(p => p.Alcance == AlcanceDelPar.Absoluto)
                .Where(p => funciones.Contains(p.Una) && funciones.Contains(p.Otra))
                .DistinctBy(p => p.Id),
        ];
    }

    /// <summary>
    /// Los pares <b>por expediente</b> que la acumulación deja latentes.
    ///
    /// §5.3.A: *«si produce una acumulación que sólo es incompatible por misión, se permite la
    /// asignación pero se marca el puesto como “de acumulación vigilada”»*. <b>No se prohíbe
    /// de entrada</b> que el Encargado de Delegación sea también Solicitante: sería inoperante.
    ///
    /// Los <see cref="NivelDeIncompatibilidad.Configurable"/> <b>no entran</b>: están apagados
    /// por omisión, y vigilar por algo que no está activo es ruido.
    /// </summary>
    public static IReadOnlyList<ParIncompatible> VigiladosQueActiva(IEnumerable<Rol> roles)
    {
        var funciones = FuncionesDeTodos(roles).ToHashSet();

        return
        [
            .. Tabla
                .Where(p => p.Alcance == AlcanceDelPar.MismoExpediente)
                .Where(p => p.Nivel is NivelDeIncompatibilidad.BloqueoDuro
                    or NivelDeIncompatibilidad.NucleoIrreductible)
                .Where(p => funciones.Contains(p.Una) && funciones.Contains(p.Otra))
                .DistinctBy(p => p.Id),
        ];
    }
}

/// <summary>
/// Qué pasa si se otorga este rol a este puesto — el <b>control preventivo</b> de §5.3.A.
/// </summary>
/// <param name="Rechazan">
/// Los pares absolutos que la acumulación activaría. <b>No vacío = la asignación se rechaza.</b>
/// </param>
/// <param name="Vigilados">
/// Los pares por expediente que quedan latentes. La asignación pasa, y el puesto queda
/// <b>de acumulación vigilada</b>: aparece en el tablero de `ACT-08` y `ACT-12`.
/// </param>
public sealed record EfectoDeLaAsignacion(
    IReadOnlyList<Rol> RolesResultantes,
    IReadOnlyList<ParIncompatible> Rechazan,
    IReadOnlyList<ParIncompatible> Vigilados)
{
    public bool SeRechaza => Rechazan.Count > 0;

    /// <summary>
    /// <b>Vigilada no es «casi rechazada».</b> Es una acumulación legítima que el sistema
    /// tiene que poder ver, porque el bloqueo real llega al ejecutar el acto sobre un
    /// expediente concreto y no acá.
    /// </summary>
    public bool QuedaVigilada => !SeRechaza && Vigilados.Count > 0;
}

/// <summary>
/// Las reglas de la asignación puesto↔rol — `PT-097`.
/// </summary>
public static class ReglasDeLaAsignacion
{
    public const string Precondicion = "I-12/I-13";

    /// <summary>
    /// Evalúa la asignación <b>antes</b> de guardarla.
    ///
    /// Se juzga contra lo que produciría en la persona, no contra lo que el puesto ya tiene:
    /// la acumulación se evalúa <b>sobre la persona, nunca sobre el puesto</b> (§5.2), y una
    /// persona con tres puestos suma las tres competencias.
    /// </summary>
    public static EfectoDeLaAsignacion Evaluar(IReadOnlyList<Rol> rolesResultantes) =>
        new(rolesResultantes,
            Incompatibilidades.AbsolutosQueActiva(rolesResultantes),
            Incompatibilidades.VigiladosQueActiva(rolesResultantes));

    /// <summary>
    /// Bloquea la asignación que produce una acumulación absoluta.
    ///
    /// <b>Nombra el par y por qué existe.</b> §5.3: *«un mensaje genérico produce una llamada
    /// a soporte; un mensaje preciso produce la acción correcta»*.
    /// </summary>
    public static void Exigir(EfectoDeLaAsignacion efecto, string quien, Rol nuevo)
    {
        if (!efecto.SeRechaza) return;

        var par = efecto.Rechazan[0];

        throw new BloqueoDuro(par.Id,
            $"No se puede otorgar {nuevo} a un puesto que ocupa {quien}: la acumulación " +
            $"activa {par.Id} ({par.Una} × {par.Otra}), que es del núcleo irreductible. " +
            $"{par.PorQue} No se levanta por régimen de excepción, ni por delegación, ni por " +
            "emergencia, ni por resolución de la máxima autoridad. Lo que corresponde es " +
            "otorgar el rol a otro puesto, o cerrar antes la asignación que lo hace " +
            "incompatible.");
    }
}
