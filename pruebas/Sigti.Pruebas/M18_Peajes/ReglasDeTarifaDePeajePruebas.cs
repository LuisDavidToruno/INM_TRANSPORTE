using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M18_Peajes;

namespace Sigti.Pruebas.M18_Peajes;

/// <summary>
/// `RN-34` — la tarifa se resuelve por <b>punto × categoría × fecha del hecho</b>.
///
/// ── Lo que la tabla tiene que soportar, y ya ocurrió ─────────────────────────
/// `NRM-10` documenta 2026: anuncio el 08/01, suspensión hacia el 15/01, prórroga al 15/02,
/// nuevo anuncio el 27/02 y confirmación de la SIT el 28/02 de que <b>no habría incremento</b>.
/// Vigencias cortas, cierre anticipado de una vigencia abierta y aumentos retroactivos no son
/// casos de laboratorio: son el año pasado.
/// </summary>
public class ReglasDeTarifaDePeajePruebas
{
    private static readonly Ulid Zambrano = Ulid.NewUlid();
    private static readonly CategoriaDePeaje Liviano = new("LIVIANO", "Liviano/Turismo");
    private static readonly DateTimeOffset Hoy = new(2026, 3, 16, 8, 0, 0, TimeSpan.FromHours(-6));

    private static TarifaDePeaje Tarifa(
        decimal monto, DateOnly desde, DateOnly? hasta = null,
        string categoria = "LIVIANO", DateTimeOffset? registrada = null) =>
        new(Ulid.NewUlid(), Zambrano, categoria, monto, "SAPP", desde, desde, hasta,
            registrada ?? new DateTimeOffset(2025, 12, 1, 0, 0, 0, TimeSpan.Zero));

    [Fact]
    public void La_tarifa_se_resuelve_a_la_fecha_del_HECHO_no_a_la_de_hoy()
    {
        // Un paso de enero se valora con la tarifa de enero, aunque en marzo rija otra.
        var tabla = new[]
        {
            Tarifa(22m, new DateOnly(2025, 1, 1), new DateOnly(2026, 1, 31)),
            Tarifa(26m, new DateOnly(2026, 2, 1)),
        };

        var enero = ReglasDeTarifaDePeaje.Resolver(
            tabla, Zambrano, Liviano, new DateOnly(2026, 1, 20), Hoy);

        var marzo = ReglasDeTarifaDePeaje.Resolver(
            tabla, Zambrano, Liviano, new DateOnly(2026, 3, 10), Hoy);

        Assert.Equal(22m, enero!.Monto);
        Assert.Equal(26m, marzo!.Monto);
    }

    [Fact]
    public void Sin_tarifa_vigente_devuelve_NULO_y_no_un_valor_por_defecto()
    {
        // `RN-34`: el sistema no calcula un valor por defecto. Quien llama decide si la ausencia
        // es bloqueo o una línea marcada, y la regla no adivina por él.
        var tabla = new[] { Tarifa(22m, new DateOnly(2026, 2, 1)) };

        Assert.Null(ReglasDeTarifaDePeaje.Resolver(
            tabla, Zambrano, Liviano, new DateOnly(2026, 1, 20), Hoy));
    }

    [Fact]
    public void La_tarifa_de_OTRA_categoria_no_sirve()
    {
        // Es el error de cuatro veces de más: un liviano no se valora con la tarifa del vehículo
        // de dos ejes sólo porque es la única cargada para ese punto.
        var tabla = new[] { Tarifa(90m, new DateOnly(2025, 1, 1), categoria: "EJES-2") };

        Assert.Null(ReglasDeTarifaDePeaje.Resolver(
            tabla, Zambrano, Liviano, new DateOnly(2026, 3, 10), Hoy));
    }

    [Fact]
    public void El_aumento_REVERTIDO_a_mitad_de_proceso_se_resuelve_con_la_vigencia_corta()
    {
        // Exactamente 2026: anunciado el 08/01, suspendido hacia el 15/01. La tabla admite
        // vigencias cortas y cierre anticipado de una vigencia ya abierta.
        var tabla = new[]
        {
            Tarifa(22m, new DateOnly(2025, 1, 1), new DateOnly(2026, 1, 7)),
            Tarifa(26m, new DateOnly(2026, 1, 8), new DateOnly(2026, 1, 15)),
            Tarifa(22m, new DateOnly(2026, 1, 16)),
        };

        Assert.Equal(26m, ReglasDeTarifaDePeaje
            .Resolver(tabla, Zambrano, Liviano, new DateOnly(2026, 1, 12), Hoy)!.Monto);

        Assert.Equal(22m, ReglasDeTarifaDePeaje
            .Resolver(tabla, Zambrano, Liviano, new DateOnly(2026, 1, 20), Hoy)!.Monto);
    }

    [Fact]
    public void El_aumento_RETROACTIVO_no_reescribe_lo_que_se_supo_entonces()
    {
        // COVI anunció uno «incluyendo subsidios pendientes de 2024 y 2025» `[V]`. El eje de
        // transacción de `ADR-006` es lo que permite reproducir el número que se pagó y el
        // corregido, que son dos preguntas legítimas.
        var enero = new DateTimeOffset(2026, 1, 5, 0, 0, 0, TimeSpan.Zero);
        var marzo = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);

        var tabla = new[]
        {
            Tarifa(22m, new DateOnly(2025, 1, 1), registrada: enero),

            // Cargada en marzo con vigencia desde enero: el aumento retroactivo.
            Tarifa(26m, new DateOnly(2026, 1, 1), registrada: marzo),
        };

        var comoSeSupoEnEnero = ReglasDeTarifaDePeaje.Resolver(
            tabla, Zambrano, Liviano, new DateOnly(2026, 1, 20), enero);

        var comoSeSabeHoy = ReglasDeTarifaDePeaje.Resolver(
            tabla, Zambrano, Liviano, new DateOnly(2026, 1, 20), marzo);

        Assert.Equal(22m, comoSeSupoEnEnero!.Monto);
        Assert.Equal(26m, comoSeSabeHoy!.Monto);
    }

    [Fact]
    public void Una_tarifa_sin_fuente_no_se_guarda()
    {
        // La tarifa que ve el usuario es política, no contractual. Sin saber quién la publicó no
        // se puede defender un cobro ante nadie — y hay contradicción abierta entre el
        // comunicado de la SIT y un agregador comercial.
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeTarifaDePeaje.ExigirFuenteYVerificacion("   ", 22m));

        Assert.Contains("política y no contractual", error.Message);
    }

    [Fact]
    public void Una_tarifa_en_CERO_si_se_admite()
    {
        // Un punto puede tener tarifa cero declarada. Lo que no se admite es negativa.
        ReglasDeTarifaDePeaje.ExigirFuenteYVerificacion("SAPP", 0m);

        Assert.Throws<BloqueoDuro>(() =>
            ReglasDeTarifaDePeaje.ExigirFuenteYVerificacion("SAPP", -1m));
    }

    [Fact]
    public void La_alerta_de_los_doce_meses_se_calcula_contra_la_fecha_de_verificacion()
    {
        var tarifa = Tarifa(22m, new DateOnly(2025, 1, 1));

        Assert.False(tarifa.SinRevisarHaceMasDeUnAnio(new DateOnly(2025, 12, 1)));
        Assert.True(tarifa.SinRevisarHaceMasDeUnAnio(new DateOnly(2026, 3, 16)));
    }

    [Fact]
    public void El_estado_del_punto_tambien_se_resuelve_a_la_fecha_del_hecho()
    {
        // «Sin el estado con vigencia no se puede recalcular un viaje pasado por una caseta que
        // ya no existe».
        var vigencias = new[]
        {
            new VigenciaDelPunto(Zambrano, EstadoDelPunto.Activo, "Concesión.",
                new DateOnly(2025, 1, 1), new DateOnly(2026, 1, 31), Hoy),

            new VigenciaDelPunto(Zambrano, EstadoDelPunto.Cerrado, "Terminación anticipada.",
                new DateOnly(2026, 2, 1), null, Hoy),
        };

        Assert.Equal(EstadoDelPunto.Activo, ReglasDeTarifaDePeaje
            .EstadoA(vigencias, Zambrano, new DateOnly(2026, 1, 10), Hoy)!.Estado);

        Assert.Equal(EstadoDelPunto.Cerrado, ReglasDeTarifaDePeaje
            .EstadoA(vigencias, Zambrano, new DateOnly(2026, 3, 10), Hoy)!.Estado);
    }
}
