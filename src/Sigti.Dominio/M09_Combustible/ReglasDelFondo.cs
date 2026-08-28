using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M09_Combustible;

/// <summary>
/// Los controles propios del expediente del fondo — `RN-26`.
///
/// ── Por qué la segregación vive acá y no en `RN-01` ─────────────────────────
/// Hallazgo `HN1-15`. `RN-01` se aplica <i>«sobre una misma Orden de Misión»</i> y sus cinco
/// funciones son funciones sobre una misión. <b>El fondo es un objeto de período</b>: lo
/// solicita ACT-04 y lo aprueba ACT-08 para un período completo. Leída como está escrita,
/// `RN-01` no lo alcanza — y la incompatibilidad más sensible del circuito de dinero quedaba
/// enunciada sin regla que la sostuviera.
///
/// La regla lo dice sin margen: es <b>control propio de `RN-26`</b>, no una aplicación de
/// `RN-01`, y <b>no es configurable</b>.
///
/// ⚠️ <b>Hallazgo abierto que esta clase hereda</b>: el par <i>solicita fondo × aprueba
/// fondo</i> no existe en la tabla `I-01`…`I-17` de `actores-y-roles.md`, que es la autoridad
/// en incompatibilidades y también razona por misión. Queda señalado ahí para que la
/// autoridad lo incorpore; acá se ejecuta igual, porque no ejecutarlo mientras se decide
/// dejaría el hueco abierto en el dinero.
/// </summary>
public static class ReglasDelFondo
{
    /// <summary>
    /// `RN-26` punto 4 — <b>quien solicita el fondo no puede aprobarlo.</b>
    ///
    /// Se verifica <b>por identidad de persona y no por rol</b>, que es lo que distingue este
    /// control de un permiso: un mismo servidor con dos cuentas, o con dos roles, sigue siendo
    /// la misma persona.
    /// </summary>
    public static void ExigirQueQuienApruebaNoSeaQuienSolicito(
        IdPersona solicito, IdPersona aprueba)
    {
        if (solicito == aprueba)
            throw new BloqueoDuro("RN-26.4",
                $"{aprueba} solicitó este fondo y no puede aprobarlo. El fondo es dinero: " +
                "quien pide y quien autoriza tienen que ser dos personas distintas, y se " +
                "verifica por identidad de persona, no por rol.");
    }

    /// <summary>
    /// `RN-26` punto 4, segunda mitad — <b>quien liquida no puede ser ninguno de los dos.</b>
    ///
    /// Es la parte que se olvida. Que solicitar y aprobar estén separados no sirve de nada si
    /// al final del período <b>el mismo que aprobó declara que todo cuadró</b>.
    /// </summary>
    public static void ExigirQueQuienLiquidaNoSeaNingunoDeLosDos(
        IdPersona solicito, IdPersona aprobo, IdPersona liquida)
    {
        if (liquida == solicito)
            throw new BloqueoDuro("RN-26.4",
                $"{liquida} solicitó este fondo y no puede liquidarlo.");

        if (liquida == aprobo)
            throw new BloqueoDuro("RN-26.4",
                $"{liquida} aprobó este fondo y no puede liquidarlo. Quien autoriza el gasto " +
                "no es quien declara que el gasto cuadró.");
    }

    /// <summary>
    /// `RN-26` — <b>no hay asignación sin saldo disponible suficiente</b>, y el mensaje dice
    /// <b>cuánto falta</b>.
    ///
    /// ── La tolerancia arranca en cero y eso es la regla, no un descuido ──────
    /// `RN-26`: <i>«Con `tolerancia_sobregiro` en cero —su valor inicial— no hay excepción»</i>.
    /// Se recibe como parámetro porque es configurable por institución, y se compara contra
    /// la tabla vigente <b>a la fecha del hecho</b> (P-4), no a la de captura.
    ///
    /// ⚠️ <b>Esto no cubre el sobregiro que llega desde campo.</b> `RN-27` es taxativo: una
    /// entrega emitida sin conectividad que al sincronizar no tiene saldo <b>no se revierte</b>
    /// —el combustible ya salió— sino que se registra como sobregiro y genera hallazgo. Este
    /// bloqueo es para el que todavía no ocurrió.
    /// </summary>
    public static void ExigirSaldoSuficiente(
        decimal saldoDisponible, decimal montoAAsignar, decimal toleranciaSobregiro)
    {
        if (montoAAsignar <= saldoDisponible + toleranciaSobregiro)
            return;

        var falta = montoAAsignar - saldoDisponible - toleranciaSobregiro;

        throw new BloqueoDuro("RN-26",
            $"El fondo no alcanza: quedan {saldoDisponible:N2} y se piden {montoAAsignar:N2}. " +
            $"Faltan {falta:N2}" +
            (toleranciaSobregiro > 0 ? $" aun contando la tolerancia de {toleranciaSobregiro:N2}." : ".") +
            " La salida es la ampliación del fondo, que sigue el mismo circuito de aprobación.");
    }

    /// <summary>
    /// `RN-26` punto 4 del comportamiento — <b>el fondo no se cierra con asignaciones vivas</b>,
    /// ni sin partida presupuestaria.
    ///
    /// ── Las dos son la misma clase de hueco ─────────────────────────────────
    /// Un fondo cerrado con vales sin liquidar deja dinero público sin descargo bajo un
    /// expediente que dice estar terminado. Y un fondo cerrado sin partida es un gasto
    /// ejecutado que <b>no se puede imputar a nada</b> — la estructura presupuestaria la define
    /// ARGOS (`DP-001 D-09`), y si el espejo no la tiene, `RN-26` manda registrar el fondo con
    /// partida pendiente y <b>bloquear su cierre</b>, no inventarla.
    /// </summary>
    public static void ExigirCierrable(int asignacionesSinLiquidar, string? partidaPresupuestaria)
    {
        if (asignacionesSinLiquidar > 0)
            throw new BloqueoDuro("RN-26",
                $"El fondo tiene {asignacionesSinLiquidar} asignación(es) sin liquidar ni anular. " +
                "Cerrarlo dejaría dinero público sin descargo bajo un expediente que dice estar " +
                "terminado.");

        if (string.IsNullOrWhiteSpace(partidaPresupuestaria))
            throw new BloqueoDuro("RN-26",
                "El fondo no tiene partida presupuestaria. Es gasto ejecutado que no se puede " +
                "imputar a nada. La estructura la define ARGOS (`DP-001 D-09`): si el espejo no " +
                "la tiene, se completa antes de cerrar — no se inventa.");
    }

    /// <summary>
    /// `RN-26` — <b>la asignación sólo se imputa a un fondo de su ámbito.</b>
    ///
    /// `[C]` Que las delegaciones manejen fondo propio <b>no está confirmado</b>. Mientras no
    /// se conteste, la comprobación es la del ámbito declarado en el fondo: si la institución
    /// resulta no usar fondos por delegación, esta comprobación nunca ve un caso — no estorba.
    /// </summary>
    public static void ExigirMismoAmbito(
        AmbitoDelFondo ambito, string ambitoDelFondo, string ambitoDeLaMision)
    {
        // A nivel institución todo cae dentro: no hay nada que comparar.
        if (ambito == AmbitoDelFondo.Institucion)
            return;

        if (!string.Equals(ambitoDelFondo, ambitoDeLaMision, StringComparison.OrdinalIgnoreCase))
            throw new BloqueoDuro("RN-26",
                $"El fondo es de «{ambitoDelFondo}» y la misión es de «{ambitoDeLaMision}». " +
                "Una asignación sólo se imputa a un fondo de su propio ámbito; si no, el cuadre " +
                "por dependencia que presenta Gerencia Administrativa no cierra en ninguno de " +
                "los dos lados.");
    }
}
