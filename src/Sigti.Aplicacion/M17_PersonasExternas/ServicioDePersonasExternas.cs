using Microsoft.EntityFrameworkCore;

using Sigti.Datos;
using Sigti.Datos.M17_PersonasExternas;
using Sigti.Dominio.M17_PersonasExternas;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M17_PersonasExternas;

/// <summary>
/// El catálogo de campos y el registro de consultas — `RN-51`, `RN-52`.
///
/// ── Lo que este servicio hace y ningún otro debería ─────────────────────────
/// <b>Registrar cada acceso.</b> `RN-52` no admite excepción: <i>«ningún rol, incluido ACT-01
/// Administrador del Sistema, debe poder consultar estos datos sin dejar rastro»</i>. Si el
/// registro dependiera de que cada consulta se acuerde de llamarlo, la primera que se olvide
/// deja un hueco que nadie va a notar hasta el hábeas data.
/// </summary>
public sealed class ServicioDePersonasExternas(SigtiDbContext contexto)
{
    /// <summary>
    /// Activa o crea un campo del manifiesto.
    ///
    /// ── Devuelve la advertencia; <b>no bloquea</b> ──────────────────────────
    /// `HU-112`: un campo sensible sin fundamento <b>se activa y queda marcado</b>. Bloquearlo
    /// mandaría el dato a la libreta de alguien, fuera de todo control.
    /// </summary>
    public async Task<string?> ActivarAsync(
        string clave, string etiqueta, ClaseDelCampo clase, IdPersona activa,
        DateTimeOffset momento, string? baseLegal = null, string? necesidadOperativa = null,
        CancellationToken cancelacion = default)
    {
        // Si viene fundamento, tiene que venir entero. Medio fundamento se rechaza **antes** de
        // activar: guardarlo a medias dejaría un campo que parece fundamentado y no lo está.
        if (baseLegal is not null || necesidadOperativa is not null)
            ReglasDelCampoSensible.ExigirFundamentoCompleto(baseLegal, necesidadOperativa);

        var fila = await contexto.CamposDelManifiesto
            .SingleOrDefaultAsync(c => c.Clave == clave, cancelacion);

        if (fila is null)
        {
            fila = new FilaDeCampoDelManifiesto
            {
                Id = Ulid.NewUlid(),
                Clave = clave,
                Etiqueta = etiqueta,
                Clase = clase,
                Activo = true,
                Activa = activa.Valor,
                ActivadoUtc = momento.UtcDateTime,
            };
            contexto.CamposDelManifiesto.Add(fila);
        }
        else
        {
            fila.Activo = true;
        }

        if (baseLegal is not null && necesidadOperativa is not null)
        {
            fila.BaseLegal = baseLegal.Trim();
            fila.NecesidadOperativa = necesidadOperativa.Trim();
            fila.FundamentaPersona = activa.Valor;
            fila.FundamentadoUtc = momento.UtcDateTime;
        }

        await contexto.SaveChangesAsync(cancelacion);

        return ReglasDelCampoSensible.AdvertenciaAlActivar(
            clase, Fundamento(fila));
    }

    /// <summary>
    /// Registra el fundamento de un campo que se activó sin él.
    /// </summary>
    public async Task FundamentarAsync(
        string clave, string baseLegal, string necesidadOperativa, IdPersona registra,
        DateTimeOffset momento, CancellationToken cancelacion = default)
    {
        ReglasDelCampoSensible.ExigirFundamentoCompleto(baseLegal, necesidadOperativa);

        var fila = await contexto.CamposDelManifiesto
            .SingleOrDefaultAsync(c => c.Clave == clave, cancelacion)
            ?? throw new CampoNoEncontrado(clave);

        fila.BaseLegal = baseLegal.Trim();
        fila.NecesidadOperativa = necesidadOperativa.Trim();
        fila.FundamentaPersona = registra.Valor;
        fila.FundamentadoUtc = momento.UtcDateTime;

        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>El catálogo entero, para `PT-128`.</summary>
    public async Task<IReadOnlyList<CampoDelManifiesto>> CamposAsync(
        CancellationToken cancelacion = default)
    {
        var filas = await contexto.CamposDelManifiesto.AsNoTracking().ToListAsync(cancelacion);

        return
        [
            .. filas
                .Select(f => new CampoDelManifiesto(
                    f.Clave, f.Etiqueta, f.Clase, f.Activo, Fundamento(f)))

                // Los que faltan fundamentar, primero: es el reporte que el Auditor Interno
                // abre, y lo que busca está arriba.
                .OrderByDescending(c => c.SinFundamento)
                .ThenBy(c => c.Etiqueta),
        ];
    }

    /// <summary>
    /// Deja el asiento de una consulta — `RN-52`.
    ///
    /// <b>Se llama antes de mostrar</b>, no después. Si el registro fuera posterior, una
    /// consulta que revienta a mitad habría mostrado el dato sin dejar rastro — y ése es
    /// justamente el acceso que interesa auditar.
    /// </summary>
    public async Task<Ulid> RegistrarConsultaAsync(
        IdPersona consultante, string rol, string registroConsultado,
        AlcanceDeLaConsulta alcance, DateTimeOffset momento,
        string? necesidadDeConocer = null, string? origen = null,
        CancellationToken cancelacion = default)
    {
        ReglasDeLaConsulta.ExigirNecesidadDeConocer(alcance, necesidadDeConocer);

        var id = Ulid.NewUlid();

        contexto.ConsultasAManifiestos.Add(new FilaDeConsultaAManifiesto
        {
            Id = id,
            Consultante = consultante.Valor,
            Rol = rol,
            MomentoUtc = momento.UtcDateTime,
            RegistroConsultado = registroConsultado,
            Alcance = alcance,
            NecesidadDeConocer = necesidadDeConocer?.Trim(),
            Origen = origen,
        });

        await contexto.SaveChangesAsync(cancelacion);
        return id;
    }

    /// <summary>
    /// El reporte de accesos de `PT-133`, y los patrones que merecen una pregunta.
    /// </summary>
    /// <param name="registro">
    /// Acota a un expediente. <b>Es la consulta del hábeas data</b>: quién vio lo mío.
    /// </param>
    public async Task<ReporteDeAccesos> AccesosAsync(
        DateTimeOffset desde, DateTimeOffset ahora, int umbral,
        string? registro = null, CancellationToken cancelacion = default)
    {
        var consulta = contexto.ConsultasAManifiestos.AsNoTracking()
            .Where(c => c.MomentoUtc >= desde.UtcDateTime);

        if (registro is not null)
            consulta = consulta.Where(c => c.RegistroConsultado == registro);

        var filas = await consulta.ToListAsync(cancelacion);
        var accesos = filas.Select(Dominio).ToList();

        return new ReporteDeAccesos(
            [.. accesos.OrderByDescending(a => a.Momento)],
            ReglasDeLaConsulta.Patrones(accesos, desde, umbral),

            // Cuánto del registro es inauditable. Va al lado del total siempre: «120 accesos» y
            // «120 accesos, 38 sin decir para qué» sostienen conclusiones distintas.
            accesos.Count(a => string.IsNullOrWhiteSpace(a.NecesidadDeConocer)));
    }

    private static FundamentoDelCampo? Fundamento(FilaDeCampoDelManifiesto f) =>
        f is { BaseLegal: { } legal, NecesidadOperativa: { } necesidad,
               FundamentaPersona: { } quien, FundamentadoUtc: { } cuando }
            ? new FundamentoDelCampo(legal, necesidad, new IdPersona(quien),
                                     new DateTimeOffset(cuando, TimeSpan.Zero))
            : null;

    private static ConsultaRegistrada Dominio(FilaDeConsultaAManifiesto f) =>
        new(f.Id, new IdPersona(f.Consultante), f.Rol,
            new DateTimeOffset(f.MomentoUtc, TimeSpan.Zero),
            f.RegistroConsultado, f.Alcance, f.NecesidadDeConocer, f.Origen);
}

public sealed class CampoNoEncontrado(string clave)
    : Exception($"No existe el campo «{clave}» en el catálogo del manifiesto.");

/// <param name="SinNecesidadDeclarada">
/// Cuántos accesos no dijeron para qué. <b>Va al lado del total siempre</b>: es la medida de
/// cuánto del registro no se puede auditar.
/// </param>
public sealed record ReporteDeAccesos(
    IReadOnlyList<ConsultaRegistrada> Accesos,
    IReadOnlyList<PatronDeAcceso> Patrones,
    int SinNecesidadDeclarada);
