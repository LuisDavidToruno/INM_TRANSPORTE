using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M05_Motoristas;

namespace Sigti.Pruebas.M05_Motoristas;

/// <summary>
/// `BD-02` — Licencia habilitante y vigente durante todo el rango.
///
/// Tres condiciones que deben cumplirse <b>las tres</b>. Sin excepción configurable:
/// el PO lo confirmó en `DP-001, D-12` —«nos tenemos que proteger con la ley también»—
/// y una excepción registrada sería evidencia en contra ante un siniestro.
///
/// <b>Los valores de matriz de estas pruebas son datos de prueba, no la matriz real.</b>
/// La matriz oficial es insumo abierto (`[C]`, el PDF de la DNVT), y por eso es un
/// catálogo con vigencia y no una tabla cableada.
/// </summary>
public class ReglasDeHabilitacionPruebas
{
    private static readonly DateOnly Salida = new(2026, 3, 12);

    private static readonly FichaTecnica Pickup = new(
        TipoDeVehiculo: "PICKUP",
        PesoBrutoKg: 2_800,
        CapacidadPasajeros: 5,
        EsArticulado: false);

    /// <summary>Matriz de prueba: la categoría B habilita hasta 3.500 kg y 8 pasajeros.</summary>
    private static readonly MatrizDeLicencias Matriz = MatrizDeLicencias.Con(
        vigenteDesde: new DateOnly(2026, 1, 1),
        version: "PRUEBA-01",
        entradas:
        [
            new EntradaDeMatriz(CategoriaDeLicencia.B, PesoBrutoMaximoKg: 3_500,
                CapacidadMaximaPasajeros: 8, PermiteArticulado: false)
        ]);

    [Fact]
    public void Una_licencia_que_vence_dentro_del_rango_de_la_mision_no_habilita()
    {
        // «Una licencia que vence el miércoles no habilita una misión que retorna el
        // viernes: el motorista conduciría sin licencia dos días, con responsabilidad
        // directa de quien autorizó.»
        var licencia = new Licencia(
            Numero: "0801-1990-01234",
            Categoria: CategoriaDeLicencia.B,
            Vencimiento: new DateOnly(2026, 3, 13),
            Restricciones: []);

        var ventana = new VentanaDeMision(Salida, new DateOnly(2026, 3, 14), HolguraDias: 0);

        var resultado = ReglasDeHabilitacion.Evaluar(licencia, Pickup, ventana, Matriz);

        Assert.False(resultado.Habilita);
        Assert.Equal(MotivoDeNoHabilitacion.LicenciaVenceDentroDelRango, resultado.Motivo);
    }

    [Fact]
    public void La_holgura_posterior_cuenta_dentro_del_rango()
    {
        // Retorno el 13 con dos días de holgura: el rango llega al 15, y una licencia que
        // vence el 14 deja al motorista conduciendo sin licencia el último día.
        var licencia = Vigente(hasta: new DateOnly(2026, 3, 14));
        var ventana = new VentanaDeMision(Salida, new DateOnly(2026, 3, 13), HolguraDias: 2);

        var resultado = ReglasDeHabilitacion.Evaluar(licencia, Pickup, ventana, Matriz);

        Assert.False(resultado.Habilita);
        Assert.Equal(new DateOnly(2026, 3, 15), resultado.FinDeRangoEvaluado);
    }

    [Fact]
    public void Una_categoria_sin_entrada_en_la_matriz_no_habilita()
    {
        // La ausencia se trata como negativa, nunca como permiso: si nadie declaró que la
        // categoría A puede conducir un pickup, no puede.
        var licencia = Vigente(hasta: new DateOnly(2027, 1, 1)) with { Categoria = CategoriaDeLicencia.A };

        var resultado = ReglasDeHabilitacion.Evaluar(licencia, Pickup, Ventana, Matriz);

        Assert.False(resultado.Habilita);
        Assert.Equal(MotivoDeNoHabilitacion.CategoriaNoHabilitaElVehiculo, resultado.Motivo);
    }

    [Fact]
    public void La_categoria_se_resuelve_por_peso_bruto_no_por_el_nombre_del_tipo()
    {
        // Mismo nombre de tipo, mismo todo, salvo el peso bruto. BD-02: la matriz no se
        // resuelve por nombre del tipo de vehículo.
        var licencia = Vigente(hasta: new DateOnly(2027, 1, 1));
        var pesado = Pickup with { PesoBrutoKg = 3_501 };

        Assert.True(ReglasDeHabilitacion.Evaluar(licencia, Pickup, Ventana, Matriz).Habilita);
        Assert.False(ReglasDeHabilitacion.Evaluar(licencia, pesado, Ventana, Matriz).Habilita);
    }

    [Fact]
    public void El_resultado_conserva_la_evidencia_aunque_habilite()
    {
        // «Guardar solo "verificado" no defiende a nadie.» El registro se conserva
        // igual cuando la evaluación es favorable: es lo que se muestra ante un siniestro.
        var licencia = Vigente(hasta: new DateOnly(2027, 1, 1));

        var resultado = ReglasDeHabilitacion.Evaluar(licencia, Pickup, Ventana, Matriz);

        Assert.True(resultado.Habilita);
        Assert.Equal("0801-1990-01234", resultado.NumeroDeLicencia);
        Assert.Equal(CategoriaDeLicencia.B, resultado.Categoria);
        Assert.Equal(new DateOnly(2027, 1, 1), resultado.VencimientoDeLicencia);
        Assert.Equal("PRUEBA-01", resultado.VersionDeMatriz);
        Assert.Equal(Pickup, resultado.AtributosDelVehiculo);
        Assert.Equal(Ventana.FinDelRango, resultado.FinDeRangoEvaluado);
    }

    private static readonly VentanaDeMision Ventana =
        new(Salida, new DateOnly(2026, 3, 14), HolguraDias: 1);

    private static Licencia Vigente(DateOnly hasta) =>
        new("0801-1990-01234", CategoriaDeLicencia.B, hasta, []);
}
