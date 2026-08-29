using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M14_Auditoria;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M14_Auditoria;

/// <summary>
/// §8.3 —artefacto autoridad— el contenido y las precondiciones del asiento reverso.
///
/// <b>No existe el reverso genérico «de la misión»</b>: se revierte un asiento concreto. Y el
/// reporte muestra <i>«el valor original, el reverso y el valor resultante. Nunca solo el
/// resultado»</i>.
/// </summary>
public class ReglasDelAsientoReversoPruebas
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
    public void No_existe_el_reverso_generico_de_la_mision()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelAsientoReverso.ExigirContenidoCompleto(
                new ReferenciaAlAsiento("", "", "la misión"),
                "1000", "Motivo", "Fundamento", "2026-08", "2026-11"));

        Assert.Contains("No existe el reverso genérico", error.Message);
    }

    [Fact]
    public void El_reverso_exige_valor_anterior_motivo_fundamento_y_los_dos_periodos()
    {
        Assert.Contains("sólo puede presentar dos", Assert.Throws<BloqueoDuro>(() =>
            ReglasDelAsientoReverso.ExigirContenidoCompleto(
                new ReferenciaAlAsiento("t", "id", "d"), "  ", "M", "F", "a", "b")).Message);

        Assert.Contains("no produce ningún indicador", Assert.Throws<BloqueoDuro>(() =>
            ReglasDelAsientoReverso.ExigirContenidoCompleto(
                new ReferenciaAlAsiento("t", "id", "d"), "1000", " ", "F", "a", "b")).Message);

        Assert.Contains("la palabra de quien revierte", Assert.Throws<BloqueoDuro>(() =>
            ReglasDelAsientoReverso.ExigirContenidoCompleto(
                new ReferenciaAlAsiento("t", "id", "d"), "1000", "M", " ", "a", "b")).Message);

        Assert.Contains("deja de ser reproducible", Assert.Throws<BloqueoDuro>(() =>
            ReglasDelAsientoReverso.ExigirContenidoCompleto(
                new ReferenciaAlAsiento("t", "id", "d"), "1000", "M", "F", " ", "b")).Message);
    }

    [Fact]
    public void Un_documento_no_se_reemite_con_el_MISMO_folio()
    {
        // §8.3: el corregido es un documento nuevo, con folio nuevo, que declara «sustituye al
        // folio X» — y ambos se conservan.
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelAsientoReverso.ExigirSustitutoConFolioNuevo(
                NaturalezaDelReverso.AnulacionDeDocumento, "VAL-2026-0418", "VAL-2026-0418"));

        Assert.Contains("nunca se reemite con el mismo folio", error.Message);

        ReglasDelAsientoReverso.ExigirSustitutoConFolioNuevo(
            NaturalezaDelReverso.AnulacionDeDocumento, "VAL-2026-0418", "VAL-2026-0533");
    }
}
