using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Aplicacion.M07_ProgramacionYDespacho;

/// <summary>Una misión en el tablero del día, con los nombres ya resueltos.</summary>
/// <param name="Vehiculo">
/// Las siglas, no el identificador. <b>Nulo cuando la reserva apunta a un vehículo que ya no
/// está en la flota</b> — se dio de baja después de programar. Se dice, no se esconde: el
/// despachador tiene que saber que ese renglón no tiene con qué salir.
/// </param>
public sealed record MisionDelDia(
    string Mision,
    string Folio,
    string Estado,
    string Dependencia,
    string Destino,
    string ObjetoDelTraslado,
    string? Vehiculo,
    string? Motorista,
    DateOnly Salida,
    DateOnly Retorno,
    int DiasDeAtraso);

/// <summary>Lo que el despachador tiene enfrente hoy.</summary>
public sealed record DiaDeDespacho(
    DateOnly Fecha,
    IReadOnlyList<MisionDelDia> SalenHoy,
    IReadOnlyList<MisionDelDia> VuelvenHoy,
    IReadOnlyList<MisionDelDia> Afuera,
    IReadOnlyList<MisionDelDia> Atrasadas);

/// <summary>
/// `PT-038` — el tablero de despacho del día.
///
/// ── Las cuatro preguntas del despachador, y por qué son cuatro listas ────────
/// <b>Qué sale hoy</b> (hay que entregar vehículo, documentos y fondo), <b>qué vuelve hoy</b>
/// (hay que recibirlo), <b>qué está afuera</b> (no se puede contar con esos vehículos) y
/// <b>qué debía haber vuelto y no volvió</b>. Son cuatro acciones distintas, con cuatro
/// urgencias distintas, y mezclarlas en una tabla ordenable obliga a filtrar mentalmente
/// cada vez que se abre la pantalla.
///
/// La cuarta es la que ninguna lista ordenada por fecha muestra sola: <b>un retorno vencido
/// no aparece «arriba», aparece en el pasado</b>, donde nadie mira.
///
/// ── Qué NO puede hacer esta consulta, y no se finge ──────────────────────────
/// El dictamen de elementos visuales pide una <b>línea de tiempo del día sobre el eje de
/// horas</b> —la ráfaga de las 5:30 con ocho salidas encimadas—. <b>No se puede</b>: la ventana
/// de la misión es <c>DateOnly</c>. La solicitud no declara a qué hora sale, así que no hay
/// dato con el que ordenar el día por dentro.
///
/// Es el mismo dato que le falta a `BD-04` para juzgar la <i>hora</i> inhábil. Dos necesidades
/// independientes apuntando al mismo campo ausente es lo que hay que llevarle al PO, no un
/// eje de horas dibujado sobre medianoches.
/// </summary>
public sealed class ConsultaDelDiaDeDespacho(SigtiDbContext contexto)
{
    /// <summary>
    /// Los estados que ponen una misión en este tablero.
    ///
    /// `PROGRAMADA` porque hay que despacharla; `DESPACHADA` y `EN_RUTA` porque el vehículo
    /// está afuera y hay que recibirlo. `APROBADA` no: todavía no tiene vehículo y no es
    /// problema del despachador — es de la cola de programación.
    /// </summary>
    private static readonly EstadoDeMision[] EnElTablero =
    [
        EstadoDeMision.Programada,
        EstadoDeMision.Despachada,
        EstadoDeMision.EnRuta,
    ];

    public async Task<DiaDeDespacho> DeLaFechaAsync(
        DateOnly fecha,
        CancellationToken cancelacion = default)
    {
        // Nada que salga después de mañana es problema de hoy. El `+1` cubre el único caso
        // que se saldría del corte: una misión despachada por anticipado, con los documentos
        // y el fondo ya entregados para una ventana que abre mañana.
        //
        // ⚠️ **El corte inferior es abierto, y tiene que serlo.** Una misión que debía volver
        // hace tres semanas sigue siendo problema de hoy —es la que más lo es—, y recortarla
        // la haría desaparecer justo cuando más importa. El costo es que esta consulta trae
        // todos los expedientes cuya salida ya pasó y descarta en memoria los terminados.
        //
        // **No se arregla con una ventana más chica**: se arregla con una proyección del
        // estado que SQL pueda filtrar, y eso choca con P-1 —el estado es el diario— hasta
        // que exista una tabla de proyección mantenida por el propio servicio. Queda dicho
        // en vez de resuelto con un recorte que borra el caso grave.
        var filas = await contexto.Expedientes
            .AsNoTracking()
            .Include(e => e.Transiciones)
            .Where(e => e.Salida <= fecha.AddDays(1))
            .ToListAsync(cancelacion);

        var vehiculos = await contexto.Vehiculos
            .AsNoTracking()
            .ToDictionaryAsync(v => v.Id, v => v.Siglas, cancelacion);

        var conductores = await contexto.Conductores
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Id, c => c.Nombre, cancelacion);

        var salen = new List<MisionDelDia>();
        var vuelven = new List<MisionDelDia>();
        var afuera = new List<MisionDelDia>();
        var atrasadas = new List<MisionDelDia>();

        foreach (var fila in filas)
        {
            var ultima = fila.Transiciones.MaxBy(t => t.Orden);
            if (ultima is null || !EnElTablero.Contains(ultima.Destino)) continue;

            var reserva = fila.Transiciones
                .Where(t => t.VehiculoTomado is not null)
                .MaxBy(t => t.Orden);

            var atraso = fecha.DayNumber - fila.Retorno.DayNumber;

            var mision = new MisionDelDia(
                Mision: fila.Id.ToString(),
                Folio: ConsultaDeMisiones.FolioProvisional(fila.Id),
                Estado: ultima.Destino.ToString(),
                Dependencia: fila.Dependencia,
                Destino: fila.Destino,
                ObjetoDelTraslado: fila.ObjetoDelTraslado,
                // Se busca por identificador y puede no encontrarse: un vehículo dado de baja
                // después de programar deja la reserva apuntando a nada.
                Vehiculo: reserva?.VehiculoTomado is { } v && vehiculos.TryGetValue(v, out var siglas)
                    ? siglas
                    : null,
                Motorista: reserva?.ConductorTomado is { } c && conductores.TryGetValue(c, out var nombre)
                    ? nombre
                    : null,
                Salida: fila.Salida,
                Retorno: fila.Retorno,
                DiasDeAtraso: Math.Max(0, atraso));

            var estaAfuera = ultima.Destino is EstadoDeMision.Despachada or EstadoDeMision.EnRuta;

            // El orden de las ramas importa: una misión atrasada TAMBIÉN «vuelve hoy», en el
            // sentido de que ya debía haber vuelto. Clasificarla en las dos la contaría dos
            // veces, y la urgencia real es el atraso.
            if (estaAfuera && atraso > 0) atrasadas.Add(mision);
            else if (estaAfuera && fila.Retorno == fecha) vuelven.Add(mision);
            else if (estaAfuera) afuera.Add(mision);

            // `<=` y no `==`: una misión que debía salir ayer y sigue PROGRAMADA no salió, y
            // es más urgente que la de hoy — no menos. Con `==` desaparecería del tablero al
            // día siguiente, que es exactamente cuando hay que ir a buscarla.
            else if (fila.Salida <= fecha) salen.Add(mision);
        }

        return new DiaDeDespacho(
            fecha,
            // Lo más atrasado primero dentro de cada lista: si algo debía salir ayer y no
            // salió, va antes que lo de hoy.
            [.. salen.OrderBy(m => m.Salida).ThenBy(m => m.Folio)],
            [.. vuelven.OrderBy(m => m.Folio)],
            [.. afuera.OrderBy(m => m.Retorno).ThenBy(m => m.Folio)],
            [.. atrasadas.OrderByDescending(m => m.DiasDeAtraso).ThenBy(m => m.Folio)]);
    }
}
