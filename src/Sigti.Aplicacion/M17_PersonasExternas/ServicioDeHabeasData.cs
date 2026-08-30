using Microsoft.EntityFrameworkCore;

using Sigti.Datos;
using Sigti.Datos.M02_Parametros;
using Sigti.Datos.M17_PersonasExternas;
using Sigti.Dominio.M17_PersonasExternas;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M17_PersonasExternas;

/// <summary>
/// El ciclo de vida del dato personal — `PT-134`, `PT-135`, `PT-136`, `PT-137`.
///
/// ── Las cuatro cosas que la Constitución permite pedir ──────────────────────
/// El hábeas data del Artículo 182 está vigente `[V]`. Este servicio resuelve las cuatro que el
/// sistema tiene que poder contestar: <b>qué guardan sobre mí</b>, <b>quién lo vio</b>,
/// <b>corríjanlo</b>, y —del lado de la institución— <b>cuándo deja de guardarse</b>.
/// </summary>
public sealed class ServicioDeHabeasData(
    SigtiDbContext contexto, ServicioDePersonasExternas personas)
{
    private readonly ParametrosNormativos _parametros = new(contexto);

    /// <summary>
    /// `PT-134` — todo lo que el sistema guarda sobre una persona.
    ///
    /// ── «En minutos» es parte del requisito ─────────────────────────────────
    /// `HU-121`: la respuesta debe ser <i>«expedita y no onerosa»</i>, y <i>«sin depender de que
    /// alguien consulte la base de datos a mano»</i>. Una institución que tarda semanas en
    /// contestar un hábeas data lo incumple aunque termine contestando.
    ///
    /// ── Y la consulta misma queda registrada ────────────────────────────────
    /// Atender un hábeas data implica <b>leer datos personales</b>, así que deja asiento como
    /// cualquier otro acceso. No hacerlo dejaría fuera del registro justamente las consultas más
    /// sensibles del sistema.
    /// </summary>
    public async Task<ExpedienteDeLaPersona> BuscarAsync(
        string identificacion, IdPersona consultante, string rol, DateTimeOffset momento,
        string? origen = null, CancellationToken cancelacion = default)
    {
        await personas.RegistrarConsultaAsync(
            consultante, rol, $"habeas-data:{identificacion}",
            AlcanceDeLaConsulta.ManifiestoCompleto, momento,
            "atención de una acción de hábeas data", origen, cancelacion);

        var apariciones = await contexto.Set<FilaDePersonaEnManifiesto>()
            .AsNoTracking()
            .Where(p => p.Identificacion == identificacion)
            .ToListAsync(cancelacion);

        var manifiestos = apariciones.Select(a => a.ManifiestoId).Distinct().ToList();

        var misiones = await contexto.Manifiestos
            .AsNoTracking()
            .Where(m => manifiestos.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, m => m.MisionId, cancelacion);

        var rectificaciones = await contexto.Set<FilaDeRectificacion>()
            .AsNoTracking()
            .Where(r => manifiestos.Contains(r.ManifiestoId))
            .ToListAsync(cancelacion);

        // Quién vio los manifiestos donde aparece. **Es la segunda pregunta del hábeas data**, y
        // la que la institución sólo puede responder si registró cada acceso.
        var misionesTexto = misiones.Values.Select(v => v.ToString()).ToList();

        var accesos = await contexto.ConsultasAManifiestos
            .AsNoTracking()
            .Where(c => misionesTexto.Contains(c.RegistroConsultado))
            .OrderByDescending(c => c.MomentoUtc)
            .ToListAsync(cancelacion);

        return new ExpedienteDeLaPersona(
            identificacion,
            [
                .. apariciones.Select(a => new AparicionEnManifiesto(
                    misiones.GetValueOrDefault(a.ManifiestoId).ToString(),
                    a.Nombre, a.Forma.ToString(), a.QueMotivaElTraslado,
                    a.Origen, a.Destino, a.RequerimientoOperativo)),
            ],
            [
                .. rectificaciones.Select(r => new RectificacionVista(
                    r.Campo, r.ValorAnterior, r.ValorRectificado, r.QuienLaPidio, r.Motivo,
                    new DateTimeOffset(r.MomentoUtc, TimeSpan.Zero))),
            ],
            [
                .. accesos.Select(c => new AccesoVisto(
                    c.Consultante, c.Rol,
                    new DateTimeOffset(c.MomentoUtc, TimeSpan.Zero),
                    c.Alcance.ToString(), c.NecesidadDeConocer)),
            ]);
    }

    /// <summary>
    /// `PT-135` — rectifica <b>sin destruir</b> el asiento original.
    /// </summary>
    public async Task RectificarAsync(
        Ulid manifiesto, string campo, string valorAnterior, string valorRectificado,
        string quienLaPidio, string motivo, IdPersona registra, DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        ReglasDeLaRectificacion.Exigir(quienLaPidio, motivo);

        contexto.Set<FilaDeRectificacion>().Add(new FilaDeRectificacion
        {
            Id = Ulid.NewUlid(),
            ManifiestoId = manifiesto,
            Campo = campo,

            // ⚠️ El manifiesto **no se toca**. Un manifiesto editado deja de coincidir con la
            // lista impresa que el motorista llevó, y esa discrepancia aparece años después sin
            // nadie que pueda explicarla.
            ValorAnterior = valorAnterior,
            ValorRectificado = valorRectificado,

            QuienLaPidio = quienLaPidio.Trim(),
            Motivo = motivo.Trim(),
            Registra = registra.Valor,
            MomentoUtc = momento.UtcDateTime,
        });

        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>
    /// `PT-136` — el reporte de transparencia, <b>sin ningún dato personal</b>.
    ///
    /// ── No filtra: sale de otro origen ──────────────────────────────────────
    /// `RN-51` punto 3: <i>«los reportes públicos se generan desde la vista de gestión pública,
    /// <b>sin acceso técnico</b> a los campos personales — no por filtrado en el reporte, sino
    /// por separación de origen»</i>.
    ///
    /// La diferencia es que un filtro <b>se puede olvidar</b>, y basta que alguien agregue una
    /// columna al reporte para publicar nombres. Acá el manifiesto ni se consulta: la única
    /// cifra que cruza la frontera es <b>cuántas personas</b>, que es dato de gestión.
    /// </summary>
    public async Task<IReadOnlyList<FilaDeTransparencia>> TransparenciaAsync(
        DateOnly desde, DateOnly hasta, CancellationToken cancelacion = default)
    {
        var expedientes = await contexto.Expedientes
            .AsNoTracking()
            .Include(e => e.Transiciones)
            .Where(e => e.Salida >= desde && e.Salida <= hasta)
            .ToListAsync(cancelacion);

        var ids = expedientes.Select(e => e.Id).ToList();

        // Sólo el RECUENTO. Ni un nombre, ni una identificación, ni un requerimiento: nada de
        // la tabla de personas entra en esta consulta.
        var cuantas = await contexto.Manifiestos
            .AsNoTracking()
            .Where(m => ids.Contains(m.MisionId))
            .Select(m => new { m.MisionId, Personas = m.Personas.Count })
            .ToDictionaryAsync(x => x.MisionId, x => x.Personas, cancelacion);

        return
        [
            .. expedientes
                .OrderByDescending(e => e.Salida)
                .Select(e => new FilaDeTransparencia(
                    M07_ProgramacionYDespacho.ConsultaDeMisiones.Folio(e),
                    e.Transiciones.MaxBy(t => t.Orden)?.Destino.ToString() ?? "sin diario",
                    e.Dependencia,
                    e.Destino,
                    e.ObjetoDelTraslado,
                    e.Salida,
                    e.Retorno,

                    // Cuántas personas se trasladaron. **Es lo máximo que cruza la frontera**, y
                    // no identifica a nadie.
                    cuantas.GetValueOrDefault(e.Id))),
        ];
    }

    /// <summary>
    /// `PT-137` — la depuración. <b>Lo único del sistema que destruye contenido.</b>
    /// </summary>
    /// <param name="soloSimular">
    /// Cuando es verdadero, cuenta qué se depuraría y <b>no borra nada</b>. Es lo que la
    /// pantalla de estado muestra antes del aviso: nadie debería poder ejecutar esto sin haber
    /// visto primero cuánto alcanza.
    /// </param>
    public async Task<ResultadoDeLaDepuracion> DepurarAsync(
        DateTimeOffset ahora, DateTimeOffset? avisadoEl, bool soloSimular,
        IReadOnlyList<string>? segmentos = null, CancellationToken cancelacion = default)
    {
        var plazo = await PlazoAsync(DateOnly.FromDateTime(ahora.UtcDateTime), cancelacion);

        ReglasDeLaDepuracion.ExigirPlazoConfigurado(plazo);
        ReglasDeLaDepuracion.ExigirSoloDatosPersonales(segmentos ?? ["manifiesto-personas"]);

        // La simulación no exige aviso: es precisamente lo que hay que ver **antes** de avisar.
        if (!soloSimular) ReglasDeLaDepuracion.ExigirAvisoPrevio(avisadoEl, ahora);

        var manifiestos = await contexto.Manifiestos
            .Include(m => m.Personas)
            .Where(m => m.CerradoUtc != null)
            .ToListAsync(cancelacion);

        var alcanzados = manifiestos
            .Where(m => ReglasDeLaDepuracion.AlcanzoElPlazo(
                new DateTimeOffset(m.CerradoUtc!.Value, TimeSpan.Zero), plazo!.Value, ahora))
            .ToList();

        var personasAlcanzadas = alcanzados.Sum(m => m.Personas.Count);

        if (!soloSimular)
        {
            // ⚠️ Se borran **las personas**, no los manifiestos. El manifiesto queda con su
            // recuento y sus novedades: la cadena de auditoría tiene que seguir verificando
            // después, y un expediente sin manifiesto sería un hueco sin explicación.
            foreach (var m in alcanzados)
            {
                contexto.Set<FilaDePersonaEnManifiesto>().RemoveRange(m.Personas);
            }

            await contexto.SaveChangesAsync(cancelacion);
        }

        return new ResultadoDeLaDepuracion(
            plazo.Value, alcanzados.Count, personasAlcanzadas, soloSimular);
    }

    /// <summary>
    /// El plazo. <b>Nulo cuando no está configurado</b>, y no se sustituye por nada.
    /// </summary>
    public async Task<int?> PlazoAsync(DateOnly fecha, CancellationToken cancelacion = default)
    {
        var catalogo = await _parametros.CatalogoDeAsync(
            ReglasDeLaDepuracion.ClaveDelPlazo, cancelacion);

        var valor = catalogo
            .ResolverSiHay(ReglasDeLaDepuracion.ClaveDelPlazo, fecha, DateTimeOffset.UtcNow)
            ?.Valor;

        return int.TryParse(valor, out var dias) && dias > 0 ? dias : null;
    }
}

public sealed record ExpedienteDeLaPersona(
    string Identificacion,
    IReadOnlyList<AparicionEnManifiesto> Apariciones,
    IReadOnlyList<RectificacionVista> Rectificaciones,

    /// <summary>
    /// Quién vio los manifiestos donde aparece. <b>La segunda pregunta del hábeas data</b>, y la
    /// que sólo se puede contestar si cada acceso quedó registrado.
    /// </summary>
    IReadOnlyList<AccesoVisto> QuienLoVio);

public sealed record AparicionEnManifiesto(
    string Mision, string? Nombre, string Forma, string QueMotivaElTraslado,
    string Origen, string Destino, string? RequerimientoOperativo);

public sealed record RectificacionVista(
    string Campo, string ValorAnterior, string ValorRectificado,
    string QuienLaPidio, string Motivo, DateTimeOffset Momento);

public sealed record AccesoVisto(
    string Consultante, string Rol, DateTimeOffset Momento, string Alcance, string? Necesidad);

/// <param name="Personas">
/// Cuántas se trasladaron. <b>Es lo máximo que cruza la frontera</b> hacia el reporte público, y
/// no identifica a nadie.
/// </param>
public sealed record FilaDeTransparencia(
    string Folio, string Estado, string Dependencia, string Destino, string ObjetoDelTraslado,
    DateOnly Salida, DateOnly Retorno, int Personas);

/// <param name="Simulacion">
/// Cuando es verdadero <b>no se borró nada</b>: es el conteo previo que hay que ver antes de
/// avisar.
/// </param>
public sealed record ResultadoDeLaDepuracion(
    int PlazoEnDias, int Manifiestos, int Personas, bool Simulacion);
