using Sigti.Dominio.M03_Flota;

namespace Sigti.Pruebas.M03_Flota;

/// <summary>
/// Qué puede declarar una persona sobre el estado de un vehículo — §10.2.
///
/// ── Las dos cosas que impone, y su consecuencia patrimonial ──────────────────
/// Un vehículo puesto <b>«en misión» a mano</b> figura fuera sin que ninguna misión lo
/// respalde. Y un <b>descargo con misiones abiertas</b> deja expedientes vivos colgando de un
/// bien que ya no existe en el registro — un hallazgo que nadie puede explicar después.
///
/// ── Estas pruebas son del dominio puro ───────────────────────────────────────
/// No tocan base ni API. La regla es pura y recibe los datos ya traídos (`ADR-009`), y eso es
/// lo que permite ejercer los casos de borde sin montar tres capas para llegar a ellos.
/// </summary>
public class ReglasDeEstadoOperativoPruebas
{
    [Theory]
    [InlineData(EstadoOperativo.Asignado)]
    [InlineData(EstadoOperativo.EnMision)]
    public void Los_estados_que_fija_el_sistema_no_se_declaran_a_mano(EstadoOperativo destino)
    {
        // §10.2 sin margen: «permitir fijarlos a mano abre la puerta a un vehículo "en misión"
        // sin misión».
        var fallo = Assert.Throws<CambioDeEstadoInvalido>(
            () => ReglasDeEstadoOperativo.ExigirDeclarable(destino, EstadoOperativo.Disponible, 0));

        Assert.Contains("sin misión que lo respalde", fallo.Message);
    }

    [Theory]
    [InlineData(EstadoOperativo.EnTaller)]
    [InlineData(EstadoOperativo.NoDisponible)]
    [InlineData(EstadoOperativo.Prestado)]
    [InlineData(EstadoOperativo.Disponible)]
    public void Los_demas_si_se_declaran(EstadoOperativo destino)
    {
        // El recíproco. Sin él, la regla podría rechazar todo y las otras pruebas seguirían en
        // verde — y un sistema donde nadie puede declarar un taller no tiene `BD-07`.
        ReglasDeEstadoOperativo.ExigirDeclarable(destino, EstadoOperativo.Disponible, 0);
    }

    [Fact]
    public void El_primer_estado_de_un_vehiculo_se_puede_declarar()
    {
        // `actual` nulo es un vehículo recién dado de alta al que nadie le fijó estado. Tiene
        // que poder recibir el primero: si no, no habría forma de habilitarlo nunca.
        ReglasDeEstadoOperativo.ExigirDeclarable(EstadoOperativo.Disponible, actual: null, 0);
    }

    [Theory]
    [InlineData(EstadoOperativo.DadoDeBaja)]
    [InlineData(EstadoOperativo.RetiradoDeFlota)]
    public void Un_terminal_con_misiones_abiertas_no_se_declara(EstadoOperativo destino)
    {
        var fallo = Assert.Throws<CambioDeEstadoInvalido>(
            () => ReglasDeEstadoOperativo.ExigirDeclarable(destino, EstadoOperativo.Disponible, 2));

        Assert.Contains("2 misión(es) sin cerrar", fallo.Message);
    }

    [Theory]
    [InlineData(EstadoOperativo.DadoDeBaja)]
    [InlineData(EstadoOperativo.RetiradoDeFlota)]
    public void Sin_misiones_abiertas_el_terminal_si_procede(EstadoOperativo destino)
    {
        ReglasDeEstadoOperativo.ExigirDeclarable(destino, EstadoOperativo.Disponible, 0);
    }

    [Fact]
    public void De_un_descargo_no_se_sale_y_el_mensaje_dice_por_donde_va_el_tramite()
    {
        var fallo = Assert.Throws<CambioDeEstadoInvalido>(
            () => ReglasDeEstadoOperativo.ExigirDeclarable(
                EstadoOperativo.Disponible, EstadoOperativo.DadoDeBaja, 0));

        Assert.Contains("registro de bienes del Estado", fallo.Message);
    }

    [Fact]
    public void De_un_retiro_de_flota_tampoco_y_NO_es_lo_mismo_que_un_descargo()
    {
        // La distinción que motivó crear el segundo terminal: el descargo extingue un bien
        // propio, el retiro devuelve uno que nunca lo fue. Confundirlos produce **un asiento
        // falso sobre un bien ajeno**, detectable cruzando el inventario contra el padrón.
        var fallo = Assert.Throws<CambioDeEstadoInvalido>(
            () => ReglasDeEstadoOperativo.ExigirDeclarable(
                EstadoOperativo.Disponible, EstadoOperativo.RetiradoDeFlota, 0));

        Assert.Contains("ya no está bajo tenencia", fallo.Message);
        Assert.DoesNotContain("registro de bienes del Estado", fallo.Message);
    }

    [Fact]
    public void Las_misiones_abiertas_solo_estorban_a_los_terminales()
    {
        // Un vehículo con misiones vivas sí puede entrar a taller — de hecho es el caso
        // normal: se avería con misiones programadas, y por eso existe `T-10` para reasignar.
        ReglasDeEstadoOperativo.ExigirDeclarable(
            EstadoOperativo.EnTaller, EstadoOperativo.Disponible, misionesAbiertas: 3);
    }

    [Theory]
    [InlineData(EstadoOperativo.DadoDeBaja, true)]
    [InlineData(EstadoOperativo.RetiradoDeFlota, true)]
    [InlineData(EstadoOperativo.Disponible, false)]
    [InlineData(EstadoOperativo.EnTaller, false)]
    [InlineData(EstadoOperativo.Prestado, false)]
    public void Cuales_son_terminales(EstadoOperativo estado, bool esperado) =>
        Assert.Equal(esperado, ReglasDeEstadoOperativo.EsTerminal(estado));
}
