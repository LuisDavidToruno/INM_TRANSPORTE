using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Aplicacion.M03_Flota;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Aplicacion.M07_ProgramacionYDespacho;

/// <summary>Un vehículo y lo que lo tiene tomado en la ventana consultada.</summary>
public sealed record CarrilDeVehiculo(
    string Vehiculo,
    string Siglas,
    string? Placa,
    string TipoDeVehiculo,
    IReadOnlyList<BarraDeOcupacion> Barras,
    /// <summary>
    /// El estado operativo — §10.2. <b>Nulo cuando nunca se declaró</b>, que no es lo mismo que
    /// disponible: el cronograma lo dice en vez de pintarlo como si estuviera listo.
    /// </summary>
    string? Estado,
    /// <summary>
    /// Si el vehículo <b>no se puede comprometer</b> — taller, no disponible, prestado o
    /// terminal. Va calculado y no deducido en el cliente: la lista de estados inutilizables
    /// es de `BD-07`, y duplicarla en la pantalla la dejaría divergir del bloqueo.
    /// </summary>
    bool Inutilizable);

/// <summary>
/// Un tramo tomado. <b>Los dos extremos son inclusivos</b>: el retorno previsto es un día
/// en que el vehículo sigue tomado, no el primer día libre.
///
/// <b>Las fechas son las reales, no las recortadas contra la ventana.</b> Una barra que
/// empieza antes se descarta o se devuelve entera, nunca truncada: el cliente necesita
/// saber que se sale para dibujar la marca de corte, y sin eso un rango cortado se ve
/// igual que uno que efectivamente empieza el lunes.
/// </summary>
public sealed record BarraDeOcupacion(
    string Mision,
    string Folio,
    string Destino,
    DateOnly Desde,
    DateOnly Hasta,
    string Estado);

/// <summary>
/// Qué tiene tomado cada vehículo, y cuándo — la ocupación de la flota de `PT-026`.
///
/// ── Es una PROYECCIÓN del diario, no una tabla de reservas ───────────────────
/// P-1. La reserva vive en la transición que reservó (<see cref="RecursosTomados"/>), y
/// esta consulta la lee. Eso hace que <b>liberar sea no volver a tomar</b>: si después de
/// `T-08` el diario siguió a `T-11` o `T-13`, la misión ya no ocupa, sin que nadie haya
/// tenido que acordarse de borrar una fila.
///
/// ── Qué estados ocupan, y por qué no es «las que no están canceladas» ───────
/// Ocupa la misión cuya <b>última</b> transición la deja en un estado en que el vehículo
/// está comprometido: `PROGRAMADA`, `DESPACHADA` o `EN_RUTA`. Una `RETORNADA` no ocupa —
/// el vehículo volvió, aunque falte liquidar. Enumerarlo por lista blanca y no por
/// descarte es deliberado: el día que se agregue un estado nuevo, el descarte lo daría
/// por ocupante en silencio y una lista blanca obliga a decidir.
///
/// ── Qué NO hace ─────────────────────────────────────────────────────────────
/// No marca conflictos ni decide compatibilidad. Devuelve lo que está tomado; quién
/// choca con qué lo resuelve `EvaluacionDeAsignacion`, que es donde vive `BD-02`.
/// </summary>
public sealed class ConsultaDeOcupacion(SigtiDbContext contexto, EstadoDeLaFlota flota)
{
    /// <summary>
    /// Los estados en que el vehículo está comprometido.
    ///
    /// `RETORNADA`, `LIQUIDADA` y `CERRADA` no están: el vehículo ya volvió. `APROBADA`
    /// tampoco — todavía no se le asignó vehículo, y de hecho no puede haberlo tomado.
    /// </summary>
    private static readonly EstadoDeMision[] Ocupan =
    [
        EstadoDeMision.Programada,
        EstadoDeMision.Despachada,
        EstadoDeMision.EnRuta,
    ];

    public async Task<IReadOnlyList<CarrilDeVehiculo>> EnVentanaAsync(
        DateOnly desde,
        DateOnly hasta,
        CancellationToken cancelacion = default)
    {
        var vehiculos = await contexto.Vehiculos
            .AsNoTracking()
            .OrderBy(v => v.Siglas)
            .ToListAsync(cancelacion);

        // El recorte por ventana va en SQL: el diario de una institución crece para
        // siempre y traerlo entero para descartar en memoria sería peor cada año.
        // Los dos extremos son inclusivos, de los dos lados de la comparación.
        var filas = await contexto.Expedientes
            .AsNoTracking()
            .Include(e => e.Transiciones)
            .Where(e => e.Retorno >= desde && e.Salida <= hasta)
            .ToListAsync(cancelacion);

        var barras = new Dictionary<Ulid, List<BarraDeOcupacion>>();

        foreach (var fila in filas)
        {
            // El estado es el destino de la ÚLTIMA transición: eso es lo que significa que
            // el estado sea la proyección del diario, y no una columna que se actualiza.
            var ultima = fila.Transiciones.MaxBy(t => t.Orden);
            if (ultima is null || !Ocupan.Contains(ultima.Destino)) continue;

            // La reserva vigente es la de la última transición que reservó. Reprogramar
            // —`T-11` y de vuelta `T-08`— deja dos en el diario, y ocupa la segunda.
            var reserva = fila.Transiciones
                .Where(t => t.VehiculoTomado is not null)
                .MaxBy(t => t.Orden);

            if (reserva?.VehiculoTomado is not { } vehiculo) continue;

            if (!barras.TryGetValue(vehiculo, out var lista))
                barras[vehiculo] = lista = [];

            lista.Add(new BarraDeOcupacion(
                Mision: fila.Id.ToString(),
                Folio: ConsultaDeMisiones.Folio(fila),
                Destino: fila.Destino,
                Desde: fila.Salida,
                Hasta: fila.Retorno,
                Estado: ultima.Destino.ToString()));
        }

        var carriles = new List<CarrilDeVehiculo>();

        foreach (var v in vehiculos)
        {
            var estado = await flota.ActualAsync(v.Id, cancelacion);

            carriles.Add(new CarrilDeVehiculo(
                Vehiculo: v.Id.ToString(),
                Siglas: v.Siglas,
                Placa: v.Placa,
                TipoDeVehiculo: v.TipoDeVehiculo,
                Barras: barras.TryGetValue(v.Id, out var b)
                    ? b.OrderBy(x => x.Desde).ToList()
                    : [],
                Estado: estado?.ToString(),
                // Los mismos tres que `BD-07` deja pasar. Si esta lista y la del bloqueo
                // divergen, el cronograma pintaría disponible un vehículo que no se puede
                // programar -- y quien programa lo descubriría al guardar.
                Inutilizable: estado is not null
                    && estado is not (EstadoOperativo.Disponible
                                   or EstadoOperativo.Asignado
                                   or EstadoOperativo.EnMision)));
        }

        return carriles;
    }

    /// <summary>
    /// Lo que <b>otras</b> misiones tienen tomado sobre este vehículo o sobre quien va a
    /// conducir — el insumo de `BD-11`.
    ///
    /// ── Trae por RECURSO, no por fecha ───────────────────────────────────────
    /// El recorte por ventana lo hace la consulta de ocupación, que dibuja. Ésta no: el
    /// solape es <b>la regla</b> y se decide en el dominio (<see cref="ReservaDeRecurso.SeSolapaCon"/>).
    /// Filtrar por fecha acá metería la regla en un <c>WHERE</c>, donde no se puede ejercer
    /// sin base de datos y los casos de borde —dos misiones que se tocan por un día— se
    /// prueban a través de tres capas o no se prueban.
    ///
    /// Son pocas filas: las misiones vivas de <b>un</b> vehículo y <b>un</b> conductor.
    ///
    /// ── Por qué se excluye la misión que se está evaluando ───────────────────
    /// Porque el conflicto es con <b>otras</b>. Hoy `T-08` no podría chocar consigo misma
    /// —exige `APROBADA`, y una aprobada no ocupa—, pero apoyarse en eso ataría esta
    /// consulta a un detalle de la máquina de estados que `T-10` va a cambiar: reasignar es
    /// `PROGRAMADA → PROGRAMADA`, y ahí la misión <b>sí</b> está ocupando.
    /// </summary>
    public async Task<IReadOnlyList<ReservaDeRecurso>> ReservasDeAsync(
        Ulid vehiculo,
        Ulid conductor,
        Ulid excluyendo,
        CancellationToken cancelacion = default)
    {
        var filas = await contexto.Expedientes
            .AsNoTracking()
            .Include(e => e.Transiciones)
            .Where(e => e.Id != excluyendo)
            .Where(e => e.Transiciones.Any(t =>
                t.VehiculoTomado == vehiculo || t.ConductorTomado == conductor))
            .ToListAsync(cancelacion);

        var reservas = new List<ReservaDeRecurso>();

        foreach (var fila in filas)
        {
            var ultima = fila.Transiciones.MaxBy(t => t.Orden);
            if (ultima is null || !Ocupan.Contains(ultima.Destino)) continue;

            var reserva = fila.Transiciones
                .Where(t => t.VehiculoTomado is not null)
                .MaxBy(t => t.Orden);

            if (reserva is null) continue;

            // Cuál de los dos recursos choca. Se distinguen porque el mensaje del bloqueo
            // lo dice, y no es lo mismo cambiar de vehículo que cambiar de motorista.
            var chocaVehiculo = reserva.VehiculoTomado == vehiculo;
            var chocaConductor = reserva.ConductorTomado == conductor;

            // La consulta trajo por «vehículo O conductor», pero la reserva vigente puede
            // ser otra que la que hizo entrar la fila —una misión reprogramada—. Si la
            // vigente no toca ninguno de los dos recursos, no hay conflicto que reportar.
            if (!chocaVehiculo && !chocaConductor) continue;

            reservas.Add(new ReservaDeRecurso(
                Mision: fila.Id,
                Folio: ConsultaDeMisiones.Folio(fila),
                Dependencia: fila.Dependencia,
                // La franja reservada llega hasta la holgura: es el último día en que el
                // vehículo podría no estar en el predio.
                Desde: fila.Salida,
                Hasta: fila.Retorno.AddDays(fila.HolguraDias),
                Vehiculo: chocaVehiculo,
                Conductor: chocaConductor));
        }

        return reservas;
    }
}
