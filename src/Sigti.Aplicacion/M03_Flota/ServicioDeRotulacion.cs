using Microsoft.EntityFrameworkCore;

using Sigti.Datos.M02_Parametros;
using Sigti.Datos;
using Sigti.Datos.M03_Flota;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M03_Flota;

/// <summary>
/// `RN-18` — constatar la identificación del vehículo del Estado, <b>con fecha y foto</b>.
///
/// ── Por qué es hallazgo frecuente de auditoría ──────────────────────────────
/// Las franjas azul–blanco–azul, la leyenda «PROPIEDAD DEL ESTADO DE HONDURAS», las siglas y el
/// correlativo son lo que distingue un vehículo del Estado de uno particular a la vista. Se
/// despintan, las calcomanías se caen, y nadie lo nota hasta que un operativo lo hace notar.
/// </summary>
public sealed class ServicioDeRotulacion(SigtiDbContext contexto)
{
    /// <summary>
    /// Registra la constatación de un elemento.
    ///
    /// <b>Exige la fotografía y que exista</b>. `RN-18` es literal: <i>«una constatación sin
    /// fotografía no debe aceptarse»</i> — y el identificador de una foto no es la foto, que es
    /// la distinción que ya costó dos veces hoy.
    /// </summary>
    public async Task<Ulid> ConstatarAsync(
        Ulid vehiculo,
        ElementoDeIdentificacion elemento,
        bool presente,
        DateOnly constatadoEl,
        Ulid fotografia,
        IdPersona constata,
        string? observacion,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        if (!await contexto.Vehiculos.AnyAsync(v => v.Id == vehiculo, cancelacion))
            throw new BloqueoDuro("RN-18", $"No existe el vehículo {vehiculo}.");

        if (!await contexto.Adjuntos.AnyAsync(a => a.Id == fotografia, cancelacion))
        {
            throw new BloqueoDuro("RN-18",
                ReglasDeLaRotulacion.PorQueNoSeAcepta(tieneFotografia: false)!);
        }

        var id = Ulid.NewUlid();

        contexto.ConstatacionesDeRotulacion.Add(new FilaDeConstatacion
        {
            Id = id,
            VehiculoId = vehiculo,
            Elemento = elemento,
            Presente = presente,
            ConstatadoEl = constatadoEl,
            Fotografia = fotografia,
            ConstatadoPor = constata.Valor,
            Observacion = observacion,
            RegistradoEnUtc = momento.UtcDateTime,
        });

        await contexto.SaveChangesAsync(cancelacion);
        return id;
    }

    /// <summary>
    /// En qué situación está la identificación del vehículo <b>a una fecha</b>.
    ///
    /// El plazo sale del parámetro, y <b>el del vehículo sin lámina es más corto</b>: ahí la
    /// rotulación es su única identificación visible como bien del Estado.
    /// </summary>
    public async Task<IdentificacionDelVehiculo?> EvaluarAsync(
        Ulid vehiculo, DateOnly aLaFecha, CancellationToken cancelacion = default)
    {
        var fila = await contexto.Vehiculos
            .AsNoTracking()
            .Include(v => v.Constataciones)
            .SingleOrDefaultAsync(v => v.Id == vehiculo, cancelacion);

        if (fila is null) return null;

        var parametros = new ParametrosNormativos(contexto);

        var general = await PlazoAsync(parametros, ReglasDeLaRotulacion.ClaveDeVigencia, aLaFecha, cancelacion);
        var sinLamina = await PlazoAsync(
            parametros, ReglasDeLaRotulacion.ClaveDeVigenciaSinLamina, aLaFecha, cancelacion);

        return ReglasDeLaRotulacion.Evaluar(
            [
                .. fila.Constataciones.Select(c => new Constatacion(
                    c.Elemento, c.Presente, c.ConstatadoEl, c.Fotografia,
                    c.ConstatadoPor, c.Observacion)),
            ],
            ReglasDeLaRotulacion.VigenciaQueAplica(fila.EstadoDePlaca, general, sinLamina),
            aLaFecha);
    }

    /// <summary>El historial completo, para el expediente del vehículo.</summary>
    public async Task<IReadOnlyList<Constatacion>> HistorialAsync(
        Ulid vehiculo, CancellationToken cancelacion = default) =>
        [
            .. (await contexto.ConstatacionesDeRotulacion
                    .AsNoTracking()
                    .Where(c => c.VehiculoId == vehiculo)
                    .OrderByDescending(c => c.ConstatadoEl)
                    .ToListAsync(cancelacion))
                .Select(c => new Constatacion(
                    c.Elemento, c.Presente, c.ConstatadoEl, c.Fotografia,
                    c.ConstatadoPor, c.Observacion)),
        ];

    /// <summary>
    /// El plazo en días. <b>Nulo es que la institución no lo cargó</b>, y entonces la
    /// constatación no caduca — inventar un plazo fijaría por omisión una regla que `RN-18`
    /// deja explícitamente configurable.
    /// </summary>
    private static async Task<int?> PlazoAsync(
        ParametrosNormativos parametros, string clave, DateOnly fecha,
        CancellationToken cancelacion)
    {
        var catalogo = await parametros.CatalogoDeAsync(clave, cancelacion);
        var vigente = catalogo.ResolverSiHay(clave, fecha, DateTimeOffset.UtcNow);

        return vigente is not null && int.TryParse(vigente.Valor, out var dias) ? dias : null;
    }
}
