using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Datos.M07_ProgramacionYDespacho;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Aplicacion.M08_Bitacora;
using Sigti.Dominio.M08_Bitacora;
using Sigti.Dominio.Organizacion;

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
    string? Justificacion = null);

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

    public async Task<ResultadoDeSincronizacion> RecibirAsync(
        IReadOnlyList<HechoCapturado> hechos,
        CancellationToken cancelacion = default)
    {
        var aplicadas = new List<Ulid>();
        var yaConocidas = new List<Ulid>();
        var rechazadas = new List<HechoRechazado>();

        // Lo que ya llegó antes. Se consulta en bloque y no uno por uno: un dispositivo
        // que estuvo siete días sin red trae el lote entero cada vez que reintenta.
        var idsDelLote = hechos.Select(h => h.IdDeCaptura).ToList();
        var conocidas = await contexto.Set<FilaDeTransicion>()
            .Where(t => t.IdDeCaptura != null && idsDelLote.Contains(t.IdDeCaptura.Value))
            .Select(t => t.IdDeCaptura!.Value)
            .ToListAsync(cancelacion);

        foreach (var hecho in hechos)
        {
            if (conocidas.Contains(hecho.IdDeCaptura))
            {
                // No es un error: es el reintento normal. Se acusa igual, para que el
                // dispositivo pueda por fin sacarlo de su cola de pendientes.
                yaConocidas.Add(hecho.IdDeCaptura);
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
                    "Hoy solo entran T-14 salida y T-18 retorno: la bitácora de paradas y " +
                    "eventos necesita M-08, que no está construido.");
        }
    }
}
