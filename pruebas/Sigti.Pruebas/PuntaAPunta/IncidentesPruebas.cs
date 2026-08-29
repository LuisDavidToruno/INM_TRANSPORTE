using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sigti.Datos;
using Sigti.Pruebas.Datos;

namespace Sigti.Pruebas.PuntaAPunta;

/// <summary>
/// M-12 cableado — el expediente de incidente.
///
/// ── Lo que este módulo desbloquea ───────────────────────────────────────────
/// `RN-97` enumera diez fuentes del saldo de apertura y cinco no se podían consultar. Dos de
/// ellas tienen <b>poder de bloqueo</b> sobre el cierre del período, y una —las interrupciones
/// sin desenlace— existe desde que M-12 se construyó. <b>Ese bloqueo ya puede disparar.</b>
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class IncidentesPruebas(BaseDePruebas baseDePruebas)
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

    // ── El registro de campo no captura culpa ───────────────────────────────

    /// <summary>
    /// `RN-74` — el contrato de registro <b>no tiene un campo de responsabilidad</b>, y por eso
    /// esta prueba no puede escribirlo. Lo que verifica es que el expediente que sale del hecho
    /// tampoco lo trae.
    ///
    /// <i>«Si registrar el hecho implica autoinculparse, <b>el hecho no se registra</b>. Y un
    /// accidente no registrado es peor que cualquier atribución mal hecha»</i>.
    /// </summary>
    [Fact]
    public async Task El_incidente_registrado_en_campo_no_trae_responsabilidad()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var id = await RegistrarAsync(cliente, tipo: "Accidente", interrumpe: true);

        var expediente = await Leer(cliente, $"/incidentes/{id}");

        // La determinación va nula: nadie la emitió todavía, y SIGTI no la produce.
        Assert.True(expediente.GetProperty("determinacion").ValueKind is JsonValueKind.Null);

        // Lo que sí trae es el hecho.
        Assert.Equal("Accidente", expediente.GetProperty("tipo").GetString());
        Assert.Equal(84_310, expediente.GetProperty("odometro").GetInt32());
        Assert.True(expediente.GetProperty("estaAbierto").GetBoolean());
    }

    /// <summary>
    /// `RN-70` admite captura sin ninguna conectividad, así que la distancia entre el hecho y la
    /// captura es <b>un dato del expediente y no un error</b>.
    /// </summary>
    [Fact]
    public async Task Las_dos_fechas_del_incidente_se_conservan_por_separado()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var id = await RegistrarAsync(
            cliente,
            hecho: new DateTimeOffset(2026, 6, 10, 14, 0, 0, TimeSpan.FromHours(-6)),
            captura: new DateTimeOffset(2026, 6, 13, 8, 0, 0, TimeSpan.FromHours(-6)));

        var expediente = await Leer(cliente, $"/incidentes/{id}");

        Assert.Equal("2026-06-10", expediente.GetProperty("fechaDelHecho").GetString());
        Assert.Equal(3, expediente.GetProperty("diasEntreElHechoYLaCaptura").GetInt32());
    }

    /// <summary>
    /// `RN-74` punto 4 — la determinación se adjunta como <b>acto de la instancia competente</b>,
    /// con su número y su emisor. Sin ellos, el servidor bloquea.
    /// </summary>
    [Fact]
    public async Task La_determinacion_se_adjunta_como_acto_de_otra_instancia()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var id = await RegistrarAsync(cliente, tipo: "Accidente");

        var sinEmisor = await cliente.PostAsJsonAsync($"/incidentes/{id}/determinacion", new
        {
            Numero = "RES-2026-14",
            Instancia = "",
            Fecha = new DateOnly(2026, 7, 1),
            Resolucion = "Sin responsabilidad atribuible.",
            Ejecuta = "P-AUDITORIA",
            Momento = DateTimeOffset.UtcNow,
        });

        Assert.False(sinEmisor.IsSuccessStatusCode);
        Assert.Contains("no la produce", await sinEmisor.Content.ReadAsStringAsync());

        await Post(cliente, $"/incidentes/{id}/determinacion", new
        {
            Numero = "RES-2026-14",
            Instancia = "Auditoría Interna",
            Fecha = new DateOnly(2026, 7, 1),
            Resolucion = "Sin responsabilidad atribuible al servidor público.",
            Ejecuta = "P-AUDITORIA",
            Momento = DateTimeOffset.UtcNow,
        });

        var expediente = await Leer(cliente, $"/incidentes/{id}");

        Assert.Equal("Auditoría Interna",
            expediente.GetProperty("determinacion").GetProperty("instancia").GetString());
    }

    // ── El bloqueo del cierre, que hasta M-12 no podía disparar ─────────────

    /// <summary>
    /// <b>La prueba que paga todo el módulo.</b>
    ///
    /// `RN-97` punto 4 le da poder de bloqueo del cierre a las interrupciones sin desenlace, y
    /// el saldo de apertura las declaraba <i>«no consultables»</i> porque no existían como
    /// registro: el bloqueo estaba escrito y no podía disparar. Ahora dispara.
    /// </summary>
    [Fact]
    public async Task La_interrupcion_sin_desenlace_IMPIDE_producir_el_saldo_de_apertura()
    {
        const int anio = 2050;
        var corte = new DateOnly(anio, 12, 31);

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var id = await RegistrarAsync(
            cliente,
            tipo: "AveriaMecanica",
            interrumpe: true,
            hecho: new DateTimeOffset(anio, 11, 18, 14, 0, 0, TimeSpan.FromHours(-6)));

        // ── Aparece en el inventario, y como fuente CONSULTADA ───────────────
        var inventario = await Leer(cliente, $"/saldo-de-apertura/inventario/{corte:yyyy-MM-dd}");

        var fuente = Assert.Single(inventario.GetProperty("fuentes").EnumerateArray(),
            f => f.GetProperty("tipo").GetString() == "InterrupcionSinDesenlace");

        Assert.True(fuente.GetProperty("sePudoConsultar").GetBoolean());

        Assert.Single(inventario.GetProperty("renglones").EnumerateArray(),
            r => r.GetProperty("referencia").GetString() == id
                && r.GetProperty("impideCerrar").GetBoolean());

        // ── Y el bloqueo dispara ────────────────────────────────────────────
        var bloqueado = await cliente.PostAsJsonAsync("/saldo-de-apertura", Saldo(anio, corte));

        Assert.False(bloqueado.IsSuccessStatusCode);

        var mensaje = await bloqueado.Content.ReadAsStringAsync();
        Assert.Contains("InterrupcionSinDesenlace", mensaje);
        Assert.Contains("no se cierra con", mensaje);

        // ── Con desenlace registrado, el saldo se produce ────────────────────
        await Post(cliente, $"/incidentes/{id}/desenlace", new
        {
            Desenlace = "RetornoAnticipado",
            Detalle = "Autorizado por ACT-04; la unidad se remolcó al taller de la delegación.",
            Ejecuta = "P-TRANSPORTE",
            Momento = new DateTimeOffset(anio, 11, 19, 9, 0, 0, TimeSpan.FromHours(-6)),
        });

        await Post(cliente, "/saldo-de-apertura", Saldo(anio, corte));
    }

    /// <summary>
    /// `RN-97` punto 4: <i>«hay que resolverlos <b>o declararlos explícitamente</b>»</i>.
    /// Declararlo es un acto con motivo que queda en el documento; ignorarlo no es opción.
    /// </summary>
    [Fact]
    public async Task La_interrupcion_declarada_explicitamente_deja_producir_el_saldo()
    {
        const int anio = 2051;
        var corte = new DateOnly(anio, 12, 31);

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await RegistrarAsync(
            cliente,
            tipo: "CondicionDeSeguridad",
            interrumpe: true,
            hecho: new DateTimeOffset(anio, 10, 2, 11, 0, 0, TimeSpan.FromHours(-6)));

        await Post(cliente, "/saldo-de-apertura", Saldo(anio, corte,
            "La zona sigue con alerta de seguridad y el desenlace depende de esa condición."));
    }

    // ── El bien no sale del registro ────────────────────────────────────────

    /// <summary>
    /// `RN-75` — <i>«el bien permanece en el registro patrimonial hasta su recuperación o su
    /// descargo formal. <b>Nunca se elimina</b>»</i>.
    /// </summary>
    [Fact]
    public async Task El_bien_sustraido_permanece_en_el_registro_y_cambia_de_estado()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var id = await RegistrarAsync(
            cliente,
            tipo: "Sustraccion",
            hecho: new DateTimeOffset(2026, 3, 14, 9, 0, 0, TimeSpan.FromHours(-6)),
            bienes: [new { Descripcion = "Pick-up doble cabina INS-P-014", EsElVehiculo = true }]);

        var conBien = await Leer(cliente, $"/incidentes/{id}");
        var bien = conBien.GetProperty("bienes").EnumerateArray().Single();

        Assert.Equal("NoRecuperado", bien.GetProperty("estado").GetString());
        Assert.True(bien.GetProperty("diasFuera").GetInt32() > 0);

        // Aparece en la lista transversal de bienes que siguen fuera.
        var afuera = await Leer(cliente, "/incidentes/bienes-no-recuperados");
        Assert.Contains(afuera.EnumerateArray(), b => b.GetProperty("incidente").GetString() == id);

        // ── El expediente no cierra con el bien afuera sin declararlo ────────
        var bloqueado = await cliente.PostAsJsonAsync($"/incidentes/{id}/resolver", new
        {
            ComoSeResolvio = "Se agotaron las gestiones.",
            Fecha = new DateOnly(2027, 1, 15),
            Ejecuta = "P-TRANSPORTE",
            Momento = DateTimeOffset.UtcNow,
            DeclaracionDeBienes = (string?)null,
        });

        Assert.False(bloqueado.IsSuccessStatusCode);
        Assert.Contains("permanece en el registro patrimonial",
            await bloqueado.Content.ReadAsStringAsync());

        // ── Recuperado: cambia de estado, no desaparece ──────────────────────
        await Post(cliente, $"/incidentes/{id}/bienes/{bien.GetProperty("id").GetString()}/recuperar",
            new
            {
                Donde = "Entregado por la Policía Nacional en el predio de Comayagüela.",
                Ejecuta = "P-TRANSPORTE",
                Momento = DateTimeOffset.UtcNow,
            });

        var recuperado = await Leer(cliente, $"/incidentes/{id}");

        var bienRecuperado = recuperado.GetProperty("bienes").EnumerateArray().Single();
        Assert.Equal("Recuperado", bienRecuperado.GetProperty("estado").GetString());

        // **Sigue en el expediente.** El registro conserva que estuvo afuera.
        Assert.Equal("2026-03-14", bienRecuperado.GetProperty("fechaDelHecho").GetString());

        // Y ahora sí cierra.
        await Post(cliente, $"/incidentes/{id}/resolver", new
        {
            ComoSeResolvio = "La unidad se recuperó y volvió a la flota.",
            Fecha = new DateOnly(2027, 1, 15),
            Ejecuta = "P-TRANSPORTE",
            Momento = DateTimeOffset.UtcNow,
            DeclaracionDeBienes = (string?)null,
        });
    }

    /// <summary>
    /// El descargo formal — `RN-75`. La única salida del registro que no es la recuperación, y
    /// exige acto con número y autoridad: sin él sería una baja sin respaldo sobre un bien del
    /// Estado.
    /// </summary>
    [Fact]
    public async Task El_bien_solo_se_descarga_con_acto_formal()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var id = await RegistrarAsync(
            cliente,
            tipo: "Sustraccion",
            bienes: [new { Descripcion = "Equipo de radio comunicación", EsElVehiculo = false }]);

        var bien = (await Leer(cliente, $"/incidentes/{id}"))
            .GetProperty("bienes").EnumerateArray().Single()
            .GetProperty("id").GetString();

        var sinActo = await cliente.PostAsJsonAsync(
            $"/incidentes/{id}/bienes/{bien}/descargar",
            new
            {
                Numero = "",
                Autoridad = "",
                Fecha = new DateOnly(2027, 3, 1),
                Ejecuta = "P-GERENCIA",
                Momento = DateTimeOffset.UtcNow,
            });

        Assert.False(sinActo.IsSuccessStatusCode);
        Assert.Contains("baja sin respaldo", await sinActo.Content.ReadAsStringAsync());

        await Post(cliente, $"/incidentes/{id}/bienes/{bien}/descargar", new
        {
            Numero = "ACU-2027-08",
            Autoridad = "Gerencia Administrativa",
            Fecha = new DateOnly(2027, 3, 1),
            Ejecuta = "P-GERENCIA",
            Momento = DateTimeOffset.UtcNow,
        });

        var descargado = (await Leer(cliente, $"/incidentes/{id}"))
            .GetProperty("bienes").EnumerateArray().Single();

        Assert.Equal("Descargado", descargado.GetProperty("estado").GetString());
        Assert.Equal("ACU-2027-08",
            descargado.GetProperty("descargo").GetProperty("numero").GetString());
    }

    // ── Andamio ─────────────────────────────────────────────────────────────

    private static async Task<string> RegistrarAsync(
        HttpClient cliente,
        string tipo = "AveriaMecanica",
        bool interrumpe = false,
        DateTimeOffset? hecho = null,
        DateTimeOffset? captura = null,
        object[]? bienes = null)
    {
        var id = Ulid.NewUlid().ToString();
        var momento = hecho ?? new DateTimeOffset(2026, 5, 4, 10, 0, 0, TimeSpan.FromHours(-6));

        await Post(cliente, "/incidentes", new
        {
            Id = id,
            Tipo = tipo,
            Causa = "Falla de transmisión",
            MomentoDelHecho = momento,
            MomentoDeCaptura = captura ?? momento.AddHours(2),
            Descripcion = "El vehículo quedó en el km 61 sin poder avanzar.",
            Registra = "P-MOTORISTA",
            ResponsableDeSeguimiento = "P-TRANSPORTE",
            Plazo = DateOnly.FromDateTime(momento.UtcDateTime).AddDays(7),
            Interrumpe = interrumpe,
            IdMision = (string?)null,
            IdVehiculo = (string?)null,
            Ubicacion = "km 61, CA-5",
            Odometro = 84_310,
            Bienes = bienes,
        });

        return id;
    }

    private static object Saldo(int anio, DateOnly corte, string? declaracion = null) => new
    {
        Id = Ulid.NewUlid().ToString(),
        Folio = $"SA-{anio}-M12",
        Ejercicio = $"{anio}",
        Corte = corte,
        Persona = "P-AUDITORIA",
        Puesto = "PU-AUDITORIA",
        Momento = new DateTimeOffset(anio + 1, 1, 5, 9, 0, 0, TimeSpan.FromHours(-6)),

        // `RN-97` punto 4 — el motivo por el que se produce con bloqueantes vivos. Declararlos
        // es un acto con motivo que queda en el documento; ignorarlos no es una opción.
        DeclaracionDeBloqueantes = declaracion,
    };

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
