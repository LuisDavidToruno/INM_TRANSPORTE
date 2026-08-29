using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M09_Combustible;

/// <summary>
/// `RN-83` punto 5 — el libro de existencias del tanque institucional.
///
/// Lo que defiende es que <b>«salió del tanque de la sede» deje de ser una palabra</b>: hasta
/// hoy la fuente se podía declarar y no descontaba de ninguna parte, con lo cual el galón
/// seguía siendo tan invisible como antes de `RN-83` — sólo que con la apariencia de estar
/// registrado.
/// </summary>
public class TanqueInstitucionalPruebas
{
    private static readonly DateTimeOffset Ahora =
        new(2026, 3, 16, 7, 30, 0, TimeSpan.FromHours(-6));

    private static readonly Ulid Vehiculo = Ulid.NewUlid();
    private static readonly IdPersonaDelReceptor Motorista = new("P-MOTORISTA");

    private static Autoria Quien(string persona, string puesto = "PU-COMBUSTIBLE") =>
        Autoria.De(new IdPersona(persona), new IdPuesto(puesto), new DateOnly(2026, 3, 16));

    private static TanqueInstitucional Abierto(decimal inicial = 500m, string combustible = "Diesel") =>
        TanqueInstitucional.Abrir(
            Ulid.NewUlid(), "Cisterna de la sede", "Delegacion de Choluteca", combustible,
            capacidadGalones: 1_000m, Quien("P-ALMACEN"), inicial, Ahora);

    // ── La existencia es la suma del libro ──────────────────────────────────

    [Fact]
    public void El_tanque_abre_con_su_existencia_como_ASIENTO()
    {
        var t = Abierto(500m);

        Assert.Equal(500m, t.Existencia);
        Assert.Single(t.Libro);
        Assert.Equal("E-01", t.Libro[0].Id);

        // La apertura es el saldo de apertura de `RN-97`, no una columna que alguien pueda
        // editar: es lo que después permite explicar de dónde salió el primer galón.
        Assert.Contains("Saldo de apertura", t.Libro[0].Motivo);
    }

    [Fact]
    public void Cada_movimiento_mueve_la_existencia_y_ninguno_es_una_columna()
    {
        var t = Abierto(500m);

        t.Despachar(Quien("P-ALMACEN"), 60m, Vehiculo, null, null, "Diesel", Motorista, Ahora);
        t.Recibir(Quien("P-ALMACEN"), 200m, "F-2026-0091", Ahora);
        t.Despachar(Quien("P-ALMACEN"), 40m, Vehiculo, null, null, "Diesel", Motorista, Ahora);

        Assert.Equal(600m, t.Existencia);
        Assert.Equal(4, t.Libro.Count);
    }

    // ── El despacho ─────────────────────────────────────────────────────────

    [Fact]
    public void No_se_despacha_lo_que_no_hay_y_el_mensaje_dice_cuanto_falta()
    {
        var t = Abierto(50m);

        var error = Assert.Throws<BloqueoDuro>(() =>
            t.Despachar(Quien("P-ALMACEN"), 80m, Vehiculo, null, null, "Diesel", Motorista, Ahora));

        Assert.Contains("50.00 galones en libros y se piden 80.00", error.Message);
        Assert.Contains("Faltan 30.00", error.Message);

        // Y dice cuál es la salida real: casi siempre el combustible sí está y lo que falta es
        // el ingreso que nadie asentó.
        Assert.Contains("el ingreso que nadie", error.Message);
    }

    [Fact]
    public void Nadie_se_despacha_combustible_a_si_mismo()
    {
        // El control más elemental de una bomba, y el más fácil de perder: el motorista que se
        // sirve solo y anota lo que quiere no deja ninguna traza distinta.
        var t = Abierto();

        var error = Assert.Throws<BloqueoDuro>(() => t.Despachar(
            Quien("P-MOTORISTA"), 40m, Vehiculo, null, null, "Diesel", Motorista, Ahora));

        Assert.Equal("RN-01", error.Precondicion);
        Assert.Contains("no puede despacharse combustible a sí mismo", error.Message);
    }

    [Fact]
    public void Un_tanque_de_diesel_no_llena_un_vehiculo_de_gasolina()
    {
        var t = Abierto(combustible: "Diesel");

        var error = Assert.Throws<BloqueoDuro>(() => t.Despachar(
            Quien("P-ALMACEN"), 40m, Vehiculo, null, null, "Gasolina", Motorista, Ahora));

        // Cuadraría en galones y sería imposible en la realidad — y el tanque del que salieron
        // de verdad quedaría con un faltante que nadie va a poder explicar.
        Assert.Contains("imposible en la realidad", error.Message);
    }

    [Fact]
    public void Sin_saber_el_combustible_del_vehiculo_el_despacho_NO_se_bloquea()
    {
        // `M-03` todavía no declara el combustible del vehículo. Bloquear contra un dato que no
        // existe pararía todos los despachos; vacío es «no se sabe», no «incompatible».
        var t = Abierto();

        t.Despachar(Quien("P-ALMACEN"), 40m, Vehiculo, null, null, "", Motorista, Ahora);

        Assert.Equal(460m, t.Existencia);
    }

    [Fact]
    public void El_despacho_imputa_el_galon_a_una_placa()
    {
        var t = Abierto();
        var abastecimiento = Ulid.NewUlid();
        var mision = Ulid.NewUlid();

        var m = t.Despachar(
            Quien("P-ALMACEN"), 40m, Vehiculo, mision, abastecimiento, "Diesel", Motorista, Ahora);

        // Sin esto el egreso dice cuánto salió pero no adónde fue, que es exactamente el
        // problema que este libro existe para resolver.
        Assert.Equal(Vehiculo, m.Vehiculo);
        Assert.Equal(mision, m.Mision);
        Assert.Equal(abastecimiento, m.Abastecimiento);
        Assert.Contains("Recibe P-MOTORISTA", m.Motivo);
    }

    // ── El ingreso ──────────────────────────────────────────────────────────

    [Fact]
    public void El_ingreso_al_tanque_exige_comprobante()
    {
        var t = Abierto();

        var error = Assert.Throws<BloqueoDuro>(() =>
            t.Recibir(Quien("P-ALMACEN"), 200m, "   ", Ahora));

        // Sin comprobante, el tanque es una fuente de combustible sin origen: cualquier
        // faltante se tapa asentando un ingreso que nadie compró.
        Assert.Contains("sin origen", error.Message);
    }

    // ── El trasiego ─────────────────────────────────────────────────────────

    [Fact]
    public void El_trasiego_mueve_los_dos_tanques_y_conserva_el_total()
    {
        var origen = Abierto(500m);
        var destino = Abierto(100m);

        origen.Trasegar(Quien("P-GERENCIA"), 120m, destino, sale: true, Ahora);
        destino.Trasegar(Quien("P-GERENCIA"), 120m, origen, sale: false, Ahora);

        Assert.Equal(380m, origen.Existencia);
        Assert.Equal(220m, destino.Existencia);

        // El total del sistema no cambió. Registrar sólo la salida haría que el combustible se
        // evaporara del sistema entero — la forma exacta en que un faltante se disfraza de
        // traslado.
        Assert.Equal(600m, origen.Existencia + destino.Existencia);
    }

    [Fact]
    public void No_se_trasiega_diesel_a_un_tanque_de_gasolina()
    {
        var diesel = Abierto(500m, "Diesel");
        var gasolina = Abierto(100m, "Gasolina");

        var error = Assert.Throws<BloqueoDuro>(() =>
            diesel.Trasegar(Quien("P-GERENCIA"), 50m, gasolina, sale: true, Ahora));

        // Si de verdad ocurrió, es una contaminación del combustible: un incidente de M-12, no
        // un movimiento de existencias.
        Assert.Contains("contaminación", error.Message);
    }

    // ── El arqueo ───────────────────────────────────────────────────────────

    [Fact]
    public void La_constatacion_MIDE_y_no_ajusta()
    {
        // Es la misma disciplina que `RN-86` punto 4 impone al plazo vencido: nunca cuadre
        // automático. Un arqueo que corrige el libro hace desaparecer la diferencia en el mismo
        // acto que la descubre.
        var t = Abierto(500m);

        t.Constatar(Quien("P-COMISION"), 470m, "Acta AR-2026-0003, medición con varilla.", Ahora);

        Assert.Equal(500m, t.Existencia);
        Assert.Equal(30m, t.DiferenciaDelUltimoArqueo);
        Assert.Contains("FALTAN 30.00 galones", t.Libro[^1].Motivo);
    }

    [Fact]
    public void La_constatacion_nombra_la_diferencia_aunque_sea_CERO()
    {
        // Callarla cuando cuadra y decirla cuando no, entrena a leer su ausencia como «no se
        // midió».
        var t = Abierto(500m);
        t.Constatar(Quien("P-COMISION"), 500m, "Acta AR-2026-0004.", Ahora);

        Assert.Equal(0m, t.DiferenciaDelUltimoArqueo);
        Assert.Contains("Cuadra exacto", t.Libro[^1].Motivo);
    }

    [Fact]
    public void Un_tanque_sin_arqueo_no_esta_cuadrado_esta_SIN_VERIFICAR()
    {
        var t = Abierto(500m);

        // Nulo, no cero. La diferencia importa: de un tanque nunca medido no se deduce que
        // cuadre.
        Assert.Null(t.DiferenciaDelUltimoArqueo);
        Assert.Null(t.UltimaConstatacion);
    }

    [Fact]
    public void La_diferencia_se_juzga_contra_la_existencia_DE_ESE_MOMENTO()
    {
        var t = Abierto(500m);
        t.Constatar(Quien("P-COMISION"), 470m, "Acta.", Ahora);

        // Lo despachado después del arqueo no es parte de lo que el arqueo encontró.
        t.Despachar(Quien("P-ALMACEN"), 100m, Vehiculo, null, null, "Diesel", Motorista, Ahora);

        Assert.Equal(400m, t.Existencia);
        Assert.Equal(30m, t.DiferenciaDelUltimoArqueo);
    }

    [Fact]
    public void El_ajuste_SI_mueve_el_libro_y_exige_motivo_tipificado()
    {
        var t = Abierto(500m);
        t.Constatar(Quien("P-COMISION"), 470m, "Acta.", Ahora);

        t.Ajustar(Quien("P-GERENCIA"), -30m, MotivoDeAjuste.MermaTecnica,
            "Evaporación del período, medición del 16/03.", Ahora);

        Assert.Equal(470m, t.Existencia);
        Assert.Contains("MermaTecnica", t.Libro[^1].Motivo);
    }

    [Fact]
    public void El_ajuste_sin_fundamento_se_rechaza()
    {
        var t = Abierto(500m);

        var error = Assert.Throws<BloqueoDuro>(() =>
            t.Ajustar(Quien("P-GERENCIA"), -30m, MotivoDeAjuste.MermaTecnica, "  ", Ahora));

        // Sin fundamento, la salida más cómoda de todo faltante sería ajustar hasta que cuadre.
        Assert.Contains("hasta que cuadre", error.Message);
    }

    [Fact]
    public void Ningun_ajuste_puede_dejar_el_tanque_en_negativo()
    {
        var t = Abierto(50m);

        var error = Assert.Throws<BloqueoDuro>(() =>
            t.Ajustar(Quien("P-GERENCIA"), -80m, MotivoDeAjuste.ErrorDeRegistro, "Corrección.", Ahora));

        Assert.Contains("no describe ningún tanque", error.Message);
    }

    [Fact]
    public void El_arqueo_que_encuentra_de_MAS_tambien_se_nombra()
    {
        // Sobrar no es inocuo: significa que hubo un ingreso que nadie asentó, y ese
        // combustible entró de algún lado.
        var t = Abierto(500m);
        t.Constatar(Quien("P-COMISION"), 540m, "Acta.", Ahora);

        Assert.Equal(-40m, t.DiferenciaDelUltimoArqueo);
        Assert.Contains("SOBRAN 40.00 galones", t.Libro[^1].Motivo);
    }
}
