using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sigti.Datos;
using Sigti.Datos.M07_ProgramacionYDespacho;
using Sigti.Dominio.M02_Parametros;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;
using Sigti.Pruebas.Datos;

namespace Sigti.Pruebas.PuntaAPunta;

/// <summary>
/// `RN-96` cableada — el cierre de ejercicio como corte de imputación y de reporte.
///
/// ── Cada ejercicio de prueba es propio ──────────────────────────────────────
/// El acta es <b>única por ejercicio</b> y el indicador de apuro cuenta <b>todos</b> los
/// cierres del año. Las pruebas comparten la base, así que cada una toma un año que ninguna
/// otra toca: mezclarlas haría fallar la que corra segunda por razones que no son la regla.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class CierreDeEjercicioPruebas(BaseDePruebas baseDePruebas)
{
    private WebApplicationFactory<Program> Aplicacion() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(constructor =>
            constructor.ConfigureServices(servicios =>
            {
                servicios.RemoveAll(typeof(DbContextOptions<SigtiDbContext>));
                servicios.AddDbContext<SigtiDbContext>(opciones =>
                    opciones.UseSqlServer(
                        baseDePruebas.CadenaDeConexion,
                        sql => sql.UseCompatibilityLevel(120)));
            }));

    // ── Lo que el cierre NO hace, y es su razón de ser ──────────────────────

    /// <summary>
    /// `RN-96`: <i>«no ejecuta ni habilita ninguna transición de la Orden de Misión. Ningún
    /// expediente cambia de estado por efecto de una fecha»</i>.
    ///
    /// Es la prueba que sostiene toda la regla. Si el acta moviera un solo expediente, todo lo
    /// demás —el inventario, el desglose, el indicador de apuro— sería el maquillaje de un
    /// cierre masivo por fecha.
    /// </summary>
    [Fact]
    public async Task Producir_el_acta_NO_mueve_ningun_expediente()
    {
        const int anio = 2031;
        var mision = await SembrarMisionAsync(anio, EstadoDeMision.EnRuta, "En ruta al corte");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var antes = await AsientosDeAsync(mision);

        await Post(cliente, "/cierre-de-ejercicio", Cuerpo($"AC-{anio}-001", anio));

        var despues = await AsientosDeAsync(mision);

        Assert.Equal(antes.Count, despues.Count);
        Assert.Equal(antes[^1], despues[^1]);
    }

    /// <summary>
    /// El acta se produce <b>una vez por ejercicio</b>. Una segunda dejaría dos documentos del
    /// mismo cierre y ni el saldo de apertura ni el acta de anulación podrían decir cuál citan.
    /// </summary>
    [Fact]
    public async Task Un_segundo_acta_del_mismo_ejercicio_no_pasa()
    {
        const int anio = 2032;
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await Post(cliente, "/cierre-de-ejercicio", Cuerpo($"AC-{anio}-001", anio));

        var segunda = await cliente.PostAsJsonAsync(
            "/cierre-de-ejercicio", Cuerpo($"AC-{anio}-002", anio));

        Assert.False(segunda.IsSuccessStatusCode);
        Assert.Contains("Ya hay un acta de cierre", await segunda.Content.ReadAsStringAsync());
    }

    // ── Nunca un motivo compartido por varios expedientes ───────────────────

    /// <summary>
    /// `RN-96` punto 3, y la frase que explica por qué: <i>«ante el Tribunal Superior de
    /// Cuentas, cincuenta expedientes cerrados el 31 de diciembre a la misma hora con el mismo
    /// motivo <b>son el hallazgo</b>, no su solución»</i>.
    /// </summary>
    [Fact]
    public async Task Dos_misiones_cerradas_con_el_mismo_motivo_salen_en_el_acta()
    {
        const int anio = 2033;
        const string motivo = "Cierre de ejercicio fiscal, sin observaciones";

        await SembrarMisionAsync(anio, EstadoDeMision.Cerrada, motivo,
            cierre: new DateTime(anio, 12, 30, 16, 40, 0));

        await SembrarMisionAsync(anio, EstadoDeMision.Cerrada, motivo,
            cierre: new DateTime(anio, 12, 30, 16, 41, 0));

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var acta = await VistaPrevia(cliente, anio);

        var compartido = Assert.Single(acta.GetProperty("motivosCompartidos").EnumerateArray(), m => m.GetProperty("motivo").GetString() == motivo);

        Assert.Equal(2, compartido.GetProperty("misiones").GetArrayLength());

        // **Un minuto entre los dos.** Es lo que separa el cierre en bloque de un motivo que se
        // repite a lo largo del año por una causa real.
        Assert.Equal(1, compartido.GetProperty("ventanaEnMinutos").GetInt32());

        Assert.Contains(acta.GetProperty("observaciones").EnumerateArray(),
            o => o.GetString()!.Contains("evaluación individual"));
    }

    /// <summary>
    /// Dos misiones cerradas en la ventana con <b>evaluación propia</b> no producen hallazgo.
    /// Si lo produjeran, la observación aparecería siempre y dejaría de significar algo.
    /// </summary>
    [Fact]
    public async Task Dos_cierres_evaluados_uno_por_uno_no_producen_hallazgo()
    {
        const int anio = 2034;

        await SembrarMisionAsync(anio, EstadoDeMision.Cerrada,
            "Bitácora conciliada: 412 km, 11.4 gal, desviación 1.8% dentro de tolerancia",
            cierre: new DateTime(anio, 12, 29, 10, 0, 0));

        await SembrarMisionAsync(anio, EstadoDeMision.Cerrada,
            "Retorno con 38 km menos por desvío declarado en La Barca, autorizado por ACT-05",
            cierre: new DateTime(anio, 12, 29, 11, 0, 0));

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var acta = await VistaPrevia(cliente, anio);

        Assert.Empty(acta.GetProperty("motivosCompartidos").EnumerateArray());
    }

    // ── El folio reservado y no consumido ───────────────────────────────────

    /// <summary>
    /// `RN-96` punto 5, el circuito entero: el acta <b>lista</b>, y anular es un acto aparte
    /// que la cita, con autor y motivo. Un documento que anulara al producirse sería un cierre
    /// masivo por fecha un nivel más abajo.
    /// </summary>
    [Fact]
    public async Task El_folio_emitido_al_corte_se_lista_y_se_anula_citando_el_acta()
    {
        const int anio = 2027;
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var folio = await SembrarValeEmitidoAsync(cliente, anio);

        var vista = await VistaPrevia(cliente, anio);

        var listado = Assert.Single(vista.GetProperty("foliosPorAnular").EnumerateArray(), f => f.GetProperty("folio").GetString() == folio);

        Assert.True(listado.GetProperty("sePuedeAnular").GetBoolean());
        Assert.Equal("Emitida", listado.GetProperty("estado").GetString());

        await Post(cliente, "/cierre-de-ejercicio", Cuerpo($"AC-{anio}-001", anio));

        // ── Y recién ahora se anula ─────────────────────────────────────────
        var respuesta = await cliente.PostAsJsonAsync(
            $"/cierre-de-ejercicio/{anio}/anular-folios",
            new
            {
                Persona = "P-ADMIN",
                Motivo = "Folio no consumido al cierre; el compromiso no se arrastra a " + (anio + 1),
                Momento = new DateTimeOffset(anio + 1, 1, 10, 9, 0, 0, TimeSpan.FromHours(-6)),
            });

        Assert.True(respuesta.IsSuccessStatusCode);

        var cuerpo = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync()).RootElement;
        Assert.True(cuerpo.GetProperty("anulados").GetInt32() >= 1);

        // El asiento `V-03` quedó en el diario del vale, citando el acta.
        await using var contexto = baseDePruebas.Contexto();

        var vale = await contexto.AsignacionesDeCombustible
            .Include(a => a.Transiciones)
            .SingleAsync(a => a.Folio == folio);

        var ultima = vale.Transiciones.OrderBy(t => t.Orden).Last();

        Assert.Equal("V-03", ultima.Transicion);
        Assert.Contains($"AC-{anio}-001", ultima.Motivo);
    }

    /// <summary>
    /// Sin acta no se anulan folios. Los folios se anulan <b>citando el acta que los listó</b>:
    /// sin ella no consta que fueran los que quedaron reservados y sin consumir al corte.
    /// </summary>
    [Fact]
    public async Task Anular_folios_sin_acta_no_pasa()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync(
            "/cierre-de-ejercicio/2039/anular-folios",
            new { Persona = "P-ADMIN", Motivo = "Cierre", Momento = DateTimeOffset.UtcNow });

        Assert.False(respuesta.IsSuccessStatusCode);
        Assert.Contains("No hay acta de cierre", await respuesta.Content.ReadAsStringAsync());
    }

    // ── Nadie aflojó un umbral en diciembre ─────────────────────────────────

    /// <summary>
    /// `RN-96` punto 6: <i>«es la evidencia de que <b>nadie aflojó un umbral en diciembre para
    /// cerrar limpio</b>, o de que alguien lo hizo y quedó a la vista»</i>.
    ///
    /// Se busca por el eje de <b>transacción</b> —cuándo se registró— y no por el de vigencia.
    /// Un umbral cargado el 28 de diciembre con vigencia retroactiva a enero es exactamente el
    /// caso que la regla quiere ver, y buscarlo por `VigenteDesde` lo dejaría fuera.
    /// </summary>
    [Fact]
    public async Task El_umbral_movido_en_la_ventana_queda_a_la_vista_con_su_valor_anterior()
    {
        const int anio = 2036;
        const string clave = "cierre.tolerancia-de-galonaje-2036";

        await using (var contexto = baseDePruebas.Contexto())
        {
            // El valor que regía desde enero, cargado en enero.
            contexto.Parametros.Add(Version(clave, "5",
                new DateOnly(anio, 1, 1),
                new DateTimeOffset(anio, 1, 5, 8, 0, 0, TimeSpan.Zero)));

            // Y el que alguien cargó el 28 de diciembre, con vigencia retroactiva a enero.
            contexto.Parametros.Add(Version(clave, "15",
                new DateOnly(anio, 1, 1),
                new DateTimeOffset(anio, 12, 28, 17, 40, 0, TimeSpan.Zero)));

            await contexto.SaveChangesAsync();
        }

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var acta = await VistaPrevia(cliente, anio);

        var cambio = Assert.Single(acta.GetProperty("cambiosDeParametros").EnumerateArray(), c => c.GetProperty("clave").GetString() == clave);

        // **Las dos mitades.** «Se cargó 15» sin decir que venía de 5 no es evidencia de nada.
        Assert.Equal("5", cambio.GetProperty("valorAnterior").GetString());
        Assert.Equal("15", cambio.GetProperty("valorNuevo").GetString());
        Assert.Equal("P-ADMIN", cambio.GetProperty("cargadoPor").GetString());
    }

    /// <summary>
    /// El parámetro cargado en marzo <b>no aparece</b> en el reporte de la ventana. Si
    /// apareciera, el reporte listaría el año entero y dejaría de señalar nada.
    /// </summary>
    [Fact]
    public async Task El_parametro_cargado_fuera_de_la_ventana_no_aparece()
    {
        const int anio = 2037;
        const string clave = "cierre.plazo-de-liquidacion-2037";

        await using (var contexto = baseDePruebas.Contexto())
        {
            contexto.Parametros.Add(Version(clave, "10",
                new DateOnly(anio, 3, 1),
                new DateTimeOffset(anio, 3, 1, 9, 0, 0, TimeSpan.Zero)));

            await contexto.SaveChangesAsync();
        }

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var acta = await VistaPrevia(cliente, anio);

        Assert.DoesNotContain(acta.GetProperty("cambiosDeParametros").EnumerateArray(),
            c => c.GetProperty("clave").GetString() == clave);
    }

    // ── El acta cuadra contra el saldo, renglón por renglón ─────────────────

    /// <summary>
    /// `RN-96` punto 2 — el inventario y su contraparte, el saldo de apertura, <b>deben
    /// coincidir renglón por renglón</b> (`RN-97`).
    ///
    /// Hasta que `RN-96` existió, esa comprobación no tenía contra qué correr.
    /// </summary>
    [Fact]
    public async Task El_acta_declara_el_saldo_que_cita_y_sus_diferencias()
    {
        const int anio = 2038;
        var corte = new DateOnly(anio, 12, 31);

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await Post(cliente, "/saldo-de-apertura", new
        {
            Id = Ulid.NewUlid().ToString(),
            Folio = $"SA-{anio}-001",
            Ejercicio = $"{anio}",
            Corte = corte,
            Persona = "P-AUDITORIA",
            Puesto = "PU-AUDITORIA",
            Momento = new DateTimeOffset(anio + 1, 1, 5, 9, 0, 0, TimeSpan.FromHours(-6)),
        });

        var vista = await VistaPrevia(cliente, anio);

        // Producido el mismo día contra el mismo corte, cuadra. Lo que importa es que la
        // comprobación **corra**: la diferencia aparece cuando alguien edita uno de los dos.
        Assert.Empty(vista.GetProperty("diferenciasConElSaldo").EnumerateArray());

        var respuesta = await cliente.PostAsJsonAsync(
            "/cierre-de-ejercicio", Cuerpo($"AC-{anio}-001", anio));

        Assert.True(respuesta.IsSuccessStatusCode);

        var actas = await Leer(cliente, "/cierre-de-ejercicio");

        var acta = Assert.Single(actas.EnumerateArray(), a => a.GetProperty("ejercicio").GetString() == $"{anio}");

        // **El acta dice qué saldo cita.** Sin el folio, el par de documentos que `RN-97` manda
        // conservar juntos queda sin la referencia que los une.
        Assert.Equal($"SA-{anio}-001", acta.GetProperty("saldoDeAperturaFolio").GetString());
    }

    // ── Fixtures ────────────────────────────────────────────────────────────

    private static VersionDeParametro Version(
        string clave, string valor, DateOnly vigenteDesde, DateTimeOffset registrado) =>
        new(clave, valor, vigenteDesde, null, registrado, null,
            new IdPersona("P-ADMIN"), new IdPersona("P-GERENCIA"))
        {
            Respaldo = new RespaldoDocumental(
                Ulid.NewUlid(), "Acuerdo interno de prueba", new DateOnly(2026, 1, 1)),
        };

    /// <summary>
    /// Un expediente con su diario, sembrado directo. Lo que estas pruebas juzgan es el acta,
    /// no el camino por el que la misión llegó a su estado.
    /// </summary>
    private async Task<Ulid> SembrarMisionAsync(
        int anio, EstadoDeMision estado, string motivo, DateTime? cierre = null)
    {
        await using var contexto = baseDePruebas.Contexto();

        var id = Ulid.NewUlid();

        var expediente = new FilaDeExpediente
        {
            Id = id,
            CapturadaPor = "P-SOLICITA",
            SolicitanteDeDerecho = "P-SOLICITA",
            Dependencia = "Dependencia de prueba",
            ObjetoDelTraslado = "Personal institucional",
            Destino = $"Destino de cierre {anio}",
            Salida = new DateOnly(anio, 12, 20),
            Retorno = new DateOnly(anio, 12, 22),
            HoraDeSalida = new TimeOnly(8, 0),
            HoraDeRetorno = new TimeOnly(17, 0),
            HolguraDias = 0,
        };

        expediente.Transiciones.Add(new FilaDeTransicion
        {
            Id = Ulid.NewUlid(),
            ExpedienteId = id,
            Orden = 1,
            Transicion = "T-01",
            Destino = EstadoDeMision.Solicitada,
            Ejecuta = "P-SOLICITA",
            MomentoUtc = new DateTime(anio, 12, 15, 9, 0, 0),
            DesfaseMinutos = -360,
        });

        expediente.Transiciones.Add(new FilaDeTransicion
        {
            Id = Ulid.NewUlid(),
            ExpedienteId = id,
            Orden = 2,
            Transicion = estado is EstadoDeMision.Cerrada ? "T-21" : "T-13",
            Destino = estado,
            Ejecuta = "P-CIERRA",
            MomentoUtc = cierre ?? new DateTime(anio, 12, 21, 9, 0, 0),
            DesfaseMinutos = -360,
            Motivo = motivo,
        });

        contexto.Expedientes.Add(expediente);
        await contexto.SaveChangesAsync();

        return id;
    }
    /// <summary>
    /// Un vale emitido y sin entregar antes del corte.
    ///
    /// El año lo elige quien llama, y tiene que caer <b>dentro de la vigencia de la licencia</b>
    /// del motorista sembrado: `BD-02` bloquea programar más allá de ella, y con razón. Salió al
    /// escribir esta prueba con un año de 2035.
    ///
    /// ── Va por la API entera, no por una fila fabricada a mano ──────────────
    /// `RN-32` no deja emitir contra una misión que no está despachada, y deriva el vehículo de
    /// la reserva. Un vale insertado a mano no probaría que el acta lista lo que el sistema
    /// realmente emite — probaría que lista lo que la prueba escribió.
    /// </summary>
    private async Task<string> SembrarValeEmitidoAsync(HttpClient cliente, int anio)
    {
        FlotaSembrada.ParaProgramar flota;

        await using (var contexto = baseDePruebas.Contexto())
            flota = await FlotaSembrada.ParaProgramarAsync(contexto, $"CE{anio % 100}");

        var momento = new DateTimeOffset(anio, 12, 10, 9, 0, 0, TimeSpan.FromHours(-6));
        var dependencia = $"Delegacion de cierre {anio}";

        var fondo = Ulid.NewUlid().ToString();

        await Post(cliente, "/fondos", new
        {
            Id = fondo,
            Ambito = "Dependencia",
            AmbitoDeclarado = dependencia,
            Desde = new DateOnly(anio, 12, 1),
            Hasta = new DateOnly(anio, 12, 31),
            Solicita = "P-TRANSPORTE",
            Monto = 50_000m,
            Justificacion = $"Operacion ordinaria de diciembre de {anio}.",
            Momento = momento,
        });

        await Post(cliente, $"/fondos/{fondo}/aprobar", new
        {
            Ejecuta = "P-GERENCIA",
            Monto = 50_000m,
            Partida = "12-01-001-4-31200",
            Momento = momento,
        });

        var mision = Ulid.NewUlid().ToString();

        await Post(cliente, "/misiones", new
        {
            Id = mision,
            CapturadaPor = "P-ASISTENTE",
            SolicitanteDeDerecho = "P-ASISTENTE",

            // Tiene que coincidir con el ambito del fondo: `RN-26` no deja imputar una mision
            // al fondo de otra delegacion.
            Dependencia = dependencia,
            ObjetoDelTraslado = "Traslado de personal",
            Destino = $"Destino de cierre {anio}",
            Salida = new DateOnly(anio, 12, 20),
            Retorno = new DateOnly(anio, 12, 22),
            HoraDeSalida = new TimeOnly(8, 0),
            HoraDeRetorno = new TimeOnly(16, 0),
            HolguraDias = 0,
            Momento = momento,
        });

        await Post(cliente, $"/misiones/{mision}/enviar",
            new { Ejecuta = "P-ASISTENTE", Momento = momento });

        await Post(cliente, $"/misiones/{mision}/aprobar",
            new { Ejecuta = "P-JEFATURA", Momento = momento });

        await Post(cliente, $"/misiones/{mision}/programar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento = momento,
            IdVehiculo = flota.Vehiculo,
            IdConductor = flota.Conductor,
        });

        await Post(cliente, $"/misiones/{mision}/despachar", new
        {
            Ejecuta = "P-DESPACHO",
            Momento = momento,
            IdVehiculo = flota.Vehiculo,
            IdConductor = flota.Conductor,
        });

        var folio = $"VC-{anio}-{Ulid.NewUlid().ToString()[^6..]}";

        // **Se emite y ahi se queda.** No se entrega: lo que `RN-96` manda anular es el folio
        // reservado y NO consumido, y entregarlo lo sacaria de esa lista.
        await Post(cliente, "/combustible", new
        {
            Id = Ulid.NewUlid().ToString(),
            Folio = folio,
            IdFondo = fondo,
            IdMision = mision,
            IdMotoristaReceptor = flota.Conductor,
            Ejecuta = "P-TRANSPORTE",
            Monto = 1_500m,
            Galones = 50m,
            Instrumento = "vale",
            TipoDeCombustible = "Diesel",
            Momento = momento,
        });

        return folio;
    }

    private async Task<List<string>> AsientosDeAsync(Ulid mision)
    {
        await using var contexto = baseDePruebas.Contexto();

        var expediente = await contexto.Expedientes
            .Include(e => e.Transiciones)
            .SingleAsync(e => e.Id == mision);

        return [.. expediente.Transiciones
            .OrderBy(t => t.Orden)
            .Select(t => $"{t.Orden}:{t.Transicion}:{t.Destino}")];
    }

    private static object Cuerpo(string folio, int anio) => new
    {
        Folio = folio,
        Ejercicio = $"{anio}",
        CorteLegal = new DateOnly(anio, 12, 31),
        CorteOperativo = new DateOnly(anio + 1, 1, 15),
        Persona = "P-ADMIN",
        Puesto = "PU-GERENCIA",
        Momento = new DateTimeOffset(anio + 1, 1, 16, 9, 0, 0, TimeSpan.FromHours(-6)),
    };

    private static Task<JsonElement> VistaPrevia(HttpClient cliente, int anio) =>
        Leer(cliente,
            $"/cierre-de-ejercicio/{anio}/vista-previa" +
            $"?corteLegal={anio}-12-31&corteOperativo={anio + 1}-01-15");

    private static async Task<JsonElement> Leer(HttpClient cliente, string ruta)
    {
        var respuesta = await cliente.GetAsync(ruta);

        if (!respuesta.IsSuccessStatusCode)
            throw new Xunit.Sdk.XunitException(
                $"GET {ruta} devolvió {(int)respuesta.StatusCode}: " +
                await respuesta.Content.ReadAsStringAsync());

        return JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync()).RootElement.Clone();
    }

    private static async Task Post(HttpClient cliente, string ruta, object cuerpo)
    {
        var respuesta = await cliente.PostAsJsonAsync(ruta, cuerpo);

        if (!respuesta.IsSuccessStatusCode)
            throw new Xunit.Sdk.XunitException(
                $"POST {ruta} devolvió {(int)respuesta.StatusCode}: " +
                await respuesta.Content.ReadAsStringAsync());
    }
}
