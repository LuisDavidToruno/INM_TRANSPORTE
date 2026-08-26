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
        Clase: ClaseNormativa.Automovil,
        PesoBrutoKg: 2_800,
        CapacidadPasajeros: 5,
        LlevaRemolque: false);

    /// <summary>Matriz de prueba: la categoría B habilita hasta 3.500 kg y 8 pasajeros.</summary>
    private static readonly MatrizDeLicencias Matriz = MatrizDeLicencias.Con("PRUEBA-01",
    [
        EntradaVigente(CategoriaDeLicencia.B, ClaseNormativa.Automovil, hasta: 3_500,
            desde: new DateOnly(2026, 1, 1), hastaFecha: null)
    ]);

    [Fact]
    public void La_licencia_A_habilita_una_motocicleta_y_la_B_no()
    {
        // El Artículo 4 define A y B1 POR CLASE DE VEHÍCULO, no por umbral: con masa,
        // pasajeros y remolque no se distingue una moto de un automóvil liviano.
        //
        // Y que la B no habilite una moto es texto de la norma, no inferencia: «Tipo B:
        // todo tipo automóviles livianos NO COMPRENDIDOS EN LA CATEGORÍA A Y B1».
        var matriz = MatrizDeLicencias.Con("ACUERDO-1012-2021",
        [
            EntradaVigente(CategoriaDeLicencia.A, ClaseNormativa.Motocicleta, hasta: 1_000,
                desde: new DateOnly(2026, 1, 1), hastaFecha: null),
            EntradaVigente(CategoriaDeLicencia.B, ClaseNormativa.Automovil, hasta: 3_500,
                desde: new DateOnly(2026, 1, 1), hastaFecha: null)
        ]);

        var moto = new FichaTecnica("MOTOCICLETA", ClaseNormativa.Motocicleta,
            PesoBrutoKg: 180, CapacidadPasajeros: 1, LlevaRemolque: false);

        var licenciaA = Vigente(hasta: new DateOnly(2028, 1, 1)) with { Categoria = CategoriaDeLicencia.A };
        var licenciaB = Vigente(hasta: new DateOnly(2028, 1, 1));

        Assert.True(ReglasDeHabilitacion.Evaluar(licenciaA, moto, Ventana, matriz, Conocido).Habilita);
        Assert.False(ReglasDeHabilitacion.Evaluar(licenciaB, moto, Ventana, matriz, Conocido).Habilita);

        // Y a la inversa: la A tampoco habilita un automóvil, aunque pese poco.
        var liviano = Pickup with { PesoBrutoKg = 900 };
        Assert.False(ReglasDeHabilitacion.Evaluar(licenciaA, liviano, Ventana, matriz, Conocido).Habilita);
    }

    [Fact]
    public void Un_pickup_con_remolque_exige_BE_y_no_le_basta_la_B()
    {
        // Artículo 4 del Acuerdo 1012-2021: `BE` es «automóviles de la categoría B
        // enganchados a un remolque». Un pick-up de 2,800 kg con plataforma NO es
        // articulado en ningún sentido, y aun así no lo habilita la `B`.
        //
        // Este es el caso que el enumerado de ocho categorías dejaba pasar.
        var matriz = MatrizDeLicencias.Con("ACUERDO-1012-2021",
        [
            EntradaVigente(CategoriaDeLicencia.B, ClaseNormativa.Automovil, hasta: 3_500,
                desde: new DateOnly(2026, 1, 1), hastaFecha: null),
            EntradaVigente(CategoriaDeLicencia.BE, ClaseNormativa.Automovil, hasta: 3_500,
                desde: new DateOnly(2026, 1, 1), hastaFecha: null, remolque: true)
        ]);

        var conPlataforma = Pickup with { LlevaRemolque = true };
        var licenciaB = Vigente(hasta: new DateOnly(2028, 1, 1));
        var licenciaBE = licenciaB with { Categoria = CategoriaDeLicencia.BE };

        var conB = ReglasDeHabilitacion.Evaluar(licenciaB, conPlataforma, Ventana, matriz, Conocido);
        Assert.False(conB.Habilita);
        Assert.Equal(MotivoDeNoHabilitacion.CategoriaNoHabilitaElVehiculo, conB.Motivo);

        Assert.True(ReglasDeHabilitacion.Evaluar(licenciaBE, conPlataforma, Ventana, matriz, Conocido).Habilita);

        // Y sin remolque, la B sigue bastando: BE no reemplaza a B, la complementa.
        Assert.True(ReglasDeHabilitacion.Evaluar(licenciaB, Pickup, Ventana, matriz, Conocido).Habilita);
    }

    [Fact]
    public void La_matriz_se_resuelve_a_la_fecha_de_salida_prevista()
    {
        // BD-02: «la matriz licencia↔vehículo vigente a la fecha de salida prevista».
        // Si el reglamento sube el límite de la categoría B en julio, una misión de marzo
        // se sigue evaluando con el límite de marzo — aunque se capture en agosto.
        var matriz = MatrizDeLicencias.Con("PRUEBA-02",
        [
            EntradaVigente(CategoriaDeLicencia.B, ClaseNormativa.Automovil, hasta: 3_500,
                desde: new DateOnly(2026, 1, 1), hastaFecha: new DateOnly(2026, 6, 30)),
            EntradaVigente(CategoriaDeLicencia.B, ClaseNormativa.Automovil, hasta: 4_000,
                desde: new DateOnly(2026, 7, 1), hastaFecha: null)
        ]);

        var licencia = Vigente(hasta: new DateOnly(2028, 1, 1));
        var camioneta = Pickup with { PesoBrutoKg = 3_800 };

        var enMarzo = new VentanaDeMision(new DateOnly(2026, 3, 12), new DateOnly(2026, 3, 14), 1);
        var enAgosto = new VentanaDeMision(new DateOnly(2026, 8, 12), new DateOnly(2026, 8, 14), 1);

        Assert.False(ReglasDeHabilitacion.Evaluar(licencia, camioneta, enMarzo, matriz, Conocido).Habilita);
        Assert.True(ReglasDeHabilitacion.Evaluar(licencia, camioneta, enAgosto, matriz, Conocido).Habilita);
    }

    private static readonly DateTimeOffset Conocido =
        new(2026, 9, 1, 0, 0, 0, TimeSpan.FromHours(-6));

    private static EntradaDeMatriz EntradaVigente(
        CategoriaDeLicencia categoria, ClaseNormativa clase, int hasta, DateOnly desde,
        DateOnly? hastaFecha, bool remolque = false) =>
        new(categoria, clase, PesoBrutoMaximoKg: hasta, CapacidadMaximaPasajeros: 8, PermiteRemolque: remolque,
            VigenteDesde: desde, VigenteHasta: hastaFecha,
            RegistradoDesde: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.FromHours(-6)),
            RegistradoHasta: null);

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

        var resultado = ReglasDeHabilitacion.Evaluar(licencia, Pickup, ventana, Matriz, Conocido);

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

        var resultado = ReglasDeHabilitacion.Evaluar(licencia, Pickup, ventana, Matriz, Conocido);

        Assert.False(resultado.Habilita);
        Assert.Equal(new DateOnly(2026, 3, 15), resultado.FinDeRangoEvaluado);
    }

    [Fact]
    public void Una_categoria_sin_entrada_en_la_matriz_no_habilita()
    {
        // La ausencia se trata como negativa, nunca como permiso: si nadie declaró que la
        // categoría A puede conducir un pickup, no puede.
        var licencia = Vigente(hasta: new DateOnly(2027, 1, 1)) with { Categoria = CategoriaDeLicencia.A };

        var resultado = ReglasDeHabilitacion.Evaluar(licencia, Pickup, Ventana, Matriz, Conocido);

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

        Assert.True(ReglasDeHabilitacion.Evaluar(licencia, Pickup, Ventana, Matriz, Conocido).Habilita);
        Assert.False(ReglasDeHabilitacion.Evaluar(licencia, pesado, Ventana, Matriz, Conocido).Habilita);
    }

    [Fact]
    public void El_resultado_conserva_la_evidencia_aunque_habilite()
    {
        // «Guardar solo "verificado" no defiende a nadie.» El registro se conserva
        // igual cuando la evaluación es favorable: es lo que se muestra ante un siniestro.
        var licencia = Vigente(hasta: new DateOnly(2027, 1, 1));

        var resultado = ReglasDeHabilitacion.Evaluar(licencia, Pickup, Ventana, Matriz, Conocido);

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
