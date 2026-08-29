using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M02_Parametros;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M09_Combustible;

/// <summary>
/// `RN-86` punto 1 y `CE-26` §1 — el dinero que está afuera, y desde cuándo.
///
/// <b>Nulo es «no se puede saber», nunca «no vence».</b> Es la disciplina que impide que el
/// arqueo declare vencido lo que no se pudo fechar, o que dé por devuelto lo que no volvió.
/// </summary>
public class ReglasDelSaldoAfueraPruebas
{
    private static readonly DateOnly Hoy = new(2026, 9, 24);
    private static readonly Ulid Motorista = Ulid.NewUlid();
    private static readonly Ulid Mision = Ulid.NewUlid();

    [Fact]
    public void El_plazo_salta_el_fin_de_semana()
    {
        // El motorista que retorna el jueves 24 de septiembre con 3 días hábiles vence el
        // martes 29, no el domingo 27. Un plazo corrido vencería un día en que no hay caja a
        // la cual devolverle el dinero.
        var jueves = new DateOnly(2026, 9, 24);
        Assert.Equal(DayOfWeek.Thursday, jueves.DayOfWeek);

        Assert.Equal(new DateOnly(2026, 9, 29), Calendario.SumarDiasHabiles(jueves, 3));
    }

    [Fact]
    public void Con_plazo_cero_vence_el_mismo_dia_del_hecho()
    {
        var jueves = new DateOnly(2026, 9, 24);
        Assert.Equal(jueves, Calendario.SumarDiasHabiles(jueves, 0));
    }

    [Fact]
    public void El_saldo_afuera_de_una_mision_que_NO_ha_retornado_no_tiene_vencimiento()
    {
        var saldo = ReglasDelSaldoAfuera.De(
            Ulid.NewUlid(), "FC-2026-0412", Motorista, Mision, "OM-2026-0540",
            asignado: 4_500m, consumido: 0m, devuelto: 0m, valeResuelto: false,
            retornoDeLaMision: null, plazoEnDiasHabiles: 5, Calendario);

        Assert.NotNull(saldo);
        Assert.Equal(4_500m, saldo.Monto);
        Assert.Null(saldo.Vence);
        Assert.False(saldo.VencidoAl(Hoy));
        Assert.Contains("no ha retornado", saldo.Explicacion);
    }

    [Fact]
    public void Sin_el_parametro_de_plazo_el_saldo_se_VE_pero_no_se_vence()
    {
        var saldo = ReglasDelSaldoAfuera.De(
            Ulid.NewUlid(), "FC-2026-0412", Motorista, Mision, "OM-2026-0540",
            asignado: 4_500m, consumido: 3_860m, devuelto: 0m, valeResuelto: false,
            retornoDeLaMision: new DateOnly(2026, 9, 17), plazoEnDiasHabiles: null, Calendario);

        // La primera pregunta del arqueo —quién tiene cuánto y desde cuándo— sí se contesta.
        Assert.NotNull(saldo);
        Assert.Equal(640m, saldo.Monto);
        Assert.Equal(new DateOnly(2026, 9, 17), saldo.Desde);
        Assert.Equal(7, saldo.DiasAfueraAl(Hoy));

        // La segunda —si está vencido— no, y lo dice.
        Assert.Null(saldo.Vence);
        Assert.False(saldo.VencidoAl(Hoy));
        Assert.Contains("insumo #32", saldo.Explicacion);
    }

    [Fact]
    public void El_vale_ya_liquidado_no_tiene_saldo_afuera()
    {
        // Su descargo está hecho. Lo que quede sin explicar después de eso es materia de la
        // obligación, que es otra entidad.
        var saldo = ReglasDelSaldoAfuera.De(
            Ulid.NewUlid(), "FC-2026-0412", Motorista, Mision, "OM-2026-0540",
            asignado: 4_500m, consumido: 3_860m, devuelto: 0m, valeResuelto: true,
            retornoDeLaMision: new DateOnly(2026, 9, 17), plazoEnDiasHabiles: 5, Calendario);

        Assert.Null(saldo);
    }

    private static readonly CalendarioDeDiasHabiles Calendario = new(
        "PRUEBA",
        new HashSet<DayOfWeek>
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday,
        },
        new HashSet<DateOnly>());
}
