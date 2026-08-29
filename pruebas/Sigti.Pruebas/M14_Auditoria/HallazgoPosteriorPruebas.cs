using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M14_Auditoria;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M14_Auditoria;

/// <summary>
/// `RN-93` — el expediente de hallazgo posterior, y su ciclo propio.
///
/// ── Por qué existe, con las palabras de la regla ─────────────────────────────
/// <i>«Basta con que la reapertura de un expediente cerrado exista para que se use, y basta con
/// que se use una vez para que <b>ningún reporte histórico vuelva a ser reproducible</b>. El
/// expediente de hallazgo posterior es la salida que permite corregir el efecto económico sin
/// destruir la reproducibilidad»</i>.
/// </summary>
public class HallazgoPosteriorPruebas
{
    private static readonly DateOnly Hecho = new(2026, 3, 15);
    private static readonly DateOnly Descubierto = new(2026, 11, 20);
    private static readonly DateOnly Hoy = new(2026, 11, 25);

    private static readonly DateTimeOffset Ahora =
        new(2026, 11, 20, 10, 0, 0, TimeSpan.FromHours(-6));

    private static readonly Ulid Mision = Ulid.NewUlid();
    private static readonly Ulid Vehiculo = Ulid.NewUlid();

    private static Autoria Quien(string persona, string puesto = "PU-AUDITORIA") =>
        Autoria.De(new IdPersona(persona), new IdPuesto(puesto), Descubierto);

    private static ExpedienteDeHallazgoPosterior Abierto(
        IReadOnlyList<Ulid>? misiones = null,
        Ulid? vehiculo = null,
        string? periodo = null) =>
        ExpedienteDeHallazgoPosterior.Abrir(
            Ulid.NewUlid(),
            "Comprobante duplicado en el estado de cuenta del proveedor",
            Hecho, Descubierto,
            "Conciliación del estado de cuenta de agosto",
            "Distribuidora Nacional, estado-de-cuenta-agosto-2026.csv",
            "adjunto-estado-de-cuenta.pdf",
            misiones ?? [Mision],
            vehiculo ?? Vehiculo,
            motorista: null,
            periodo,
            Quien("P-AUDITORIA"),
            Ahora);

    private static AsientoReverso Reverso(
        string identificador = "V-04-0091",
        decimal? efecto = -1_760m,
        string periodoAfectado = "2026-08",
        string periodoDeImputacion = "2026-11",
        string autoriza = "P-GERENCIA",
        string autorOriginal = "P-MOTORISTA",
        NaturalezaDelReverso naturaleza = NaturalezaDelReverso.ReversoEconomico,
        string valorAnterior = "1,760.00",
        string? valorNuevo = "0.00") =>
        new(Ulid.NewUlid(),
            new ReferenciaAlAsiento("consumo del vale", identificador,
                "Consumo F-88201 del vale VAL-CHO-2026-000418"),
            naturaleza, valorAnterior, valorNuevo,
            Hecho, Ahora,
            Quien("P-GERENCIA", "PU-GERENCIA-ADMIN"),
            new IdPersona(autoriza), new IdPersona(autorOriginal),
            "Cobro duplicado del proveedor",
            "Nota de crédito NC-0091 emitida por la distribuidora.",
            "nota-de-credito-0091.pdf",
            periodoAfectado, periodoDeImputacion, efecto);

    [Fact]
    public void La_antiguedad_se_cuenta_desde_el_HECHO_no_desde_el_descubrimiento()
    {
        // `RN-93`: «evita el incentivo perverso más obvio: descubrir tarde para que el
        // indicador se vea mejor».
        var e = Abierto();

        Assert.Equal(255, e.AntiguedadEnDias(Hoy));
        Assert.Equal(250, e.DiasHastaElDescubrimiento);
    }

    [Fact]
    public void Un_descubrimiento_ANTERIOR_al_hecho_se_rechaza()
    {
        // Eso no describe un hallazgo posterior. Son campos distintos precisamente para poder
        // verlo.
        var error = Assert.Throws<BloqueoDuro>(() =>
            ExpedienteDeHallazgoPosterior.Abrir(
                Ulid.NewUlid(), "Cualquiera",
                fechaDelHecho: new DateOnly(2026, 11, 20),
                fechaDelDescubrimiento: new DateOnly(2026, 3, 15),
                "Revisión", "Acta", null, [Mision], Vehiculo, null, null,
                Quien("P-AUDITORIA"), Ahora));

        Assert.Contains("anterior al hecho", error.Message);
    }

    [Fact]
    public void CERO_misiones_es_valido_y_es_el_caso_interesante()
    {
        // El paso por caseta de un domingo, el consumo de un vehículo que ese día no tenía
        // orden. **La ausencia de misión es el hallazgo** (`RN-59`).
        var e = Abierto(misiones: []);

        Assert.Empty(e.Misiones);
        Assert.Contains("SIN MISIÓN VINCULABLE", e.Diario[0].Motivo);
    }

    [Fact]
    public void Un_hallazgo_puede_vincular_VARIAS_misiones()
    {
        // Un comprobante duplicado en dos delegaciones es un solo hallazgo con dos misiones y
        // un asiento por cada efecto económico.
        var otra = Ulid.NewUlid();
        var e = Abierto();

        e.Vincular(otra, Quien("P-AUDITORIA"), "La segunda delegación con el mismo comprobante.",
            Ahora);

        Assert.Equal(2, e.Misiones.Count);
        Assert.Contains(otra, e.Misiones);
    }

    [Fact]
    public void Vincular_dos_veces_la_misma_mision_no_la_duplica()
    {
        var e = Abierto();
        e.Vincular(Mision, Quien("P-AUDITORIA"), "Otra vez.", Ahora);

        Assert.Single(e.Misiones);
    }

    [Fact]
    public void El_reverso_muestra_los_TRES_valores()
    {
        // §8.3: «todo reporte presenta el valor original, el reverso y el valor resultante.
        // Nunca solo el resultado».
        var e = Abierto();
        e.Revertir(Reverso(), Ahora);

        var cadena = e.Diario[^1].Motivo;

        Assert.Contains("«1,760.00» → «0.00»", cadena);
        Assert.Contains("efecto económico -1,760.00", cadena);
        Assert.Contains("período 2026-08", cadena);
        Assert.Contains("imputado a 2026-11", cadena);
    }

    [Fact]
    public void Un_valor_nuevo_NULO_se_dice_y_no_se_calla()
    {
        // Nulo es un valor: significa que el dato se declara sin valor correcto conocido, y eso
        // es distinto de no haberlo declarado.
        var e = Abierto();
        e.Revertir(Reverso(valorNuevo: null, efecto: null,
            naturaleza: NaturalezaDelReverso.CorreccionDeDato), Ahora);

        Assert.Contains("SIN VALOR CORRECTO CONOCIDO", e.Diario[^1].Motivo);
    }

    [Fact]
    public void Quien_produjo_el_asiento_no_puede_autorizar_su_reverso()
    {
        // `BD-06`. Corregirse a sí mismo un asiento cerrado es exactamente lo que la
        // inmutabilidad existe para impedir.
        var e = Abierto();

        var error = Assert.Throws<BloqueoDuro>(() =>
            e.Revertir(Reverso(autoriza: "P-MOTORISTA", autorOriginal: "P-MOTORISTA"), Ahora));

        Assert.Equal("BD-06", error.Precondicion);
        Assert.Contains("Corregirse a sí mismo", error.Message);
    }

    [Fact]
    public void El_reverso_economico_NO_se_imputa_al_periodo_que_afecta()
    {
        // §8.3: afecta los acumulados del período en que se registra, no los del original.
        // Reimputarlo haría que un reporte ya publicado diera un número distinto según cuándo
        // se pida.
        var e = Abierto();

        var error = Assert.Throws<BloqueoDuro>(() =>
            e.Revertir(Reverso(periodoAfectado: "2026-08", periodoDeImputacion: "2026-08"), Ahora));

        Assert.Contains("diera un número distinto según cuándo se pida", error.Message);
    }

    [Fact]
    public void Una_correccion_de_DATO_si_puede_ir_al_mismo_periodo()
    {
        // No mueve dinero, así que no hay acumulado que desincronizar.
        var e = Abierto();

        e.Revertir(Reverso(
            efecto: null, naturaleza: NaturalezaDelReverso.CorreccionDeDato,
            periodoAfectado: "2026-08", periodoDeImputacion: "2026-08"), Ahora);

        Assert.Single(e.Reversos);
    }

    [Fact]
    public void El_mismo_asiento_no_se_revierte_dos_veces()
    {
        // Un segundo reverso duplicaría el efecto económico sobre el período corriente, y esa
        // corrección de más no la va a poder rastrear nadie.
        var e = Abierto();
        e.Revertir(Reverso(identificador: "V-04-0091"), Ahora);

        var error = Assert.Throws<BloqueoDuro>(() =>
            e.Revertir(Reverso(identificador: "V-04-0091"), Ahora));

        Assert.Contains("ya tiene un reverso", error.Message);
    }

    [Fact]
    public void Un_hallazgo_con_varios_efectos_lleva_un_asiento_por_cada_uno()
    {
        var e = Abierto();
        e.Revertir(Reverso(identificador: "V-04-0091", efecto: -1_760m), Ahora);
        e.Revertir(Reverso(identificador: "V-04-0092", efecto: -900m), Ahora);

        Assert.Equal(2, e.Reversos.Count);
        Assert.Equal(-2_660m, e.EfectoEconomicoTotal);
    }

    [Fact]
    public void El_expediente_no_se_resuelve_sin_fundamento()
    {
        var e = Abierto();

        var error = Assert.Throws<BloqueoDuro>(() =>
            e.Resolver(ResolucionDelHallazgo.SinEfectoEconomico, "  ",
                Quien("P-GERENCIA"), Ahora));

        Assert.Contains("archivar el expediente sin mirarlo", error.Message);
    }

    [Fact]
    public void No_se_resuelve_CON_REVERSO_sin_ningun_reverso()
    {
        var e = Abierto();

        var error = Assert.Throws<BloqueoDuro>(() =>
            e.Resolver(ResolucionDelHallazgo.ConAsientoReverso, "Se corrigió.",
                Quien("P-GERENCIA"), Ahora));

        Assert.Contains("se corrigió algo que nadie tocó", error.Message);
    }

    [Fact]
    public void No_se_resuelve_SIN_EFECTO_habiendo_revertido_dinero()
    {
        // Sería falso de una forma que ningún reporte podría detectar después.
        var e = Abierto();
        e.Revertir(Reverso(), Ahora);

        var error = Assert.Throws<BloqueoDuro>(() =>
            e.Resolver(ResolucionDelHallazgo.SinEfecto, "Era un error mío.",
                Quien("P-GERENCIA"), Ahora));

        Assert.Contains("el hallazgo tuvo efecto", error.Message);
    }

    [Fact]
    public void El_error_del_propio_descubridor_se_CIERRA_no_se_borra()
    {
        // `RN-93` casos límite: «se resuelve como sin efecto, con su fundamento. Se cierra, no
        // se borra». Borrarlo dejaría a quien fue señalado sin constancia.
        var e = Abierto();

        e.Resolver(ResolucionDelHallazgo.SinEfecto,
            "La línea del estado de cuenta correspondía a otra institución. Error de lectura.",
            Quien("P-GERENCIA"), Ahora);

        Assert.False(e.EstaAbierto);
        Assert.Equal(ResolucionDelHallazgo.SinEfecto, e.Resolucion);

        // Y el asiento de apertura sigue ahí, con quién lo abrió y cómo.
        Assert.Equal("H-01", e.Diario[0].Id);
        Assert.Contains("SIN EFECTO", e.Diario[^1].Motivo);
    }

    [Fact]
    public void El_hallazgo_real_sin_dinero_tiene_su_propia_resolucion()
    {
        // El vehículo que circuló sin orden: la ausencia de misión ES el hallazgo, y no hay
        // monto que revertir. Llamarlo «sin efecto» diría que no pasó nada.
        var e = Abierto(misiones: []);

        e.Resolver(ResolucionDelHallazgo.SinEfectoEconomico,
            "El vehículo circuló sin orden de misión. Expediente disciplinario en M-12.",
            Quien("P-GERENCIA"), Ahora);

        Assert.Equal(ResolucionDelHallazgo.SinEfectoEconomico, e.Resolucion);
        Assert.Contains("no tiene efecto económico", e.Diario[^1].Motivo);
    }

    [Fact]
    public void Un_expediente_RESUELTO_ya_no_se_modifica()
    {
        // Igual que una misión cerrada no se reabre: lo que aparezca después es un hallazgo
        // nuevo, no una corrección de éste.
        var e = Abierto();
        e.Resolver(ResolucionDelHallazgo.SinEfecto, "Error de lectura.", Quien("P-GERENCIA"), Ahora);

        Assert.Contains("no una corrección de éste", Assert.Throws<BloqueoDuro>(() =>
            e.Revertir(Reverso(), Ahora)).Message);

        Assert.Throws<BloqueoDuro>(() =>
            e.Vincular(Ulid.NewUlid(), Quien("P-AUDITORIA"), "Otra.", Ahora));

        Assert.Throws<BloqueoDuro>(() =>
            e.Resolver(ResolucionDelHallazgo.SinEfectoEconomico, "Otra cosa.",
                Quien("P-GERENCIA"), Ahora));
    }
}
