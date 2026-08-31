using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Sigti.Datos;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M04_Documentacion;
using Sigti.Datos.M16_Sincronizacion;
using Sigti.Pruebas.Datos;

namespace Sigti.Pruebas.PuntaAPunta;

/// <summary>
/// `HU-020` — el feriado largo, de punta a punta.
///
/// ── Por qué esto no es una comodidad ────────────────────────────────────────
/// El Tribunal Superior de Cuentas hace operativos de fiscalización vehicular <b>específicamente
/// en Semana Santa</b> `[V]`. Es el pico anual de riesgo, y es <b>predecible</b>.
///
/// Un flujo que le exige a la máxima autoridad abrir veinte expedientes uno por uno a las cinco
/// de la tarde del jueves santo produce una de dos cosas: <b>permisos que no se firman y
/// misiones que salen sin amparo, o la clave prestada a un asistente</b>. La segunda es la que
/// el sistema entero está diseñado para evitar.
///
/// ── Y por eso las tres listas ───────────────────────────────────────────────
/// El reporte no sirve para enumerar los que salen: sirve para que <b>ningún vehículo quede sin
/// respuesta</b>. Uno del que nadie confirmó dónde está es exactamente lo que un operativo
/// encuentra.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class FeriadoLargoPruebas(BaseDePruebas baseDePruebas)
{
    /// <summary>Semana Santa de 2026: del lunes 30 de marzo al domingo 5 de abril.</summary>
    private static readonly DateOnly Desde = new(2026, 3, 30);

    /// <inheritdoc cref="Desde"/>
    private static readonly DateOnly Hasta = new(2026, 4, 5);

    private static readonly DateTimeOffset Momento =
        new(2026, 3, 26, 9, 0, 0, TimeSpan.FromHours(-6));

    /// <summary>
    /// El vehículo que sale aparece <b>con su permiso a firmar</b>, y el que no sale aparece
    /// <b>a resguardar y sin confirmar</b>.
    ///
    /// Las dos mitades importan: un reporte que liste sólo los que circulan deja al resto
    /// invisible.
    /// </summary>
    [Fact]
    public async Task El_que_sale_y_el_que_se_queda_aparecen_los_dos()
    {
        var sale = await SembrarAsync("FERIADO-A");
        var seQueda = await SembrarAsync("FERIADO-B");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await ProgramarConPermisoAsync(cliente, sale);

        var reporte = await ReporteAsync(cliente);

        Assert.Contains(
            reporte.GetProperty("circulan").EnumerateArray(),
            v => v.GetProperty("vehiculo").GetString() == sale.Vehiculo);

        var resguardado = reporte.GetProperty("resguardados").EnumerateArray()
            .Single(v => v.GetProperty("vehiculo").GetString() == seQueda.Vehiculo);

        Assert.Equal("NoConfirmado", resguardado.GetProperty("resguardo").GetString());

        // Nadie fue a mirar todavía: no hay ni fecha ni predio, y son nulos de verdad —
        // no cadenas vacías que se vean como un dato.
        Assert.Equal(JsonValueKind.Null, resguardado.GetProperty("predio").ValueKind);
    }

    /// <summary>
    /// ⚠️ <b>La propiedad que hace útil el reporte:</b> las tres listas suman la flota entera.
    ///
    /// Con la base compartida no se puede afirmar un número; sí se puede afirmar que <b>ningún
    /// vehículo se quedó fuera y ninguno aparece dos veces</b>, que es lo que la cifra
    /// significaba.
    /// </summary>
    [Fact]
    public async Task El_reporte_cuadra_contra_la_flota_entera()
    {
        await SembrarAsync("FERIADO-C");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var reporte = await ReporteAsync(cliente);

        Assert.Equal(
            JsonValueKind.Null, reporte.GetProperty("noCuadraPorque").ValueKind);

        int enElReporte =
            reporte.GetProperty("circulan").GetArrayLength() +
            reporte.GetProperty("resguardados").GetArrayLength() +
            reporte.GetProperty("exceptuados").GetArrayLength();

        // Contra la flota que SIGUE siendo flota: los dos estados terminales de §10.2 dejan de
        // serlo, y contarlos acá mediría algo que el reporte no promete.
        Assert.Equal(await EnLaFlotaAsync(), enElReporte);
    }

    /// <summary>
    /// ⚠️ <b>Los incompletos no detienen a los completos.</b>
    ///
    /// `HU-020` es explícita: se firman los que están y <b>se nombra el que no</b>. Abortar el
    /// lote entero por uno obligaría a la máxima autoridad a volver, y volver a las cinco de la
    /// tarde del jueves santo es precisamente lo que no ocurre.
    ///
    /// Y «4 de 5 firmados» sin decir cuál faltó deja a quien firma buscando el que quedó —
    /// que es el que va a salir sin amparo.
    /// </summary>
    [Fact]
    public async Task El_lote_firma_los_completos_y_nombra_el_que_no()
    {
        var programado = await SembrarAsync("FERIADO-D");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var bueno = await ProgramarConPermisoAsync(cliente, programado);

        // El otro tiene trámite abierto y **la misión sin programar**: no hay vehículo ni
        // motorista que amparar, y el permiso es nominativo — `RN-23`.
        var sinProgramar = await TramitarSinProgramarAsync(cliente);

        var respuesta = await cliente.PostAsJsonAsync("/periodos/firmar-lote", new
        {
            Permisos = new[] { bueno, sinProgramar },
            Firma = "P-MAXIMA",
            Momento,
        });

        Assert.True(respuesta.IsSuccessStatusCode, await respuesta.Content.ReadAsStringAsync());

        var cuerpo = await respuesta.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Single(cuerpo.GetProperty("firmados").EnumerateArray());

        var noFirmado = cuerpo.GetProperty("noFirmados").EnumerateArray().Single();

        Assert.Equal(sinProgramar, noFirmado.GetProperty("id").GetString());

        // Con folio y motivo: lo que hace falta para ir a resolverlo, no sólo el aviso.
        Assert.False(string.IsNullOrWhiteSpace(noFirmado.GetProperty("folio").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(noFirmado.GetProperty("motivo").GetString()));

        // ⚠️ **Y el firmado deja de contar como firmable.** Sin esto la cifra no baja al firmar
        // y la sesión de firma no termina nunca: quien firma vuelve a abrirla creyendo que
        // quedaron permisos pendientes, y encuentra los mismos.
        var reporte = await ReporteAsync(cliente);

        var yaFirmado = reporte.GetProperty("circulan").EnumerateArray()
            .Single(v => v.GetProperty("vehiculo").GetString() == programado.Vehiculo);

        Assert.True(yaFirmado.GetProperty("firmado").GetBoolean());

        // Y sin motivo: el permiso sigue amparando, y decir «ya está firmado» acá lo pintaría
        // como un bloqueo que hay que ir a arreglar.
        Assert.Equal(JsonValueKind.Null, yaFirmado.GetProperty("porQueNoSeFirma").ValueKind);
    }

    /// <summary>
    /// ⚠️ <b>Quien no es la máxima autoridad no firma ni uno.</b>
    ///
    /// El lote es el lugar donde la clave prestada rendiría más: veinte firmas en un clic. El
    /// rechazo es <b>del lote entero y antes de tocar nada</b> — comprobarlo permiso por permiso
    /// dejaría veinte intentos idénticos en la bitácora en vez de un rechazo claro.
    /// </summary>
    [Fact]
    public async Task Quien_no_es_la_maxima_autoridad_no_firma_ninguno()
    {
        var r = await SembrarAsync("FERIADO-E");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var permiso = await ProgramarConPermisoAsync(cliente, r);

        var respuesta = await cliente.PostAsJsonAsync("/periodos/firmar-lote", new
        {
            Permisos = new[] { permiso },
            Firma = "P-ASISTENTE",
            Momento,
        });

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);

        // Y el permiso sigue sin firmar: el rechazo no dejó nada a medias.
        await using var contexto = baseDePruebas.Contexto();
        var fila = await contexto.Permisos.SingleAsync(p => p.Id == Ulid.Parse(permiso));

        Assert.Equal("Solicitado", fila.Estado);
    }

    /// <summary>
    /// La confirmación de resguardo mueve el vehículo de «nadie fue a mirar» a <b>«está aquí, y
    /// esta foto lo prueba»</b>.
    /// </summary>
    [Fact]
    public async Task Confirmar_el_resguardo_lo_saca_de_los_sin_confirmar()
    {
        var r = await SembrarAsync("FERIADO-F");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync("/periodos/resguardos", new
        {
            Vehiculo = r.Vehiculo,
            Desde,
            Hasta,
            Predio = "Predio de la sede central, portón oriente",
            Evidencia = (await AdjuntoAsync()).ToString(),
            ConfirmadoEl = new DateOnly(2026, 3, 27),
            Confirma = "P-TRANSPORTE",
            Momento,
        });

        Assert.True(respuesta.IsSuccessStatusCode, await respuesta.Content.ReadAsStringAsync());

        var reporte = await ReporteAsync(cliente);

        var fila = reporte.GetProperty("resguardados").EnumerateArray()
            .Single(v => v.GetProperty("vehiculo").GetString() == r.Vehiculo);

        Assert.Equal("Confirmado", fila.GetProperty("resguardo").GetString());
        Assert.Contains("portón oriente", fila.GetProperty("predio").GetString());

        // La fecha del hecho —cuándo alguien fue a mirar—, no la de captura: una foto de hace
        // tres semanas confirma menos que una de ayer, y sin la fecha las dos se ven iguales.
        Assert.Equal("2026-03-27", fila.GetProperty("confirmadoEl").GetString());
    }

    /// <summary>
    /// ⚠️ <b>Sin evidencia no se confirma.</b> Misma disciplina de `RN-18`: sin ella lo único que
    /// queda registrado es que alguien dijo que el vehículo estaba ahí, y eso es lo que un
    /// operativo viene a discutir.
    /// </summary>
    [Fact]
    public async Task Un_resguardo_cuya_evidencia_no_existe_no_se_confirma()
    {
        var r = await SembrarAsync("FERIADO-G");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync("/periodos/resguardos", new
        {
            Vehiculo = r.Vehiculo,
            Desde,
            Hasta,
            Predio = "Predio de la sede central",

            // El identificador de un adjunto no es el adjunto: uno que apunta a nada se ve
            // igual que uno que existe.
            Evidencia = Ulid.NewUlid().ToString(),
            ConfirmadoEl = new DateOnly(2026, 3, 27),
            Confirma = "P-TRANSPORTE",
            Momento,
        });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);

        Assert.Contains(
            "alguien dijo que el vehículo estaba ahí",
            await respuesta.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// ⚠️ <b>El vehículo dado de baja no aparece — y el reporte sigue cuadrando.</b>
    ///
    /// Pedirle a alguien que confirme dónde quedó resguardado un bien que ya se descargó del
    /// registro es mandarlo a una tarea que puede ser imposible. Y el daño no es la tarea de
    /// más: <b>cada uno infla «sin confirmar»</b>, y los tres que de verdad nadie fue a mirar
    /// quedan enterrados entre decenas.
    ///
    /// Lo que esta prueba fija es la otra mitad, que es donde se rompe: <b>listar con un
    /// criterio y contar con otro</b> haría que la comprobación de que el reporte cuadra fallara
    /// siempre, o —peor— que pasara escondiendo un vehículo.
    /// </summary>
    [Fact]
    public async Task El_vehiculo_dado_de_baja_no_entra_y_el_reporte_sigue_cuadrando()
    {
        var r = await SembrarAsync("FERIADO-H");
        await DarDeBajaAsync(Ulid.Parse(r.Vehiculo));

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var reporte = await ReporteAsync(cliente);

        Assert.DoesNotContain(
            reporte.GetProperty("resguardados").EnumerateArray(),
            v => v.GetProperty("vehiculo").GetString() == r.Vehiculo);

        Assert.Equal(JsonValueKind.Null, reporte.GetProperty("noCuadraPorque").ValueKind);
    }

    // ── Andamios ────────────────────────────────────────────────────────────

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

    /// <summary>
    /// Lo descarga del registro de bienes. Es terminal: deja de ser flota, y el reporte del
    /// feriado deja de preguntarse dónde está.
    /// </summary>
    private async Task DarDeBajaAsync(Ulid vehiculo)
    {
        await using var contexto = baseDePruebas.Contexto();

        contexto.CambiosDeEstado.Add(new FilaDeCambioDeEstado
        {
            Id = Ulid.NewUlid(),
            VehiculoId = vehiculo,
            Estado = EstadoOperativo.DadoDeBaja,
            MomentoUtc = Momento,
            Orden = 1,
            Ejecuta = "P-GERENCIA",
            Motivo = "Acta de descargo GA-2026-0044",
            Automatico = false,
        });

        await contexto.SaveChangesAsync();
    }

    /// <summary>
    /// Cuántos vehículos sigue teniendo la flota, resuelto por el diario de §10.2 (`P-1`).
    ///
    /// Se usa la regla del dominio y no una copia: la prueba comprueba que <b>la ruta lista y
    /// cuenta con el mismo criterio</b>, no reimplementa cuál es el criterio.
    /// </summary>
    private async Task<int> EnLaFlotaAsync()
    {
        await using var contexto = baseDePruebas.Contexto();

        var vehiculos = await contexto.Vehiculos.AsNoTracking().Select(v => v.Id).ToListAsync();
        var cambios = await contexto.CambiosDeEstado.AsNoTracking().ToListAsync();

        var ultimo = cambios
            .GroupBy(c => c.VehiculoId)
            .ToDictionary(g => g.Key, g => g.MaxBy(c => c.Orden)!.Estado);

        return vehiculos.Count(v => ReglasDelReporteDelPeriodo.EstaEnLaFlota(
            ultimo.TryGetValue(v, out var estado) ? estado : null));
    }

    private async Task<FlotaSembrada.ParaProgramar> SembrarAsync(string prefijo)
    {
        await using var contexto = baseDePruebas.Contexto();
        return await FlotaSembrada.ParaProgramarAsync(contexto, prefijo);
    }

    private async Task<JsonElement> ReporteAsync(HttpClient cliente) =>
        await cliente.GetFromJsonAsync<JsonElement>(
            $"/periodos/reporte?desde={Desde:yyyy-MM-dd}&hasta={Hasta:yyyy-MM-dd}");

    /// <summary>
    /// Una misión del <b>sábado 4 al domingo 5 de abril</b>: circula en franja inhábil sin
    /// depender de que el calendario de feriados esté cargado — el fin de semana lo es siempre.
    /// </summary>
    private static async Task<string> CrearYAprobarAsync(HttpClient cliente)
    {
        var id = Ulid.NewUlid().ToString();

        await cliente.PostAsJsonAsync("/misiones", new
        {
            Id = id,
            CapturadaPor = "P-ASISTENTE",
            SolicitanteDeDerecho = "P-ASISTENTE",
            Dependencia = "Delegacion de Choluteca",
            ObjetoDelTraslado = "Relevo de personal en el puesto fronterizo",
            Destino = "Choluteca",
            Salida = new DateOnly(2026, 4, 4),
            Retorno = new DateOnly(2026, 4, 5),
            HoraDeSalida = "06:00",
            HoraDeRetorno = "18:00",
            HolguraDias = 0,
            Momento,
        });

        await cliente.PostAsJsonAsync($"/misiones/{id}/enviar",
            new { Ejecuta = "P-ASISTENTE", Momento });

        await cliente.PostAsJsonAsync($"/misiones/{id}/aprobar",
            new { Ejecuta = "P-JEFATURA", Momento });

        return id;
    }

    /// <summary>Programa la misión y abre el trámite. Devuelve el identificador del permiso.</summary>
    private static async Task<string> ProgramarConPermisoAsync(
        HttpClient cliente, FlotaSembrada.ParaProgramar r)
    {
        var mision = await CrearYAprobarAsync(cliente);

        var programada = await cliente.PostAsJsonAsync($"/misiones/{mision}/programar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            IdVehiculo = r.Vehiculo,
            IdConductor = r.Conductor,
        });

        Assert.True(programada.IsSuccessStatusCode, await programada.Content.ReadAsStringAsync());

        return await AbrirTramiteAsync(cliente, mision);
    }

    /// <summary>
    /// El trámite sobre una misión aprobada <b>y sin programar</b>: el caso que el lote tiene que
    /// dejar fuera nombrándolo.
    /// </summary>
    private static async Task<string> TramitarSinProgramarAsync(HttpClient cliente) =>
        await AbrirTramiteAsync(cliente, await CrearYAprobarAsync(cliente));

    private static async Task<string> AbrirTramiteAsync(HttpClient cliente, string mision)
    {
        var respuesta = await cliente.PostAsJsonAsync($"/misiones/{mision}/permiso", new
        {
            Justificacion = "Relevo de turno en frontera: el puesto no cierra el fin de semana.",
            Solicita = "P-ASISTENTE",
            Momento,
        });

        Assert.True(respuesta.IsSuccessStatusCode, await respuesta.Content.ReadAsStringAsync());

        var cuerpo = await respuesta.Content.ReadFromJsonAsync<JsonElement>();

        // Si el sistema dijera que no hace falta, la prueba estaría midiendo otra cosa: la
        // ventana del 4 al 5 de abril es sábado y domingo.
        Assert.True(cuerpo.GetProperty("abierto").GetBoolean(), cuerpo.ToString());

        return cuerpo.GetProperty("id").GetString()!;
    }

    private async Task<Ulid> AdjuntoAsync()
    {
        var id = Ulid.NewUlid();

        await using var contexto = baseDePruebas.Contexto();

        contexto.Adjuntos.Add(new FilaDeAdjunto
        {
            Id = id,
            IdTransicion = null,
            Ruta = $"resguardos/{id}.jpg",
            Hash = "sha256:" + new string('0', 64),
            Tipo = "image/jpeg",
            Bytes = 2048,
            Clasificacion = "ADMINISTRATIVO",
            CapturadoEnUtc = Momento.UtcDateTime,
            RecibidoEnUtc = Momento.UtcDateTime,
        });

        await contexto.SaveChangesAsync();
        return id;
    }
}
