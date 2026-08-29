using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M14_Auditoria;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M14_Auditoria;

/// <summary>
/// `RN-93` — lo que todo expediente exige para existir y para cerrarse.
///
/// <b>Fecha del hecho y fecha del descubrimiento son campos distintos y ambos obligatorios.</b>
/// Contar la antigüedad desde el hecho <i>«evita el incentivo perverso más obvio: descubrir
/// tarde para que el indicador se vea mejor»</i>.
/// </summary>
public class ReglasDelHallazgoPosteriorPruebas
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
    public void El_expediente_exige_tipo_como_y_contra_que_fuente()
    {
        Assert.Contains("no se agrupa", Assert.Throws<BloqueoDuro>(() =>
            ReglasDelHallazgoPosterior.ExigirDatosDelDescubrimiento(
                "  ", "Revisión", "Acta", Hecho, Descubierto)).Message);

        Assert.Contains("qué control funcionó", Assert.Throws<BloqueoDuro>(() =>
            ReglasDelHallazgoPosterior.ExigirDatosDelDescubrimiento(
                "Tipo", "  ", "Acta", Hecho, Descubierto)).Message);

        Assert.Contains("no se puede volver a comprobar", Assert.Throws<BloqueoDuro>(() =>
            ReglasDelHallazgoPosterior.ExigirDatosDelDescubrimiento(
                "Tipo", "Revisión", "  ", Hecho, Descubierto)).Message);
    }

    [Fact]
    public void Un_expediente_sin_NINGUN_vinculo_no_se_puede_investigar()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ExpedienteDeHallazgoPosterior.Abrir(
                Ulid.NewUlid(), "Tipo", Hecho, Descubierto, "Revisión", "Acta", null,
                [], vehiculo: null, motorista: null, periodo: null,
                Quien("P-AUDITORIA"), Ahora));

        Assert.Contains("no se puede investigar ni reportar", error.Message);
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
}
