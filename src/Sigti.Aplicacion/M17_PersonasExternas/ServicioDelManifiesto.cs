using Microsoft.EntityFrameworkCore;

using Sigti.Datos;
using Sigti.Datos.M17_PersonasExternas;
using Sigti.Dominio.M17_PersonasExternas;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M17_PersonasExternas;

/// <summary>
/// El manifiesto y sus novedades — `RN-53`, `PT-095`, `PT-131`.
///
/// ── Toda lectura pasa por el registro de consultas ──────────────────────────
/// <b>No hay forma de leer un manifiesto sin dejar asiento</b>, y eso es deliberado: si el
/// registro dependiera de que cada consulta se acuerde de llamarlo, la primera que se olvide
/// deja un hueco que nadie va a notar hasta el hábeas data.
///
/// Por eso <see cref="VerAsync"/> registra <b>antes</b> de devolver, y no existe una vía
/// alternativa que devuelva lo mismo sin registrar.
/// </summary>
public sealed class ServicioDelManifiesto(
    SigtiDbContext contexto, ServicioDePersonasExternas personas)
{
    /// <summary>Abre el manifiesto de una misión, o devuelve el que ya tiene.</summary>
    public async Task<Ulid> AbrirAsync(Ulid mision, CancellationToken cancelacion = default)
    {
        var existente = await contexto.Manifiestos
            .SingleOrDefaultAsync(m => m.MisionId == mision, cancelacion);

        if (existente is not null) return existente.Id;

        var id = Ulid.NewUlid();
        contexto.Manifiestos.Add(new FilaDeManifiesto { Id = id, MisionId = mision });

        await contexto.SaveChangesAsync(cancelacion);
        return id;
    }

    /// <summary>
    /// Agrega una persona. Sólo mientras el manifiesto esté abierto.
    /// </summary>
    public async Task AgregarAsync(
        Ulid mision, PersonaEnManifiesto persona, CancellationToken cancelacion = default)
    {
        ReglasDelManifiesto.ExigirIdentificacionCoherente(persona.Forma, persona.Identificacion);

        var fila = await CargarAsync(mision, cancelacion);
        ReglasDelManifiesto.ExigirAbierto(Dominio(fila));

        fila.Personas.Add(new FilaDePersonaEnManifiesto
        {
            Id = Ulid.NewUlid(),
            ManifiestoId = fila.Id,
            Nombre = persona.Nombre,
            Identificacion = persona.Identificacion,
            Forma = persona.Forma,
            QueMotivaElTraslado = persona.QueMotivaElTraslado,
            Origen = persona.Origen,
            Destino = persona.Destino,
            RequerimientoOperativo = persona.RequerimientoOperativo,
        });

        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>
    /// Cierra el manifiesto al despachar — `RN-53`. <b>Después de esto no se toca.</b>
    /// </summary>
    public async Task CerrarAsync(
        Ulid mision, IdPersona cierra, DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var fila = await CargarAsync(mision, cancelacion);
        ReglasDelManifiesto.ExigirAbierto(Dominio(fila));

        fila.CerradoUtc = momento.UtcDateTime;
        fila.CierraQuien = cierra.Valor;

        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>
    /// `PT-131` — registra lo que cambió en ruta. <b>No modifica el manifiesto.</b>
    /// </summary>
    public async Task RegistrarNovedadAsync(
        Ulid mision, TipoDeNovedad tipo, string? aQuien, string motivo, string? dondePaso,
        DateTimeOffset fechaDelHecho, IdPersona registra, IdPersona? autoriza,
        CancellationToken cancelacion = default)
    {
        ReglasDelManifiesto.ExigirMotivo(motivo);
        ReglasDelManifiesto.ExigirAutorizacionSiSubio(tipo, autoriza);

        var fila = await CargarAsync(mision, cancelacion);
        ReglasDelManifiesto.ExigirCerrado(Dominio(fila));

        fila.Novedades.Add(new FilaDeNovedadDeRuta
        {
            Id = Ulid.NewUlid(),
            ManifiestoId = fila.Id,
            Tipo = tipo,
            AQuien = aQuien,
            Motivo = motivo.Trim(),
            DondePaso = dondePaso,
            FechaDelHechoUtc = fechaDelHecho.UtcDateTime,
            DesfaseMinutos = (int)fechaDelHecho.Offset.TotalMinutes,
            Registra = registra.Valor,
            Autoriza = autoriza?.Valor,
        });

        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>
    /// `PT-095` — ver el manifiesto, <b>dejando asiento de la consulta</b>.
    ///
    /// ── El alcance decide qué se devuelve, y queda registrado ───────────────
    /// `SoloRecuento` no lleva ningún dato personal, y por eso no exige declarar necesidad: es
    /// dato de gestión. Los otros dos sí — y lo que se devuelve es distinto, de modo que el
    /// asiento describe con precisión <b>qué vio esa persona</b>, no sólo qué abrió.
    /// </summary>
    public async Task<ManifiestoVisto?> VerAsync(
        Ulid mision, IdPersona consultante, string rol, AlcanceDeLaConsulta alcance,
        DateTimeOffset momento, string? necesidadDeConocer, string? origen,
        CancellationToken cancelacion = default)
    {
        var fila = await contexto.Manifiestos
            .AsNoTracking()
            .Include(m => m.Personas)
            .Include(m => m.Novedades)
            .SingleOrDefaultAsync(m => m.MisionId == mision, cancelacion);

        if (fila is null) return null;

        // ⚠️ **Se registra ANTES de devolver.** Si fuera después, una consulta que revienta a
        // mitad habría mostrado el dato sin dejar rastro — y ése es justamente el acceso que
        // interesa auditar.
        await personas.RegistrarConsultaAsync(
            consultante, rol, mision.ToString(), alcance, momento,
            necesidadDeConocer, origen, cancelacion);

        var m = Dominio(fila);

        return new ManifiestoVisto(
            m.Declaradas,
            m.Efectivas,
            m.EstaCerrado,
            m.CerradoEl,

            // Con recuento no viaja ni un nombre. **No es un filtro de presentación**: la lista
            // no sale de acá, así que no hay nada que alguien pueda destapar en el cliente.
            alcance == AlcanceDeLaConsulta.SoloRecuento ? [] : m.Personas,

            // Las novedades son actos de gestión —quién autorizó qué— y van salvo en recuento.
            alcance == AlcanceDeLaConsulta.SoloRecuento ? [] : m.Novedades);
    }

    private async Task<FilaDeManifiesto> CargarAsync(Ulid mision, CancellationToken cancelacion) =>
        await contexto.Manifiestos
            .Include(m => m.Personas)
            .Include(m => m.Novedades)
            .SingleOrDefaultAsync(m => m.MisionId == mision, cancelacion)
        ?? throw new ManifiestoNoEncontrado(mision);

    private static Manifiesto Dominio(FilaDeManifiesto f) =>
        new(f.Id, f.MisionId,
            [.. f.Personas.Select(p => new PersonaEnManifiesto(
                p.Nombre, p.Identificacion, p.Forma, p.QueMotivaElTraslado,
                p.Origen, p.Destino, p.RequerimientoOperativo))],
            f.CerradoUtc is { } c ? new DateTimeOffset(c, TimeSpan.Zero) : null,
            f.CierraQuien is { } q ? new IdPersona(q) : null,
            [.. f.Novedades.Select(n => new NovedadDeRuta(
                n.Id, n.Tipo, n.AQuien, n.Motivo, n.DondePaso,
                new DateTimeOffset(n.FechaDelHechoUtc, TimeSpan.Zero)
                    .ToOffset(TimeSpan.FromMinutes(n.DesfaseMinutos)),
                new IdPersona(n.Registra),
                n.Autoriza is { } a ? new IdPersona(a) : null))]);
}

public sealed class ManifiestoNoEncontrado(Ulid mision)
    : Exception($"La misión {mision} no tiene manifiesto abierto.");

/// <param name="Personas">
/// <b>Vacía cuando el alcance es sólo recuento.</b> No se filtró en la respuesta: nunca se
/// cargó en ella.
/// </param>
public sealed record ManifiestoVisto(
    int Declaradas,
    int Efectivas,
    bool Cerrado,
    DateTimeOffset? CerradoEl,
    IReadOnlyList<PersonaEnManifiesto> Personas,
    IReadOnlyList<NovedadDeRuta> Novedades);
