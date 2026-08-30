using Microsoft.EntityFrameworkCore;

using Sigti.Aplicacion.M07_ProgramacionYDespacho;
using Sigti.Datos;
using Sigti.Datos.M02_Parametros;
using Sigti.Datos.M19_Seguimiento;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M19_Seguimiento;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M19_Seguimiento;

/// <summary>
/// El seguimiento en ruta — M-19.
///
/// ── Lo que este servicio no hace ────────────────────────────────────────────
/// <b>No deduce nada del silencio.</b> No cierra misiones por inactividad, no marca
/// interrupciones, no infiere que un vehículo se detuvo. `RN-76` lo prohíbe y la razón es
/// operativa antes que normativa: más de dos millones de personas del área rural no tienen
/// conectividad, así que el silencio es la <b>condición esperada</b> y una inferencia sobre él
/// sería falsa la mayoría de las veces.
/// </summary>
public sealed class ServicioDeSeguimiento(SigtiDbContext contexto)
{
    private readonly ParametrosNormativos _parametros = new(contexto);

    /// <summary>Catálogo cerrado `estado_en_ruta`. Los valores van separados por barra vertical.</summary>
    public const string ClaveDeEstados = "seguimiento.estados_en_ruta";

    /// <summary>Cuáles causas de `causa_de_espera` cuentan como espera improductiva.</summary>
    public const string ClaveDeCausasImproductivas = "seguimiento.causas_improductivas";

    /// <summary>
    /// Registra lo que el motorista declaró.
    ///
    /// La precondición se juzga contra el <b>diario</b> a la fecha del hecho, no contra el
    /// estado de hoy: un lote que sube cuatro días tarde encuentra la misión ya liquidada.
    /// </summary>
    public async Task<Ulid> RegistrarAsync(
        Ulid misionId,
        TipoDeReporte tipo,
        DateTimeOffset momentoDelHecho,
        IdPersona declara,
        string? estado = null,
        string? destino = null,
        Posicion? posicion = null,
        string? causaDeEspera = null,
        string? seAtribuyeA = null,
        bool? motorEncendido = null,
        DateTimeOffset? momentoDeCaptura = null,
        CancellationToken cancelacion = default)
    {
        var fila = await contexto.Expedientes
            .AsNoTracking()
            .Include(e => e.Transiciones)
            .SingleOrDefaultAsync(e => e.Id == misionId, cancelacion)
            ?? throw new ExpedienteNoEncontrado(misionId);

        var diario = fila.Transiciones
            .OrderBy(t => t.Orden)
            .Select(t => new Transicion(
                t.Transicion,
                t.Destino,
                new IdPersona(t.Ejecuta),
                new DateTimeOffset(t.MomentoUtc, TimeSpan.Zero)
                    .ToOffset(TimeSpan.FromMinutes(t.DesfaseMinutos)),
                t.Motivo))
            .ToList();

        ReglasDelSeguimiento.ExigirQueEstuvieraEnRuta(diario, momentoDelHecho);
        ReglasDelSeguimiento.ExigirDestino(tipo, destino);
        ReglasDelSeguimiento.ExigirPosicionUsable(posicion);

        if (tipo == TipoDeReporte.EstadoDeclarado)
            ReglasDelSeguimiento.ExigirEstadoDelCatalogo(
                estado,
                await ListaAsync(
                    ClaveDeEstados,
                    DateOnly.FromDateTime(momentoDelHecho.UtcDateTime),
                    cancelacion));

        var captura = momentoDeCaptura ?? DateTimeOffset.UtcNow;
        var id = Ulid.NewUlid();

        contexto.ReportesDeCampo.Add(new FilaDeReporteDeCampo
        {
            Id = id,
            MisionId = misionId,
            Tipo = tipo,
            Estado = estado,
            Destino = destino,
            MomentoDelHechoUtc = momentoDelHecho.UtcDateTime,
            DesfaseDelHechoMinutos = (int)momentoDelHecho.Offset.TotalMinutes,
            MomentoDeCapturaUtc = captura.UtcDateTime,
            DesfaseDeCapturaMinutos = (int)captura.Offset.TotalMinutes,
            Latitud = posicion?.Latitud,
            Longitud = posicion?.Longitud,
            PrecisionMetros = posicion?.PrecisionMetros,
            CausaDeEspera = causaDeEspera,
            SeAtribuyeA = seAtribuyeA,
            MotorEncendido = motorEncendido,
            Declara = declara.Valor,
        });

        await contexto.SaveChangesAsync(cancelacion);
        return id;
    }

    /// <summary>
    /// El tablero de `PT-058`: las misiones que están afuera, con la antigüedad de lo último
    /// que se sabe de cada una.
    /// </summary>
    public async Task<Tablero> TableroAsync(
        DateTimeOffset ahora, CancellationToken cancelacion = default)
    {
        // ⚠️ **No se filtra por la fecha planificada de salida**, y es la diferencia con el
        // tablero del día de despacho —de donde este filtro se copió al principio, mal—.
        //
        // Aquel organiza el día por lo planificado; éste contesta «¿qué vehículos están afuera
        // AHORA?», y eso lo dice el diario, no el plan (`P-1`). Una misión con asiento `T-14` y
        // fecha de salida futura —porque salió antes, o porque la fecha se capturó mal— está
        // afuera igual, y filtrarla la borraba del tablero justo mientras el vehículo circulaba.
        //
        // El corte que sí corresponde es haber salido alguna vez: sin `T-14` no hay nada que
        // seguir. Va en SQL para no traer los expedientes que nunca se despacharon.
        var filas = await contexto.Expedientes
            .AsNoTracking()
            .Include(e => e.Transiciones)
            .Where(e => e.Transiciones.Any(t => t.Transicion == "T-14"))
            .ToListAsync(cancelacion);

        var enRuta = filas
            .Select(f => (Fila: f, Ultima: f.Transiciones.MaxBy(t => t.Orden)))
            .Where(x => x.Ultima?.Destino == EstadoDeMision.EnRuta)
            .ToList();

        var ids = enRuta.Select(x => x.Fila.Id).ToList();

        // ⚠️ **Son dos preguntas distintas y hacen falta las dos.**
        //
        // «¿Cuándo supimos de él por última vez?» la contesta cualquier reporte —un arribo es
        // señal de vida igual que una declaración de estado—. «¿Qué es lo último que declaró?»
        // sólo la contesta un reporte que traiga estado.
        //
        // Con un solo dato el tablero decía «sin estado declarado» cuando el último reporte era
        // un arribo, aunque el motorista hubiera declarado su estado una hora antes. Eso es
        // afirmar que no declaró: exactamente lo que `RN-76` existe para impedir.
        var ultimos = await contexto.ReportesDeCampo
            .AsNoTracking()
            .Where(r => ids.Contains(r.MisionId))
            .GroupBy(r => r.MisionId)
            .Select(g => g.OrderByDescending(r => r.MomentoDelHechoUtc).First())
            .ToListAsync(cancelacion);

        var ultimosEstados = await contexto.ReportesDeCampo
            .AsNoTracking()
            .Where(r => ids.Contains(r.MisionId) && r.Estado != null)
            .GroupBy(r => r.MisionId)
            .Select(g => g.OrderByDescending(r => r.MomentoDelHechoUtc).First())
            .ToListAsync(cancelacion);

        var porMision = ultimos.ToDictionary(r => r.MisionId);
        var estadoPorMision = ultimosEstados.ToDictionary(r => r.MisionId);

        var vehiculos = await contexto.Vehiculos
            .AsNoTracking().ToDictionaryAsync(v => v.Id, v => v.Siglas, cancelacion);
        var conductores = await contexto.Conductores
            .AsNoTracking().ToDictionaryAsync(c => c.Id, c => c.Nombre, cancelacion);

        var umbral = await UmbralAsync(DateOnly.FromDateTime(ahora.UtcDateTime), cancelacion);

        var enTablero = enRuta.Select(x =>
        {
            var reserva = x.Fila.Transiciones
                .Where(t => t.VehiculoTomado is not null)
                .MaxBy(t => t.Orden);

            porMision.TryGetValue(x.Fila.Id, out var ultimo);
            estadoPorMision.TryGetValue(x.Fila.Id, out var ultimoEstado);

            var hecho = ultimo is null
                ? (DateTimeOffset?)null
                : new DateTimeOffset(ultimo.MomentoDelHechoUtc, TimeSpan.Zero)
                    .ToOffset(TimeSpan.FromMinutes(ultimo.DesfaseDelHechoMinutos));

            // La hora del ESTADO, que puede ser más vieja que la del último reporte. Mostrar el
            // estado con la hora del arribo posterior lo haría ver más fresco de lo que es.
            var declaradoEl = ultimoEstado is null
                ? (DateTimeOffset?)null
                : new DateTimeOffset(ultimoEstado.MomentoDelHechoUtc, TimeSpan.Zero)
                    .ToOffset(TimeSpan.FromMinutes(ultimoEstado.DesfaseDelHechoMinutos));

            return new MisionEnRuta(
                x.Fila.Id.ToString(),
                ConsultaDeMisiones.Folio(x.Fila),
                x.Fila.Dependencia,
                x.Fila.Destino,
                x.Fila.ObjetoDelTraslado,
                reserva?.VehiculoTomado is { } v && vehiculos.TryGetValue(v, out var siglas)
                    ? siglas : null,
                reserva?.ConductorTomado is { } c && conductores.TryGetValue(c, out var nombre)
                    ? nombre : null,
                x.Fila.Retorno,

                // Nulos cuando nunca declaró nada. No se sustituyen por el momento del
                // despacho: eso mostraría como declaración del motorista un acto de oficina.
                ultimoEstado?.Estado,
                declaradoEl,
                hecho,
                ultimo is { Latitud: not null, Longitud: not null }
                    ? new Posicion(ultimo.Latitud.Value, ultimo.Longitud.Value,
                                   ultimo.PrecisionMetros)
                    : null,
                ReglasDeLaFrescura.Evaluar(hecho, ahora, umbral));
        }).ToList();

        return new Tablero(
            ahora,
            umbral,

            // Lo más viejo primero, y lo que nunca declaró al principio de todo: son las que
            // el Jefe de Transporte no puede explicar si le preguntan.
            [.. enTablero
                .OrderByDescending(m => m.Frescura.Grado == GradoDeFrescura.NuncaHuboDato)
                .ThenByDescending(m => m.Frescura.Antiguedad ?? TimeSpan.Zero)
                .ThenBy(m => m.Folio)]);
    }

    /// <summary>
    /// El detalle de `PT-059`: los hitos de una misión en ruta, con las estadías derivadas.
    /// </summary>
    public async Task<DetalleEnRuta?> DetalleAsync(
        Ulid misionId, DateTimeOffset ahora, CancellationToken cancelacion = default)
    {
        var fila = await contexto.Expedientes
            .AsNoTracking()
            .Include(e => e.Transiciones)
            .SingleOrDefaultAsync(e => e.Id == misionId, cancelacion);

        if (fila is null) return null;

        // ⚠️ Por la HORA DEL HECHO, nunca por la de captura. `HU-057` lo exige: al reconectar
        // llegan de golpe los reportes acumulados, y el orden de recepción los pone al revés.
        var reportes = await contexto.ReportesDeCampo
            .AsNoTracking()
            .Where(r => r.MisionId == misionId)
            .OrderBy(r => r.MomentoDelHechoUtc)
            .ToListAsync(cancelacion);

        var enDominio = reportes.Select(Dominio).ToList();

        var causas = await ListaAsync(
            ClaveDeCausasImproductivas,
            DateOnly.FromDateTime(ahora.UtcDateTime),
            cancelacion);

        var estadias = ReglasDeLaEstadia.Derivar(enDominio, ahora, causas);
        var umbral = await UmbralAsync(DateOnly.FromDateTime(ahora.UtcDateTime), cancelacion);

        var ultimo = enDominio.LastOrDefault();
        var ultimoEstado = enDominio.LastOrDefault(r => r.Estado is not null);

        return new DetalleEnRuta(
            fila.Id.ToString(),
            ConsultaDeMisiones.Folio(fila),
            fila.Transiciones.MaxBy(t => t.Orden)?.Destino.ToString() ?? "sin diario",
            fila.Dependencia,
            fila.Destino,
            fila.ObjetoDelTraslado,
            ultimoEstado?.Estado,
            ultimoEstado?.MomentoDelHecho,
            ReglasDeLaFrescura.Evaluar(ultimo?.MomentoDelHecho, ahora, umbral),
            enDominio,
            estadias);
    }

    // ── Los catálogos ───────────────────────────────────────────────────────

    /// <summary>
    /// Un catálogo guardado como un solo parámetro con vigencia. <b>Vacío cuando la clave no
    /// está fijada</b>, y quien lo reciba tiene que distinguir eso de un catálogo con cero
    /// entradas — que no existe.
    /// </summary>
    private async Task<IReadOnlySet<string>> ListaAsync(
        string clave, DateOnly fechaDelHecho, CancellationToken cancelacion)
    {
        var catalogo = await _parametros.CatalogoDeAsync(clave, cancelacion);
        var valor = catalogo.ResolverSiHay(clave, fechaDelHecho, DateTimeOffset.UtcNow);

        return valor is null
            ? new HashSet<string>()
            : valor.Valor
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// El umbral de degradación de `RN-50`. <b>Nulo cuando la institución no lo fijó</b> —
    /// insumo #68. Se devuelve nulo y no un valor por omisión: un umbral inventado haría que el
    /// tablero degradara según un número que nadie decidió y que nadie puede rastrear.
    /// </summary>
    private async Task<TimeSpan?> UmbralAsync(DateOnly fecha, CancellationToken cancelacion)
    {
        var catalogo = await _parametros.CatalogoDeAsync(
            ReglasDeLaFrescura.ClaveDelUmbral, cancelacion);

        var valor = catalogo.ResolverSiHay(
            ReglasDeLaFrescura.ClaveDelUmbral, fecha, DateTimeOffset.UtcNow);

        return valor is not null && double.TryParse(valor.Valor, out var horas) && horas > 0
            ? TimeSpan.FromHours(horas)
            : null;
    }

    private static ReporteDeCampo Dominio(FilaDeReporteDeCampo f) => new()
    {
        Id = f.Id,
        MisionId = f.MisionId,
        Tipo = f.Tipo,
        Estado = f.Estado,
        Destino = f.Destino,
        MomentoDelHecho = new DateTimeOffset(f.MomentoDelHechoUtc, TimeSpan.Zero)
            .ToOffset(TimeSpan.FromMinutes(f.DesfaseDelHechoMinutos)),
        MomentoDeCaptura = new DateTimeOffset(f.MomentoDeCapturaUtc, TimeSpan.Zero)
            .ToOffset(TimeSpan.FromMinutes(f.DesfaseDeCapturaMinutos)),
        Posicion = f is { Latitud: not null, Longitud: not null }
            ? new Posicion(f.Latitud.Value, f.Longitud.Value, f.PrecisionMetros)
            : null,
        CausaDeEspera = f.CausaDeEspera,
        SeAtribuyeA = f.SeAtribuyeA,
        MotorEncendido = f.MotorEncendido,
        Declara = new IdPersona(f.Declara),
    };
}

/// <param name="Umbral">
/// <b>Nulo cuando no está fijado</b> (insumo #68). El tablero muestra la antigüedad igual —eso
/// es lo duro de `HU-057`— pero no puede decir si es mucha, y lo declara.
/// </param>
public sealed record Tablero(
    DateTimeOffset Ahora, TimeSpan? Umbral, IReadOnlyList<MisionEnRuta> Misiones);

/// <param name="UltimoEstado">
/// Lo <b>declarado</b> por el motorista. Nulo es que no declaró nada, y no se sustituye por
/// nada calculado: `RN-76` prohíbe inferir el estado de la falta de movimiento o de señal.
/// </param>
/// <param name="DeclaradoEl">
/// La hora de <b>ese</b> reporte, no la del último. Un estado de las 17:00 seguido de un arribo
/// a las 21:00 se muestra con las 17:00: presentarlo con la hora del arribo lo haría ver cuatro
/// horas más fresco de lo que es.
/// </param>
/// <param name="UltimoHecho">
/// La última señal de vida, del tipo que sea. Sobre ésta se mide la antigüedad — un arribo dice
/// que el equipo tuvo cobertura tanto como una declaración de estado.
/// </param>
public sealed record MisionEnRuta(
    string Mision,
    string Folio,
    string Dependencia,
    string Destino,
    string ObjetoDelTraslado,
    string? Vehiculo,
    string? Motorista,
    DateOnly Retorno,
    string? UltimoEstado,
    DateTimeOffset? DeclaradoEl,
    DateTimeOffset? UltimoHecho,
    Posicion? UltimaPosicion,
    Frescura Frescura);

public sealed record DetalleEnRuta(
    string Mision,
    string Folio,
    string Estado,
    string Dependencia,
    string Destino,
    string ObjetoDelTraslado,
    string? UltimoEstadoDeclarado,
    DateTimeOffset? DeclaradoEl,
    Frescura Frescura,
    IReadOnlyList<ReporteDeCampo> Hitos,
    ResultadoDeEstadias Estadias);
