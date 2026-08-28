using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sigti.Datos;
using Sigti.Dominio.M03_Flota;
using Sigti.Pruebas.Datos;

namespace Sigti.Pruebas.PuntaAPunta;

/// <summary>
/// La ocupación de la flota — lo que hace que elegir vehículo deje de ser adivinar.
///
/// ── Lo que estaba roto y no se veía ──────────────────────────────────────────
/// `T-08` decía <i>«aquí se reserva vehículo y motorista»</i> desde que se escribió la
/// máquina de estados, y <b>no reservaba nada</b>: la identidad del vehículo quedaba
/// dentro del texto de evidencia, en prosa. La misión se programaba, el diario quedaba
/// perfecto, y el vehículo se seguía ofreciendo libre. Nadie lo notó porque el síntoma
/// —una pantalla que no muestra la ocupación— es indistinguible de una que no la tiene.
///
/// ── Por qué la reserva vive en el diario ─────────────────────────────────────
/// P-1: el estado es la proyección del diario. Una tabla de reservas sería una segunda
/// copia con su propia forma de desincronizarse — una misión anulada cuya reserva
/// sobrevive deja un vehículo fantasma ocupado y el sistema reporta falta de flota que no
/// existe. Con la reserva en la transición, <b>liberar es no volver a tomar</b>.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class OcupacionDeFlotaPruebas(BaseDePruebas baseDePruebas)
{
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

    /// <summary>
    /// La ventana: del <b>lunes 16</b> al <b>miércoles 18</b> de marzo, con un día de holgura.
    ///
    /// ⚠️ <b>Entre semana a propósito.</b> Desde `BD-04`, una ventana que cruza el fin de semana
    /// exige permiso de la máxima autoridad — y estas pruebas no van de eso. Se cambia la
    /// ventana en vez de sembrar un permiso falso: un permiso inventado las haría pasar por
    /// `BD-04` sin que nadie las hubiera escrito para eso.
    /// </summary>
    private const string Desde = "2026-03-14";
    private const string Hasta = "2026-03-20";

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
    public async Task Programar_ocupa_el_vehiculo_y_la_ocupacion_lo_dice()
    {
        var r = await Sembrar("OC-0001");
        var idMision = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearYAprobar(cliente, idMision);

        // Antes de programar, el carril del vehículo está vacío. Se comprueba **antes** y
        // no sólo después: una prueba que sólo mira el final pasaría igual si el carril
        // hubiera estado ocupado desde siempre por otra cosa.
        Assert.Empty(await BarrasDe(cliente, r.Vehiculo));

        await Programar(cliente, idMision, r);

        var barras = await BarrasDe(cliente, r.Vehiculo);
        var barra = Assert.Single(barras);

        Assert.Equal("2026-03-16", barra.GetProperty("desde").GetString());
        // El retorno, **inclusivo**: es un día en que el vehículo sigue tomado.
        Assert.Equal("2026-03-18", barra.GetProperty("hasta").GetString());
        Assert.Equal("Programada", barra.GetProperty("estado").GetString());
    }

    [Fact]
    public async Task El_vehiculo_que_retorna_deja_de_ocupar_sin_que_nadie_borre_una_reserva()
    {
        // **Es la prueba que justifica la decisión de diseño.** Con una tabla de reservas
        // habría que acordarse de borrar la fila en cada salida del estado; acá la reserva
        // deja de contar porque el diario siguió, y no hay nada que olvidar.
        //
        // Se recorre entero `T-08 → T-12 → T-14 → T-16` porque el punto es JUSTO ESE: el
        // vehículo sigue ocupando mientras está despachado y en ruta —está afuera—, y deja
        // de ocupar al retornar. Una prueba que sólo mirara el final no distinguiría
        // «libera al retornar» de «nunca ocupó».
        var r = await Sembrar("OC-0002");
        var idMision = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearYAprobar(cliente, idMision);
        await Programar(cliente, idMision, r);
        Assert.Single(await BarrasDe(cliente, r.Vehiculo));

        await Despachar(cliente, idMision, r);
        Assert.Equal("Despachada", (await BarrasDe(cliente, r.Vehiculo))[0].GetProperty("estado").GetString());

        await cliente.PostAsJsonAsync($"/misiones/{idMision}/iniciar-ruta", new { Ejecuta = "P-MOTORISTA", Momento });
        Assert.Equal("EnRuta", (await BarrasDe(cliente, r.Vehiculo))[0].GetProperty("estado").GetString());

        var retorno = await cliente.PostAsJsonAsync(
            $"/misiones/{idMision}/retornar", new { Ejecuta = "P-MOTORISTA", Momento });
        retorno.EnsureSuccessStatusCode();

        // Retornada NO ocupa: el vehículo volvió, aunque falte liquidar.
        Assert.Empty(await BarrasDe(cliente, r.Vehiculo));
    }

    [Fact]
    public async Task BD_11_impide_dos_misiones_sobre_el_mismo_vehiculo_en_la_misma_franja()
    {
        // **Esto se aceptaba hasta hoy.** `EF-01` es taxativo — «no sobre-asigna, ni
        // siquiera con advertencia; dos misiones con el mismo vehículo el mismo día es el
        // error que termina con un servidor público esperando en la puerta»— y `BD-11`
        // estaba escrita y sin implementar.
        var r = await Sembrar("OC-0011");

        var primera = Ulid.NewUlid().ToString();
        var segunda = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearYAprobar(cliente, primera);
        await Programar(cliente, primera, r);

        await CrearYAprobar(cliente, segunda);

        var otroMotorista = (await OtroMotorista("Motorista libre de BD-11")).ToString();

        // **El segundo motorista es OTRO, y eso es lo que hace válida la prueba.** Con el
        // mismo, el bloqueo podría estar disparando por el conductor y no por el vehículo,
        // y la prueba diría que verificó algo que no verificó.
        var respuesta = await cliente.PostAsJsonAsync($"/misiones/{segunda}/programar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            IdVehiculo = r.Vehiculo,
            IdConductor = otroMotorista,
        });

        Assert.Equal(System.Net.HttpStatusCode.Conflict, respuesta.StatusCode);

        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("BD-11", cuerpo);
        // `EF-01` exige nombrar al titular: sin la dependencia, quien programa no sabe a
        // quién llamar para consolidar, reprogramar o escalar.
        Assert.Contains("Delegacion de Choluteca", cuerpo);

        // Y la segunda misión no quedó a medias: sigue aprobada, lista para otro vehículo.
        var estado = await cliente.GetFromJsonAsync<JsonElement>($"/misiones/{segunda}");
        Assert.Equal("Aprobada", estado.GetProperty("estado").GetString());
    }

    [Fact]
    public async Task El_conflicto_se_ve_en_la_vista_previa_y_no_solo_al_guardar()
    {
        // Las cuatro salidas de `EF-01` —consolidar, otro recurso, reprogramar, escalar—
        // se deciden ANTES de apretar el botón. Descubrir el choque recién al guardar
        // obliga a rehacer la elección entera.
        var r = await Sembrar("OC-0012");

        var primera = Ulid.NewUlid().ToString();
        var segunda = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearYAprobar(cliente, primera);
        await Programar(cliente, primera, r);
        await CrearYAprobar(cliente, segunda);

        var evaluacion = await cliente.PostAsJsonAsync($"/misiones/{segunda}/evaluar-asignacion", new
        {
            IdVehiculo = r.Vehiculo,
            // Otro motorista, por lo mismo: el conflicto que se comprueba es el del vehículo.
            IdConductor = (await OtroMotorista("Motorista libre de la vista previa")).ToString(),
            HayConduccionNocturna = false,
        });

        evaluacion.EnsureSuccessStatusCode();
        var cuerpo = await evaluacion.Content.ReadFromJsonAsync<JsonElement>();

        Assert.False(cuerpo.GetProperty("habilita").GetBoolean());

        var conflicto = cuerpo.GetProperty("conflicto");
        Assert.Equal("Delegacion de Choluteca", conflicto.GetProperty("dependencia").GetString());
        Assert.True(conflicto.GetProperty("vehiculo").GetBoolean());
    }

    [Fact]
    public async Task Otro_vehiculo_en_la_misma_franja_si_se_programa()
    {
        // El recíproco. Sin esto, `BD-11` podría bloquear toda segunda programación y las
        // pruebas de bloqueo seguirían en verde: lo que hay que probar es que bloquea por
        // el RECURSO y no por la fecha.
        // Dos pares COMPLETOS: si compartieran motorista, `BD-11` bloquearía con razón por
        // el conductor y la prueba fallaría sin que el vehículo tuviera nada que ver.
        var uno = await Sembrar("OC-0013");
        var otro = await Sembrar("OC-0014");

        var primera = Ulid.NewUlid().ToString();
        var segunda = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearYAprobar(cliente, primera);
        await Programar(cliente, primera, uno);

        await CrearYAprobar(cliente, segunda);
        await Programar(cliente, segunda, otro);

        // Las dos ocupan la misma franja, cada una su carril.
        Assert.Single(await BarrasDe(cliente, uno.Vehiculo));
        Assert.Single(await BarrasDe(cliente, otro.Vehiculo));
    }

    [Fact]
    public async Task Desprogramar_libera_el_recurso_y_otra_mision_lo_puede_tomar()
    {
        // **El ciclo completo, y es lo que `T-11` existe para permitir.** Hasta ahora una
        // misión programada no se podía deshacer: un vehículo asignado por error quedaba
        // tomado hasta que alguien lo despachara.
        //
        // Es además la cuarta salida de un conflicto de `BD-11` —escalar la prioridad—:
        // `EF-01` exige que desplazar a una misión pase por devolverla explícitamente a la
        // cola, nunca por quitarle el vehículo en silencio.
        var r = await Sembrar("OC-0021");

        var primera = Ulid.NewUlid().ToString();
        var segunda = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearYAprobar(cliente, primera);
        await Programar(cliente, primera, r);
        Assert.Single(await BarrasDe(cliente, r.Vehiculo));

        // La segunda choca, como debe.
        await CrearYAprobar(cliente, segunda);
        var chocada = await cliente.PostAsJsonAsync($"/misiones/{segunda}/programar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            IdVehiculo = r.Vehiculo,
            IdConductor = r.Conductor,
        });
        Assert.Equal(System.Net.HttpStatusCode.Conflict, chocada.StatusCode);

        // Se libera la primera.
        var liberacion = await cliente.PostAsJsonAsync($"/misiones/{primera}/desprogramar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            Motivo = "Desplazada por prioridad superior",
        });
        liberacion.EnsureSuccessStatusCode();

        Assert.Empty(await BarrasDe(cliente, r.Vehiculo));

        // Y ahora la segunda sí entra. **Este es el punto**: liberar no es cosmético.
        await Programar(cliente, segunda, r);

        var barra = Assert.Single(await BarrasDe(cliente, r.Vehiculo));
        Assert.Equal("Programada", barra.GetProperty("estado").GetString());

        // La primera conserva su aprobación: vuelve a la cola, no a solicitada.
        var estado = await cliente.GetFromJsonAsync<JsonElement>($"/misiones/{primera}");
        Assert.Equal("Aprobada", estado.GetProperty("estado").GetString());
    }

    [Fact]
    public async Task Desprogramar_sin_motivo_se_rechaza()
    {
        // La dependencia pierde un vehículo que ya tenía. Una notificación sin razón no es
        // una notificación.
        var r = await Sembrar("OC-0022");
        var id = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearYAprobar(cliente, id);
        await Programar(cliente, id, r);

        var respuesta = await cliente.PostAsJsonAsync($"/misiones/{id}/desprogramar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            Motivo = "  ",
        });

        Assert.Equal(System.Net.HttpStatusCode.Conflict, respuesta.StatusCode);
        // Y no quedó a medias: el vehículo sigue tomado.
        Assert.Single(await BarrasDe(cliente, r.Vehiculo));
    }

    [Fact]
    public async Task Anular_una_programada_libera_el_recurso_y_la_mata()
    {
        // `T-13`. La diferencia con `T-11` es que de acá no se vuelve: quien quiera el
        // viaje presenta una solicitud nueva.
        var r = await Sembrar("OC-0023");
        var id = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearYAprobar(cliente, id);
        await Programar(cliente, id, r);
        Assert.Single(await BarrasDe(cliente, r.Vehiculo));

        var anulacion = await cliente.PostAsJsonAsync($"/misiones/{id}/anular-programada", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            Motivo = "CausaExterna",
            Comentario = "Cierre de carretera por derrumbe",
        });
        anulacion.EnsureSuccessStatusCode();

        Assert.Empty(await BarrasDe(cliente, r.Vehiculo));

        var estado = await cliente.GetFromJsonAsync<JsonElement>($"/misiones/{id}");
        Assert.Equal("Anulada", estado.GetProperty("estado").GetString());
    }

    [Fact]
    public async Task Reasignar_mueve_la_barra_de_un_carril_al_otro()
    {
        // **El caso de borde de `BD-11` que sólo aparece en `T-10`.** Acá la misión está
        // PROGRAMADA mientras se evalúa, así que ocupa: si la consulta no la excluyera,
        // chocaría contra su propia reserva y ningún cambio sería posible. Contra la base,
        // que es donde el `Where(e => e.Id != excluyendo)` se ejerce de verdad.
        var original = await Sembrar("OC-0031");
        var entrante = await Sembrar("OC-0032");

        var id = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearYAprobar(cliente, id);
        await Programar(cliente, id, original);

        Assert.Single(await BarrasDe(cliente, original.Vehiculo));
        Assert.Empty(await BarrasDe(cliente, entrante.Vehiculo));

        var reasignacion = await cliente.PostAsJsonAsync($"/misiones/{id}/reasignar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            IdVehiculo = entrante.Vehiculo,
            IdConductor = entrante.Conductor,
            Motivo = "VehiculoATaller",
            Comentario = "Falla de frenos detectada en la revisión previa",
        });

        reasignacion.EnsureSuccessStatusCode();

        // La barra se movió de carril: el saliente quedó libre y el entrante tomado.
        Assert.Empty(await BarrasDe(cliente, original.Vehiculo));
        Assert.Single(await BarrasDe(cliente, entrante.Vehiculo));

        // Y la misión NO pasó por un estado sin vehículo.
        var estado = await cliente.GetFromJsonAsync<JsonElement>($"/misiones/{id}");
        Assert.Equal("Programada", estado.GetProperty("estado").GetString());

        // `DP-001 D-07`: el diario conserva a quién se había asignado y por qué se cambió.
        var diario = estado.GetProperty("diario").EnumerateArray().ToList();
        Assert.Contains(diario, t => t.GetProperty("id").GetString() == "T-08");
        var cambio = diario.Single(t => t.GetProperty("id").GetString() == "T-10");
        Assert.Contains("VehiculoATaller", cambio.GetProperty("motivo").GetString()!);
    }

    [Fact]
    public async Task Reasignar_a_un_vehiculo_ya_tomado_bloquea()
    {
        // Cambiar de vehículo no es excusa para tomar uno ocupado: `BD-11` también acá.
        var mia = await Sembrar("OC-0033");
        var ajena = await Sembrar("OC-0034");

        var propia = Ulid.NewUlid().ToString();
        var deOtro = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearYAprobar(cliente, propia);
        await Programar(cliente, propia, mia);

        await CrearYAprobar(cliente, deOtro);
        await Programar(cliente, deOtro, ajena);

        var respuesta = await cliente.PostAsJsonAsync($"/misiones/{propia}/reasignar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            IdVehiculo = ajena.Vehiculo,
            IdConductor = ajena.Conductor,
            Motivo = "CambioDeRequerimiento",
        });

        Assert.Equal(System.Net.HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Contains("BD-11", await respuesta.Content.ReadAsStringAsync());

        // Y la mía sigue con su vehículo original: no quedó sin ninguno.
        Assert.Single(await BarrasDe(cliente, mia.Vehiculo));
    }

    [Fact]
    public async Task BD_13_impide_despachar_un_vehiculo_sin_custodio()
    {
        // «Un vehículo del Estado sin responsable identificado es un hallazgo esperando
        // ocurrir.» Y no es una formalidad: sin custodio no hay de quién recibir el bien ni
        // a quién devolverlo, y el acta de entrega quedaría sin una de sus dos firmas.
        //
        // Contra la base, que es donde `ConsultaDeCustodias` se ejerce de verdad: el
        // vehículo se siembra **sin** custodia.
        var sinCustodio = await SembrarSinCustodia("SC-0001");
        var id = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearYAprobar(cliente, id);
        await Programar(cliente, id, sinCustodio);

        // Programar SÍ se pudo: `BD-13` es de `T-12`, no de `T-08`. El vehículo se puede
        // reservar; lo que no se puede es entregarlo.
        Assert.Single(await BarrasDe(cliente, sinCustodio.Vehiculo));

        var despacho = await cliente.PostAsJsonAsync($"/misiones/{id}/despachar", new
        {
            Ejecuta = "P-ENCARGADO",
            Momento,
            IdVehiculo = sinCustodio.Vehiculo,
            IdConductor = sinCustodio.Conductor,
        });

        Assert.Equal(System.Net.HttpStatusCode.Conflict, despacho.StatusCode);

        var cuerpo = await despacho.Content.ReadAsStringAsync();
        Assert.Contains("BD-13", cuerpo);
        Assert.Contains("no tiene ninguna custodia registrada", cuerpo);

        // Y la misión quedó donde estaba, con su vehículo reservado.
        var estado = await cliente.GetFromJsonAsync<JsonElement>($"/misiones/{id}");
        Assert.Equal("Programada", estado.GetProperty("estado").GetString());
    }

    [Fact]
    public async Task Registrar_la_custodia_destraba_el_despacho()
    {
        // El recíproco. Sin él, `BD-13` podría estar bloqueando todo despacho y la prueba
        // anterior seguiría en verde: lo que hay que probar es que bloquea por la AUSENCIA
        // de custodio, no por despachar.
        var v = await SembrarSinCustodia("SC-0002");
        var id = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearYAprobar(cliente, id);
        await Programar(cliente, id, v);

        // Se firma la tarjeta de responsabilidad.
        await using (var contexto = baseDePruebas.Contexto())
            await FlotaSembrada.CustodiarAsync(contexto, Ulid.Parse(v.Vehiculo));

        await Despachar(cliente, id, v);

        var estado = await cliente.GetFromJsonAsync<JsonElement>($"/misiones/{id}");
        Assert.Equal("Despachada", estado.GetProperty("estado").GetString());

        // Y el diario dice quién respondía por el bien al salir: es la pregunta que la
        // cadena de custodia existe para contestar años después.
        var despacho = estado.GetProperty("diario").EnumerateArray()
            .Single(t => t.GetProperty("id").GetString() == "T-12");
        Assert.Contains("P-CUSTODIO", despacho.GetProperty("motivo").GetString()!);
    }

    /// <summary>
    /// Un pick-up y su motorista, <b>sin custodia registrada</b>. Es el estado que `BD-13`
    /// bloquea, y hay que poder llegar a él para probarlo.
    /// </summary>
    private async Task<FlotaSembrada.ParaProgramar> SembrarSinCustodia(string prefijo)
    {
        await using var contexto = baseDePruebas.Contexto();
        var r = await FlotaSembrada.ParaProgramarAsync(contexto, prefijo);

        var fila = contexto.Custodias.Single(c => c.VehiculoId == Ulid.Parse(r.Vehiculo));
        contexto.Custodias.Remove(fila);
        await contexto.SaveChangesAsync();

        return r;
    }

    [Fact]
    public async Task BD_04_impide_salir_en_fin_de_semana_sin_permiso_y_el_permiso_lo_destraba()
    {
        // Contra la API real y el <b>calendario provisional</b> —lunes a viernes hábiles—,
        // que es el que va a usar la institución hasta que cargue el suyo.
        //
        // La ventana va del <b>viernes 20</b> al <b>domingo 22</b> de marzo: cruza sábado y
        // domingo. Es la única prueba de punta a punta que sale en fin de semana, y por eso
        // es la única que necesita permiso.
        var r = await Sembrar("BD04-0001");
        var id = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearYAprobarEnFinDeSemana(cliente, id);
        await Programar(cliente, id, r);

        // Programar SÍ se puede: `BD-04` es de `T-12`. Se reserva el vehículo y se pide el
        // permiso mientras tanto — que es el orden real de las cosas.
        var sinPermiso = await cliente.PostAsJsonAsync($"/misiones/{id}/despachar", new
        {
            Ejecuta = "P-ENCARGADO",
            Momento,
            IdVehiculo = r.Vehiculo,
            IdConductor = r.Conductor,
        });

        Assert.Equal(System.Net.HttpStatusCode.Conflict, sinPermiso.StatusCode);
        var cuerpo = await sinPermiso.Content.ReadAsStringAsync();
        Assert.Contains("BD-04", cuerpo);
        Assert.Contains("2026-03-21", cuerpo);
        Assert.Contains("No hay ningún permiso registrado", cuerpo);

        // La máxima autoridad firma el salvoconducto.
        await using (var contexto = baseDePruebas.Contexto())
        {
            contexto.Permisos.Add(new FilaDePermisoDeCirculacion
            {
                Id = Ulid.NewUlid(),
                ExpedienteId = Ulid.Parse(id),
                Folio = "SC-000042",
                EmitidoPor = "P-MAXIMA-AUTORIDAD",
                Vehiculo = Ulid.Parse(r.Vehiculo),
                Motorista = Ulid.Parse(r.Conductor),
                Destino = "Choluteca",
                Desde = new DateOnly(2026, 3, 20),
                Hasta = new DateOnly(2026, 3, 23),
            });
            await contexto.SaveChangesAsync();
        }

        await Despachar(cliente, id, r);

        var estado = await cliente.GetFromJsonAsync<JsonElement>($"/misiones/{id}");
        Assert.Equal("Despachada", estado.GetProperty("estado").GetString());

        // El diario cita el permiso y el calendario contra el que se juzgó: sin las dos
        // cosas, reconstruir la decisión dentro de dos años es imposible.
        var despacho = estado.GetProperty("diario").EnumerateArray()
            .Single(t => t.GetProperty("id").GetString() == "T-12");
        var motivo = despacho.GetProperty("motivo").GetString()!;
        Assert.Contains("SC-000042", motivo);
        Assert.Contains("PROVISIONAL-SIN-FERIADOS", motivo);
    }

    /// <summary>Del viernes 20 al domingo 22 de marzo de 2026 — cruza el fin de semana.</summary>
    private static async Task CrearYAprobarEnFinDeSemana(HttpClient cliente, string id)
    {
        await cliente.PostAsJsonAsync("/misiones", new
        {
            Id = id,
            CapturadaPor = "P-ASISTENTE",
            SolicitanteDeDerecho = "P-ASISTENTE",
            Dependencia = "Delegacion de Choluteca",
            ObjetoDelTraslado = "Traslado de personal",
            Destino = "Choluteca",
            Salida = new DateOnly(2026, 3, 20),
            Retorno = new DateOnly(2026, 3, 22),
            HoraDeSalida = new TimeOnly(8, 0),
            HoraDeRetorno = new TimeOnly(16, 0),
            HolguraDias = 1,
            Momento,
        });

        await cliente.PostAsJsonAsync($"/misiones/{id}/enviar", new { Ejecuta = "P-ASISTENTE", Momento });
        await cliente.PostAsJsonAsync($"/misiones/{id}/aprobar", new { Ejecuta = "P-JEFATURA", Momento });
    }

    [Fact]
    public async Task Una_mision_fuera_de_la_ventana_no_aparece()
    {
        // Sin esto, la pantalla de una semana mostraría la ocupación de todo el año y el
        // dibujo dejaría de decir nada. El recorte va en SQL: el diario crece para
        // siempre y traerlo entero para descartar en memoria sería peor cada año.
        var r = await Sembrar("OC-0003");
        var idMision = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearYAprobar(cliente, idMision);
        await Programar(cliente, idMision, r);

        // Una semana de abril: la misión es del 20 al 22 de marzo.
        var barras = await BarrasDe(cliente, r.Vehiculo, "2026-04-06", "2026-04-12");

        Assert.Empty(barras);
    }

    [Fact]
    public async Task Un_rango_invertido_se_rechaza_en_vez_de_pasar_por_flota_libre()
    {
        // Devolver cero carriles ante un rango al revés haría pasar una petición mal
        // armada por «no hay nada ocupado» — que es la respuesta que lleva a asignar un
        // vehículo que ya está tomado.
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.GetAsync("/flota/ocupacion?desde=2026-03-24&hasta=2026-03-18");

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    private static async Task<JsonElement[]> BarrasDe(
        HttpClient cliente,
        string idVehiculo,
        string desde = Desde,
        string hasta = Hasta)
    {
        var cuerpo = await cliente.GetFromJsonAsync<JsonElement>(
            $"/flota/ocupacion?desde={desde}&hasta={hasta}");

        var carril = cuerpo.GetProperty("carriles")
            .EnumerateArray()
            .Single(c => c.GetProperty("vehiculo").GetString() == idVehiculo);

        return [.. carril.GetProperty("barras").EnumerateArray()];
    }

    /// <summary>
    /// `T-12`. <b>No manda recursos y es correcto</b>: despachar revalida sobre lo que ya
    /// se reservó en `T-08`. Volver a tomar acá dejaría dos reservas en el diario para la
    /// misma misión, y la segunda no libera a la primera.
    /// </summary>
    private static async Task Despachar(HttpClient cliente, string idMision, FlotaSembrada.ParaProgramar r)
    {
        var respuesta = await cliente.PostAsJsonAsync($"/misiones/{idMision}/despachar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            IdVehiculo = r.Vehiculo,
            IdConductor = r.Conductor,
        });

        respuesta.EnsureSuccessStatusCode();
    }

    private static async Task Programar(HttpClient cliente, string idMision, FlotaSembrada.ParaProgramar r)
    {
        var respuesta = await cliente.PostAsJsonAsync($"/misiones/{idMision}/programar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            IdVehiculo = r.Vehiculo,
            IdConductor = r.Conductor,
        });

        respuesta.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Un pick-up <b>y su motorista</b>, los dos propios de la prueba.
    ///
    /// `BD-11` bloquea el solapamiento de vehículo <b>y</b> de motorista. Compartir
    /// cualquiera de los dos entre pruebas que usan la misma franja es una doble asignación
    /// real, no un artefacto del entorno — y el bloqueo tendría razón.
    /// </summary>
    /// <summary>
    /// Un motorista libre, sin vehículo. Lo piden las pruebas que necesitan comprobar que
    /// el choque es por el <b>vehículo</b>: con el mismo motorista no se podría distinguir.
    /// </summary>
    private async Task<Ulid> OtroMotorista(string nombre)
    {
        await using var contexto = baseDePruebas.Contexto();
        return await FlotaSembrada.NuevoConductorAsync(contexto, nombre);
    }

    private async Task<FlotaSembrada.ParaProgramar> Sembrar(string siglas)
    {
        await using var contexto = baseDePruebas.Contexto();
        return await FlotaSembrada.ParaProgramarAsync(contexto, siglas);
    }

    private static async Task CrearYAprobar(HttpClient cliente, string id)
    {
        await cliente.PostAsJsonAsync("/misiones", new
        {
            Id = id,
            CapturadaPor = "P-ASISTENTE",
            SolicitanteDeDerecho = "P-ASISTENTE",
            Dependencia = "Delegacion de Choluteca",
            ObjetoDelTraslado = "Traslado de personal",
            Destino = "Choluteca",
            Salida = new DateOnly(2026, 3, 16),
            Retorno = new DateOnly(2026, 3, 18),
            HoraDeSalida = new TimeOnly(8, 0),
            HoraDeRetorno = new TimeOnly(16, 0),
            HolguraDias = 1,
            Momento,
        });

        await cliente.PostAsJsonAsync($"/misiones/{id}/enviar", new { Ejecuta = "P-ASISTENTE", Momento });
        await cliente.PostAsJsonAsync($"/misiones/{id}/aprobar", new { Ejecuta = "P-JEFATURA", Momento });
    }
}
