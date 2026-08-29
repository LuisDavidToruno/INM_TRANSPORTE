using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Dominio.M09_Combustible;

/// <summary>
/// Los controles del libro de existencias — `RN-83` punto 5.
/// </summary>
public static class ReglasDelTanque
{
    public static void ExigirGalonesPositivos(decimal galones)
    {
        if (galones <= 0)
            throw new BloqueoDuro("RN-83",
                "Un movimiento de cero galones no mueve existencias. El signo lo pone el tipo " +
                "de asiento, no el número: un galonaje negativo en una columna es un dato que " +
                "se puede teclear al revés sin que nada lo note.");
    }

    /// <summary>
    /// <b>No se despacha lo que no hay.</b>
    ///
    /// ── Y esto no contradice que el abastecimiento nunca se rechace ─────────
    /// Son dos actos distintos. <b>Despachar</b> es entrega, y los bloqueos duros se aplican a
    /// los actos que autorizan, reservan o entregan. El <b>abastecimiento</b> que un motorista
    /// declara desde el campo es un hecho consumado: se registra igual y queda como discrepancia
    /// contra este libro.
    ///
    /// Un tanque que despacha en negativo no describe ningún tanque: describe un libro al que le
    /// faltan ingresos, y eso es un hallazgo — no un saldo.
    /// </summary>
    public static void ExigirExistenciaSuficiente(
        string tanque, decimal existencia, decimal aDespachar)
    {
        if (aDespachar <= existencia) return;

        throw new BloqueoDuro("RN-83",
            $"El tanque «{tanque}» tiene {existencia:N2} galones en libros y se piden " +
            $"{aDespachar:N2}. Faltan {aDespachar - existencia:N2}. " +
            "Si el combustible físicamente está, lo que falta es el ingreso que nadie " +
            "registró: se asienta la compra o el trasiego que lo trajo, y entonces el despacho " +
            "procede. Forzarlo dejaría el libro en negativo, que no describe ningún tanque.");
    }

    /// <summary>
    /// `RN-83` punto 5 — <b>la misma segregación de `RN-01`</b>: quien despacha no puede ser
    /// quien recibe.
    ///
    /// Es el control más elemental de una bomba de combustible y el más fácil de perder: el
    /// motorista que se sirve solo y anota lo que quiere no deja ninguna traza distinta de la
    /// del motorista al que le sirvieron. Se verifica <b>por identidad de persona</b>, no por
    /// rol: un mismo servidor con dos cuentas sigue siendo la misma persona.
    /// </summary>
    public static void ExigirQueDespachaNoSeaQuienRecibe(
        Autoria despacha, IdPersonaDelReceptor recibe)
    {
        if (despacha.Persona.Valor != recibe.Valor) return;

        throw new BloqueoDuro("RN-01",
            $"{recibe.Valor} no puede despacharse combustible a sí mismo. `RN-83` punto 5 " +
            "exige responsable de despacho identificado con la misma segregación de `RN-01`: " +
            "quien abre la llave y quien recibe el combustible son dos personas.");
    }

    /// <summary>
    /// Un tanque despacha <b>su</b> combustible.
    ///
    /// Un registro que llene un camión diésel desde el tanque de gasolina cuadra en galones y es
    /// imposible en la realidad — así que cuando aparece, lo que hay es un asiento en el tanque
    /// equivocado, y el otro tanque tiene un faltante que nadie va a poder explicar.
    /// </summary>
    public static void ExigirCombustibleCompatible(string delTanque, string delVehiculo)
    {
        // Vacío es «no se sabe» y no bloquea: `M-03` todavía no declara el combustible del
        // vehículo, y bloquear contra un dato que no existe pararía todos los despachos.
        if (string.IsNullOrWhiteSpace(delVehiculo)) return;

        if (string.Equals(delTanque, delVehiculo, StringComparison.OrdinalIgnoreCase)) return;

        throw new BloqueoDuro("RN-83",
            $"El tanque despacha {delTanque} y el vehículo usa {delVehiculo}. El asiento " +
            "cuadraría en galones y sería imposible en la realidad — y el tanque del que " +
            "salieron de verdad quedaría con un faltante que nadie va a poder explicar.");
    }

    public static void ExigirTanquesDistintos(Ulid origen, Ulid destino)
    {
        if (origen != destino) return;

        throw new BloqueoDuro("RN-83",
            "Un trasiego de un tanque a sí mismo no mueve combustible: mueve dos asientos que " +
            "se anulan y ensucian el libro que después hay que arquear.");
    }

    public static void ExigirMismoCombustible(string origen, string destino)
    {
        if (string.Equals(origen, destino, StringComparison.OrdinalIgnoreCase)) return;

        throw new BloqueoDuro("RN-83",
            $"No se trasiega {origen} a un tanque de {destino}. Si de verdad ocurrió, lo que " +
            "hay es una contaminación del combustible y eso es un incidente (M-12), no un " +
            "movimiento de existencias.");
    }
}
