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
/// `RN-62` cableada — el título de tenencia.
///
/// ── Lo que cierra ───────────────────────────────────────────────────────────
/// La corrección `HB3-17` <b>ya puede juzgar</b>: hasta que el título existió, la verificación de
/// que el descargo sea de bienes propios y el retiro de ajenos siempre advertía, porque el
/// régimen de tenencia no estaba en ninguna parte.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class TitulosDeTenenciaPruebas(BaseDePruebas baseDePruebas)
{
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

    private WebApplicationFactory<Program> Aplicacion() => FabricaDeSigti.Crear(baseDePruebas);

    /// <summary>
    /// <b>La prueba que cierra `HB3-17`.</b>
    ///
    /// §10.2: declarar <i>«dado de baja del registro de bienes del Estado»</i> un vehículo en
    /// comodato es <i>«un asiento falso sobre un bien ajeno, detectable cruzando el inventario
    /// institucional contra el padrón de flota»</i>. Con el título registrado, el sistema ya no
    /// advierte: <b>bloquea</b>.
    /// </summary>
    [Fact]
    public async Task Un_vehiculo_en_comodato_NO_se_da_de_baja_del_registro_de_bienes()
    {
        var r = await Sembrar("TT-0001");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        await RegistrarTitulo(cliente, r.Vehiculo, "Comodato", hasta: new DateOnly(2026, 12, 31));

        // El vehículo se inhabilita primero: `W-11` no es lo que esta prueba juzga.
        await Declarar(cliente, r.Vehiculo, "NoDisponible", "Fin del comodato, pendiente devolver");

        var descargo = await cliente.PostComoAsync($"/flota/{r.Vehiculo}/estado", new
        {
            Ejecuta = "P-GERENCIA",
            Estado = "DadoDeBaja",
            Momento,
            Motivo = "Acta de descargo 2026-11",
        });

        Assert.Equal(HttpStatusCode.Conflict, descargo.StatusCode);

        var mensaje = await descargo.Content.ReadAsStringAsync();
        Assert.Contains("asiento falso sobre un bien ajeno", mensaje);
        Assert.Contains("RETIRADO_DE_FLOTA", mensaje);

        // Y el terminal que sí corresponde pasa.
        await Declarar(
            cliente, r.Vehiculo, "RetiradoDeFlota", "Acta de devolución de comodato 2026-12");
    }

    /// <summary>
    /// El recíproco: un bien <b>propio</b> no se retira de flota. Sin él, la prueba anterior
    /// seguiría en verde aunque el sistema bloqueara los dos terminales por igual.
    /// </summary>
    [Fact]
    public async Task Un_bien_propio_NO_se_retira_de_flota()
    {
        var r = await Sembrar("TT-0002");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        await RegistrarTitulo(cliente, r.Vehiculo, "Propiedad", hasta: null);
        await Declarar(cliente, r.Vehiculo, "NoDisponible", "En trámite de descargo");

        var retiro = await cliente.PostComoAsync($"/flota/{r.Vehiculo}/estado", new
        {
            Ejecuta = "P-GERENCIA",
            Estado = "RetiradoDeFlota",
            Momento,
            Motivo = "Acta de devolución",
        });

        Assert.Equal(HttpStatusCode.Conflict, retiro.StatusCode);
        Assert.Contains("sale del registro por DESCARGO", await retiro.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// <b>Sin título registrado se advierte, no se bloquea.</b> Frenar el descargo de toda la
    /// flota por un dato de alta que nadie llenó sería peor que el asiento que se quiere evitar.
    /// </summary>
    [Fact]
    public async Task Sin_titulo_el_terminal_pasa_con_advertencia()
    {
        var r = await Sembrar("TT-0003");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        await Declarar(cliente, r.Vehiculo, "NoDisponible", "Alta en flota");

        var respuesta = await cliente.PostComoAsync($"/flota/{r.Vehiculo}/estado", new
        {
            Ejecuta = "P-GERENCIA",
            Estado = "DadoDeBaja",
            Momento,
            Motivo = "Acta de descargo 2026-14",
        });

        respuesta.EnsureSuccessStatusCode();

        var cuerpo = await respuesta.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Contains("régimen de tenencia del vehículo no está declarado",
            cuerpo.GetProperty("advertencia").GetString());
    }

    /// <summary>
    /// `RN-62` — <b>ninguna misión se programa si su ventana excede la vigencia del título</b>,
    /// con el mismo patrón de `RN-10`.
    ///
    /// No alcanza con que el título esté vigente el día de la salida: un comodato que vence el 20
    /// no ampara una misión que vuelve el 22.
    /// </summary>
    [Fact]
    public async Task Una_mision_que_vuelve_despues_del_vencimiento_no_se_programa()
    {
        var r = await Sembrar("TT-0004");
        var id = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        // El comodato vence el 17 de marzo; la misión sale el 16 y vuelve el 18.
        await RegistrarTitulo(cliente, r.Vehiculo, "Comodato", hasta: new DateOnly(2026, 3, 17));

        await CrearYAprobar(cliente, id);

        var respuesta = await cliente.PostComoAsync($"/misiones/{id}/programar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            IdVehiculo = r.Vehiculo,
            IdConductor = r.Conductor,
        });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);

        var mensaje = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("excede la vigencia del título", mensaje);
        Assert.Contains("tiene que cubrir todo el rango", mensaje);
    }

    /// <summary>
    /// El recíproco. Sin él, `RN-62` podría estar bloqueando toda programación y la prueba
    /// anterior seguiría en verde.
    /// </summary>
    [Fact]
    public async Task Con_el_titulo_cubriendo_toda_la_ventana_la_mision_se_programa()
    {
        var r = await Sembrar("TT-0005");
        var id = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        await RegistrarTitulo(cliente, r.Vehiculo, "Comodato", hasta: new DateOnly(2026, 12, 31));

        await CrearYAprobar(cliente, id);

        var respuesta = await cliente.PostComoAsync($"/misiones/{id}/programar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            IdVehiculo = r.Vehiculo,
            IdConductor = r.Conductor,
        });

        respuesta.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Dos títulos vigentes a la vez dejarían al vehículo en dos regímenes al mismo tiempo, y la
    /// pregunta de si el bien es del Estado no tendría respuesta.
    /// </summary>
    [Fact]
    public async Task Dos_titulos_solapados_sobre_el_mismo_vehiculo_no_pasan()
    {
        var r = await Sembrar("TT-0006");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        await RegistrarTitulo(cliente, r.Vehiculo, "Comodato", hasta: new DateOnly(2026, 12, 31));

        var segundo = await cliente.PostComoAsync("/titulos", Cuerpo(
            r.Vehiculo, "Alquiler", new DateOnly(2026, 6, 1), new DateOnly(2027, 6, 1)));

        Assert.Equal(HttpStatusCode.Conflict, segundo.StatusCode);
        Assert.Contains("no tendría respuesta", await segundo.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// La ficha muestra régimen, titular, vigencia, días restantes y <b>la matriz de rubros</b> —
    /// `RN-62` punto 1. Lo que cubre el titular no se imputa a nuestro presupuesto.
    /// </summary>
    [Fact]
    public async Task La_serie_de_titulos_muestra_los_rubros_asumidos()
    {
        var r = await Sembrar("TT-0007");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        await Post(cliente, "/titulos", new
        {
            Id = Ulid.NewUlid().ToString(),
            IdVehiculo = r.Vehiculo,
            Regimen = "Alquiler",
            Titular = "Rentacar Honduras",
            Documento = "Contrato de alquiler RC-2026-88",
            Desde = new DateOnly(2026, 1, 1),
            Hasta = new DateOnly(2026, 12, 31),
            Combustible = "Institucion",
            Mantenimiento = "Titular",
            Llantas = "Titular",
            Seguro = "Titular",
            Peajes = "Institucion",
            Multas = "Institucion",
            Danios = "SinPactar",
        });

        var titulo = Assert.Single(
            (await Leer(cliente, $"/titulos/{r.Vehiculo}")).EnumerateArray());

        Assert.Equal("Alquiler", titulo.GetProperty("regimen").GetString());
        Assert.False(titulo.GetProperty("esBienPropio").GetBoolean());

        // Lo que cubre el titular: no se imputa al presupuesto de la institución.
        Assert.Equal(
            ["mantenimiento", "llantas", "seguro"],
            titulo.GetProperty("rubrosDelTitular").EnumerateArray()
                .Select(x => x.GetString()));

        // Y el que nadie pactó va nombrado, no supuesto de la institución.
        Assert.Equal(["daños"],
            titulo.GetProperty("rubrosSinPactar").EnumerateArray().Select(x => x.GetString()));
    }

    /// <summary>
    /// La cobertura sobre la flota — lo que contesta <b>cuántos controles están apagados</b>.
    ///
    /// ── Lo que esta prueba defiende ─────────────────────────────────────────
    /// <b>«Nunca tuvo título» y «se le venció» llegan las dos sin título vigente</b>, y son
    /// cosas opuestas: la primera es un dato de alta que nadie llenó; la segunda es un bien
    /// ajeno que ya debía haberse devuelto. Sin `ultimo` se ven iguales, y la que hay que
    /// ver queda escondida entre las que no.
    /// </summary>
    [Fact]
    public async Task La_cobertura_distingue_al_que_nunca_tuvo_del_que_se_le_vencio()
    {
        var sinNada = await Sembrar("TT-0008");
        var vencido = await Sembrar("TT-0009");
        var afuera = await Sembrar("TT-0010");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        // Un comodato que terminó el año pasado: hoy no rige ninguno.
        await Post(cliente, "/titulos", Cuerpo(
            vencido.Vehiculo, "Comodato", new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31)));

        // Y una unidad que ya salió de la flota, también sin título.
        await Declarar(cliente, afuera.Vehiculo, "NoDisponible", "Baja por obsolescencia");
        await Declarar(cliente, afuera.Vehiculo, "DadoDeBaja", "Acta de descargo 2026-20");

        var cobertura = (await Leer(cliente, "/titulos")).EnumerateArray()
            .ToDictionary(v => v.GetProperty("vehiculo").GetString()!);

        // Ninguno de los tres tiene título vigente…
        Assert.True(cobertura[sinNada.Vehiculo].GetProperty("titulo").ValueKind is JsonValueKind.Null);
        Assert.True(cobertura[vencido.Vehiculo].GetProperty("titulo").ValueKind is JsonValueKind.Null);

        // …y sin embargo NO son lo mismo.
        Assert.True(cobertura[sinNada.Vehiculo].GetProperty("ultimo").ValueKind is JsonValueKind.Null);
        Assert.Equal("Comodato",
            cobertura[vencido.Vehiculo].GetProperty("ultimo").GetProperty("regimen").GetString());

        // Y el que salió de la flota no cuenta como hueco: no le queda control que encender.
        Assert.False(cobertura[sinNada.Vehiculo].GetProperty("fueraDeLaFlota").GetBoolean());
        Assert.True(cobertura[afuera.Vehiculo].GetProperty("fueraDeLaFlota").GetBoolean());
    }

    /// <summary>
    /// De la serie manda <b>el que rige hoy</b>, no el último registrado. Un vehículo que va a
    /// pasar a propiedad el mes que viene sigue siendo ajeno hasta ese día.
    /// </summary>
    [Fact]
    public async Task La_cobertura_muestra_el_titulo_que_rige_hoy_y_no_el_ultimo_cargado()
    {
        var r = await Sembrar("TT-0011");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        await Post(cliente, "/titulos", Cuerpo(
            r.Vehiculo, "Comodato", hoy.AddYears(-1), hoy.AddMonths(1)));

        // La propiedad empieza cuando termina el comodato: hoy todavía no rige.
        await Post(cliente, "/titulos", Cuerpo(
            r.Vehiculo, "Propiedad", hoy.AddMonths(1).AddDays(1), null));

        var fila = (await Leer(cliente, "/titulos")).EnumerateArray()
            .Single(v => v.GetProperty("vehiculo").GetString() == r.Vehiculo);

        Assert.Equal("Comodato", fila.GetProperty("titulo").GetProperty("regimen").GetString());
        Assert.False(fila.GetProperty("titulo").GetProperty("esBienPropio").GetBoolean());
        Assert.Equal(2, fila.GetProperty("enLaSerie").GetInt32());
    }

    // ── Andamio ─────────────────────────────────────────────────────────────

    private static object Cuerpo(
        string vehiculo, string regimen, DateOnly desde, DateOnly? hasta) => new
        {
            Id = Ulid.NewUlid().ToString(),
            IdVehiculo = vehiculo,
            Regimen = regimen,
            Titular = regimen == "Propiedad" ? "Estado de Honduras" : "Secretaría de Salud",
            Documento = $"Documento {regimen} 2026",
            Desde = desde,
            Hasta = hasta,
            Combustible = "Institucion",
            Mantenimiento = "Institucion",
            Llantas = "Institucion",
            Seguro = "Institucion",
            Peajes = "Institucion",
            Multas = "Institucion",
            Danios = "Institucion",
        };

    private static Task RegistrarTitulo(
        HttpClient cliente, string vehiculo, string regimen, DateOnly? hasta) =>
        Post(cliente, "/titulos", Cuerpo(vehiculo, regimen, new DateOnly(2026, 1, 1), hasta));

    private static Task Declarar(
        HttpClient cliente, string vehiculo, string estado, string motivo) =>
        Post(cliente, $"/flota/{vehiculo}/estado", new
        {
            Ejecuta = "P-GERENCIA",
            Estado = estado,
            Momento,
            Motivo = motivo,
        });

    private async Task<FlotaSembrada.ParaProgramar> Sembrar(string prefijo)
    {
        await using var contexto = baseDePruebas.Contexto();
        return await FlotaSembrada.ParaProgramarAsync(contexto, prefijo);
    }

    /// <summary>Del 16 al 18 de marzo de 2026 — lunes a miércoles, sin días inhábiles.</summary>
    private static async Task CrearYAprobar(HttpClient cliente, string id)
    {
        await Post(cliente, "/misiones", new
        {
            Id = id,
            CapturadaPor = "P-ASISTENTE",
            SolicitanteDeDerecho = "P-ASISTENTE",
            Dependencia = "Delegacion de prueba RN-62",
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
