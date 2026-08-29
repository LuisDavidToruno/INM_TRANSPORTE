using Microsoft.EntityFrameworkCore;
using Sigti.Dominio.M18_Peajes;
using Sigti.Dominio.Organizacion;

namespace Sigti.Datos.M18_Peajes;

/// <summary>
/// Repositorio con intención (`ADR-009`) de M-18.
///
/// ── Las preguntas reales ────────────────────────────────────────────────────
/// <b>¿Cuánto cuesta pasar por acá con este vehículo, en esta fecha?</b> ·
/// <b>¿Qué categoría le corresponde a esta unidad y por qué?</b> ·
/// <b>¿Dónde nos están cobrando mal?</b> — la tercera es el expediente de reclamo ante la SAPP,
/// y es la que justifica que las dos categorías se guarden por separado.
/// </summary>
public sealed class PeajesDelPais(SigtiDbContext contexto)
{
    // ── El catálogo ─────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<PuntoDePeaje>> PuntosAsync(
        CancellationToken cancelacion = default)
    {
        var filas = await contexto.PuntosDePeaje
            .OrderBy(p => p.Carretera).ThenBy(p => p.Nombre)
            .ToListAsync(cancelacion);

        return [.. filas.Select(A)];
    }

    public async Task<PuntoDePeaje?> PuntoAsync(Ulid id, CancellationToken cancelacion = default)
    {
        var fila = await contexto.PuntosDePeaje.SingleOrDefaultAsync(p => p.Id == id, cancelacion);
        return fila is null ? null : A(fila);
    }

    public async Task<IReadOnlyList<VigenciaDelPunto>> VigenciasAsync(
        CancellationToken cancelacion = default) =>
        [.. (await contexto.VigenciasDePunto.ToListAsync(cancelacion)).Select(v =>
            new VigenciaDelPunto(
                v.PuntoId, v.Estado, v.Fundamento, v.VigenteDesde, v.VigenteHasta,
                new DateTimeOffset(v.RegistradoDesdeUtc, TimeSpan.Zero),
                v.RegistradoHastaUtc is { } h ? new DateTimeOffset(h, TimeSpan.Zero) : null))];

    public async Task<IReadOnlyList<TarifaDePeaje>> TarifasAsync(
        CancellationToken cancelacion = default) =>
        [.. (await contexto.TarifasDePeaje.ToListAsync(cancelacion)).Select(t =>
            new TarifaDePeaje(
                t.Id, t.PuntoId, t.Categoria, t.Monto, t.Fuente, t.FechaDeVerificacion,
                t.VigenteDesde, t.VigenteHasta,
                new DateTimeOffset(t.RegistradoDesdeUtc, TimeSpan.Zero),
                t.RegistradoHastaUtc is { } h ? new DateTimeOffset(h, TimeSpan.Zero) : null))];

    /// <summary>
    /// Las categorías por código. <b>Sin normalizar la caja</b> al guardar, y comparando sin
    /// ella al leer: la tabla la carga una persona.
    /// </summary>
    public async Task<IReadOnlyDictionary<string, string>> NombresDeCategoriaAsync(
        CancellationToken cancelacion = default) =>
        (await contexto.CategoriasDePeaje.ToListAsync(cancelacion))
            .ToDictionary(c => c.Codigo, c => c.Nombre, StringComparer.OrdinalIgnoreCase);

    public async Task<IReadOnlyList<ReglaDeCategoria>> MatrizAsync(
        CancellationToken cancelacion = default) =>
        [.. (await contexto.ReglasDeCategoriaDePeaje.ToListAsync(cancelacion)).Select(r =>
            new ReglaDeCategoria(
                r.Id, r.Categoria, r.Prioridad, r.Fundamento, r.Clase, r.TipoDeVehiculo,
                r.PesoBrutoDesdeKg, r.PesoBrutoHastaKg, r.EjesDesde, r.EjesHasta,
                r.PasajerosDesde, r.PasajerosHasta, r.LlevaRemolque,
                r.VigenteDesde, r.VigenteHasta,
                new DateTimeOffset(r.RegistradoDesdeUtc, TimeSpan.Zero),
                r.RegistradoHastaUtc is { } h ? new DateTimeOffset(h, TimeSpan.Zero) : null))];

    public async Task<IReadOnlyList<ExoneracionDePeaje>> ExoneracionesAsync(
        CancellationToken cancelacion = default) =>
        [.. (await contexto.ExoneracionesDePeaje.ToListAsync(cancelacion)).Select(e =>
            new ExoneracionDePeaje(
                e.Id, e.VehiculoId, e.PuntoId, e.Operador, e.Fundamento,
                e.VigenteDesde, e.VigenteHasta,
                new DateTimeOffset(e.RegistradoDesdeUtc, TimeSpan.Zero),
                e.RegistradoHastaUtc is { } h ? new DateTimeOffset(h, TimeSpan.Zero) : null))];

    // ── Los pasos ───────────────────────────────────────────────────────────

    /// <summary>
    /// Guarda un paso. <b>Idempotente por `IdDeCaptura`</b>: el paso se captura sin conectividad
    /// (`RN-43`) y el dispositivo reintenta hasta que le contesten. Un paso duplicado infla el
    /// gasto de la misión y produce una discrepancia de conciliación inventada por el sistema.
    /// </summary>
    public async Task<Ulid> GuardarPasoAsync(
        PasoPorCaseta paso, string? causaSinTicket, Ulid? idDeCaptura,
        CancellationToken cancelacion = default)
    {
        if (idDeCaptura is { } captura)
        {
            var yaEsta = await contexto.PasosPorCaseta
                .SingleOrDefaultAsync(p => p.IdDeCaptura == captura, cancelacion);

            if (yaEsta is not null) return yaEsta.Id;
        }

        contexto.PasosPorCaseta.Add(new FilaDePaso
        {
            Id = paso.Id,
            PuntoId = paso.PuntoNoCatalogado ? null : paso.Punto,
            VehiculoId = paso.Vehiculo,
            MisionId = paso.Mision,
            MomentoUtc = paso.OcurridoEn.UtcDateTime,
            DesfaseMinutos = (int)paso.OcurridoEn.Offset.TotalMinutes,
            Odometro = paso.Odometro,
            MontoPagado = paso.MontoPagado,
            Medio = paso.Medio,
            Registra = paso.Registra.Valor,
            CategoriaEsperada = paso.CategoriaEsperada?.Codigo,
            CategoriaCobrada = paso.CategoriaCobrada?.Codigo,
            MontoEsperado = paso.MontoEsperado,
            Ticket = paso.Ticket,
            CausaSinTicket = causaSinTicket,
            PuntoNoCatalogado = paso.PuntoNoCatalogado,
            UbicacionDeclarada = paso.UbicacionDeclarada,
            IdDeCaptura = idDeCaptura,
        });

        await contexto.SaveChangesAsync(cancelacion);
        return paso.Id;
    }

    public async Task<IReadOnlyList<PasoPorCaseta>> PasosDeLaMisionAsync(
        Ulid mision, CancellationToken cancelacion = default)
    {
        var nombres = await NombresDeCategoriaAsync(cancelacion);

        var filas = await contexto.PasosPorCaseta
            .Where(p => p.MisionId == mision)
            .OrderBy(p => p.MomentoUtc)
            .ToListAsync(cancelacion);

        return [.. filas.Select(f => A(f, nombres))];
    }

    /// <summary>
    /// <b>Dónde nos están cobrando mal</b> — el insumo del expediente de reclamo ante la SAPP
    /// (`RN-36` punto 4).
    ///
    /// La comparación se hace en memoria y no en SQL a propósito: la equivalencia de categorías
    /// ignora la caja de las letras, y traducir eso a una comparación de la base bajo
    /// `UseCompatibilityLevel(120)` es exactamente donde se cuelan los falsos negativos.
    /// </summary>
    public async Task<IReadOnlyList<PasoPorCaseta>> DiscrepanciasAsync(
        CancellationToken cancelacion = default)
    {
        var nombres = await NombresDeCategoriaAsync(cancelacion);

        var filas = await contexto.PasosPorCaseta
            .Where(p => p.CategoriaEsperada != null && p.CategoriaCobrada != null)
            .OrderByDescending(p => p.MomentoUtc)
            .ToListAsync(cancelacion);

        return [.. filas.Select(f => A(f, nombres)).Where(p => p.HayDiscrepanciaDeClasificacion)];
    }

    private static PuntoDePeaje A(FilaDePunto f) =>
        new(f.Id, f.Nombre, f.Operador, f.Carretera, f.SentidoDeCobro);

    private static PasoPorCaseta A(FilaDePaso f, IReadOnlyDictionary<string, string> nombres)
    {
        CategoriaDePeaje? Cat(string? codigo) =>
            codigo is null
                ? null
                : new CategoriaDePeaje(codigo, nombres.GetValueOrDefault(codigo, codigo));

        return new PasoPorCaseta(
            f.Id, f.PuntoId ?? default, f.VehiculoId, f.MisionId,
            new DateTimeOffset(f.MomentoUtc, TimeSpan.Zero)
                .ToOffset(TimeSpan.FromMinutes(f.DesfaseMinutos)),
            f.Odometro, f.MontoPagado, f.Medio, new IdPersona(f.Registra),
            Cat(f.CategoriaEsperada), Cat(f.CategoriaCobrada),
            f.MontoEsperado, f.Ticket, f.PuntoNoCatalogado, f.UbicacionDeclarada);
    }
}
