using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M09_Combustible;

/// <summary>
/// Los controles propios del fondo — `RN-26`.
///
/// ── Lo que estas pruebas protegen ────────────────────────────────────────────
/// El hallazgo `HN1-15` no fue que la segregación estuviera mal escrita: fue que <b>no estaba
/// sostenida por ninguna regla aplicable</b>. `RN-01` razona por misión y el fondo es de
/// período. Si esta clase se debilita, el circuito de dinero vuelve a quedar sin control y
/// nada más en el sistema se entera.
/// </summary>
public class ReglasDelFondoPruebas
{
    private static readonly IdPersona Jefe = new("P-TRANSPORTE");
    private static readonly IdPersona Gerente = new("P-GERENCIA");
    private static readonly IdPersona Contador = new("P-CONTABILIDAD");

    [Fact]
    public void Quien_solicita_el_fondo_no_lo_aprueba()
    {
        var fallo = Assert.Throws<BloqueoDuro>(
            () => ReglasDelFondo.ExigirQueQuienApruebaNoSeaQuienSolicito(Jefe, Jefe));

        Assert.Equal("RN-26.4", fallo.Precondicion);
        // Que el mensaje diga «identidad de persona, no rol» no es adorno: es la diferencia
        // entre este control y un permiso, y quien lo lea tiene que poder distinguirla.
        Assert.Contains("identidad de persona", fallo.Message);
    }

    [Fact]
    public void Dos_personas_distintas_si_pueden_solicitar_y_aprobar()
    {
        // El recíproco. Sin él la regla podría rechazar toda aprobación y la prueba anterior
        // seguiría en verde sobre un sistema donde ningún fondo se aprueba nunca.
        ReglasDelFondo.ExigirQueQuienApruebaNoSeaQuienSolicito(Jefe, Gerente);
    }

    [Fact]
    public void Quien_aprobo_el_fondo_no_lo_liquida()
    {
        // La mitad que se olvida: separar pedir de autorizar no sirve de nada si al final del
        // período el mismo que autorizó declara que todo cuadró.
        var fallo = Assert.Throws<BloqueoDuro>(
            () => ReglasDelFondo.ExigirQueQuienLiquidaNoSeaNingunoDeLosDos(Jefe, Gerente, Gerente));

        Assert.Contains("no es quien declara que el gasto cuadró", fallo.Message);
    }

    [Fact]
    public void Quien_solicito_el_fondo_tampoco_lo_liquida()
    {
        var fallo = Assert.Throws<BloqueoDuro>(
            () => ReglasDelFondo.ExigirQueQuienLiquidaNoSeaNingunoDeLosDos(Jefe, Gerente, Jefe));

        Assert.Contains("solicitó", fallo.Message);
    }

    [Fact]
    public void Un_tercero_si_liquida()
    {
        ReglasDelFondo.ExigirQueQuienLiquidaNoSeaNingunoDeLosDos(Jefe, Gerente, Contador);
    }

    [Fact]
    public void Sin_saldo_no_hay_asignacion_y_el_mensaje_dice_cuanto_falta()
    {
        var fallo = Assert.Throws<BloqueoDuro>(
            () => ReglasDelFondo.ExigirSaldoSuficiente(
                saldoDisponible: 1_500m, montoAAsignar: 2_000m, toleranciaSobregiro: 0m));

        // «No alcanza» manda a quien está en la ventanilla a hacer la resta. El número es lo
        // que le dice si pide una ampliación de 500 o si el fondo del mes se acabó.
        Assert.Contains("Faltan 500.00", fallo.Message);
        Assert.Contains("ampliación", fallo.Message);
    }

    [Fact]
    public void Con_saldo_exacto_la_asignacion_procede()
    {
        // El borde. Gastar hasta el último lempira aprobado es legítimo: el bloqueo es por
        // exceder, no por agotar.
        ReglasDelFondo.ExigirSaldoSuficiente(2_000m, 2_000m, 0m);
    }

    [Fact]
    public void La_tolerancia_inicial_es_cero_y_por_eso_un_centavo_de_mas_bloquea()
    {
        // `RN-26`: «Con tolerancia_sobregiro en cero —su valor inicial— no hay excepción».
        Assert.Throws<BloqueoDuro>(
            () => ReglasDelFondo.ExigirSaldoSuficiente(2_000m, 2_000.01m, 0m));
    }

    [Fact]
    public void Con_tolerancia_configurada_el_sobregiro_cabe_y_el_mensaje_lo_dice()
    {
        ReglasDelFondo.ExigirSaldoSuficiente(2_000m, 2_050m, toleranciaSobregiro: 100m);

        var fallo = Assert.Throws<BloqueoDuro>(
            () => ReglasDelFondo.ExigirSaldoSuficiente(2_000m, 2_200m, toleranciaSobregiro: 100m));

        // Que la tolerancia ya se contó tiene que estar en el mensaje: si no, quien lo lee
        // cree que puede pedir la excepción que ya se le aplicó.
        Assert.Contains("tolerancia", fallo.Message);
        Assert.Contains("Faltan 100.00", fallo.Message);
    }

    [Fact]
    public void Un_fondo_con_asignaciones_vivas_no_se_cierra()
    {
        var fallo = Assert.Throws<BloqueoDuro>(
            () => ReglasDelFondo.ExigirCierrable(asignacionesSinLiquidar: 3, "12-01-001-4-31200"));

        Assert.Contains("3 asignación(es) sin liquidar", fallo.Message);
    }

    [Fact]
    public void Un_fondo_sin_partida_presupuestaria_tampoco()
    {
        // `RN-26`: la partida la define ARGOS. Si el espejo no la tiene, el fondo se registra
        // con partida pendiente y se BLOQUEA su cierre. No se inventa un código.
        var fallo = Assert.Throws<BloqueoDuro>(
            () => ReglasDelFondo.ExigirCierrable(0, partidaPresupuestaria: "   "));

        Assert.Contains("no se inventa", fallo.Message);
    }

    [Fact]
    public void Con_todo_liquidado_y_partida_completa_el_fondo_cierra()
    {
        ReglasDelFondo.ExigirCierrable(0, "12-01-001-4-31200");
    }

    [Fact]
    public void Una_mision_no_se_imputa_al_fondo_de_otra_delegacion()
    {
        var fallo = Assert.Throws<BloqueoDuro>(
            () => ReglasDelFondo.ExigirMismoAmbito(
                AmbitoDelFondo.Delegacion, "Delegacion de Choluteca", "Delegacion de Danli"));

        Assert.Contains("Choluteca", fallo.Message);
        Assert.Contains("Danli", fallo.Message);
    }

    [Fact]
    public void El_fondo_de_institucion_cubre_a_cualquier_dependencia()
    {
        // A nivel institución no hay nada que comparar, y comparar igual rompería toda
        // asignación de un fondo central.
        ReglasDelFondo.ExigirMismoAmbito(
            AmbitoDelFondo.Institucion, "Instituto", "Delegacion de Danli");
    }
}
