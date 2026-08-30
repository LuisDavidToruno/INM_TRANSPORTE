using Microsoft.EntityFrameworkCore;

using Sigti.Datos;
using Sigti.Dominio.M02_Parametros;

namespace Sigti.Aplicacion.M02_Parametros;

/// <summary>
/// `PT-100` — lo que quien aprueba tiene que ver <b>antes</b> de aprobar.
///
/// ── Por qué no basta con mostrar el valor nuevo ─────────────────────────────
/// `HU-145` enumera lo que la pantalla debe traer: valor anterior, valor nuevo, fuente, fecha de
/// verificación, respaldo adjunto, quién cargó y desde cuándo regiría. La lista no es de adorno:
/// <b>aprobar un parámetro normativo es un acto de control interno</b>, y quien lo firma responde
/// por él ante el Tribunal Superior de Cuentas. Un botón «Aprobar» junto a un número suelto
/// convierte ese acto en un trámite.
///
/// El valor anterior es el que más se olvida y el que más importa: <b>sin él no hay nada que
/// comparar</b>, y «25 %» no le dice a nadie si es un ajuste menor o si duplica la tolerancia.
/// </summary>
public sealed class ConsultaDeAprobaciones(SigtiDbContext contexto)
{
    /// <summary>
    /// Las cargas pendientes de aprobación, cada una con su contexto completo.
    ///
    /// <b>Sólo las pendientes.</b> Una lista de todas las versiones sería un historial —útil, y
    /// otra pantalla—; ésta es una bandeja de trabajo, y lo que no se puede aprobar sobra en ella.
    /// </summary>
    public async Task<IReadOnlyList<CargaPorAprobar>> PendientesAsync(
        DateOnly hoy, CancellationToken cancelacion = default)
    {
        var pendientes = await contexto.Parametros
            .AsNoTracking()
            .Where(v => v.AprobadoPor == null && v.RegistradoHasta == null)
            .ToListAsync(cancelacion);

        if (pendientes.Count == 0) return [];

        var claves = pendientes.Select(v => v.Clave).Distinct().ToList();

        // Lo aprobado de esas mismas claves: de aquí sale el valor anterior.
        var aprobadas = await contexto.Parametros
            .AsNoTracking()
            .Where(v => claves.Contains(v.Clave) && v.AprobadoPor != null && v.RegistradoHasta == null)
            .ToListAsync(cancelacion);

        // ⚠️ **El identificador del adjunto no es el adjunto.** Se comprueba contra la tabla,
        // no contra el tipo: el `Ulid` siempre está — lo que puede faltar es el archivo.
        var adjuntos = pendientes.Select(v => v.Respaldo.Adjunto).Distinct().ToList();
        var existentes = await contexto.Adjuntos
            .AsNoTracking()
            .Where(a => adjuntos.Contains(a.Id))
            .Select(a => a.Id)
            .ToListAsync(cancelacion);

        var resultado = new List<CargaPorAprobar>();

        foreach (var v in pendientes.OrderBy(v => v.Clave).ThenBy(v => v.VigenteDesde))
        {
            // El que regía justo antes de que empiece el nuevo. No es «el último cargado»:
            // es el que la línea de vigencias pone inmediatamente antes.
            var anterior = aprobadas
                .Where(a => a.Clave == v.Clave && a.VigenteDesde < v.VigenteDesde)
                .MaxBy(a => a.VigenteDesde);

            resultado.Add(new CargaPorAprobar(
                v.Id, v.Clave, v.Valor,
                ValorAnterior: anterior?.Valor,
                AnteriorVigenteDesde: anterior?.VigenteDesde,
                VigenteDesde: v.VigenteDesde,
                VigenteHasta: v.VigenteHasta,
                CargadoPor: v.CargadoPor.Valor,
                CargadoEl: v.RegistradoDesde,
                Fuente: v.Respaldo.Fuente,
                VerificadoEl: v.Respaldo.FechaDeVerificacion,
                Adjunto: v.Respaldo.Adjunto,
                ElRespaldoExiste: existentes.Contains(v.Respaldo.Adjunto),
                Alcance: await AlcanceAsync(v, hoy, cancelacion)));
        }

        return resultado;
    }

    /// <summary>
    /// A qué alcanza aprobar esto. Ver <see cref="AlcanceDeLaAprobacion"/> para por qué es
    /// esto y no el «impacto estimado» que pedía la historia.
    /// </summary>
    private async Task<AlcanceDeLaAprobacion> AlcanceAsync(
        VersionDeParametro v, DateOnly hoy, CancellationToken cancelacion)
    {
        if (v.VigenteDesde > hoy)
            return AlcanceDeLaAprobacion.SoloHaciaAdelante(v.VigenteDesde);

        // Retroactivo. **Éste es el caso que nadie mira y el único que puede reabrir lo
        // cerrado**, porque `P-4` obliga a calcular con la tabla vigente a la fecha del hecho.
        var fin = v.VigenteHasta is { } h && h < hoy ? h : hoy;

        var misiones = await contexto.Expedientes
            .AsNoTracking()
            .CountAsync(e => e.Salida >= v.VigenteDesde && e.Salida <= fin, cancelacion);

        return AlcanceDeLaAprobacion.Retroactivo(v.VigenteDesde, fin, misiones);
    }
}

/// <summary>
/// Qué queda alcanzado por aprobar esta carga.
///
/// ── ⚠️ Por qué esto y no el «impacto estimado» de `HU-145` ──────────────────
/// La historia pide una frase del tipo <i>«con 25 % dejarían de generarse 34 de los 41 hallazgos
/// de consumo del último trimestre»</i>. Es una buena idea y <b>no se puede sostener de forma
/// general</b>: sólo tiene sentido para los parámetros que son umbrales de un cálculo, y no
/// significa nada para el formato del folio o el canal de aviso. Producirla para unos y dejarla
/// en blanco para otros haría que la ausencia se leyera como «sin impacto», que es falso.
///
/// Peor: recalcularla exige rehacer la conciliación de cada misión del período con el valor
/// nuevo. Una cifra aproximada, en una pantalla cuyo propósito es que alguien firme, es
/// exactamente el tipo de dato que después se cita como si fuera exacto.
///
/// Lo que sí es cierto para <b>todo</b> parámetro, exacto, y más grave que un conteo de
/// hallazgos, es <b>desde cuándo rige</b>. Si la vigencia arranca antes de hoy, aprobar no
/// cambia el futuro: cambia la base de cálculo de hechos <b>ya ocurridos y ya registrados</b>,
/// porque `P-4` manda usar la tabla vigente a la fecha del hecho. Eso se sabe sin estimar nada.
/// </summary>
/// <param name="MisionesAlcanzadas">
/// Cuántas misiones tienen su fecha de salida dentro de la ventana retroactiva.
///
/// <b>Es una cota superior, no una cuenta de afectadas</b>: no toda misión usa todo parámetro.
/// La pantalla lo dice con esas palabras — un número presentado como exacto se cita como exacto.
/// </param>
public sealed record AlcanceDeLaAprobacion(
    bool EsRetroactivo, DateOnly Desde, DateOnly? Hasta, int MisionesAlcanzadas)
{
    public static AlcanceDeLaAprobacion SoloHaciaAdelante(DateOnly desde) =>
        new(false, desde, null, 0);

    public static AlcanceDeLaAprobacion Retroactivo(DateOnly desde, DateOnly hasta, int misiones) =>
        new(true, desde, hasta, misiones);
}

/// <param name="ValorAnterior">
/// El que regía inmediatamente antes. <b>Nulo cuando es la primera carga de la clave</b> — y eso
/// no es un dato que falte: significa que hasta hoy el control estaba apagado, que es
/// información distinta y la pantalla la dice distinto.
/// </param>
/// <param name="ElRespaldoExiste">
/// Si el adjunto al que apunta la carga está realmente. <b>Falso bloquea la aprobación</b>: la
/// fuente y la fecha de verificación son texto que alguien escribió, y aprobar sin poder abrir
/// el documento es firmar que se verificó algo que nadie vio.
/// </param>
public sealed record CargaPorAprobar(
    Ulid Id,
    string Clave,
    string Valor,
    string? ValorAnterior,
    DateOnly? AnteriorVigenteDesde,
    DateOnly VigenteDesde,
    DateOnly? VigenteHasta,
    string CargadoPor,
    DateTimeOffset CargadoEl,
    string Fuente,
    DateOnly VerificadoEl,
    Ulid Adjunto,
    bool ElRespaldoExiste,
    AlcanceDeLaAprobacion Alcance);
