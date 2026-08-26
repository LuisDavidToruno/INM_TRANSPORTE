using Sigti.Dominio.M02_Parametros;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M02_Parametros;

/// <summary>
/// El catálogo de parámetros normativos, bitemporal (`ADR-006`, decisión `D-13`).
///
/// Dos ejes que responden preguntas distintas: <b>qué decía el reglamento el día del
/// viaje</b> y <b>qué sabía el sistema el día que se liquidó</b>. Un solo eje responde una
/// y falsifica la otra.
/// </summary>
public class CatalogoDeParametrosPruebas
{
    private static readonly IdPersona Carlos = new("P-CARLOS");
    private static readonly IdPersona Gerencia = new("P-GERENCIA");

    private const string Zambrano = "peaje:zambrano:liviana";

    [Fact]
    public void Sin_vigencia_aprobada_para_la_fecha_del_hecho_se_bloquea_el_calculo()
    {
        // HU-147: no se toma la vigencia más cercana ni un valor por omisión. Se bloquea.
        // Un cálculo con la tarifa equivocada produce un número que acabaría en un reporte
        // del TSC, y nadie sabría que está mal.
        var catalogo = new CatalogoDeParametros([
            Aprobada("22.00", desde: new DateOnly(2026, 7, 1))
        ]);

        var fallo = Assert.Throws<ParametroSinVigencia>(
            () => catalogo.Resolver(Zambrano, fechaDelHecho: new DateOnly(2026, 3, 12), conocidoAl: Ahora));

        Assert.Equal(Zambrano, fallo.Clave);
        Assert.Equal(new DateOnly(2026, 3, 12), fallo.FechaDelHecho);
    }

    [Fact]
    public void Entre_dos_vigencias_resuelve_la_que_regia_el_dia_del_viaje()
    {
        // No la vigente hoy: la que regía cuando pasó el hecho (P-4, RNF-05).
        var catalogo = new CatalogoDeParametros([
            Aprobada("22.00", desde: new DateOnly(2026, 1, 1), hasta: new DateOnly(2026, 6, 30)),
            Aprobada("25.00", desde: new DateOnly(2026, 7, 1))
        ]);

        Assert.Equal("22.00",
            catalogo.Resolver(Zambrano, new DateOnly(2026, 3, 12), Ahora).Valor);
        Assert.Equal("25.00",
            catalogo.Resolver(Zambrano, new DateOnly(2026, 8, 2), Ahora).Valor);
    }

    [Fact]
    public void Una_version_sin_aprobar_no_resuelve()
    {
        // Si una carga pendiente ya se usara para calcular, el doble control de HU-145
        // sería decorativo: el valor estaría en producción antes de que nadie lo revisara.
        var catalogo = new CatalogoDeParametros([
            Aprobada("22.00", desde: new DateOnly(2026, 1, 1)) with { AprobadoPor = null }
        ]);

        Assert.Throws<ParametroSinVigencia>(
            () => catalogo.Resolver(Zambrano, new DateOnly(2026, 3, 12), Ahora));
    }

    [Fact]
    public void Una_correccion_retroactiva_no_cambia_lo_que_la_liquidacion_pago()
    {
        // El escenario de HU-148. La tarifa de marzo se corrigió en septiembre, y hay DOS
        // preguntas legítimas sobre el mismo día de marzo:
        //   ¿qué decía el reglamento?      → 24.00, la corregida
        //   ¿qué creía el sistema al pagar? → 22.00, la que explica el monto emitido
        // Un solo eje responde una y falsifica la otra.
        var correccion = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.FromHours(-6));

        var catalogo = new CatalogoDeParametros([
            Aprobada("22.00", desde: new DateOnly(2026, 1, 1), hasta: new DateOnly(2026, 6, 30))
                with { RegistradoHasta = correccion },
            Aprobada("24.00", desde: new DateOnly(2026, 1, 1), hasta: new DateOnly(2026, 6, 30))
                with { RegistradoDesde = correccion }
        ]);

        var marzo = new DateOnly(2026, 3, 12);
        var cuandoSeLiquido = new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.FromHours(-6));
        var hoy = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.FromHours(-6));

        Assert.Equal("22.00", catalogo.Resolver(Zambrano, marzo, cuandoSeLiquido).Valor);
        Assert.Equal("24.00", catalogo.Resolver(Zambrano, marzo, hoy).Valor);
    }

    [Fact]
    public void El_valor_resuelto_trae_la_evidencia_de_por_que()
    {
        // RNF-06: un reporte reemitido años después tiene que dar lo mismo, y eso exige
        // guardar contra qué se resolvió, no solo el resultado.
        var catalogo = new CatalogoDeParametros([Aprobada("22.00", desde: new DateOnly(2026, 1, 1))]);

        var resuelto = catalogo.Resolver(Zambrano, new DateOnly(2026, 3, 12), Ahora);

        Assert.Equal(Zambrano, resuelto.Clave);
        Assert.Equal(new DateOnly(2026, 3, 12), resuelto.FechaDelHecho);
        Assert.Equal(Ahora, resuelto.ConocidoAl);
        Assert.Equal(new DateOnly(2026, 1, 1), resuelto.VigenteDesde);
    }

    private static readonly DateTimeOffset Ahora =
        new(2026, 8, 14, 10, 0, 0, TimeSpan.FromHours(-6));

    private static VersionDeParametro Aprobada(string valor, DateOnly desde, DateOnly? hasta = null) =>
        new(Clave: Zambrano,
            Valor: valor,
            VigenteDesde: desde,
            VigenteHasta: hasta,
            RegistradoDesde: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.FromHours(-6)),
            RegistradoHasta: null,
            CargadoPor: Carlos,
            AprobadoPor: Gerencia);
}
