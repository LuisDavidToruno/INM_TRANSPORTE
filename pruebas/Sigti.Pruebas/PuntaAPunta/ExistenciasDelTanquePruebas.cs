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
/// `RN-83` punto 5 cableada — el abastecimiento desde el tanque <b>descuenta de las
/// existencias</b>.
///
/// ── El hueco que esto cierra, dicho con números ──────────────────────────────
/// `FuenteDeAbastecimiento.TanqueInstitucional` existía desde `RN-83`, se podía elegir en la
/// pantalla, y <b>no descontaba de ninguna parte</b>. El galón quedaba imputado al vehículo y
/// el tanque de la sede no se enteraba: exactamente igual de invisible que antes de la regla,
/// sólo que con la apariencia de estar registrado.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class ExistenciasDelTanquePruebas(BaseDePruebas baseDePruebas)
{
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 16, 9, 0, 0, TimeSpan.FromHours(-6));

    private WebApplicationFactory<Program> Aplicacion() => FabricaDeSigti.Crear(baseDePruebas);

    [Fact]
    public async Task El_abastecimiento_desde_el_tanque_DESCUENTA_de_las_existencias()
    {
        var r = await Sembrar("TQ-0001");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var tanque = await AbrirTanque(cliente, 500m);

        await Post(cliente, "/abastecimientos", new
        {
            Id = Ulid.NewUlid().ToString(),
            IdVehiculo = r.Vehiculo,
            OcurridoEn = Momento,
            Galones = 60m,
            Odometro = 84_000,
            Fuente = "TanqueInstitucional",
            Registra = "P-ALMACEN",

            // Los cuatro que convierten una resta en un despacho.
            IdTanque = tanque,
            PuestoDespacha = "PU-ALMACEN",
            IdReceptor = "P-MOTORISTA",
            CombustibleDelVehiculo = "Diesel",
        });

        var t = await Leer(cliente, $"/tanques/{tanque}");

        Assert.Equal(440m, t.GetProperty("existencia").GetDecimal());

        // Y el asiento imputa el galón a una placa. Sin eso el egreso diría cuánto salió pero
        // no adónde fue, que es el problema entero.
        var despacho = t.GetProperty("libro").EnumerateArray()
            .Single(m => m.GetProperty("movimiento").GetString() == "E-02");

        Assert.Equal(r.Vehiculo, despacho.GetProperty("vehiculo").GetString());
        Assert.Contains("Recibe P-MOTORISTA", despacho.GetProperty("motivo").GetString());
    }

    [Fact]
    public async Task Sin_existencia_el_despacho_se_rechaza_y_el_abastecimiento_TAMPOCO_entra()
    {
        // Van en la misma transacción a propósito: si el despacho falla y el abastecimiento
        // entrara igual, quedaría un galón imputado a un vehículo contra un tanque que nunca
        // lo soltó — que es peor que no tener el libro.
        var r = await Sembrar("TQ-0002");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var tanque = await AbrirTanque(cliente, 40m);
        var abastecimiento = Ulid.NewUlid().ToString();

        var respuesta = await cliente.PostComoAsync("/abastecimientos", new
        {
            Id = abastecimiento,
            IdVehiculo = r.Vehiculo,
            OcurridoEn = Momento,
            Galones = 60m,
            Odometro = 84_000,
            Fuente = "TanqueInstitucional",
            Registra = "P-ALMACEN",
            IdTanque = tanque,
            PuestoDespacha = "PU-ALMACEN",
            IdReceptor = "P-MOTORISTA",
            CombustibleDelVehiculo = "Diesel",
        });

        var texto = await respuesta.Content.ReadAsStringAsync();

        Assert.False(respuesta.IsSuccessStatusCode);
        Assert.Contains("40.00 galones en libros y se piden 60.00", texto);

        // El tanque quedó intacto y el abastecimiento no existe.
        var t = await Leer(cliente, $"/tanques/{tanque}");
        Assert.Equal(40m, t.GetProperty("existencia").GetDecimal());

        var sinRespaldo = await Leer(cliente, "/tanques/despachos-sin-respaldo");
        Assert.DoesNotContain(sinRespaldo.EnumerateArray(), d =>
            d.GetProperty("abastecimiento").GetString() == abastecimiento);
    }

    [Fact]
    public async Task Nadie_se_despacha_combustible_a_si_mismo()
    {
        var r = await Sembrar("TQ-0003");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var tanque = await AbrirTanque(cliente, 500m);

        var respuesta = await cliente.PostComoAsync("/abastecimientos", new
        {
            Id = Ulid.NewUlid().ToString(),
            IdVehiculo = r.Vehiculo,
            OcurridoEn = Momento,
            Galones = 60m,
            Odometro = 84_000,
            Fuente = "TanqueInstitucional",
            Registra = "P-MOTORISTA",
            IdTanque = tanque,
            PuestoDespacha = "PU-MOTORISTA",
            IdReceptor = "P-MOTORISTA",
            CombustibleDelVehiculo = "Diesel",
        });

        Assert.False(respuesta.IsSuccessStatusCode);
        Assert.Contains("no puede despacharse combustible a sí mismo",
            await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task El_galon_declarado_del_tanque_sin_despacho_ENTRA_y_queda_como_discrepancia()
    {
        // `RN-83`: «el registro del abastecimiento no se omite nunca». Un motorista que declara
        // desde el campo «cargué de la cisterna» reporta un hecho consumado — no tiene el tanque
        // a mano ni puede firmar el despacho. Rechazarlo no devolvería el combustible: lo sacaría
        // del denominador de `RN-30`, que es donde más falta hace.
        var r = await Sembrar("TQ-0004");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var abastecimiento = Ulid.NewUlid().ToString();

        await Post(cliente, "/abastecimientos", new
        {
            Id = abastecimiento,
            IdVehiculo = r.Vehiculo,
            OcurridoEn = Momento,
            Galones = 40m,
            Odometro = 84_000,
            Fuente = "TanqueInstitucional",
            Registra = "P-MOTORISTA",
        });

        // Entró. Y la contradicción tiene nombre y sale en una lista: es el préstamo invisible
        // de `CE-23`, ahora visible.
        var sinRespaldo = await Leer(cliente, "/tanques/despachos-sin-respaldo");

        var fila = sinRespaldo.EnumerateArray()
            .Single(d => d.GetProperty("abastecimiento").GetString() == abastecimiento);

        Assert.Equal(40m, fila.GetProperty("galones").GetDecimal());
        Assert.Equal(r.Vehiculo, fila.GetProperty("vehiculo").GetString());
    }

    [Fact]
    public async Task El_galon_CON_despacho_no_figura_como_discrepancia()
    {
        var r = await Sembrar("TQ-0005");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var tanque = await AbrirTanque(cliente, 500m);
        var abastecimiento = Ulid.NewUlid().ToString();

        await Post(cliente, "/abastecimientos", new
        {
            Id = abastecimiento,
            IdVehiculo = r.Vehiculo,
            OcurridoEn = Momento,
            Galones = 60m,
            Odometro = 84_000,
            Fuente = "TanqueInstitucional",
            Registra = "P-ALMACEN",
            IdTanque = tanque,
            PuestoDespacha = "PU-ALMACEN",
            IdReceptor = "P-MOTORISTA",
            CombustibleDelVehiculo = "Diesel",
        });

        var sinRespaldo = await Leer(cliente, "/tanques/despachos-sin-respaldo");

        Assert.DoesNotContain(sinRespaldo.EnumerateArray(), d =>
            d.GetProperty("abastecimiento").GetString() == abastecimiento);
    }

    [Fact]
    public async Task El_trasiego_mueve_los_dos_tanques_y_conserva_el_total()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var origen = await AbrirTanque(cliente, 500m);
        var destino = await AbrirTanque(cliente, 100m);

        await Post(cliente, "/tanques/trasiegos", new
        {
            IdOrigen = origen,
            IdDestino = destino,
            Galones = 120m,
            Persona = "P-GERENCIA",
            Puesto = "PU-GERENCIA-ADMIN",
            Momento,
        });

        var uno = await Leer(cliente, $"/tanques/{origen}");
        var dos = await Leer(cliente, $"/tanques/{destino}");

        Assert.Equal(380m, uno.GetProperty("existencia").GetDecimal());
        Assert.Equal(220m, dos.GetProperty("existencia").GetDecimal());

        // Registrar sólo la salida haría que el combustible se evaporara del sistema entero:
        // la forma exacta en que un faltante se disfraza de traslado.
        Assert.Equal(600m,
            uno.GetProperty("existencia").GetDecimal() +
            dos.GetProperty("existencia").GetDecimal());
    }

    [Fact]
    public async Task El_arqueo_MIDE_y_no_cuadra_el_libro_por_su_cuenta()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var tanque = await AbrirTanque(cliente, 500m);

        await Post(cliente, $"/tanques/{tanque}/movimiento", new
        {
            Movimiento = "E-05",
            Persona = "P-COMISION",
            Puesto = "PU-AUDITORIA",
            Texto = "Acta AR-2026-0003, medición con varilla ante dos testigos.",
            ExistenciaMedida = 470m,
            Momento,
        });

        var t = await Leer(cliente, $"/tanques/{tanque}");

        // El libro sigue en 500 y la diferencia está nombrada. Un arqueo que ajusta solo hace
        // desaparecer la diferencia en el mismo acto que la descubre.
        Assert.Equal(500m, t.GetProperty("existencia").GetDecimal());
        Assert.Equal(30m, t.GetProperty("diferenciaDelUltimoArqueo").GetDecimal());

        // Y ajustar es otro acto, de otro, con motivo tipificado.
        await Post(cliente, $"/tanques/{tanque}/movimiento", new
        {
            Movimiento = "E-06",
            Persona = "P-GERENCIA",
            Puesto = "PU-GERENCIA-ADMIN",
            Texto = "Se acoge el acta AR-2026-0003: merma del período.",
            Galones = -30m,
            MotivoDelAjuste = "MermaTecnica",
            Momento,
        });

        var despues = await Leer(cliente, $"/tanques/{tanque}");
        Assert.Equal(470m, despues.GetProperty("existencia").GetDecimal());
    }

    [Fact]
    public async Task Un_tanque_nunca_arqueado_no_esta_cuadrado_esta_SIN_VERIFICAR()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var tanque = await AbrirTanque(cliente, 500m);
        var t = await Leer(cliente, $"/tanques/{tanque}");

        // Nulo, no cero. De un tanque nunca medido no se deduce que cuadre.
        Assert.Equal(JsonValueKind.Null,
            t.GetProperty("diferenciaDelUltimoArqueo").ValueKind);
    }

    // ── Andamio ─────────────────────────────────────────────────────────────

    private async Task<FlotaSembrada.ParaProgramar> Sembrar(string prefijo)
    {
        await using var contexto = baseDePruebas.Contexto();
        return await FlotaSembrada.ParaProgramarAsync(contexto, prefijo);
    }

    private static async Task<string> AbrirTanque(HttpClient cliente, decimal inicial)
    {
        var id = Ulid.NewUlid().ToString();

        await Post(cliente, "/tanques", new
        {
            Id = id,
            Nombre = $"Cisterna {id[..8]}",
            Ambito = "Delegacion de Choluteca",
            TipoDeCombustible = "Diesel",
            Capacidad = 1_000m,
            ExistenciaInicial = inicial,
            Persona = "P-ALMACEN",
            Puesto = "PU-ALMACEN",
            Momento,
        });

        return id;
    }

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
