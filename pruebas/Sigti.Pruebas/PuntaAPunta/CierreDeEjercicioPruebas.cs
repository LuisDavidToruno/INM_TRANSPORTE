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
    private WebApplicationFactory<Program> Aplicacion() => FabricaDeSigti.Crear(baseDePruebas);

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

        await SembrarCortesAsync(anio);

        var mision = await SembrarMisionAsync(anio, EstadoDeMision.EnRuta, "En ruta al corte");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

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

        await SembrarCortesAsync(anio);

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        await Post(cliente, "/cierre-de-ejercicio", Cuerpo($"AC-{anio}-001", anio));

        var segunda = await cliente.PostComoAsync(
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

        await SembrarCortesAsync(anio);
        await SembrarVentanaAsync(anio, dias: 15);

        await SembrarMisionAsync(anio, EstadoDeMision.Cerrada, motivo,
            cierre: new DateTime(anio, 12, 30, 16, 40, 0));

        await SembrarMisionAsync(anio, EstadoDeMision.Cerrada, motivo,
            cierre: new DateTime(anio, 12, 30, 16, 41, 0));

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

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

        await SembrarCortesAsync(anio);
        await SembrarVentanaAsync(anio, dias: 15);

        await SembrarMisionAsync(anio, EstadoDeMision.Cerrada,
            "Bitácora conciliada: 412 km, 11.4 gal, desviación 1.8% dentro de tolerancia",
            cierre: new DateTime(anio, 12, 29, 10, 0, 0));

        await SembrarMisionAsync(anio, EstadoDeMision.Cerrada,
            "Retorno con 38 km menos por desvío declarado en La Barca, autorizado por ACT-05",
            cierre: new DateTime(anio, 12, 29, 11, 0, 0));

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var acta = await VistaPrevia(cliente, anio);

        // **Primero: que la ventana esté.** Sin este aserto la prueba pasaría igual con el
        // parámetro sin cargar, y estaría verificando «no se buscaron motivos» en vez de «se
        // buscaron y no había». Son cosas distintas y solo una es la regla.
        Assert.False(acta.GetProperty("ventana").ValueKind is JsonValueKind.Null);

        Assert.Empty(acta.GetProperty("motivosCompartidos").EnumerateArray());
    }

    // ── Las fechas de corte son parámetros con vigencia ─────────────────────

    /// <summary>
    /// `RN-96` declara las dos fechas configurables con vigencia. El acta las toma del parámetro
    /// y <b>declara de qué versiones salieron</b>.
    /// </summary>
    [Fact]
    public async Task Los_cortes_salen_del_parametro_y_el_acta_declara_su_origen()
    {
        const int anio = 2041;

        // Una institución que cierra el 30 de junio con diez días de ventana operativa. Si
        // hubiera un «31 de diciembre» escondido, esto lo delataría.
        await SembrarCortesAsync(anio, diaYMes: "06-30", diasDespues: 10);

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var acta = await VistaPrevia(cliente, anio);

        Assert.Equal($"{anio}-06-30", acta.GetProperty("corteLegal").GetString());
        Assert.Equal($"{anio}-07-10", acta.GetProperty("corteOperativo").GetString());

        var origen = acta.GetProperty("origenDeLosCortes").GetString()!;
        Assert.Contains("06-30", origen);
        Assert.Contains("10 días", origen);
    }

    /// <summary>
    /// <b>Sin los cortes parametrizados no hay acta.</b>
    ///
    /// A diferencia de la ventana —que apaga dos reportes— los cortes deciden qué expedientes
    /// entran al inventario y a qué ejercicio se imputa cada hecho: un acta producida con fechas
    /// supuestas afirmaría cosas falsas sobre todo lo demás.
    /// </summary>
    [Fact]
    public async Task Sin_los_cortes_parametrizados_el_acta_NO_se_arma()
    {
        // Un ejercicio que ninguna prueba siembra.
        const int anio = 2042;

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var respuesta = await cliente.GetAsync($"/cierre-de-ejercicio/{anio}/vista-previa");

        Assert.False(respuesta.IsSuccessStatusCode);

        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("no están parametrizadas", cuerpo);
        Assert.Contains("falsearía todo lo demás", cuerpo);
    }

    [Fact]
    public async Task Sin_los_cortes_parametrizados_el_acta_tampoco_se_produce()
    {
        const int anio = 2043;

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var respuesta = await cliente.PostComoAsync(
            "/cierre-de-ejercicio", Cuerpo($"AC-{anio}-001", anio));

        Assert.False(respuesta.IsSuccessStatusCode);
        Assert.Contains("no están parametrizadas", await respuesta.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// La vista previa <b>sí</b> admite cortes impuestos —es «qué pasaría si»— y el acta lo
    /// declara para que esa respuesta no se confunda con el cierre real.
    ///
    /// <b>Producir con cortes impuestos no se puede</b>: el endpoint ni siquiera los recibe.
    /// </summary>
    [Fact]
    public async Task La_vista_previa_con_cortes_impuestos_se_declara_exploratoria()
    {
        const int anio = 2044;
        await SembrarCortesAsync(anio);

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var acta = await Leer(cliente,
            $"/cierre-de-ejercicio/{anio}/vista-previa" +
            $"?corteLegal={anio}-09-30&corteOperativo={anio}-10-05");

        Assert.Equal($"{anio}-09-30", acta.GetProperty("corteLegal").GetString());

        Assert.Contains("NO son los parámetros de la institución",
            acta.GetProperty("origenDeLosCortes").GetString());
    }

    /// <summary>
    /// Los cortes se consultan aparte para que la pantalla pueda mostrarlos antes de armar nada.
    /// </summary>
    [Fact]
    public async Task Los_cortes_del_ejercicio_se_pueden_consultar_solos()
    {
        const int anio = 2045;
        await SembrarCortesAsync(anio, diaYMes: "12-31", diasDespues: 20);

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var conCortes = await Leer(cliente, $"/cierre-de-ejercicio/{anio}/cortes");

        Assert.Equal($"{anio + 1}-01-20",
            conCortes.GetProperty("cortes").GetProperty("operativo").GetString());

        Assert.True(conCortes.GetProperty("sinCortes").ValueKind is JsonValueKind.Null);

        // Y el ejercicio sin parámetros dice por qué, en vez de devolver una fecha inventada.
        var sinCortes = await Leer(cliente, "/cierre-de-ejercicio/2046/cortes");

        Assert.True(sinCortes.GetProperty("cortes").ValueKind is JsonValueKind.Null);

        Assert.Contains("no hay versión aprobada",
            sinCortes.GetProperty("sinCortes").GetProperty("porQueNo").GetString());
    }

    // ── La ventana de cierre es parámetro con vigencia ──────────────────────

    /// <summary>
    /// `RN-96` la declara configurable con vigencia, y la ventana <b>dice de qué versión
    /// salió</b>. Un indicador que no declara contra qué ventana se midió no se puede reproducir
    /// ni discutir años después.
    /// </summary>
    [Fact]
    public async Task La_ventana_sale_del_parametro_y_el_acta_declara_su_origen()
    {
        const int anio = 2040;

        await SembrarCortesAsync(anio);
        await SembrarVentanaAsync(anio, dias: 40);

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var acta = await VistaPrevia(cliente, anio);
        var ventana = acta.GetProperty("ventana");

        Assert.Equal($"{anio}-11-21", ventana.GetProperty("desde").GetString());
        Assert.Equal(40 + 15 + 1, ventana.GetProperty("dias").GetInt32());
        Assert.Contains("40 días", ventana.GetProperty("origen").GetString());

        Assert.True(acta.GetProperty("sinVentana").ValueKind is JsonValueKind.Null);
    }

    /// <summary>
    /// <b>Sin el parámetro cargado no hay ventana por omisión</b>, y los dos reportes que
    /// dependen de ella salen <b>sin medir</b> — que no es lo mismo que salir limpios.
    ///
    /// Es la disciplina que este sistema aplica al rendimiento esperado y al horario hábil:
    /// <i>«suponer uno produciría hallazgos falsos que en tres meses nadie miraría — que es como
    /// muere un control»</i>. Acá el riesgo es el simétrico y peor: un cierre en bloque que no
    /// aparece porque nadie configuró contra qué buscarlo.
    /// </summary>
    [Fact]
    public async Task Sin_el_parametro_los_reportes_de_la_ventana_salen_SIN_MEDIR()
    {
        // Un ejercicio que ninguna prueba siembra. Las siembras cierran su vigencia al 31 de
        // diciembre de su año, así que ninguna alcanza a éste.
        const int anio = 2030;
        const string motivo = "Cierre de ejercicio fiscal, sin observaciones";

        // Los cortes sí, la ventana no: sin cortes el acta no se arma siquiera, y lo que esta
        // prueba mira es lo que pasa cuando falta **solo** la ventana.
        await SembrarCortesAsync(anio);

        await SembrarMisionAsync(anio, EstadoDeMision.Cerrada, motivo,
            cierre: new DateTime(anio, 12, 30, 16, 40, 0));

        await SembrarMisionAsync(anio, EstadoDeMision.Cerrada, motivo,
            cierre: new DateTime(anio, 12, 30, 16, 41, 0));

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var acta = await VistaPrevia(cliente, anio);

        Assert.True(acta.GetProperty("ventana").ValueKind is JsonValueKind.Null);
        Assert.True(acta.GetProperty("apuro").ValueKind is JsonValueKind.Null);

        var sin = acta.GetProperty("sinVentana");
        Assert.Equal("cierre.ventana_de_cierre_dias", sin.GetProperty("clave").GetString());
        Assert.Contains("no hay versión aprobada", sin.GetProperty("porQueNo").GetString());

        // **Hay dos misiones con el mismo motivo, y la lista sale vacía.** El acta no puede
        // dejar que eso se lea como que no hubo hallazgo.
        Assert.Empty(acta.GetProperty("motivosCompartidos").EnumerateArray());

        Assert.Contains(acta.GetProperty("observaciones").EnumerateArray(),
            o => o.GetString()!.Contains("están sin medir, no en cero"));
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

        await SembrarCortesAsync(anio);

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var folio = await SembrarValeEmitidoAsync(cliente, anio);

        var vista = await VistaPrevia(cliente, anio);

        var listado = Assert.Single(vista.GetProperty("foliosPorAnular").EnumerateArray(), f => f.GetProperty("folio").GetString() == folio);

        Assert.True(listado.GetProperty("sePuedeAnular").GetBoolean());
        Assert.Equal("Emitida", listado.GetProperty("estado").GetString());

        await Post(cliente, "/cierre-de-ejercicio", Cuerpo($"AC-{anio}-001", anio));

        // ── Y recién ahora se anula ─────────────────────────────────────────
        var respuesta = await cliente.PostComoAsync(
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

    // ── El reporte de reversión para ARGOS y SIAFI ──────────────────────────

    /// <summary>
    /// `RN-96` punto 5 y `RN-81` — el circuito entero, hasta el archivo de conciliación.
    ///
    /// ── Y lo que NO reporta antes de anular ─────────────────────────────────
    /// Un folio listado y todavía sin anular <b>no liberó nada</b>: su compromiso sigue vivo en
    /// SIGTI, y reportarlo haría que SIAFI revirtiera un dinero que acá sigue comprometido — el
    /// descuadre simétrico del que `RN-81` existe para impedir.
    /// </summary>
    [Fact]
    public async Task El_compromiso_liberado_se_reporta_recien_cuando_el_folio_se_anula()
    {
        const int anio = 2028;

        await SembrarCortesAsync(anio);

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var folio = await SembrarValeEmitidoAsync(cliente, anio);

        await Post(cliente, "/cierre-de-ejercicio", Cuerpo($"AC-{anio}-001", anio));

        // El corte de conocimiento va explícito y posterior a la anulación. Sin él se toma
        // «ahora», y una anulación registrada con fecha de enero del año siguiente queda fuera
        // del corte de hoy —correctamente— y el reporte saldría vacío por otra razón.
        var corte = $"corteDeConocimiento={anio + 1}-02-01T00:00:00Z";

        // ── Con el acta producida pero sin anular: nada que revertir ─────────
        var antes = await Leer(cliente, $"/cierre-de-ejercicio/{anio}/reversion?{corte}");

        Assert.Empty(antes.GetProperty("renglones").EnumerateArray());
        Assert.Equal(0m, antes.GetProperty("totalLiberado").GetDecimal());

        // `RN-94` — las dos fechas van igual, aunque no haya renglones.
        Assert.Equal($"{anio}-12-31", antes.GetProperty("periodoDesde").GetString());
        Assert.Equal($"AC-{anio}-001", antes.GetProperty("actaQueLoRespalda").GetString());

        await Post(cliente, $"/cierre-de-ejercicio/{anio}/anular-folios", new
        {
            Persona = "P-ADMIN",
            Motivo = $"Folio no consumido al cierre; no se arrastra a {anio + 1}",
            Momento = new DateTimeOffset(anio + 1, 1, 10, 9, 0, 0, TimeSpan.FromHours(-6)),
        });

        // ── Y ahora sí ──────────────────────────────────────────────────────
        var despues = await Leer(cliente, $"/cierre-de-ejercicio/{anio}/reversion?{corte}");

        var renglon = Assert.Single(despues.GetProperty("renglones").EnumerateArray(),
            r => r.GetProperty("folio").GetString() == folio);

        Assert.Equal(1_500m, renglon.GetProperty("comprometido").GetDecimal());

        // El vale se anuló desde `Emitida`, así que no se ejecutó nada y libera entero. Se
        // calcula igual, para que el día que llegue uno con consumo el reporte lo diga.
        Assert.Equal(0m, renglon.GetProperty("ejecutado").GetDecimal());
        Assert.Equal(1_500m, renglon.GetProperty("liberado").GetDecimal());

        Assert.Equal("12-01-001-4-31200", renglon.GetProperty("objetoDelGasto").GetString());
        Assert.True(renglon.GetProperty("seConcilia").GetBoolean());

        // El detalle por objeto del gasto que `RN-81` punto 4 pide para conciliar.
        Assert.Equal(1_500m, despues.GetProperty("porObjetoDelGasto")
            .GetProperty("12-01-001-4-31200").GetDecimal());
    }

    /// <summary>
    /// El archivo de conciliación — `RN-96` punto 5.
    ///
    /// ⚠️ <b>No es el formato de SIAFI.</b> `RN-81` punto 3: sin contrato de API conocido el
    /// mecanismo inicial es el reporte con formato acordado, y este CSV es el mínimo que se
    /// puede conciliar a mano.
    /// </summary>
    [Fact]
    public async Task El_archivo_de_conciliacion_sale_con_una_fila_por_compromiso_liberado()
    {
        const int anio = 2029;

        await SembrarCortesAsync(anio);

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var folio = await SembrarValeEmitidoAsync(cliente, anio);

        await Post(cliente, "/cierre-de-ejercicio", Cuerpo($"AC-{anio}-001", anio));

        await Post(cliente, $"/cierre-de-ejercicio/{anio}/anular-folios", new
        {
            Persona = "P-ADMIN",
            Motivo = "Folio no consumido al cierre",
            Momento = new DateTimeOffset(anio + 1, 1, 10, 9, 0, 0, TimeSpan.FromHours(-6)),
        });

        var respuesta = await cliente.GetAsync(
            $"/cierre-de-ejercicio/{anio}/reversion.csv" +
            $"?corteDeConocimiento={anio + 1}-02-01T00:00:00Z");

        Assert.True(respuesta.IsSuccessStatusCode);
        Assert.Equal("text/csv", respuesta.Content.Headers.ContentType?.MediaType);

        var csv = await respuesta.Content.ReadAsStringAsync();
        var lineas = csv.Split('\n');

        Assert.Contains("clave_de_vinculacion", lineas[0]);

        var fila = Assert.Single(lineas.Skip(1), l => l.Contains(folio));

        // Las dos fechas de `RN-94` van en la fila, no en un bloque de metadatos que una hoja
        // de cálculo pierde al ordenar.
        Assert.Contains($"{anio}-12-31", fila);
        Assert.Contains($"AC-{anio}-001", fila);

        // Monto invariante: lo lee otro sistema.
        Assert.Contains("1500.00", fila);
    }

    /// <summary>
    /// `RN-94` — <b>el corte de conocimiento es lo que hace el reporte reproducible.</b>
    ///
    /// El mismo período con un corte anterior a la anulación no la ve; con uno posterior, sí. Un
    /// reporte que cambiara de valor sin que cambiara ninguno de sus dos parámetros sería
    /// <i>«un defecto, no una actualización»</i>.
    /// </summary>
    [Fact]
    public async Task El_reporte_con_corte_anterior_a_la_anulacion_no_la_ve()
    {
        const int anio = 2048;

        await SembrarCortesAsync(anio);

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var folio = await SembrarValeEmitidoAsync(cliente, anio);

        await Post(cliente, "/cierre-de-ejercicio", Cuerpo($"AC-{anio}-001", anio));

        await Post(cliente, $"/cierre-de-ejercicio/{anio}/anular-folios", new
        {
            Persona = "P-ADMIN",
            Motivo = "Folio no consumido al cierre",
            Momento = new DateTimeOffset(anio + 1, 1, 10, 9, 0, 0, TimeSpan.FromHours(-6)),
        });

        // Al 5 de enero todavía no se había anulado: el compromiso seguía vivo.
        var antes = await Leer(cliente,
            $"/cierre-de-ejercicio/{anio}/reversion" +
            $"?corteDeConocimiento={anio + 1}-01-05T00:00:00Z");

        Assert.Empty(antes.GetProperty("renglones").EnumerateArray());

        // Al 1 de febrero sí. Mismo período, otro corte, otro resultado — y las dos respuestas
        // son correctas, cada una a su pregunta.
        var despues = await Leer(cliente,
            $"/cierre-de-ejercicio/{anio}/reversion" +
            $"?corteDeConocimiento={anio + 1}-02-01T00:00:00Z");

        Assert.Single(despues.GetProperty("renglones").EnumerateArray(),
            r => r.GetProperty("folio").GetString() == folio);

        // Y volver a pedir el primero da lo mismo que la primera vez: es la reproducibilidad
        // que `RN-94` exige, no una foto que envejece.
        var otraVez = await Leer(cliente,
            $"/cierre-de-ejercicio/{anio}/reversion" +
            $"?corteDeConocimiento={anio + 1}-01-05T00:00:00Z");

        Assert.Empty(otraVez.GetProperty("renglones").EnumerateArray());
    }

    /// <summary>
    /// Sin acta no hay reversión: la reversión reporta lo que un acta listó y se anuló.
    /// </summary>
    [Fact]
    public async Task Sin_acta_no_hay_reporte_de_reversion()
    {
        var respuesta = await Aplicacion().CrearCliente()
            .GetAsync("/cierre-de-ejercicio/2047/reversion");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.Contains("sin acta no hay nada", await respuesta.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// Sin acta no se anulan folios. Los folios se anulan <b>citando el acta que los listó</b>:
    /// sin ella no consta que fueran los que quedaron reservados y sin consumir al corte.
    /// </summary>
    [Fact]
    public async Task Anular_folios_sin_acta_no_pasa()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var respuesta = await cliente.PostComoAsync(
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

        await SembrarCortesAsync(anio);
        await SembrarVentanaAsync(anio, dias: 15);

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
        using var cliente = aplicacion.CrearCliente();

        var acta = await VistaPrevia(cliente, anio);

        // La ventana tiene que estar: sin ella el reporte sale vacio por falta de parametro y
        // la prueba pasaria --o fallaria-- por una razon que no es la regla.
        Assert.False(acta.GetProperty("ventana").ValueKind is JsonValueKind.Null);

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

        await SembrarCortesAsync(anio);
        await SembrarVentanaAsync(anio, dias: 15);

        await using (var contexto = baseDePruebas.Contexto())
        {
            contexto.Parametros.Add(Version(clave, "10",
                new DateOnly(anio, 3, 1),
                new DateTimeOffset(anio, 3, 1, 9, 0, 0, TimeSpan.Zero)));

            await contexto.SaveChangesAsync();
        }

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var acta = await VistaPrevia(cliente, anio);

        Assert.False(acta.GetProperty("ventana").ValueKind is JsonValueKind.Null);

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

        await SembrarCortesAsync(anio);

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        await Post(cliente, "/saldo-de-apertura", new
        {
            Id = Ulid.NewUlid().ToString(),
            Folio = $"SA-{anio}-001",
            Ejercicio = $"{anio}",
            Corte = corte,
            Persona = "P-AUDITORIA",
            Puesto = "PU-AUDITORIA",
            Momento = new DateTimeOffset(anio + 1, 1, 5, 9, 0, 0, TimeSpan.FromHours(-6)),

            // ── Declarados, porque desde M-12 el bloqueo dispara ─────────────
            // Esta prueba producía el saldo sin declarar nada, y pasaba porque las
            // interrupciones sin desenlace no existían como registro. Al construirse M-12
            // empezó a fallar **con razón**: `RN-97` punto 4 no deja cerrar el período con
            // ellas vivas. Declararlas con motivo es lo que la regla prevé, y es además lo
            // realista: en una institución siempre hay algo abierto al corte.
            DeclaracionDeBloqueantes =
                "Producido para verificar el cuadre contra el acta de cierre; los pendientes " +
                "vivos al corte se declaran y siguen su curso.",
        });

        var vista = await VistaPrevia(cliente, anio);

        // Producido el mismo día contra el mismo corte, cuadra. Lo que importa es que la
        // comprobación **corra**: la diferencia aparece cuando alguien edita uno de los dos.
        Assert.Empty(vista.GetProperty("diferenciasConElSaldo").EnumerateArray());

        var respuesta = await cliente.PostComoAsync(
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
    /// Carga la ventana de cierre del ejercicio — `RN-96`, <b>parámetro con vigencia</b>.
    ///
    /// ── Hay que sembrarla, y eso es el punto ────────────────────────────────
    /// Sin este parámetro, ni los motivos compartidos ni el ritmo de cierre se evalúan. Que las
    /// pruebas tengan que cargarlo es la prueba de que no hay valor por omisión escondido: si lo
    /// hubiera, todas pasarían sin sembrar nada.
    ///
    /// Se carga con vigencia desde el 1 de enero <b>del ejercicio</b>, que es contra lo que se
    /// resuelve — no contra hoy.
    ///
    /// ── Y se cierra al 31 de diciembre del mismo ejercicio ──────────────────
    /// En producción la ventana rige hasta que alguien la cambie. Acá se acota <b>para que los
    /// años que ninguna prueba siembra queden de verdad sin ventana</b>: con vigencia abierta,
    /// la siembra de una prueba resolvería el cierre de todas las de años posteriores, y la que
    /// comprueba la ausencia pasaría o fallaría según el orden de ejecución.
    ///
    /// ── Cargada por anticipado, y los dos ejes explican por qué ─────────────
    /// La vigencia normativa empieza el 1 de enero del ejercicio, pero el <b>registro</b> va en
    /// 2020: es cuándo se supo, y tiene que ser anterior al instante desde el que se mira. La
    /// vista previa mira desde hoy, y una versión registrada en 2040 todavía no se conoce hoy —
    /// no por un defecto, sino porque eso es exactamente lo que el eje de transacción significa.
    /// Salió al correr estas pruebas contra ejercicios futuros.
    ///
    /// Y de paso lo deja fuera de la ventana de cierre, donde contaminaría el reporte de
    /// `RN-96` punto 6 de la propia prueba.
    /// </summary>
    private Task SembrarVentanaAsync(int anio, int dias) =>
        SembrarParametroAsync(anio, "cierre.ventana_de_cierre_dias", $"{dias}");

    /// <summary>
    /// Las dos fechas de corte del ejercicio — `RN-96`, parámetros con vigencia.
    ///
    /// <b>Sin esto no se arma ni se produce ninguna acta</b>, y por eso todas las pruebas lo
    /// siembran: si hubiera un «31 de diciembre» por omisión escondido, pasarían sin sembrar nada.
    /// </summary>
    private async Task SembrarCortesAsync(int anio, string diaYMes = "12-31", int diasDespues = 15)
    {
        await SembrarParametroAsync(anio, "cierre.corte_legal_dia_y_mes", diaYMes);
        await SembrarParametroAsync(anio, "cierre.corte_operativo_dias_despues", $"{diasDespues}");
    }

    private async Task SembrarParametroAsync(int anio, string clave, string valor)
    {
        await using var contexto = baseDePruebas.Contexto();

        contexto.Parametros.Add(new VersionDeParametro(
            clave,
            valor,
            new DateOnly(anio, 1, 1),
            new DateOnly(anio, 12, 31),
            new DateTimeOffset(2020, 1, 2, 8, 0, 0, TimeSpan.Zero),
            null,
            new IdPersona("P-ADMIN"),
            new IdPersona("P-GERENCIA"))
        {
            Respaldo = new RespaldoDocumental(
                Ulid.NewUlid(), "Acuerdo interno de prueba", new DateOnly(2020, 1, 2)),
        });

        await contexto.SaveChangesAsync();
    }

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
        {
            flota = await FlotaSembrada.ParaProgramarAsync(contexto, $"CE{anio % 100}");

            // ── Los documentos se renuevan, no se debilitan los bloqueos ─────
            // El motorista sembrado trae licencia hasta abril de 2028 y el vehículo matrícula
            // hasta diciembre de 2030; estas pruebas cierran ejercicios posteriores. `BD-02` y
            // `BD-03` bloquean programar más allá de esas vigencias —con razón, y ya lo hicieron
            // tres veces acá—. Lo que corresponde es que el motorista y el vehículo de la prueba
            // tengan documentos para el año que la prueba usa, no aflojar los bloqueos.
            //
            // Los dos campos son de solo inicialización a propósito —la ficha no se edita en
            // caliente— así que se actualizan por consulta, que es lo que haría una renovación
            // real en su propio circuito.
            var conductor = Ulid.Parse(flota.Conductor);
            var vehiculo = Ulid.Parse(flota.Vehiculo);

            await contexto.Conductores
                .Where(c => c.Id == conductor)
                .ExecuteUpdateAsync(c =>
                    c.SetProperty(x => x.VenceLicencia, new DateOnly(anio + 2, 4, 30)));

            await contexto.Vehiculos
                .Where(v => v.Id == vehiculo)
                .ExecuteUpdateAsync(v =>
                    v.SetProperty(x => x.VenceMatricula, new DateOnly(anio + 2, 12, 31)));
        }

        // Cinco días antes de la salida. La aprobación **caduca** si la ventana solicitada ya
        // inició —«pida a la dependencia una solicitud nueva»— y capturar el mismo día de la
        // salida la deja caducada al llegar a programar. Salió al mover las fechas a días
        // hábiles.
        var salida = LunesDeDiciembre(anio);

        var momento = new DateTimeOffset(
            salida.AddDays(-5).ToDateTime(new TimeOnly(9, 0)), TimeSpan.FromHours(-6));
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
            Salida = salida,
            Retorno = salida.AddDays(2),
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

    /// <summary>
    /// Un lunes de diciembre, para que la misión salga el lunes y retorne el miércoles.
    ///
    /// ── `BD-04` bloquea la franja inhábil, y hace bien ──────────────────────
    /// El andamio fijaba el 20 al 22 de diciembre, que en unos años cae en fin de semana:
    /// despachar exige entonces permiso de la máxima autoridad. Estas pruebas juzgan el cierre
    /// de ejercicio, no `BD-04`, así que el andamio elige días hábiles en vez de pedir un
    /// salvoconducto que la prueba no está probando.
    /// </summary>
    private static DateOnly LunesDeDiciembre(int anio)
    {
        var dia = new DateOnly(anio, 12, 10);

        while (dia.DayOfWeek != DayOfWeek.Monday) dia = dia.AddDays(1);

        return dia;
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

    /// <summary>
    /// El cuerpo para producir. <b>Ya no lleva fechas de corte</b>: salen de los parámetros de la
    /// institución, porque un acta producida contra un corte que alguien escribió en el momento
    /// afirmaría sobre todo lo demás contra un criterio que nadie autorizó.
    /// </summary>
    private static object Cuerpo(string folio, int anio) => new
    {
        Folio = folio,
        Ejercicio = $"{anio}",
        Persona = "P-ADMIN",
        Puesto = "PU-GERENCIA",
        Momento = new DateTimeOffset(anio + 1, 1, 16, 9, 0, 0, TimeSpan.FromHours(-6)),
    };

    /// <summary>
    /// La vista previa <b>sin imponer cortes</b>: los toma del parámetro, que es lo que hace la
    /// pantalla. Pasarlos sería explorar, y entonces el acta lo declara.
    /// </summary>
    private static Task<JsonElement> VistaPrevia(HttpClient cliente, int anio) =>
        Leer(cliente, $"/cierre-de-ejercicio/{anio}/vista-previa");

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
