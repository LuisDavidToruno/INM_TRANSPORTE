using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sigti.Datos;
using Sigti.Datos.M18_Peajes;
using Sigti.Dominio.M03_Flota;
using Sigti.Pruebas.Datos;

namespace Sigti.Pruebas.PuntaAPunta;

/// <summary>
/// M-18 cableado — el catálogo, la categoría, el estimado y el paso por caseta.
///
/// ── Los dos errores que esto tiene que impedir, con números ──────────────────
/// <b>Cobrarle mal a la propia flota:</b> un pickup de dos ejes paga L 22 y un «Vehículo de 2
/// Ejes» paga L 90. Resolver por ejes multiplicaría por cuatro el estimado de cada pickup `[V]`.
///
/// <b>Que un cobro indebido se vuelva la verdad institucional:</b> COVI-H cobró L 90 en lugar
/// de L 22 a los H-100, K2700 y Sprinter, y la SAPP tuvo que ordenar suspenderlo el 17/09/2025
/// `[V]`.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class PeajesPruebas(BaseDePruebas baseDePruebas)
{
    private static readonly DateOnly Salida = new(2026, 4, 10);

    private static readonly DateTimeOffset Momento =
        new(2026, 4, 10, 9, 30, 0, TimeSpan.FromHours(-6));

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

    [Fact]
    public async Task Un_pickup_de_dos_ejes_estima_como_LIVIANO_y_no_como_vehiculo_de_2_ejes()
    {
        var (vehiculo, zambrano, _) = await SembrarCatalogo("PJ-0001", ejes: 2, camion: false);
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var e = await Estimar(cliente, vehiculo, [(zambrano, 2)]);

        // 2 cruces × L 22 = L 44. Con la tarifa de dos ejes serían L 180.
        Assert.Equal(44m, e.GetProperty("total").GetDecimal());
        Assert.Equal("Liviano/Turismo", e.GetProperty("lineas")[0].GetProperty("categoria").GetString());
    }

    [Fact]
    public async Task Un_camion_de_dos_ejes_estima_con_SU_tarifa()
    {
        var (vehiculo, zambrano, _) = await SembrarCatalogo("PJ-0002", ejes: 2, camion: true);
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var e = await Estimar(cliente, vehiculo, [(zambrano, 2)]);

        Assert.Equal(180m, e.GetProperty("total").GetDecimal());
    }

    [Fact]
    public async Task El_estimado_se_DESGLOSA_por_punto_y_cuenta_cruces()
    {
        // Tegucigalpa → San Pedro Sula: ida y vuelta por dos casetas son cuatro cruces. Sin
        // desglose el autorizador no puede distinguir un estimado correcto de uno que duplicó
        // un cruce.
        var (vehiculo, zambrano, comayagua) = await SembrarCatalogo("PJ-0003", 2, camion: false);
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var e = await Estimar(cliente, vehiculo, [(zambrano, 2), (comayagua, 2)]);

        var lineas = e.GetProperty("lineas").EnumerateArray().ToList();

        Assert.Equal(2, lineas.Count);
        Assert.Equal(2, lineas[0].GetProperty("cruces").GetInt32());
        Assert.Contains("2 cruce(s) × 22.00", lineas[0].GetProperty("fundamento").GetString());
        Assert.Contains("fuente SAPP", lineas[0].GetProperty("fundamento").GetString());
        Assert.Equal(88m, e.GetProperty("total").GetDecimal());
    }

    [Fact]
    public async Task Sin_numero_de_ejes_la_categoria_NO_se_resuelve_y_el_estimado_lo_dice()
    {
        // `RN-33` punto 3: el sistema no adivina. Y `RN-35`: la orden se puede aprobar igual,
        // con el estimado marcado como no disponible y su causa.
        var (vehiculo, zambrano, _) = await SembrarCatalogo("PJ-0004", ejes: null, camion: true);
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var categoria = await Leer(cliente, $"/peajes/categoria/{vehiculo}");

        Assert.False(categoria.GetProperty("resuelta").GetBoolean());
        Assert.Equal("el número de ejes", categoria.GetProperty("atributoQueFalta").GetString());

        var e = await Estimar(cliente, vehiculo, [(zambrano, 2)]);

        Assert.False(e.GetProperty("disponible").GetBoolean());
        Assert.Equal(JsonValueKind.Null, e.GetProperty("total").ValueKind);
        Assert.Single(e.GetProperty("faltantes").EnumerateArray());
    }

    [Fact]
    public async Task La_categoria_SIEMPRE_sale_marcada_provisional()
    {
        // El criterio legal es el Artículo 51 y el PDF oficial es un escaneo sin capa de texto
        // (`[C]`, insumo #23). Una categoría provisional mostrada igual que una firme se cita
        // después como si lo fuera.
        var (vehiculo, _, _) = await SembrarCatalogo("PJ-0005", 2, camion: false);
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var categoria = await Leer(cliente, $"/peajes/categoria/{vehiculo}");

        Assert.True(categoria.GetProperty("provisional").GetBoolean());
        Assert.Contains("Resuelta por", categoria.GetProperty("explicacion").GetString());
    }

    [Fact]
    public async Task Sin_tarifa_cargada_la_linea_no_vale_CERO_y_el_mensaje_es_accionable()
    {
        // `RN-34`: el sistema arranca sin tarifas cargadas (insumo #21). Un cero indistinguible
        // de un error es peor que la ausencia declarada.
        var (vehiculo, _, _) = await SembrarCatalogo("PJ-0006", 2, camion: false);
        var huerfano = await SembrarPuntoSinTarifa("Villa de San Antonio");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var e = await Estimar(cliente, vehiculo, [(huerfano, 2)]);

        Assert.Equal(JsonValueKind.Null, e.GetProperty("total").ValueKind);

        var fundamento = e.GetProperty("lineas")[0].GetProperty("fundamento").GetString();
        Assert.Contains("No hay tarifa vigente", fundamento);
        Assert.Contains("Gerencia Administrativa", fundamento);
    }

    [Fact]
    public async Task Un_vehiculo_EXONERADO_estima_cero_con_el_fundamento_visible()
    {
        var (vehiculo, zambrano, _) = await SembrarCatalogo("PJ-0007", 2, camion: false);
        await SembrarExoneracion(vehiculo, zambrano);

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var e = await Estimar(cliente, vehiculo, [(zambrano, 2)]);

        Assert.Equal(0m, e.GetProperty("total").GetDecimal());

        // Un cero sin explicación es indistinguible de un error de cálculo — `RN-35` punto 3.
        Assert.Contains("Exonerado: Convenio",
            e.GetProperty("lineas")[0].GetProperty("fundamento").GetString());
    }

    // ── El paso por caseta ──────────────────────────────────────────────────

    [Fact]
    public async Task Cobrar_con_otra_categoria_queda_como_DISCREPANCIA_y_no_cambia_la_del_vehiculo()
    {
        // El caso de la SAPP, exacto: liviano cobrado como vehículo de 2 ejes, L 90 por L 22.
        var (vehiculo, zambrano, _) = await SembrarCatalogo("PJ-0008", 2, camion: false);
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var paso = Ulid.NewUlid().ToString();

        await Post(cliente, "/peajes/pasos", new
        {
            Id = paso,
            IdPunto = zambrano,
            IdVehiculo = vehiculo,
            OcurridoEn = Momento,
            Odometro = 84_120,
            MontoPagado = 90m,
            Medio = "Efectivo",
            Registra = "P-MOTORISTA",
            CategoriaCobrada = "EJES-2",
            Ticket = "foto-ticket-0091.jpg",
        });

        var discrepancias = await Leer(cliente, "/peajes/discrepancias");
        var fila = discrepancias.EnumerateArray().Single(d =>
            d.GetProperty("id").GetString() == paso);

        Assert.True(fila.GetProperty("discrepancia").GetBoolean());
        Assert.Equal("Liviano/Turismo", fila.GetProperty("categoriaEsperada").GetString());
        Assert.Equal("Vehículo de 2 Ejes", fila.GetProperty("categoriaCobrada").GetString());
        Assert.Equal(68m, fila.GetProperty("diferencia").GetDecimal());

        // Y la categoría del vehículo sigue siendo la suya. Si se ajustara al cobro recibido,
        // el error de la caseta se volvería la verdad institucional y el reclamo nunca
        // ocurriría.
        var categoria = await Leer(cliente, $"/peajes/categoria/{vehiculo}");
        Assert.Equal("LIVIANO", categoria.GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task La_discrepancia_sin_ticket_exige_causa()
    {
        var (vehiculo, zambrano, _) = await SembrarCatalogo("PJ-0009", 2, camion: false);
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync("/peajes/pasos", new
        {
            Id = Ulid.NewUlid().ToString(),
            IdPunto = zambrano,
            IdVehiculo = vehiculo,
            OcurridoEn = Momento,
            Odometro = 84_120,
            MontoPagado = 90m,
            Medio = "Efectivo",
            Registra = "P-MOTORISTA",
            CategoriaCobrada = "EJES-2",
        });

        Assert.False(respuesta.IsSuccessStatusCode);
        Assert.Contains("la palabra del motorista",
            await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Un_paso_NORMAL_sin_ticket_se_registra_sin_pedir_nada()
    {
        // La caseta a veces no da ticket, y un paso sin discrepancia no tiene por qué
        // justificarse.
        var (vehiculo, zambrano, _) = await SembrarCatalogo("PJ-0010", 2, camion: false);
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var paso = Ulid.NewUlid().ToString();

        await Post(cliente, "/peajes/pasos", new
        {
            Id = paso,
            IdPunto = zambrano,
            IdVehiculo = vehiculo,
            IdMision = (string?)null,
            OcurridoEn = Momento,
            Odometro = 84_120,
            MontoPagado = 22m,
            Medio = "Efectivo",
            Registra = "P-MOTORISTA",
            CategoriaCobrada = "LIVIANO",
        });

        var discrepancias = await Leer(cliente, "/peajes/discrepancias");
        Assert.DoesNotContain(discrepancias.EnumerateArray(), d =>
            d.GetProperty("id").GetString() == paso);
    }

    [Fact]
    public async Task El_paso_reenviado_por_el_dispositivo_NO_se_duplica()
    {
        // El paso se captura sin conectividad (`RN-43`) y el dispositivo reintenta. Un paso
        // duplicado infla el gasto y produce una discrepancia inventada por el sistema.
        var (vehiculo, zambrano, _) = await SembrarCatalogo("PJ-0011", 2, camion: false);
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var captura = Ulid.NewUlid().ToString();
        var mision = Ulid.NewUlid().ToString();

        object Cuerpo() => new
        {
            Id = Ulid.NewUlid().ToString(),
            IdPunto = zambrano,
            IdVehiculo = vehiculo,
            IdMision = mision,
            OcurridoEn = Momento,
            Odometro = 84_120,
            MontoPagado = 22m,
            Medio = "Efectivo",
            Registra = "P-MOTORISTA",
            IdDeCaptura = captura,
        };

        await Post(cliente, "/peajes/pasos", Cuerpo());
        await Post(cliente, "/peajes/pasos", Cuerpo());

        var pasos = await Leer(cliente, $"/peajes/pasos/mision/{mision}");
        Assert.Single(pasos.EnumerateArray());
    }

    [Fact]
    public async Task Un_paso_por_un_punto_NO_CATALOGADO_se_registra_con_su_ubicacion()
    {
        // `NRM-10` menciona casetas antiguas en San Pedro Sula sin verificar si operan `[C]`.
        // Descartar el paso perdería el gasto y la evidencia de que la caseta existe.
        var (vehiculo, _, _) = await SembrarCatalogo("PJ-0012", 2, camion: false);
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var mision = Ulid.NewUlid().ToString();

        await Post(cliente, "/peajes/pasos", new
        {
            Id = Ulid.NewUlid().ToString(),
            IdPunto = (string?)null,
            IdVehiculo = vehiculo,
            IdMision = mision,
            OcurridoEn = Momento,
            Odometro = 84_500,
            MontoPagado = 30m,
            Medio = "Efectivo",
            Registra = "P-MOTORISTA",
            PuntoNoCatalogado = true,
            UbicacionDeclarada = "Salida norte de San Pedro Sula, antes del desvío a Choloma.",
        });

        var pasos = await Leer(cliente, $"/peajes/pasos/mision/{mision}");
        var p = pasos.EnumerateArray().Single();

        Assert.True(p.GetProperty("puntoNoCatalogado").GetBoolean());
        Assert.Contains("Choloma", p.GetProperty("ubicacion").GetString());
        Assert.Equal(30m, p.GetProperty("montoPagado").GetDecimal());
    }

    [Fact]
    public async Task Sin_ubicacion_el_punto_no_catalogado_se_rechaza()
    {
        var (vehiculo, _, _) = await SembrarCatalogo("PJ-0013", 2, camion: false);
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync("/peajes/pasos", new
        {
            Id = Ulid.NewUlid().ToString(),
            IdPunto = (string?)null,
            IdVehiculo = vehiculo,
            OcurridoEn = Momento,
            Odometro = 84_500,
            MontoPagado = 30m,
            Medio = "Efectivo",
            Registra = "P-MOTORISTA",
            PuntoNoCatalogado = true,
        });

        Assert.False(respuesta.IsSuccessStatusCode);
        Assert.Contains("depurar el catálogo", await respuesta.Content.ReadAsStringAsync());
    }

    // ── Andamio ─────────────────────────────────────────────────────────────

    /// <summary>Siembra un vehículo, dos puntos con tarifa y la matriz de derivación.</summary>
    private async Task<(string Vehiculo, string Zambrano, string Comayagua)> SembrarCatalogo(
        string prefijo, int? ejes, bool camion)
    {
        await using var contexto = baseDePruebas.Contexto();

        var r = await FlotaSembrada.ParaProgramarAsync(contexto, prefijo);
        var vehiculo = Ulid.Parse(r.Vehiculo);

        // La siembra da un pick-up. Para el caso del camión se planta otro vehículo con su
        // ficha, en vez de mutar la del pick-up: dos pruebas que comparten base no pueden
        // reescribirse la flota entre ellas.
        if (camion)
        {
            vehiculo = Ulid.NewUlid();

            contexto.Vehiculos.Add(FlotaSembrada.Vehiculo(
                vehiculo, $"{prefijo}-CAM-{vehiculo.ToString()[^5..]}", null,
                "Camión de carga", ClaseNormativa.Camion, 12_000, 3, remolque: false,
                ejes: ejes));
        }
        else
        {
            var fila = await contexto.Vehiculos.SingleAsync(v => v.Id == vehiculo);
            fila.NumeroDeEjes = ejes;
        }

        var ahora = DateTime.UtcNow;

        if (!await contexto.CategoriasDePeaje.AnyAsync())
        {
            contexto.CategoriasDePeaje.AddRange(
                new FilaDeCategoriaDePeaje { Codigo = "LIVIANO", Nombre = "Liviano/Turismo" },
                new FilaDeCategoriaDePeaje { Codigo = "EJES-2", Nombre = "Vehículo de 2 Ejes" });

            contexto.ReglasDeCategoriaDePeaje.AddRange(
                new FilaDeReglaDeCategoria
                {
                    Id = Ulid.NewUlid(), Categoria = "LIVIANO", Prioridad = 20,
                    Fundamento = "Clasificación general de automóviles livianos por peso bruto.",
                    Clase = ClaseNormativa.Automovil, PesoBrutoHastaKg = 3_500,
                    VigenteDesde = new DateOnly(2025, 1, 1), RegistradoDesdeUtc = ahora,
                },
                new FilaDeReglaDeCategoria
                {
                    Id = Ulid.NewUlid(), Categoria = "EJES-2", Prioridad = 30,
                    Fundamento = "Vehículo de carga de 2 ejes.",
                    Clase = ClaseNormativa.Camion, EjesDesde = 2, EjesHasta = 2,
                    VigenteDesde = new DateOnly(2025, 1, 1), RegistradoDesdeUtc = ahora,
                });
        }

        var zambrano = await PuntoConTarifa(contexto, "Zambrano", ahora);
        var comayagua = await PuntoConTarifa(contexto, "Comayagua", ahora);

        await contexto.SaveChangesAsync();

        return (vehiculo.ToString(), zambrano.ToString(), comayagua.ToString());
    }

    private static async Task<Ulid> PuntoConTarifa(
        SigtiDbContext contexto, string nombre, DateTime ahora)
    {
        var existente = await contexto.PuntosDePeaje
            .SingleOrDefaultAsync(p => p.Nombre == nombre);

        if (existente is not null) return existente.Id;

        var id = Ulid.NewUlid();

        contexto.PuntosDePeaje.Add(new FilaDePunto
        {
            Id = id, Nombre = nombre, Operador = "COVI-H", Carretera = "CA-5 Norte",
        });

        contexto.VigenciasDePunto.Add(new FilaDeVigenciaDelPunto
        {
            Id = Ulid.NewUlid(), PuntoId = id, Estado = Dominio.M18_Peajes.EstadoDelPunto.Activo,
            Fundamento = "Concesión vigente.",
            VigenteDesde = new DateOnly(2025, 1, 1), RegistradoDesdeUtc = ahora,
        });

        contexto.TarifasDePeaje.AddRange(
            new FilaDeTarifa
            {
                Id = Ulid.NewUlid(), PuntoId = id, Categoria = "LIVIANO", Monto = 22m,
                Fuente = "SAPP", FechaDeVerificacion = new DateOnly(2026, 3, 1),
                VigenteDesde = new DateOnly(2026, 1, 1), RegistradoDesdeUtc = ahora,
            },
            new FilaDeTarifa
            {
                Id = Ulid.NewUlid(), PuntoId = id, Categoria = "EJES-2", Monto = 90m,
                Fuente = "SAPP", FechaDeVerificacion = new DateOnly(2026, 3, 1),
                VigenteDesde = new DateOnly(2026, 1, 1), RegistradoDesdeUtc = ahora,
            });

        return id;
    }

    private async Task<string> SembrarPuntoSinTarifa(string nombre)
    {
        await using var contexto = baseDePruebas.Contexto();

        var existente = await contexto.PuntosDePeaje.SingleOrDefaultAsync(p => p.Nombre == nombre);
        if (existente is not null) return existente.Id.ToString();

        var id = Ulid.NewUlid();

        contexto.PuntosDePeaje.Add(new FilaDePunto
        {
            Id = id, Nombre = nombre, Operador = "COVI-H", Carretera = "CA-5 Norte",
        });

        contexto.VigenciasDePunto.Add(new FilaDeVigenciaDelPunto
        {
            Id = Ulid.NewUlid(), PuntoId = id, Estado = Dominio.M18_Peajes.EstadoDelPunto.Activo,
            Fundamento = "Concesión vigente.",
            VigenteDesde = new DateOnly(2025, 1, 1), RegistradoDesdeUtc = DateTime.UtcNow,
        });

        await contexto.SaveChangesAsync();
        return id.ToString();
    }

    private async Task SembrarExoneracion(string vehiculo, string punto)
    {
        await using var contexto = baseDePruebas.Contexto();

        contexto.ExoneracionesDePeaje.Add(new FilaDeExoneracion
        {
            Id = Ulid.NewUlid(),
            VehiculoId = Ulid.Parse(vehiculo),
            PuntoId = Ulid.Parse(punto),
            Fundamento = "Convenio SAPP-INM 2026-004 para unidades de rescate.",
            VigenteDesde = new DateOnly(2026, 1, 1),
            RegistradoDesdeUtc = DateTime.UtcNow,
        });

        await contexto.SaveChangesAsync();
    }

    private static async Task<JsonElement> Estimar(
        HttpClient cliente, string vehiculo, (string Punto, int Cruces)[] cruces)
    {
        var respuesta = await cliente.PostAsJsonAsync("/peajes/estimacion", new
        {
            Cruces = cruces.Select(c => new { IdPunto = c.Punto, c.Cruces }),
            IdVehiculo = vehiculo,
            CategoriaDelTipo = (string?)null,
            TipoDeVehiculo = (string?)null,
            FechaPrevista = Salida,
        });

        if (!respuesta.IsSuccessStatusCode)
            throw new Xunit.Sdk.XunitException(
                $"POST /peajes/estimacion devolvió {(int)respuesta.StatusCode}: " +
                await respuesta.Content.ReadAsStringAsync());

        return JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync()).RootElement.Clone();
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
        var respuesta = await cliente.PostAsJsonAsync(ruta, cuerpo);

        if (!respuesta.IsSuccessStatusCode)
            throw new Xunit.Sdk.XunitException(
                $"POST {ruta} devolvió {(int)respuesta.StatusCode}: " +
                await respuesta.Content.ReadAsStringAsync());
    }
}
