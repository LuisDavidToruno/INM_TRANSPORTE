using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M09_Combustible;

/// <summary>
/// Contra qué se emite y a quién se entrega — `RN-32`.
///
/// ── La corrección `HB1-06`, que es lo que hace evaluable esta regla ─────────
/// `RN-32` tenía valor inicial `APROBADA` <b>y</b> exigía que el receptor fuera el vehículo y
/// el motorista «de esa orden». En `APROBADA` no hay ninguno de los dos (`INV-11`:
/// <i>«aprobar no es programar»</i>). <b>La regla no podía evaluar sus propios requisitos con
/// su propio valor inicial.</b>
///
/// La máquina de estados separa los dos momentos que la regla mezclaba:
///
/// | Momento | Estado | Qué ocurre |
/// |---|---|---|
/// | <b>Emisión</b> | `PROGRAMADA` | Ya hay vehículo y motorista (`INV-12`). El instrumento existe con folio, en `EMITIDA` |
/// | <b>Entrega</b> | dentro de `T-12` | ACT-07 entrega contra firma. Pasa a `ENTREGADA` |
/// </summary>
public static class ReglasDeEmisionDeCombustible
{
    /// <summary>
    /// El piso del parámetro <c>estado_minimo_orden_para_emitir_combustible</c>.
    ///
    /// <b>No se puede configurar por debajo de esto</b>, y la razón no es de estilo: `RN-32`
    /// lo dice explícito — <i>«hacerlo dejaría los requisitos 2 y 3 sin nada contra qué
    /// evaluarse»</i>.
    /// </summary>
    public const EstadoDeMision PisoDelEstadoMinimo = EstadoDeMision.Programada;

    /// <summary>
    /// `RN-32` requisito 1 — la orden alcanzó el estado mínimo configurado.
    ///
    /// El parámetro se recibe porque es configurable por institución; el piso <b>no</b>. Un
    /// valor por debajo de `PROGRAMADA` no se rechaza como dato inválido del usuario: se
    /// rechaza acá, donde la configuración se usa, para que no haya forma de cargarlo por otra
    /// puerta y que la regla quede inerte sin que nadie lo note.
    /// </summary>
    public static void ExigirEstadoMinimo(EstadoDeMision estadoDeLaOrden, EstadoDeMision minimoConfigurado)
    {
        if (minimoConfigurado < PisoDelEstadoMinimo)
            throw new BloqueoDuro("RN-32",
                $"El parámetro estado_minimo_orden_para_emitir_combustible está en " +
                $"{minimoConfigurado}, por debajo de {PisoDelEstadoMinimo}. Ahí no hay vehículo " +
                "ni motorista asignados, así que los requisitos de receptor de `RN-32` no " +
                "tendrían contra qué evaluarse.");

        // **Las ramas van antes que la comparación de orden.** `Rechazada` y `Anulada` están
        // declaradas DESPUÉS de `Cerrada` en el enum, así que `estado >= minimo` las daría por
        // buenas — y emitir un vale contra una misión anulada es exactamente el desembolso sin
        // causa que `RN-32` existe para impedir. El orden del enum es del camino feliz; las
        // ramas no están en ninguna línea.
        if (estadoDeLaOrden is EstadoDeMision.Rechazada or EstadoDeMision.Anulada)
            throw new BloqueoDuro("RN-32",
                $"La orden está {estadoDeLaOrden} y no se le emite combustible. Un vale contra " +
                "una misión que no va a ocurrir es un desembolso sin expediente al cual " +
                "imputarlo.");

        // Los estados posteriores sí sirven: emitir contra una misión ya despachada es tarde,
        // pero no es un fraude — y `EN_RUTA` con prórroga (`T-17`) es un caso real.
        if (estadoDeLaOrden < minimoConfigurado)
            throw new TransicionInvalida("V-01", estadoDeLaOrden, minimoConfigurado);
    }

    /// <summary>
    /// `RN-32` requisitos 2 y 3 — <b>el receptor es el vehículo y el motorista de esa orden.</b>
    ///
    /// ── Qué cierra exactamente ──────────────────────────────────────────────
    /// `RN-32`: <i>«el desvío más simple de todos: sacar el vale a nombre de una misión real y
    /// cargarlo en otro vehículo»</i>. El mensaje nombra al motorista asignado porque quien
    /// está en la ventanilla necesita saber si el que tiene enfrente puede recibir o no —
    /// «no coincide» lo manda a adivinar.
    ///
    /// `[C]` <b>El encargado de delegación facultado para recibir en nombre del motorista no
    /// está confirmado.</b> `RN-32` lo prevé y la institución no lo ha contestado (`RN-27` lo
    /// repite: dos niveles de folio y dos constancias, nunca una entrega colectiva sin
    /// desglose). Mientras no se conteste, <b>sólo recibe el motorista de la orden</b>: abrir
    /// la excepción por inferencia es abrir exactamente la puerta que la regla cierra.
    /// </summary>
    public static void ExigirReceptorDeLaOrden(
        Ulid vehiculoDeLaOrden, Ulid vehiculoReceptor,
        Ulid motoristaDeLaOrden, Ulid motoristaReceptor)
    {
        if (vehiculoDeLaOrden != vehiculoReceptor)
            throw new BloqueoDuro("RN-32",
                "El vale se está sacando para un vehículo distinto del asignado a la orden. " +
                "Es el desvío más simple que existe: el vale sale a nombre de una misión real " +
                "y el combustible entra en otro tanque.");

        if (motoristaDeLaOrden != motoristaReceptor)
            throw new BloqueoDuro("RN-32",
                $"El motorista asignado a esta orden es {motoristaDeLaOrden}, y quien recibe " +
                $"es {motoristaReceptor}. Para cambiarlo hay un solo camino: la sustitución de " +
                "motorista (`RN-14`), que revalida licencia y habilitación.");
    }

    /// <summary>
    /// `RN-32` caso límite — <b>el vale corresponde al combustible que el vehículo usa.</b>
    ///
    /// <i>«Un vale de diésel para un vehículo de gasolina es un error caro y perfectamente
    /// evitable»</i>. No es fraude, es desperdicio: el vale se anula y se reemite, y para
    /// entonces la misión ya salió tarde.
    /// </summary>
    public static void ExigirCombustibleCompatible(string? tipoDelVehiculo, string tipoDelVale)
    {
        // Nulo es «la ficha no lo declara», y se dice — no se supone que coincide. Es la misma
        // distinción de `BD-07`: no evaluada nunca se disfraza de conforme.
        if (string.IsNullOrWhiteSpace(tipoDelVehiculo))
            return;

        if (!string.Equals(tipoDelVehiculo, tipoDelVale, StringComparison.OrdinalIgnoreCase))
            throw new BloqueoDuro("RN-32",
                $"El vehículo usa {tipoDelVehiculo} y el vale es de {tipoDelVale}.");
    }

    /// <summary>
    /// `BD-06` sobre el instrumento — §10.1: <i>«emite ACT-04 ≠ entrega ACT-07 ≠ consume ACT-06
    /// ≠ liquida ≠ concilia»</i>.
    ///
    /// ── Por qué se compara contra el diario y no contra roles ───────────────
    /// Porque el diario es el único que sabe <b>quién</b> hizo cada acto. Un control por rol
    /// deja pasar a la persona que tiene los dos roles, que es justamente el caso de la
    /// delegación pequeña — donde `DP-002` manda escalar, no relajar.
    ///
    /// `I-03` de la tabla de incompatibilidades lo marca como <b>bloqueo duro</b> y lo nombra:
    /// <i>«es el par que habilita el fraude de combustible más simple: quien pide el viaje
    /// también entrega el dinero»</i>.
    /// </summary>
    public static void ExigirActorDistinto(
        string acto, IdPersona quienVaAActuar, IReadOnlyDictionary<string, IdPersona> yaActuaron)
    {
        foreach (var (actoPrevio, persona) in yaActuaron)
        {
            if (persona != quienVaAActuar)
                continue;

            throw new BloqueoDuro("BD-06",
                $"{quienVaAActuar} ya {actoPrevio} esta asignación y no puede {acto}. " +
                "El circuito del combustible exige personas distintas en cada eslabón: es el " +
                "par que habilita el fraude más simple del sistema.");
        }
    }
}
