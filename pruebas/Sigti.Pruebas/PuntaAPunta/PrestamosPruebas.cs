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
/// `RN-63` cableada — el préstamo como expediente del bien.
///
/// ── Lo que cierra ───────────────────────────────────────────────────────────
/// `RN-97` punto 4 le da poder de bloqueo del cierre a dos fuentes. Con M-12 llegó una; ésta es
/// la otra. <b>El bloqueo del saldo de apertura ya dispara entero.</b>
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class PrestamosPruebas(BaseDePruebas baseDePruebas)
{
    private WebApplicationFactory<Program> Aplicacion() => FabricaDeSigti.Crear(baseDePruebas);

    /// <summary>
    /// `RN-63`: <i>«cuando el vehículo se cede con motorista de la institución propietaria, sí es
    /// una Orden de Misión con motivo apoyo institucional: ahí no se cedió la tenencia, se prestó
    /// un servicio»</i>.
    /// </summary>
    [Fact]
    public async Task Ceder_el_vehiculo_con_motorista_propio_NO_abre_expediente_de_prestamo()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var respuesta = await cliente.PostComoAsync(
            "/prestamos", Cuerpo(conMotoristaPropio: true));

        Assert.False(respuesta.IsSuccessStatusCode);
        Assert.Contains("apoyo institucional", await respuesta.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// `RN-63` punto 2 — quien autoriza no puede ser el receptor. Es la misma persona decidiendo
    /// entregarse a sí misma un vehículo del Estado.
    ///
    /// ⚠️ El par no está en `actores-y-roles.md`, que es la autoridad sobre incompatibilidades:
    /// la propia regla deja esa nota de hallazgo abierta. Esta comprobación es lo único que lo
    /// sostiene mientras tanto.
    /// </summary>
    [Fact]
    public async Task Quien_autoriza_el_prestamo_no_puede_ser_el_receptor()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var respuesta = await cliente.PostComoAsync(
            "/prestamos", Cuerpo(autoriza: "Ana Discua", receptor: "Ana Discua"));

        Assert.False(respuesta.IsSuccessStatusCode);
        Assert.Contains("entregarse a sí misma", await respuesta.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// <b>El entregable de `RN-63`</b> punto 7: en cualquier fecha del período, quién respondía
    /// por la unidad.
    /// </summary>
    [Fact]
    public async Task El_sistema_responde_quien_respondia_por_la_unidad_en_cada_fecha()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var vehiculo = Ulid.NewUlid().ToString();
        var id = Ulid.NewUlid().ToString();

        await Post(cliente, "/prestamos", Cuerpo(id: id, vehiculo: vehiculo));

        // Durante la ventana: responde el receptor, con cargo e institución.
        var durante = await Leer(cliente, $"/prestamos/quien-respondia/{vehiculo}/2026-04-20");

        Assert.True(durante.GetProperty("esTenenciaAjena").GetBoolean());
        Assert.Equal("Ana Discua", durante.GetProperty("persona").GetString());
        Assert.Equal("Secretaría de Salud", durante.GetProperty("institucion").GetString());

        // Antes: responde la institución propietaria.
        var antes = await Leer(cliente, $"/prestamos/quien-respondia/{vehiculo}/2026-04-01");

        Assert.False(antes.GetProperty("esTenenciaAjena").GetBoolean());
        Assert.True(antes.GetProperty("persona").ValueKind is JsonValueKind.Null);
    }

    /// <summary>
    /// <b>La prueba que cierra el bloqueo del cierre.</b>
    ///
    /// `RN-97` punto 4 impide cerrar el período con préstamos vencidos, y esa fuente estuvo
    /// declarada y vacía desde que el saldo de apertura se construyó.
    /// </summary>
    [Fact]
    public async Task El_prestamo_vencido_IMPIDE_producir_el_saldo_de_apertura()
    {
        const int anio = 2052;
        var corte = new DateOnly(anio, 12, 31);

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var id = Ulid.NewUlid().ToString();

        await Post(cliente, "/prestamos", Cuerpo(
            id: id,
            desde: new DateOnly(anio, 4, 6),
            comprometida: new DateOnly(anio, 5, 6)));

        // ── Aparece como fuente CONSULTADA, con sus días de mora ─────────────
        var inventario = await Leer(cliente, $"/saldo-de-apertura/inventario/{corte:yyyy-MM-dd}");

        var fuente = Assert.Single(inventario.GetProperty("fuentes").EnumerateArray(),
            f => f.GetProperty("tipo").GetString() == "PrestamoVencido");

        Assert.True(fuente.GetProperty("sePudoConsultar").GetBoolean());

        var renglon = Assert.Single(inventario.GetProperty("renglones").EnumerateArray(),
            r => r.GetProperty("referencia").GetString() == id);

        Assert.True(renglon.GetProperty("impideCerrar").GetBoolean());
        Assert.Contains("días de mora", renglon.GetProperty("estado").GetString());

        // La antigüedad se cuenta desde la fecha COMPROMETIDA: es la que venció.
        Assert.Equal(239, renglon.GetProperty("antiguedadEnDias").GetInt32());

        // ── Y el bloqueo dispara ────────────────────────────────────────────
        var bloqueado = await cliente.PostComoAsync("/saldo-de-apertura", Saldo(anio, corte));

        Assert.False(bloqueado.IsSuccessStatusCode);
        Assert.Contains("PrestamoVencido", await bloqueado.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// El acta de devolución — `RN-63` punto 6. <b>El vehículo no vuelve a `DISPONIBLE` sin
    /// ella</b>, y quien la firma no puede ser quien recibió.
    /// </summary>
    [Fact]
    public async Task El_acta_de_devolucion_no_la_firma_quien_recibio()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var id = Ulid.NewUlid().ToString();
        await Post(cliente, "/prestamos", Cuerpo(id: id));

        var autodeclarada = await cliente.PostComoAsync($"/prestamos/{id}/devolver", new
        {
            Fecha = new DateOnly(2026, 5, 4),
            Odometro = 92_800,
            Firma = "Ana Discua",
            NivelDeCombustible = "1/2",
            RotulacionConstatada = true,
            Novedades = (string?)null,
            QuienFirmaLaDevolucion = "Ana Discua",
        });

        Assert.False(autodeclarada.IsSuccessStatusCode);
        Assert.Contains("autodeclaración", await autodeclarada.Content.ReadAsStringAsync());

        // ── El odómetro tampoco retrocede ────────────────────────────────────
        var retrocede = await cliente.PostComoAsync($"/prestamos/{id}/devolver", new
        {
            Fecha = new DateOnly(2026, 5, 4),
            Odometro = 90_000,
            Firma = "P-TRANSPORTE",
            NivelDeCombustible = "1/2",
            RotulacionConstatada = true,
            Novedades = (string?)null,
            QuienFirmaLaDevolucion = "P-TRANSPORTE",
        });

        Assert.False(retrocede.IsSuccessStatusCode);
        Assert.Contains("no retrocede", await retrocede.Content.ReadAsStringAsync());

        // ── Y devuelto en regla, sin rotulación ──────────────────────────────
        await Post(cliente, $"/prestamos/{id}/devolver", new
        {
            Fecha = new DateOnly(2026, 5, 4),
            Odometro = 92_800,
            Firma = "P-TRANSPORTE",
            NivelDeCombustible = "1/2",
            RotulacionConstatada = false,
            Novedades = "Vuelve sin las franjas del costado derecho.",
            QuienFirmaLaDevolucion = "P-TRANSPORTE",
        });

        var expediente = Assert.Single(
            (await Leer(cliente, "/prestamos")).EnumerateArray(),
            p => p.GetProperty("id").GetString() == id);

        Assert.False(expediente.GetProperty("estaVigente").GetBoolean());

        // `RN-63` punto 3 — los kilómetros bajo tenencia ajena, de las dos lecturas.
        Assert.Equal(1_400, expediente.GetProperty("kilometrosBajoTenenciaAjena").GetInt32());

        // Hallazgo frecuente de auditoría, y por eso se reconstata.
        Assert.True(expediente.GetProperty("volvioSinRotulacion").GetBoolean());
    }

    /// <summary>
    /// Dos préstamos vivos sobre la misma unidad dejarían sin poder decir quién respondía por
    /// ella — justo lo que el expediente existe para contestar.
    /// </summary>
    [Fact]
    public async Task Un_vehiculo_no_se_presta_dos_veces_a_la_vez()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var vehiculo = Ulid.NewUlid().ToString();

        await Post(cliente, "/prestamos", Cuerpo(vehiculo: vehiculo));

        var segundo = await cliente.PostComoAsync("/prestamos", Cuerpo(vehiculo: vehiculo));

        Assert.False(segundo.IsSuccessStatusCode);
        Assert.Contains("ya está prestado", await segundo.Content.ReadAsStringAsync());
    }

    // ── Andamio ─────────────────────────────────────────────────────────────

    private static object Cuerpo(
        string? id = null,
        string? vehiculo = null,
        string autoriza = "Rolando Discua",
        string receptor = "Ana Discua",
        bool conMotoristaPropio = false,
        DateOnly? desde = null,
        DateOnly? comprometida = null) => new
        {
            Id = id ?? Ulid.NewUlid().ToString(),
            IdVehiculo = vehiculo ?? Ulid.NewUlid().ToString(),
            ActoFolio = $"ACU-{Ulid.NewUlid().ToString()[^8..]}",
            ActoFirmante = "Máxima Autoridad",
            ActoFecha = new DateOnly(2026, 4, 2),
            ActoAdjunto = (string?)null,
            Autoriza = autoriza,
            ReceptorPersona = receptor,
            ReceptorCargo = "Jefe de Transporte",
            ReceptorInstitucion = "Secretaría de Salud",
            ReceptorConstancia = "Acta de recepción firmada",
            Motivo = "Apoyo a jornada de vacunación",
            Desde = desde ?? new DateOnly(2026, 4, 6),
            DevolucionComprometida = comprometida ?? new DateOnly(2026, 5, 6),
            EntregaOdometro = 91_400,
            EntregaFirma = "P-TRANSPORTE",
            EntregaCombustible = "3/4",
            EntregaAccesorios = "Llanta de repuesto, gato, triángulos",
            EntregaDocumentos = "Matrícula y tarjeta de circulación",
            EntregaRotulacion = true,
            EntregaNovedades = (string?)null,
            RubroCombustible = "Receptor",
            RubroPeajes = "Receptor",
            RubroMantenimiento = "Propietaria",
            RubroMultas = "Receptor",
            RubroDanios = "Receptor",
            ConMotoristaPropio = conMotoristaPropio,
        };

    private static object Saldo(int anio, DateOnly corte) => new
    {
        Id = Ulid.NewUlid().ToString(),
        Folio = $"SA-{anio}-P63",
        Ejercicio = $"{anio}",
        Corte = corte,
        Persona = "P-AUDITORIA",
        Puesto = "PU-AUDITORIA",
        Momento = new DateTimeOffset(anio + 1, 1, 5, 9, 0, 0, TimeSpan.FromHours(-6)),
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
        var respuesta = await cliente.PostComoAsync(ruta, cuerpo);

        if (!respuesta.IsSuccessStatusCode)
            throw new Xunit.Sdk.XunitException(
                $"POST {ruta} devolvió {(int)respuesta.StatusCode}: " +
                await respuesta.Content.ReadAsStringAsync());
    }
}
