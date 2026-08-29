using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Dominio.M09_Combustible;

/// <summary>
/// El acto por el que Gerencia Administrativa deja pasar una emisión bloqueada — `RN-86` casos
/// límite y `HU-078`.
///
/// ── Por qué existe la válvula ───────────────────────────────────────────────
/// `RN-86`: <i>«persona bloqueada que es la única disponible para una misión urgente»</i>. Un
/// bloqueo sin salida se termina esquivando por fuera del sistema —emitiendo a nombre de otro
/// motorista— y entonces el registro miente sobre quién recibió el dinero, que es peor que no
/// haber bloqueado.
///
/// ── Por qué es por misión y no por persona ──────────────────────────────────
/// `HU-078` lo ata a una orden concreta: <i>«el sistema permite emitir la asignación de
/// OM-2026-0540»</i>. Un levantamiento por persona sin fecha de fin sería un permiso
/// permanente que nadie se acuerda de revocar, y el bloqueo dejaría de existir para esa
/// persona sin que ningún acto lo diga.
/// </summary>
/// <param name="Autoriza">
/// Quién y <b>con qué competencia</b>. `RN-86`: <i>«se levanta solo por acto registrado de
/// ACT-08 con motivo, no por decisión de quien programa»</i>. Que sea `ACT-08` se comprueba
/// contra el organigrama antes de construir esto — <see cref="Autoria"/> registra lo
/// verificado, no verifica.
/// </param>
public sealed record LevantamientoDeBloqueo(
    Ulid Mision,
    Autoria Autoriza,
    string Motivo,
    DateTimeOffset Momento);

/// <summary>
/// Los controles del circuito de reintegro — `RN-86`.
/// </summary>
public static class ReglasDelReintegro
{
    /// <summary>
    /// Sólo el peculio propio va a favor del servidor, y sólo él.
    ///
    /// Cruzarlas produciría una obligación que dice que la institución le debe al motorista el
    /// dinero que el motorista perdió — y eso no es un error de captura que alguien vaya a
    /// notar leyendo el reporte: cuadra formalmente y miente en el signo.
    /// </summary>
    public static void ExigirCausaCoherenteConLaDireccion(
        DireccionDelReintegro direccion, CausaDelReintegro causa)
    {
        var esPeculio = causa is CausaDelReintegro.PeculioPropio;
        var aFavorDelServidor = direccion is DireccionDelReintegro.AFavorDelServidor;

        if (esPeculio == aFavorDelServidor) return;

        throw new BloqueoDuro("RN-86",
            esPeculio
                ? "El peculio propio genera obligación a favor del servidor: fue su dinero. " +
                  "Registrarla a cargo de él lo haría deudor de lo que puso."
                : $"La causa {causa} es un faltante del fondo y va a cargo del servidor. " +
                  "A favor del servidor sólo va el peculio propio (`RN-C26d`).");
    }

    /// <summary>
    /// `RN-86` — <b>no se le asigna fondo nuevo a quien tiene un saldo vencido o una obligación
    /// abierta.</b>
    ///
    /// ── Las dos mitades no son la misma ─────────────────────────────────────
    /// La regla las enumera aparte, y `HU-078` les da un escenario a cada una. La obligación
    /// es una deuda determinada por alguien; el saldo vencido es dinero que simplemente no
    /// volvió y que todavía nadie determinó. Bloquear sólo por obligación dejaría pasar todo
    /// el intervalo entre que el plazo vence y que alguien se sienta a nominar — que es
    /// justamente el hueco donde `CE-26` dice que nace el faltante.
    ///
    /// ── Lo que NO bloquea ───────────────────────────────────────────────────
    /// El saldo <b>dentro de plazo</b>: el motorista que volvió anoche tiene dinero afuera y
    /// está en su derecho. Y la obligación <b>a favor del servidor</b>: que la institución le
    /// deba a alguien no es motivo para negarle un vale — sería castigarlo por haber puesto de
    /// su bolsillo.
    /// </summary>
    /// <param name="quien">
    /// Cómo nombrar a la persona en el mensaje. El bloqueo se juzga por el ULID del padrón, que
    /// ya viene resuelto en los saldos y las obligaciones; esto es sólo para que el mensaje
    /// diga un nombre y no un identificador.
    /// </param>
    /// <param name="levantamiento">
    /// El acto de ACT-08, si lo hubo, <b>para esta misión</b>. Un levantamiento de otra misión
    /// no sirve: se verifica acá y no en quien llama.
    /// </param>
    public static void ExigirQueNoDebaReintegro(
        string quien,
        Ulid mision,
        IReadOnlyList<ObligacionDeReintegro> obligaciones,
        IReadOnlyList<SaldoAfuera> saldos,
        DateOnly hoy,
        LevantamientoDeBloqueo? levantamiento = null)
    {
        var debe = obligaciones
            .Where(o => o.EstaAbierta && o.Direccion is DireccionDelReintegro.AFavorDeLaInstitucion)
            .ToList();

        var vencidos = saldos.Where(s => s.VencidoAl(hoy)).ToList();

        if (debe.Count == 0 && vencidos.Count == 0) return;

        // El levantamiento sólo vale para la misión que lo motivó. Verificarlo acá y no antes
        // es lo que impide que quien llama traiga el de otra orden.
        if (levantamiento is { } acto && acto.Mision == mision) return;

        var motivos = new List<string>();

        foreach (var o in debe)
            motivos.Add(
                $"obligación de reintegro abierta de {o.Saldo:N2} por {Nombre(o.Causa)}, " +
                $"del hecho del {o.FechaDelHecho:dd/MM/yyyy} — {o.AntiguedadEnDias(hoy)} días");

        foreach (var s in vencidos)
            motivos.Add(
                $"{s.Monto:N2} sin comprobar del vale {s.FolioDelVale} de la misión " +
                $"{s.ReferenciaDeLaMision}, con plazo vencido el {s.Vence:dd/MM/yyyy}");

        throw new BloqueoDuro("RN-86",
            $"{quien} no puede recibir nueva asignación: " +
            string.Join("; ", motivos) + ". " +
            "El levantamiento del bloqueo es acto de Gerencia Administrativa con motivo " +
            "escrito, no decisión de quien programa ni de quien emite. La otra salida es " +
            "sustituir al motorista de la misión (`RN-14`).");
    }

    /// <summary>
    /// El levantamiento exige motivo escrito, y `RN-03` exige que sea un acto con autor,
    /// competencia y momento.
    ///
    /// ⚠️ <b>Que quien autoriza sea ACT-08 se comprueba contra el organigrama</b>, y el mapa
    /// rol↔puesto es de la institución — `[C]`, insumo #1. Mientras no exista, esto verifica
    /// lo que sí se puede verificar: que haya competencia declarada y motivo. Lo que no se
    /// hace es fingir que el puesto se validó.
    /// </summary>
    public static LevantamientoDeBloqueo Levantar(
        Ulid mision, Autoria autoriza, string motivo, DateTimeOffset momento)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new BloqueoDuro("RN-86",
                "El levantamiento exige motivo escrito. Queda en el expediente de la misión, en " +
                "el de la obligación y en el indicador de levantamientos por persona y período.");

        return new LevantamientoDeBloqueo(mision, autoriza, motivo.Trim(), momento);
    }

    private static string Nombre(CausaDelReintegro causa) => causa switch
    {
        CausaDelReintegro.SinCausaIdentificada => "faltante sin causa identificada",
        CausaDelReintegro.AplicacionAFinDistinto => "aplicación del fondo a fin distinto",
        CausaDelReintegro.Extravio => "extravío",
        CausaDelReintegro.PeculioPropio => "peculio propio",
        _ => causa.ToString(),
    };
}
