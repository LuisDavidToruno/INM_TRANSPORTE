using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M12_Incidentes;

namespace Sigti.Pruebas.M12_Incidentes;

/// <summary>
/// `RN-75` — el bien retenido, sustraído o no recuperado no sale del registro.
///
/// <i>«El bien permanece en el registro patrimonial hasta su recuperación o su descargo formal.
/// <b>Nunca se elimina</b>»</i>.
/// </summary>
public class ReglasDelBienNoRecuperadoPruebas
{
    private static readonly DateOnly Hecho = new(2026, 3, 14);

    // ── La custodia conocida ────────────────────────────────────────────────

    /// <summary>
    /// De una <b>retención por autoridad</b> se sabe quién tiene el bien y bajo qué expediente:
    /// el acta lo dice. No saberlo es un dato que falta, no una situación distinta.
    /// </summary>
    [Fact]
    public void Un_bien_retenido_sin_autoridad_ni_expediente_no_pasa()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelBienNoRecuperado.ExigirCustodiaConocida(
                TipoDeIncidente.RetencionPorAutoridad, Bien()));

        Assert.Equal("RN-75", error.Precondicion);
        Assert.Contains("no hay a quién reclamarle la devolución", error.Message);
    }

    [Fact]
    public void Un_bien_retenido_con_autoridad_y_expediente_pasa() =>
        ReglasDelBienNoRecuperado.ExigirCustodiaConocida(
            TipoDeIncidente.RetencionPorAutoridad,
            Bien(autoridad: "Fiscalía Especial contra el Crimen Organizado",
                expediente: "MP-2026-4417"));

    /// <summary>
    /// <b>De una sustracción puede no saberse nada</b>, y exigir la ubicación impediría registrar
    /// el robo — que es el peor de los resultados posibles, porque el hecho quedaría sin registrar.
    /// </summary>
    [Fact]
    public void Una_sustraccion_sin_ubicacion_conocida_SI_se_puede_registrar() =>
        ReglasDelBienNoRecuperado.ExigirCustodiaConocida(TipoDeIncidente.Sustraccion, Bien());

    /// <summary>Un bien ya recuperado no necesita custodia declarada: volvió.</summary>
    [Fact]
    public void Un_bien_ya_recuperado_no_exige_custodia() =>
        ReglasDelBienNoRecuperado.ExigirCustodiaConocida(
            TipoDeIncidente.RetencionPorAutoridad, Bien(estado: EstadoDelBien.Recuperado));

    // ── El descargo formal ──────────────────────────────────────────────────

    /// <summary>
    /// Es la <b>única salida que no es la recuperación</b>. Sin acto formal sería una baja sin
    /// respaldo sobre un bien del Estado, que es lo que `NRM-02` no admite.
    /// </summary>
    [Fact]
    public void El_descargo_sin_numero_ni_autoridad_no_pasa()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelBienNoRecuperado.ExigirDescargoFormal(
                Bien(), new ConstanciaDeDescargo("", "", new DateOnly(2027, 2, 1))));

        Assert.Contains("baja sin respaldo", error.Message);
    }

    [Fact]
    public void El_descargo_con_acto_formal_pasa() =>
        ReglasDelBienNoRecuperado.ExigirDescargoFormal(
            Bien(),
            new ConstanciaDeDescargo(
                "ACU-2027-08", "Gerencia Administrativa", new DateOnly(2027, 2, 1)));

    /// <summary>
    /// Un bien que ya volvió no se descarga: el descargo saca del registro lo que sigue afuera.
    /// </summary>
    [Fact]
    public void Un_bien_ya_recuperado_no_se_descarga()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelBienNoRecuperado.ExigirDescargoFormal(
                Bien(estado: EstadoDelBien.Recuperado),
                new ConstanciaDeDescargo("ACU-1", "GA", new DateOnly(2027, 2, 1))));

        Assert.Contains("ya salió por otra vía", error.Message);
    }

    // ── El cierre del expediente ────────────────────────────────────────────

    /// <summary>
    /// Cerrar con bienes afuera y sin declararlo los haría desaparecer de la vista sin que la
    /// recuperación ni el descargo hubieran ocurrido — el mismo abandono silencioso que `RN-97`
    /// persigue, un módulo más acá.
    /// </summary>
    [Fact]
    public void No_se_cierra_el_expediente_con_bienes_afuera_sin_declararlo()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelBienNoRecuperado.ExigirCierrePosible(
                Expediente(Bien()), "Se agotaron las gestiones.", declaracionDeBienes: null));

        Assert.Equal("RN-75", error.Precondicion);
        Assert.Contains("permanece en el registro patrimonial", error.Message);
    }

    [Fact]
    public void Declarados_los_bienes_el_expediente_cierra() =>
        ReglasDelBienNoRecuperado.ExigirCierrePosible(
            Expediente(Bien()),
            "Se agotaron las gestiones ante la Fiscalía.",
            "La unidad sigue bajo expediente MP-2026-4417 y no depende de la institución.");

    [Fact]
    public void Sin_decir_como_se_resolvio_no_cierra()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelBienNoRecuperado.ExigirCierrePosible(Expediente(), "  ", null));

        Assert.Contains("indistinguible de archivar el problema", error.Message);
    }

    /// <summary>
    /// Y una interrupción sin desenlace tampoco deja cerrar el expediente: cerrarlo dejaría la
    /// misión marcada como interrumpida para siempre, sin decir cómo siguió.
    /// </summary>
    [Fact]
    public void No_se_cierra_el_expediente_de_una_interrupcion_sin_desenlace()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelBienNoRecuperado.ExigirCierrePosible(
                Expediente(interrumpe: true), "Cerrado.", null));

        Assert.Equal("RN-70", error.Precondicion);
        Assert.Contains("sin decir cómo siguió", error.Message);
    }

    [Fact]
    public void Un_expediente_sin_bienes_afuera_cierra() =>
        ReglasDelBienNoRecuperado.ExigirCierrePosible(
            Expediente(Bien(estado: EstadoDelBien.Recuperado)),
            "La unidad se recuperó el 2 de abril y volvió a la flota.",
            declaracionDeBienes: null);

    // ── La antigüedad se cuenta desde el hecho ──────────────────────────────

    /// <summary>
    /// Como toda antigüedad de este sistema (`RN-97` punto 3): un bien que lleva tres años
    /// sustraído no se presenta como reciente.
    /// </summary>
    [Fact]
    public void Los_dias_fuera_se_cuentan_desde_el_hecho()
    {
        Assert.Equal(300, Bien().DiasFuera(Hecho.AddDays(300)));

        // Y el que volvió no acumula: dejó de estar afuera.
        Assert.Equal(0, Bien(estado: EstadoDelBien.Recuperado).DiasFuera(Hecho.AddDays(300)));
    }

    private static BienAfectado Bien(
        EstadoDelBien estado = EstadoDelBien.NoRecuperado,
        string? autoridad = null,
        string? expediente = null) =>
        new(Ulid.NewUlid(), "Pick-up doble cabina INS-P-014", true, estado, Hecho,
            null, autoridad, expediente);

    private static ExpedienteDeIncidente Expediente(
        BienAfectado? bien = null, bool interrumpe = false) =>
        new(Ulid.NewUlid(),
            TipoDeIncidente.Sustraccion,
            "Robo con violencia",
            Hecho,
            new DateTimeOffset(2026, 3, 14, 9, 0, 0, TimeSpan.FromHours(-6)),
            new DateTimeOffset(2026, 3, 14, 13, 0, 0, TimeSpan.FromHours(-6)),
            "Sustracción del vehículo en el estacionamiento del destino.",
            "P-MOTORISTA",
            Ulid.NewUlid(),
            Ulid.NewUlid(),
            "Choluteca",
            91_400,
            interrumpe,
            [],
            bien is null ? [] : [bien],
            [],
            "P-TRANSPORTE",
            new DateOnly(2026, 3, 21));
}
