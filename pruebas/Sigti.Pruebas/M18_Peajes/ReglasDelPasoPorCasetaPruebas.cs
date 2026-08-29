using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M18_Peajes;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M18_Peajes;

/// <summary>
/// `RN-36` — el cobro de la caseta es un hecho; la clasificación es una derivación.
///
/// ── Lo que pasa si se confunden ──────────────────────────────────────────────
/// `RN-36`, textual: <i>«si el sistema ajustara la categoría del vehículo al cobro recibido, el
/// error de la caseta se volvería la verdad institucional y <b>el reclamo nunca ocurriría</b>»</i>.
///
/// Y no es hipotético: entre agosto y septiembre de 2025 COVI-H cobró <b>L 90 en lugar de
/// L 22</b> a Hyundai H-100, Kia K2700 y Sprinter — cuatro veces de más. La SAPP lo resolvió el
/// 17/09/2025 `[V]`.
/// </summary>
public class ReglasDelPasoPorCasetaPruebas
{
    private static readonly CategoriaDePeaje Liviano = new("LIVIANO", "Liviano/Turismo");
    private static readonly CategoriaDePeaje DosEjes = new("EJES-2", "Vehículo de 2 Ejes");

    private static readonly DateTimeOffset Ahora =
        new(2026, 4, 10, 9, 30, 0, TimeSpan.FromHours(-6));

    private static PasoPorCaseta Paso(
        decimal pagado = 22m,
        CategoriaDePeaje? esperada = null,
        CategoriaDePeaje? cobrada = null,
        decimal? montoEsperado = 22m,
        MedioDePagoDelPeaje medio = MedioDePagoDelPeaje.Efectivo) =>
        new(Ulid.NewUlid(), Ulid.NewUlid(), Ulid.NewUlid(), null, Ahora,
            Odometro: 84_120, MontoPagado: pagado, Medio: medio,
            Registra: new IdPersona("P-MOTORISTA"),
            CategoriaEsperada: esperada ?? Liviano,
            CategoriaCobrada: cobrada,
            MontoEsperado: montoEsperado);

    // ── La discrepancia ─────────────────────────────────────────────────────

    [Fact]
    public void Cobrar_con_otra_categoria_es_discrepancia_y_el_paso_conserva_LAS_DOS()
    {
        // El caso exacto de la SAPP: liviano cobrado como vehículo de 2 ejes, L 90 en lugar de
        // L 22.
        var p = Paso(pagado: 90m, esperada: Liviano, cobrada: DosEjes);

        Assert.True(p.HayDiscrepanciaDeClasificacion);
        Assert.Equal(68m, p.Diferencia);

        // La categoría del vehículo NO cambia. Es lo que hace posible el reclamo.
        Assert.Equal("LIVIANO", p.CategoriaEsperada!.Codigo);
        Assert.Equal("EJES-2", p.CategoriaCobrada!.Codigo);
    }

    [Fact]
    public void Cobrar_con_la_MISMA_categoria_no_es_discrepancia()
    {
        Assert.False(Paso(esperada: Liviano, cobrada: Liviano).HayDiscrepanciaDeClasificacion);

        // Y da igual la caja de las letras: la carga la hace una persona.
        Assert.False(Paso(
            esperada: Liviano,
            cobrada: new CategoriaDePeaje("liviano", "Liviano")).HayDiscrepanciaDeClasificacion);
    }

    [Fact]
    public void Sin_categoria_esperada_no_hay_discrepancia_que_declarar()
    {
        // No hay contra qué comparar. Declararla igual produciría un reclamo sin fundamento de
        // clasificación, que es justamente lo que el expediente ante la SAPP necesita.
        var p = new PasoPorCaseta(
            Ulid.NewUlid(), Ulid.NewUlid(), Ulid.NewUlid(), null, Ahora, 84_120, 90m,
            MedioDePagoDelPeaje.Efectivo, new IdPersona("P-MOTORISTA"),
            CategoriaEsperada: null, CategoriaCobrada: DosEjes);

        Assert.False(p.HayDiscrepanciaDeClasificacion);
    }

    [Fact]
    public void Sin_categoria_cobrada_solo_se_sabe_el_monto()
    {
        // El ticket a veces no dice la categoría. La diferencia de monto se juzga aparte: pudo
        // ser un cambio de tarifa entre la aprobación y el viaje, que `RN-34` tipifica como
        // causa legítima.
        var p = Paso(pagado: 26m, esperada: Liviano, cobrada: null, montoEsperado: 22m);

        Assert.False(p.HayDiscrepanciaDeClasificacion);
        Assert.Equal(4m, p.Diferencia);
    }

    [Fact]
    public void Sin_monto_esperado_la_diferencia_es_NULA_y_no_cero()
    {
        // Cero diría que pagó exactamente lo previsto. Nulo dice que no había previsión.
        Assert.Null(Paso(montoEsperado: null).Diferencia);
    }

    // ── Pagar estando exonerado ─────────────────────────────────────────────

    [Fact]
    public void Pagar_donde_estaba_exonerado_habilita_reclamo()
    {
        // `RN-38` punto 3: pudo ser cobro indebido. La caseta que no reconoce la exoneración en
        // el momento existe, y el motorista paga para no detener la misión — la contradicción
        // entre el pago y la exoneración vigente es la base del reclamo.
        Assert.True(Paso(pagado: 22m).PagoEstandoExonerado(estabaExonerado: true));
        Assert.False(Paso(pagado: 22m).PagoEstandoExonerado(estabaExonerado: false));
    }

    [Fact]
    public void Pasar_con_LIBRE_PASO_no_es_cobro_indebido()
    {
        var p = Paso(pagado: 0m, medio: MedioDePagoDelPeaje.LibrePaso);

        Assert.False(p.PagoEstandoExonerado(estabaExonerado: true));
    }

    // ── Lo que todo paso exige ──────────────────────────────────────────────

    [Fact]
    public void El_paso_exige_el_odometro_del_momento()
    {
        // Es lo que permite el cruce de `RN-37`: un vehículo que declara 980 km pero sólo cruzó
        // una caseta dos veces está diciendo dos cosas incompatibles.
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelPasoPorCaseta.ExigirDatosDelHecho(odometro: 0, montoPagado: 22m));

        Assert.Contains("no queda anclado al recorrido", error.Message);
    }

    [Fact]
    public void Un_paso_en_CERO_se_admite_porque_el_libre_paso_existe()
    {
        ReglasDelPasoPorCaseta.ExigirDatosDelHecho(odometro: 84_120, montoPagado: 0m);

        Assert.Throws<BloqueoDuro>(() =>
            ReglasDelPasoPorCaseta.ExigirDatosDelHecho(84_120, montoPagado: -1m));
    }

    [Fact]
    public void La_discrepancia_sin_ticket_exige_CAUSA_pero_no_rechaza_el_paso()
    {
        // La caseta a veces no da ticket. El registro de un hecho no se omite por falta de
        // papel — lo que se exige es que la ausencia se declare, porque decide si el reclamo
        // procede.
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelPasoPorCaseta.ExigirEvidenciaDeLaDiscrepancia(
                hayDiscrepancia: true, ticket: null, causaSinTicket: null));

        Assert.Contains("la palabra del motorista", error.Message);

        // Con causa declarada, pasa.
        ReglasDelPasoPorCaseta.ExigirEvidenciaDeLaDiscrepancia(
            true, null, "La caseta de Zambrano no entregó ticket, aparato sin papel.");

        // Y con ticket, ni se pregunta.
        ReglasDelPasoPorCaseta.ExigirEvidenciaDeLaDiscrepancia(true, "foto-ticket-0091.jpg", null);
    }

    [Fact]
    public void Sin_discrepancia_no_se_exige_ticket()
    {
        ReglasDelPasoPorCaseta.ExigirEvidenciaDeLaDiscrepancia(false, null, null);
    }

    // ── El punto que no está en el catálogo ─────────────────────────────────

    [Fact]
    public void Un_paso_por_un_punto_no_catalogado_NO_se_descarta_pero_exige_ubicacion()
    {
        // `NRM-10` menciona casetas antiguas en San Pedro Sula sin verificar si operan `[C]`.
        // Descartar el paso perdería el gasto y la evidencia de que la caseta existe.
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelPasoPorCaseta.ExigirUbicacionSiNoEstaCatalogado(true, ubicacion: "  "));

        Assert.Contains("depurar el catálogo", error.Message);

        ReglasDelPasoPorCaseta.ExigirUbicacionSiNoEstaCatalogado(
            true, "Salida norte de San Pedro Sula, antes del desvío a Choloma.");

        ReglasDelPasoPorCaseta.ExigirUbicacionSiNoEstaCatalogado(false, null);
    }
}
