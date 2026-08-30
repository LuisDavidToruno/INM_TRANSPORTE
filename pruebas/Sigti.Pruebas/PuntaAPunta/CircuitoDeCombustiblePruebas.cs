using System.Net;
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
/// El circuito del dinero, de punta a punta — `RN-26`, `RN-27`, `RN-32` y §10.1.
///
/// ── Por qué esto no se puede probar sólo en el dominio ───────────────────────
/// Porque las tres comprobaciones que más importan <b>cruzan agregados</b>: el saldo sale del
/// fondo, el receptor sale de la reserva de la orden, y el momento de entregar sale del estado
/// de la misión. El dominio las tiene, pero <b>quien las conecta es el servicio</b> — y un
/// servicio que le pase el dato equivocado deja la regla comparando algo consigo mismo, que es
/// exactamente el defecto que estas pruebas existen para atrapar.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class CircuitoDeCombustiblePruebas(BaseDePruebas baseDePruebas)
{
    // Antes de la ventana del 16 al 18: aprobar el mismo día en que la misión sale hace
    // caducar la aprobación. Y dentro de marzo, que es el período del fondo.
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

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
    public async Task El_recorrido_completo_del_dinero_desde_el_fondo_hasta_la_conciliacion()
    {
        var r = await Sembrar("CB-0001");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var fondo = await FondoAprobado(cliente, 40_000m);
        var mision = await MisionDespachada(cliente, r);

        var vale = await Emitir(cliente, fondo, mision, r, monto: 2_500m);

        await EntregarYSalir(cliente, vale, mision);

        await Post(cliente, $"/combustible/{vale}/consumo", new
        {
            Ejecuta = "P-MOTORISTA",
            Galones = 30m,
            Monto = 1_500m,
            Estacion = "Estación Uno, Choluteca",
            Odometro = 84_120,
            Comprobante = "F-0011-9932",
            Momento,
        });

        await Post(cliente, $"/combustible/{vale}/liquidar", new
        {
            Ejecuta = "P-CONTABILIDAD",
            SaldoDevuelto = 1_000m,
            Observacion = (string?)null,
            Momento,
        });

        await Post(cliente, $"/combustible/{vale}/conciliar", new
        {
            Ejecuta = "P-AUDITORIA",
            DentroDeUmbral = true,
            Dictamen = "18.5 km/galón contra 17 esperado.",
            Momento,
        });

        var vales = await cliente.GetFromJsonAsync<JsonElement>($"/combustible/mision/{mision}");
        var uno = vales.EnumerateArray().Single();

        Assert.Equal("Conciliada", uno.GetProperty("estado").GetString());
        Assert.Equal(1_500m, uno.GetProperty("consumido").GetDecimal());
        Assert.Equal(1_000m, uno.GetProperty("devuelto").GetDecimal());

        // Cinco actos, cinco personas. El diario es donde se comprueba que `BD-06` ocurrió.
        var diario = uno.GetProperty("diario").EnumerateArray().ToList();
        Assert.Equal(["V-01", "V-02", "V-04", "V-07", "V-09"],
            diario.Select(t => t.GetProperty("transicion").GetString()));
        Assert.Equal(5, diario.Select(t => t.GetProperty("ejecuta").GetString()).Distinct().Count());
    }

    [Fact]
    public async Task El_saldo_del_fondo_BAJA_al_emitir_y_VUELVE_al_devolver_lo_no_consumido()
    {
        // El saldo es la resta sobre asientos de `RN-26`. Que baje es la mitad fácil; que
        // vuelva lo devuelto es la que sostiene el cuadre del período.
        var r = await Sembrar("CB-0002");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var fondo = await FondoAprobado(cliente, 40_000m);
        Assert.Equal(40_000m, await Saldo(cliente, fondo));

        var mision = await MisionDespachada(cliente, r);
        var vale = await Emitir(cliente, fondo, mision, r, monto: 2_500m);

        Assert.Equal(37_500m, await Saldo(cliente, fondo));

        await EntregarYSalir(cliente, vale, mision);

        await Post(cliente, $"/combustible/{vale}/consumo", new
        {
            Ejecuta = "P-MOTORISTA", Galones = 30m, Monto = 1_500m,
            Estacion = "Estación Uno", Odometro = 84_120, Comprobante = "F-1", Momento,
        });
        await Post(cliente, $"/combustible/{vale}/liquidar", new
        {
            Ejecuta = "P-CONTABILIDAD", SaldoDevuelto = 1_000m, Observacion = (string?)null, Momento,
        });

        // 40,000 − 2,500 asignados + 1,000 devueltos constatados = 38,500.
        Assert.Equal(38_500m, await Saldo(cliente, fondo));
    }

    [Fact]
    public async Task Un_vale_anulado_devuelve_TODO_su_valor_al_fondo()
    {
        // `RN-27` punto 4: el valor retorna al saldo **solo si no fue canjeado**. Un vale
        // anulado que siguiera comiendo saldo haría que un mes con misiones desprogramadas
        // apareciera sin fondo teniendo el dinero intacto.
        var r = await Sembrar("CB-0003");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var fondo = await FondoAprobado(cliente, 40_000m);
        var mision = await MisionDespachada(cliente, r);
        var vale = await Emitir(cliente, fondo, mision, r, monto: 2_500m);

        Assert.Equal(37_500m, await Saldo(cliente, fondo));

        await Post(cliente, $"/combustible/{vale}/anular", new
        {
            Ejecuta = "P-TRANSPORTE",
            Motivo = "Misión desprogramada. Acta 2026-041.",
            Momento,
        });

        Assert.Equal(40_000m, await Saldo(cliente, fondo));
    }

    [Fact]
    public async Task Sin_saldo_no_se_emite_y_el_mensaje_dice_cuanto_falta()
    {
        var r = await Sembrar("CB-0004");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var fondo = await FondoAprobado(cliente, 2_000m);
        var mision = await MisionDespachada(cliente, r);

        var respuesta = await Emitir(cliente, fondo, mision, r, monto: 5_000m, esperarExito: false);

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("RN-26", cuerpo);
        Assert.Contains("Faltan 3,000.00", cuerpo);
    }

    [Fact]
    public async Task El_vale_NO_lo_recibe_otro_motorista()
    {
        // **La prueba que atrapó el defecto real.** El servicio pasaba el motorista de la orden
        // a los dos lados de `RN-32`, así que la regla comparaba algo consigo mismo y el bloqueo
        // no podía disparar nunca. Sólo se ve cruzando el servicio: en el dominio la regla
        // siempre estuvo bien.
        var r = await Sembrar("CB-0005");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var fondo = await FondoAprobado(cliente, 40_000m);
        var mision = await MisionDespachada(cliente, r);

        var respuesta = await cliente.PostAsJsonAsync("/combustible", new
        {
            Id = Ulid.NewUlid().ToString(),
            Folio = $"VAL-{Ulid.NewUlid()}",
            IdFondo = fondo,
            IdMision = mision,

            // Otro motorista del padrón, no el de la orden.
            IdMotoristaReceptor = Ulid.NewUlid().ToString(),

            Ejecuta = "P-TRANSPORTE",
            Monto = 2_500m,
            Galones = 50m,
            Instrumento = "vale",
            TipoDeCombustible = "Diesel",
            Momento,
        });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("RN-32", cuerpo);
        // Y dice por dónde se cambia: la sustitución revalida licencia, que es la razón de que
        // sea el único camino.
        Assert.Contains("RN-14", cuerpo);
    }

    [Fact]
    public async Task El_vale_NO_puede_ser_de_otro_combustible_que_el_del_vehiculo()
    {
        // **El hermano del defecto de arriba, y del mismo tipo.** `RN-32` caso límite dice que
        // «un vale de diésel para un vehículo de gasolina es un error caro y perfectamente
        // evitable». La regla existía, era correcta y tenía sus pruebas de dominio — y el
        // servicio la llamaba **siempre con nulo**, porque la ficha del vehículo no declaraba
        // qué combustible usa. El bloqueo no podía disparar nunca.
        //
        // No es fraude, es desperdicio: el vale se anula y se reemite, y para entonces la
        // misión ya salió tarde. O peor, alguien lo carga y arruina el motor.
        var r = await Sembrar("CB-0011");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var fondo = await FondoAprobado(cliente, 40_000m);
        var mision = await MisionDespachada(cliente, r);

        var respuesta = await cliente.PostAsJsonAsync("/combustible", new
        {
            Id = Ulid.NewUlid().ToString(),
            Folio = $"VAL-{Ulid.NewUlid()}",
            IdFondo = fondo,
            IdMision = mision,
            IdMotoristaReceptor = r.Conductor,
            Ejecuta = "P-TRANSPORTE",
            Monto = 2_500m,
            Galones = 50m,
            Instrumento = "vale",

            // La flota sembrada es de diésel.
            TipoDeCombustible = "Gasolina superior",

            Momento,
        });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);

        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("RN-32", cuerpo);

        // El mensaje nombra los dos: sin eso, quien lo lee tiene que ir a averiguar cuál de los
        // dos está mal.
        Assert.Contains("Diesel", cuerpo);
        Assert.Contains("Gasolina superior", cuerpo);
    }

    /// <summary>
    /// Y el vale del combustible correcto sí sale — <b>la otra mitad</b>.
    ///
    /// Un bloqueo que rechaza todo se ve igual que uno que funciona hasta que alguien intenta
    /// el caso bueno.
    /// </summary>
    [Fact]
    public async Task El_vale_del_combustible_correcto_si_se_emite()
    {
        var r = await Sembrar("CB-0012");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var fondo = await FondoAprobado(cliente, 40_000m);
        var mision = await MisionDespachada(cliente, r);

        // El ayudante ya exige exito: si el bloqueo disparara de mas, esto revienta aca.
        var vale = await Emitir(cliente, fondo, mision, r, monto: 2_500m);

        Assert.False(string.IsNullOrWhiteSpace(vale));
    }

    [Fact]
    public async Task No_se_entrega_fondo_a_una_mision_que_solo_esta_PROGRAMADA()
    {
        // `EF-04` y §10.1: `V-02` ocurre **dentro de** `T-12`. `PROGRAMADA` lista expresamente
        // «Entregar fondo de combustible» entre lo que no se puede.
        var r = await Sembrar("CB-0006");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var fondo = await FondoAprobado(cliente, 40_000m);
        var mision = await MisionProgramada(cliente, r);
        var vale = await Emitir(cliente, fondo, mision, r, monto: 2_500m);

        var respuesta = await cliente.PostAsJsonAsync($"/combustible/{vale}/entregar", new
        {
            Ejecuta = "P-COMBUSTIBLE", Constancia = "Firma", Momento,
        });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Contains("no despachada", await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task La_mision_NO_se_liquida_con_un_vale_vivo()
    {
        // `INV-34` cruzando las dos máquinas. Es la regla de acoplamiento que hace que el
        // expediente no se pueda declarar cuadrado mientras haya dinero sin descargar.
        var r = await Sembrar("CB-0007");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var fondo = await FondoAprobado(cliente, 40_000m);
        var mision = await MisionDespachada(cliente, r);
        var vale = await Emitir(cliente, fondo, mision, r, monto: 2_500m);

        await EntregarYSalir(cliente, vale, mision);

        await Post(cliente, $"/misiones/{mision}/retornar", new
        {
            Ejecuta = "P-MOTORISTA", Momento, Odometro = 84_420,
        });

        var respuesta = await cliente.PostAsJsonAsync($"/misiones/{mision}/liquidar", new
        {
            Ejecuta = "P-TRANSPORTE", Momento,
        });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("INV-34", cuerpo);
        Assert.Contains("sin liquidar", cuerpo);
    }

    [Fact]
    public async Task El_fondo_NO_cierra_con_vales_vivos_y_SI_cierra_cuando_se_resuelven()
    {
        var r = await Sembrar("CB-0008");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var fondo = await FondoAprobado(cliente, 40_000m);
        var mision = await MisionDespachada(cliente, r);
        var vale = await Emitir(cliente, fondo, mision, r, monto: 2_500m);

        var rechazo = await cliente.PostAsJsonAsync($"/fondos/{fondo}/cerrar", new
        {
            Ejecuta = "P-CONTABILIDAD", Partida = "12-01-001-4-31200", Momento,
        });

        Assert.Equal(HttpStatusCode.Conflict, rechazo.StatusCode);
        Assert.Contains("sin descargo", await rechazo.Content.ReadAsStringAsync());

        await Post(cliente, $"/combustible/{vale}/anular", new
        {
            Ejecuta = "P-TRANSPORTE", Motivo = "Misión desprogramada. Acta 2026-042.", Momento,
        });

        await Post(cliente, $"/fondos/{fondo}/cerrar", new
        {
            Ejecuta = "P-CONTABILIDAD", Partida = "12-01-001-4-31200", Momento,
        });
    }

    [Fact]
    public async Task Quien_aprobo_el_fondo_no_lo_cierra()
    {
        // `RN-26.4`, la mitad que se olvida: separar pedir de autorizar no sirve si el mismo
        // que autorizó declara al final que todo cuadró.
        var r = await Sembrar("CB-0009");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var fondo = await FondoAprobado(cliente, 40_000m);

        var respuesta = await cliente.PostAsJsonAsync($"/fondos/{fondo}/cerrar", new
        {
            Ejecuta = "P-GERENCIA", Partida = "12-01-001-4-31200", Momento,
        });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Contains("no es quien declara que el gasto cuadró",
            await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Un_consumo_reenviado_por_el_dispositivo_no_se_cuenta_dos_veces()
    {
        // `V-04` se ejecuta sin conectividad y el dispositivo reintenta hasta que le contesten.
        // Un galón contado dos veces inventa una desviación de conciliación que nadie va a
        // poder explicar — y la unicidad la impone la BASE, no una comprobación que se olvida.
        var r = await Sembrar("CB-0010");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var fondo = await FondoAprobado(cliente, 40_000m);
        var mision = await MisionDespachada(cliente, r);
        var vale = await Emitir(cliente, fondo, mision, r, monto: 2_500m);

        await EntregarYSalir(cliente, vale, mision);

        var captura = Ulid.NewUlid().ToString();
        object consumo = new
        {
            Ejecuta = "P-MOTORISTA", Galones = 30m, Monto = 1_500m,
            Estacion = "Estación Uno", Odometro = 84_120, Comprobante = "F-1",
            Momento, IdDeCaptura = captura,
        };

        await Post(cliente, $"/combustible/{vale}/consumo", consumo);

        // El reenvío. Que falle es aceptable; lo que NO es aceptable es que sume otra vez.
        try { await cliente.PostAsJsonAsync($"/combustible/{vale}/consumo", consumo); }
        catch { /* la base rechaza el duplicado, que es exactamente lo que se busca */ }

        var vales = await cliente.GetFromJsonAsync<JsonElement>($"/combustible/mision/{mision}");
        var uno = vales.EnumerateArray().Single();

        Assert.Equal(1_500m, uno.GetProperty("consumido").GetDecimal());
        Assert.Equal(30m, uno.GetProperty("galonesConsumidos").GetDecimal());
    }

    // ── Andamio ─────────────────────────────────────────────────────────────

    private async Task<FlotaSembrada.ParaProgramar> Sembrar(string prefijo)
    {
        await using var contexto = baseDePruebas.Contexto();
        return await FlotaSembrada.ParaProgramarAsync(contexto, prefijo);
    }

    private static async Task<decimal> Saldo(HttpClient cliente, string fondoId)
    {
        var fondos = await cliente.GetFromJsonAsync<JsonElement>("/fondos");
        return fondos.EnumerateArray()
            .Single(f => f.GetProperty("id").GetString() == fondoId)
            .GetProperty("saldo").GetDecimal();
    }

    private static async Task<string> FondoAprobado(HttpClient cliente, decimal monto)
    {
        var id = Ulid.NewUlid().ToString();

        await Post(cliente, "/fondos", new
        {
            Id = id,
            Ambito = "Dependencia",
            AmbitoDeclarado = "Delegacion de Choluteca",
            Desde = new DateOnly(2026, 3, 1),
            Hasta = new DateOnly(2026, 3, 31),
            Solicita = "P-TRANSPORTE",
            Monto = monto,
            Justificacion = "Operación ordinaria de marzo.",
            Momento,
        });

        await Post(cliente, $"/fondos/{id}/aprobar", new
        {
            Ejecuta = "P-GERENCIA", Monto = monto, Partida = "12-01-001-4-31200", Momento,
        });

        return id;
    }

    private static async Task<string> MisionProgramada(
        HttpClient cliente, FlotaSembrada.ParaProgramar r)
    {
        var id = Ulid.NewUlid().ToString();

        await Post(cliente, "/misiones", new
        {
            Id = id,
            CapturadaPor = "P-ASISTENTE",
            SolicitanteDeDerecho = "P-ASISTENTE",
            // Tiene que coincidir con el ámbito del fondo: `RN-26` no deja imputar una misión
            // al fondo de otra delegación.
            Dependencia = "Delegacion de Choluteca",
            ObjetoDelTraslado = "Traslado de personal",
            Destino = "Choluteca",
            Salida = new DateOnly(2026, 3, 16),
            Retorno = new DateOnly(2026, 3, 18),
            HoraDeSalida = new TimeOnly(8, 0),
            HoraDeRetorno = new TimeOnly(16, 0),
            HolguraDias = 0,
            Momento,
        });

        await Post(cliente, $"/misiones/{id}/enviar", new { Ejecuta = "P-ASISTENTE", Momento });
        await Post(cliente, $"/misiones/{id}/aprobar", new { Ejecuta = "P-JEFATURA", Momento });
        await Post(cliente, $"/misiones/{id}/programar", new
        {
            Ejecuta = "P-TRANSPORTE", Momento, IdVehiculo = r.Vehiculo, IdConductor = r.Conductor,
        });

        return id;
    }

    private static async Task<string> MisionDespachada(
        HttpClient cliente, FlotaSembrada.ParaProgramar r)
    {
        var id = await MisionProgramada(cliente, r);

        await Post(cliente, $"/misiones/{id}/despachar", new
        {
            Ejecuta = "P-DESPACHO", Momento, IdVehiculo = r.Vehiculo, IdConductor = r.Conductor,
        });

        return id;
    }

    private static async Task<string> Emitir(
        HttpClient cliente, string fondo, string mision,
        FlotaSembrada.ParaProgramar r, decimal monto)
    {
        var respuesta = await Emitir(cliente, fondo, mision, r, monto, esperarExito: true);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
        return cuerpo.GetProperty("id").GetString()!;
    }

    private static async Task<HttpResponseMessage> Emitir(
        HttpClient cliente, string fondo, string mision,
        FlotaSembrada.ParaProgramar r, decimal monto, bool esperarExito)
    {
        var respuesta = await cliente.PostAsJsonAsync("/combustible", new
        {
            Id = Ulid.NewUlid().ToString(),
            Folio = $"VAL-{Ulid.NewUlid()}",
            IdFondo = fondo,
            IdMision = mision,
            IdMotoristaReceptor = r.Conductor,
            Ejecuta = "P-TRANSPORTE",
            Monto = monto,
            Galones = 50m,
            Instrumento = "vale",
            TipoDeCombustible = "Diesel",
            Momento,
        });

        if (esperarExito) respuesta.EnsureSuccessStatusCode();
        return respuesta;
    }

    /// <summary>
    /// El vale entregado y la misión <b>en ruta</b> — el único estado en que `V-04` existe.
    ///
    /// Se llama acá y no en el andamio de la misión porque `V-02` ocurre <b>dentro</b> del
    /// despacho y `T-14` es posterior: si el andamio dejara la misión en ruta desde el
    /// principio, la prueba que comprueba que no se entrega a una misión sin despachar
    /// dejaría de poder escribirse.
    /// </summary>
    private static async Task EntregarYSalir(HttpClient cliente, string vale, string mision)
    {
        await Post(cliente, $"/combustible/{vale}/entregar", new
        {
            Ejecuta = "P-COMBUSTIBLE", Constancia = "Firma de recepción", Momento,
        });

        await Post(cliente, $"/misiones/{mision}/iniciar-ruta", new
        {
            Ejecuta = "P-MOTORISTA", Momento, Odometro = 84_000,
        });
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
