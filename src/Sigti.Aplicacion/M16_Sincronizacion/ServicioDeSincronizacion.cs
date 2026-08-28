using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Datos.M07_ProgramacionYDespacho;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Aplicacion.M08_Bitacora;
using Sigti.Dominio.M08_Bitacora;
using Sigti.Dominio.Organizacion;
using Sigti.Datos.M09_Combustible;
using Sigti.Dominio.M09_Combustible;
using Sigti.Aplicacion.M09_Combustible;
using Sigti.Aplicacion.M07_ProgramacionYDespacho;

namespace Sigti.Aplicacion.M16_Sincronizacion;

/// <summary>Un hecho que el dispositivo capturó sin red y ahora entrega.</summary>
/// <param name="IdDeCaptura">
/// Lo generó el dispositivo (`ADR-005`). <b>Es la identidad del hecho</b>, y lo que hace
/// que reenviarlo sea inofensivo.
/// </param>
/// <param name="OcurridoEn">
/// La <b>fecha del hecho</b>, no la de captura ni la de llegada (`P-4`, `RN-46`). Puede
/// ser de hace cuatro días: es exactamente para lo que existe este endpoint.
/// </param>
public sealed record HechoCapturado(
    Ulid IdDeCaptura,
    Ulid IdExpediente,
    string Transicion,
    string Ejecuta,
    DateTimeOffset OcurridoEn,
    /// <summary>
    /// La lectura del odómetro. <b>Obligatoria para `T-14` y `T-18`</b>, y anulable acá
    /// porque el tipo tiene que poder representar un hecho mal armado para <b>rechazarlo con
    /// un motivo legible</b> — el dispositivo no lo puede resolver reintentando.
    /// </summary>
    int? Odometro = null,
    /// <summary>
    /// Ordinario o constatado — `T-18`. La diferencia decide si una lectura menor que la de
    /// salida bloquea o se registra con la inconsistencia marcada (`RN-79`, `HB3-04`).
    /// </summary>
    SubtipoDeRetorno Subtipo = SubtipoDeRetorno.Ordinario,
    string? Justificacion = null,
    /// <summary>
    /// El vale contra el que se consume — <b>obligatorio para `V-04`</b>, nulo para todo lo
    /// demás.
    ///
    /// Va aparte del expediente porque <b>una misión lleva varios vales</b>: mandar sólo la
    /// misión obligaría al servidor a adivinar a cuál imputar el galón, y adivinar sobre
    /// dinero es exactamente lo que el folio existe para impedir.
    /// </summary>
    Ulid? IdAsignacion = null,
    /// <summary>
    /// La carga. Nula salvo en `V-04`, y anulable por la misma razón que el odómetro: el
    /// tipo tiene que poder representar un hecho mal armado para <b>rechazarlo con motivo
    /// legible</b>.
    /// </summary>
    CargaSincronizada? Carga = null,
    /// <summary>
    /// El ingreso de combustible que <b>no salió del vale</b> — `A-01`, `RN-83`.
    ///
    /// Viaja por el mismo canal que las transiciones porque eso es lo que da <b>una sola cola,
    /// una sola idempotencia y un solo acuse</b>. Abrirle un endpoint propio duplicaría los
    /// tres, y son justo los tres que `RNF-03` obliga a que funcionen sin fallo.
    /// </summary>
    AbastecimientoSincronizado? Abastecimiento = null);

/// <summary>Un ingreso de combustible de fuente distinta del fondo — `RN-83`.</summary>
/// <param name="IdVehiculo">
/// A qué tanque entró. <b>Es lo único que no puede faltar</b>: el abastecimiento cuelga del
/// vehículo, no de la misión, porque la regla aplica «en misión o fuera de ella».
/// </param>
public sealed record AbastecimientoSincronizado(
    Ulid IdVehiculo,
    FuenteDeAbastecimiento Fuente,
    decimal Galones,
    int Odometro,
    string Estacion,
    decimal? Monto = null,
    string? Comprobante = null,
    string? CausaSinComprobante = null);

/// <summary>
/// Lo que el motorista tecleó en la estación — los cinco datos de §10.1.
/// </summary>
/// <param name="Comprobante">
/// Nulo es un caso previsto: `RN-85` tipifica la ausencia. <b>El registro del abastecimiento
/// no se omite nunca por falta de papel.</b>
/// </param>
/// <param name="CausaSinComprobante">
/// Por qué no lo hay. El dispositivo la exige antes de dejar capturar; acá se conserva
/// porque es lo que sostiene el descargo alternativo, y perderla en el camino dejaría la
/// ausencia sin explicación en el único lugar donde alguien la va a leer.
/// </param>
public sealed record CargaSincronizada(
    decimal Galones,
    decimal Monto,
    string Estacion,
    int Odometro,
    string? Comprobante = null,
    string? CausaSinComprobante = null);

/// <summary>Qué pasó con cada hecho. El dispositivo lo necesita para depurar su cola.</summary>
public sealed record ResultadoDeSincronizacion(
    IReadOnlyList<Ulid> Aplicadas,
    IReadOnlyList<Ulid> YaConocidas,
    IReadOnlyList<HechoRechazado> Rechazadas);

/// <param name="Motivo">
/// Por qué no entró. <b>El dispositivo no lo puede resolver reintentando</b>, y por eso el
/// motivo tiene que ser legible: alguien va a leerlo en una cola de conflictos.
/// </param>
public sealed record HechoRechazado(Ulid IdDeCaptura, string Motivo);

/// <summary>
/// Recibe lo que un dispositivo capturó sin red.
///
/// ── La propiedad que sostiene todo `RNF-03` ──────────────────────────────────
/// <b>Reenviar es inofensivo.</b> Tiene que serlo, porque el dispositivo que no supo si
/// el servidor recibió <b>va a reenviar</b> — se corta la conexión bajo un puente, el
/// servidor cierra el socket después de aplicar pero antes de acusar, la batería se
/// agota. Si el servidor duplicara, cada corte produciría una transición fantasma en el
/// diario, y el diario es de donde se reconstruye el estado (`P-1`).
///
/// ── Por qué no se comprueba antes de aplicar, sino que se deja chocar ────────
/// La unicidad de `IdDeCaptura` la impone <b>la base</b>. Un `SELECT` previo parece más
/// limpio y es una condición de carrera: dos lotes del mismo dispositivo en vuelo a la
/// vez pasarían los dos la comprobación. El índice único no se equivoca, y no se olvida
/// al escribir el próximo endpoint.
///
/// ── Lo que este servicio NO hace ─────────────────────────────────────────────
/// No resuelve conflictos. `RN-45` es explícita: dos versiones distintas del mismo hecho
/// <b>se conservan las dos</b> y van a cola humana. Detectarlas es del núcleo de campo
/// (`campo/nucleo/Conciliacion.ts`); la cola de resolución es de `M-16` y no está
/// construida.
/// </summary>
public sealed class ServicioDeSincronizacion(SigtiDbContext contexto, ConsultaDeOdometro odometros)
{
    private readonly ExpedientesDeMision _expedientes = new(contexto);
    private readonly CombustibleDeLaInstitucion _combustible = new(contexto);
    private readonly ServicioDeAbastecimientos _abastecimientosDelServicio = new(contexto);

    public async Task<ResultadoDeSincronizacion> RecibirAsync(
        IReadOnlyList<HechoCapturado> hechos,
        CancellationToken cancelacion = default)
    {
        var aplicadas = new List<Ulid>();
        var yaConocidas = new List<Ulid>();
        var rechazadas = new List<HechoRechazado>();

        var conocidas = await YaRecibidasAsync(hechos, cancelacion);

        foreach (var hecho in hechos)
        {
            if (conocidas.Contains(hecho.IdDeCaptura))
            {
                // No es un error: es el reintento normal. Se acusa igual, para que el
                // dispositivo pueda por fin sacarlo de su cola de pendientes.
                yaConocidas.Add(hecho.IdDeCaptura);
                continue;
            }

            // `A-01` no es una transición de nada: es un registro que cuelga del vehículo.
            // Puede llegar **sin misión** —el reabastecimiento de rutina en el predio—, así
            // que ni siquiera se busca el expediente.
            if (hecho.Transicion == "A-01")
            {
                var motivo = await AplicarAbastecimientoAsync(hecho, cancelacion);

                if (motivo is null) aplicadas.Add(hecho.IdDeCaptura);
                else rechazadas.Add(new HechoRechazado(hecho.IdDeCaptura, motivo));

                continue;
            }

            // `V-04` es de otro agregado: el vale. Va por su propio camino, y el expediente
            // sólo se consulta para comprobar que la misión está donde el consumo cabe.
            if (hecho.Transicion == "V-04")
            {
                var resultado = await AplicarConsumoAsync(hecho, cancelacion);

                if (resultado is null) aplicadas.Add(hecho.IdDeCaptura);
                else rechazadas.Add(new HechoRechazado(hecho.IdDeCaptura, resultado));

                continue;
            }

            var expediente = await _expedientes.BuscarAsync(hecho.IdExpediente, cancelacion);

            if (expediente is null)
            {
                rechazadas.Add(new HechoRechazado(
                    hecho.IdDeCaptura,
                    $"El expediente {hecho.IdExpediente} no existe en el servidor. " +
                    "Si se creó en el dispositivo, tiene que sincronizarse antes que sus transiciones."));
                continue;
            }

            try
            {
                // La lectura de referencia se busca por VEHICULO y cruza misiones: el
                // dispositivo solo conoce la suya, y un odometro que retrocede entre dos
                // misiones distintas es justo lo que `BD-05` existe para detectar.
                var ultima = expediente.Diario
                    .Where(t => t.Recursos is not null)
                    .LastOrDefault()?.Recursos is { } recursos
                    ? await odometros.UltimaLecturaAsync(recursos.Vehiculo, cancelacion)
                    : null;

                Aplicar(expediente, hecho, ultima);
                await _expedientes.GuardarAsync(expediente, cancelacion);
                aplicadas.Add(hecho.IdDeCaptura);
            }
            catch (Exception error) when (error is BloqueoDuro or TransicionInvalida)
            {
                // El hecho ya ocurrió en el mundo — el vehículo salió. Que el servidor lo
                // rechace no lo deshace: queda declarado para que alguien lo resuelva, en
                // vez de desaparecer sin rastro.
                rechazadas.Add(new HechoRechazado(hecho.IdDeCaptura, error.Message));
            }
        }

        return new ResultadoDeSincronizacion(aplicadas, yaConocidas, rechazadas);
    }

    /// <summary>
    /// Cuáles de estos hechos <b>ya están</b> en el servidor, en cualquiera de los dos diarios.
    ///
    /// ── Por qué es un punto por hecho y no un `IN (...)` ────────────────────
    /// Porque <b>el `Contains` no traduce</b>. `IdDeCaptura` lleva convertidor de valor —ULID a
    /// `binary(16)`— y con `UseCompatibilityLevel(120)` la traducción de una colección
    /// parametrizada sobre una propiedad convertida <b>devuelve vacío en vez de fallar</b>.
    ///
    /// Eso es peor que un error: la consulta corría, no encontraba nada, y <b>cada reenvío
    /// pasaba por nuevo</b>. En las transiciones de misión no se notaba porque la máquina de
    /// estados frenaba el duplicado —`T-14` sobre una misión ya en ruta es inválida— y el
    /// hecho terminaba en `rechazadas`. Pero un hecho rechazado <b>nunca se acusa</b>, así que
    /// el dispositivo lo reintentaría para siempre: exactamente el fallo que `RNF-03` existe
    /// para impedir. Con `V-04` se vio de golpe, porque un vale admite varias cargas y ahí no
    /// hay máquina de estados que lo frene: el duplicado llegaba hasta el índice único.
    ///
    /// ── El costo, medido contra lo que hay que soportar ─────────────────────
    /// Un lote de siete días son decenas de hechos, y cada comprobación es una búsqueda por
    /// índice único. Traer la tabla entera para filtrar en memoria sí sería caro: el diario de
    /// vales crece con cada carga de la institución.
    /// </summary>
    private async Task<List<Ulid>> YaRecibidasAsync(
        IReadOnlyList<HechoCapturado> hechos, CancellationToken cancelacion)
    {
        var conocidas = new List<Ulid>();

        foreach (var id in hechos.Select(h => h.IdDeCaptura).Distinct())
        {
            var enMisiones = await contexto.Set<FilaDeTransicion>()
                .AnyAsync(t => t.IdDeCaptura == id, cancelacion);

            var enVales = enMisiones || await contexto.Set<FilaDeTransicionDeAsignacion>()
                .AnyAsync(t => t.IdDeCaptura == id, cancelacion);

            // Y el tercer diario: los abastecimientos de `RN-83`, que no viven en ninguno de
            // los otros dos. Sin esto, cada reenvío de una carga del tanque de la sede
            // pasaría por nueva y el galón se contaría de nuevo en el denominador.
            var enAbastecimientos = enVales || await contexto.Abastecimientos
                .AnyAsync(a => a.IdDeCaptura == id, cancelacion);

            if (enMisiones || enVales || enAbastecimientos) conocidas.Add(id);
        }

        return conocidas;
    }

    /// <summary>
    /// `V-04` — la carga que el motorista capturó en la estación, sin red.
    ///
    /// ── Qué revalida el servidor, y por qué no es duplicar el dispositivo ───
    /// El dispositivo comprueba lo que la persona con el surtidor delante puede corregir:
    /// galones, estación, un odómetro que retrocede contra <b>su</b> última lectura. Acá se
    /// comprueba lo que sólo el servidor sabe — que el vale existe, que está entregado, que
    /// quien consume no es quien lo emitió ni quien lo entregó (`BD-06`), y que la misión
    /// está donde el consumo cabe.
    ///
    /// ── Y lo que NO se hace es rechazar por llegar tarde ────────────────────
    /// `RETORNADA` se admite: el consumo se capturó sin conectividad y el vehículo ya volvió
    /// cuando el lote llega. Negarlo perdería el hecho, y `P-2` dice que los hechos
    /// consumados se registran.
    /// </summary>
    /// <returns>Nulo si entró; el motivo del rechazo si no.</returns>
    private async Task<string?> AplicarConsumoAsync(
        HechoCapturado hecho, CancellationToken cancelacion)
    {
        if (hecho.IdAsignacion is not { } idAsignacion)
            return "Un consumo sin vale no se puede imputar a nada. Una misión lleva varios " +
                   "vales, y adivinar a cuál cargarle el galón es lo que el folio impide.";

        if (hecho.Carga is not { } carga)
            return "Un consumo sin galones, estación y odómetro no es un abastecimiento.";

        var vale = await _combustible.BuscarAsignacionAsync(idAsignacion, cancelacion);

        if (vale is null)
            return $"El vale {idAsignacion} no existe en el servidor. Si se emitió en la " +
                   "oficina, el dispositivo lo tiene que haber recibido antes de consumirlo.";

        var expediente = await _expedientes.BuscarAsync(vale.Mision, cancelacion);

        if (expediente is null)
            return $"El vale {idAsignacion} apunta a una misión que no existe.";

        if (expediente.Estado is not (EstadoDeMision.EnRuta or EstadoDeMision.Retornada))
            return $"La misión está {expediente.Estado}. Un consumo sólo ocurre en ruta: " +
                   "registrar uno antes de salir sería declarar un gasto que todavía no pudo pasar.";

        try
        {
            vale.RegistrarConsumo(
                new IdPersona(hecho.Ejecuta),
                new ConsumoRegistrado(
                    carga.Galones, carga.Monto, carga.Estacion, carga.Odometro,
                    carga.Comprobante,
                    // Viaja hasta el asiento: es lo que sostiene el descargo alternativo de
                    // `RN-85`, y perderla en el camino dejaría la ausencia sin explicación
                    // en el único lugar donde alguien la va a leer.
                    carga.CausaSinComprobante),
                hecho.OcurridoEn,
                hecho.IdDeCaptura);

            await _combustible.GuardarAsignacionAsync(vale, cancelacion);
            return null;
        }
        catch (Exception error)
            when (error is BloqueoDuro or TransicionInvalidaDeAsignacion)
        {
            return error.Message;
        }
    }

    /// <summary>
    /// `A-01` — el combustible que entró al tanque y <b>no salió del vale</b> (`RN-83`).
    ///
    /// ── Lo que el servidor comprueba, y lo que no ───────────────────────────
    /// El dominio ya exige galones, odómetro y el respaldo que la fuente deba traer. Acá se
    /// añade lo único que el dispositivo no puede saber: que el vehículo <b>existe</b>, y que
    /// si declara misión, esa misión lleva ese vehículo — los galones de un tanque no explican
    /// los kilómetros de otro.
    ///
    /// <b>No se rechaza por llegar tarde.</b> El combustible ya entró al tanque: `P-2` manda
    /// registrar el hecho consumado, y negarlo sólo lo vuelve invisible.
    /// </summary>
    /// <returns>Nulo si entró; el motivo del rechazo si no.</returns>
    private async Task<string?> AplicarAbastecimientoAsync(
        HechoCapturado hecho, CancellationToken cancelacion)
    {
        if (hecho.Abastecimiento is not { } carga)
            return "Un abastecimiento sin galones, odómetro y fuente no es un abastecimiento.";

        // La misión es opcional: `RN-83` aplica en misión o fuera de ella. Cuando viene, se
        // comprueba; cuando no, el galón se imputa al vehículo y ya.
        Ulid? mision = hecho.IdExpediente == default ? null : hecho.IdExpediente;

        try
        {
            await _abastecimientosDelServicio.RegistrarAsync(
                Ulid.NewUlid(), carga.IdVehiculo, hecho.OcurridoEn, carga.Galones,
                carga.Odometro, carga.Fuente, new IdPersona(hecho.Ejecuta),
                mision, carga.Monto, carga.Estacion, carga.Comprobante,
                carga.CausaSinComprobante,
                // **El identificador del dispositivo**, que es lo que hace inofensivo el
                // reenvío. Sin él, cada reintento sumaría el mismo galón otra vez.
                hecho.IdDeCaptura,
                cancelacion);

            return null;
        }
        catch (Exception error) when (error is BloqueoDuro or ExpedienteNoEncontrado)
        {
            return error.Message;
        }
    }

    /// <summary>
    /// Las transiciones que <b>hoy</b> puede producir un dispositivo de campo.
    ///
    /// Es una lista corta a propósito: lo que el motorista captura sin red es la salida y
    /// el retorno. La bitácora de paradas y eventos —`T-15`, `T-16`— necesita `M-08`, que
    /// no está construido, y aceptarla acá antes de tiempo sería fingir que existe.
    /// </summary>
    /// <param name="ultimaLecturaConocida">
    /// La del <b>vehículo</b>, no la de esta misión — `BD-05` compara contra la última venga de
    /// donde venga, porque un odómetro que retrocede entre dos misiones distintas es
    /// exactamente el fraude que el control existe para detectar.
    ///
    /// Nula sólo si el vehículo no tiene ninguna lectura.
    /// </param>
    private static void Aplicar(
        OrdenDeMision expediente,
        HechoCapturado hecho,
        int? ultimaLecturaConocida)
    {
        var quien = new IdPersona(hecho.Ejecuta);

        // `BD-05` se evalúa «en el dispositivo, sin red» — pero el servidor lo revalida al
        // recibir, porque la lectura de referencia cruza misiones y el dispositivo sólo
        // conoce la suya. Sin odómetro no hay nada que verificar y el hecho se rechaza:
        // aceptarlo dejaría un `T-14` sin el único ancla que el sistema tiene.
        if (hecho.Transicion is "T-14" or "T-18" && hecho.Odometro is null)
            throw new BloqueoDuro("BD-05",
                $"«{hecho.Transicion}» sin lectura de odómetro. Es el único ancla que el " +
                "sistema tiene para detectar consumo de combustible sin relación con el uso.");

        switch (hecho.Transicion)
        {
            case "T-14":
                expediente.IniciarRuta(
                    quien, hecho.OcurridoEn,
                    new OdometroAlSalir(hecho.Odometro!.Value, ultimaLecturaConocida),
                    hecho.IdDeCaptura);
                break;

            case "T-18":
                expediente.Retornar(
                    quien, hecho.OcurridoEn,
                    new OdometroAlRetornar(hecho.Odometro!.Value, hecho.Subtipo, hecho.Justificacion),
                    hecho.IdDeCaptura);
                break;

            default:
                throw new BloqueoDuro(
                    hecho.Transicion,
                    $"El cliente de campo todavía no sincroniza «{hecho.Transicion}». " +
                    "Hoy entran T-14 salida, T-18 retorno, V-04 consumo del vale y A-01 " +
                    "abastecimiento de otra fuente: la bitácora de paradas y eventos necesita " +
                    "M-08, que no está construido.");
        }
    }
}
