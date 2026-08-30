using Microsoft.EntityFrameworkCore;

using Sigti.Datos;
using Sigti.Datos.M02_Parametros;
using Sigti.Datos.M06_Solicitudes;
using Sigti.Dominio.M06_Solicitudes;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M06_Solicitudes;

/// <summary>
/// La emisión de folios contra los rangos pre-asignados — `RN-44`, `RNF-21`.
///
/// ── Por qué el folio se emite al ENVIAR y no al crear ───────────────────────
/// `HU-004`: el número de expediente institucional se asigna cuando la solicitud se somete a
/// autorización. Antes de eso es un borrador que su autor puede descartar, y darle folio
/// gastaría números del rango en cosas que nunca existieron — dejando huecos que después hay
/// que explicar uno por uno ante el auditor.
/// </summary>
public sealed class ServicioDeFolios(SigtiDbContext contexto)
{
    private readonly ParametrosNormativos _parametros = new(contexto);

    public const string TipoOrdenDeMision = "orden-de-mision";

    /// <summary>
    /// Asigna un rango a una delegación. <b>Rechaza el solape</b>, que es la única garantía
    /// real de la unicidad institucional.
    /// </summary>
    public async Task<Ulid> AsignarAsync(
        string delegacion, string tipoDeDocumento, int desde, int hasta,
        IdPersona asigna, DateOnly asignadoEl, string? dispositivo = null,
        CancellationToken cancelacion = default)
    {
        var existentes = await RangosAsync(tipoDeDocumento, cancelacion);
        var id = Ulid.NewUlid();

        var nuevo = new RangoDeFolios(
            id, delegacion, tipoDeDocumento, desde, hasta, 0, dispositivo,
            asigna.Valor, asignadoEl);

        ReglasDelFolio.ExigirSinSolape(nuevo, existentes);

        contexto.RangosDeFolio.Add(new FilaDeRango
        {
            Id = id,
            Delegacion = delegacion,
            TipoDeDocumento = tipoDeDocumento,
            Desde = desde,
            Hasta = hasta,
            Emitidos = 0,
            Dispositivo = dispositivo,
            Asigna = asigna.Valor,
            AsignadoEl = asignadoEl,
        });

        await contexto.SaveChangesAsync(cancelacion);
        return id;
    }

    /// <summary>
    /// Toma el siguiente folio del rango de la delegación.
    ///
    /// ── Nulo cuando la delegación no tiene rango ────────────────────────────
    /// <b>Y eso no bloquea el envío.</b> Hoy ninguna delegación tiene rango asignado —el
    /// circuito acaba de existir— y negar el envío a todas dejaría el sistema inoperante por
    /// una configuración que nadie cargó. Sin rango se sigue con el folio provisional, y la
    /// pantalla dice que lo es.
    ///
    /// Lo que sí bloquea es el rango <b>agotado</b>: ahí hay un rango, se acabó, y emitir
    /// fuera de él produciría un folio que pisa el de otra delegación.
    /// </summary>
    public async Task<FolioEmitido?> EmitirAsync(
        string delegacion, string tipoDeDocumento, DateOnly fechaDelHecho,
        CancellationToken cancelacion = default)
    {
        var fila = await contexto.RangosDeFolio
            .Where(r => r.TipoDeDocumento == tipoDeDocumento && r.Delegacion == delegacion)
            .OrderBy(r => r.Desde)
            .ToListAsync(cancelacion);

        // El primero con saldo. Con varios rangos —una reposición sobre uno casi agotado— se
        // consume el más viejo primero, que es como se gasta un talonario.
        var conSaldo = fila.FirstOrDefault(r => r.Emitidos < r.Hasta - r.Desde + 1);

        if (fila.Count == 0) return null;

        if (conSaldo is null)
        {
            // Todos agotados: se bloquea con el mensaje del dominio, que dice que reponer
            // exige conectividad.
            var ultimo = fila[^1];
            ReglasDelFolio.Siguiente(Dominio(ultimo));
            return null;
        }

        var rango = Dominio(conSaldo);
        var numero = ReglasDelFolio.Siguiente(rango);

        var plantilla = await PlantillaAsync(fechaDelHecho, cancelacion);
        var texto = ReglasDelFolio.Componer(plantilla, rango, fechaDelHecho.Year, numero);

        conSaldo.Emitidos += 1;

        return new FolioEmitido(conSaldo.Id, numero, texto);
    }

    /// <summary>El control de folios de `RNF-21`: rangos, saldo y aviso.</summary>
    public async Task<IReadOnlyList<ControlDeRango>> ControlAsync(
        DateOnly fecha, CancellationToken cancelacion = default)
    {
        var filas = await contexto.RangosDeFolio.AsNoTracking().ToListAsync(cancelacion);
        var umbral = await UmbralAsync(fecha, cancelacion);

        return
        [
            .. filas
                .Select(f => Dominio(f))
                .OrderBy(r => r.Delegacion).ThenBy(r => r.TipoDeDocumento).ThenBy(r => r.Desde)
                .Select(r => new ControlDeRango(r, ReglasDelFolio.Evaluar(r, umbral))),
        ];
    }

    private async Task<IReadOnlyList<RangoDeFolios>> RangosAsync(
        string tipoDeDocumento, CancellationToken cancelacion)
    {
        var filas = await contexto.RangosDeFolio
            .AsNoTracking()
            .Where(r => r.TipoDeDocumento == tipoDeDocumento)
            .ToListAsync(cancelacion);

        return [.. filas.Select(Dominio)];
    }

    /// <summary>
    /// La plantilla del folio. <b>Nula cuando no está fijada</b> — `RNF-21` dice que el formato
    /// «no se decide por inferencia» (insumo #34), así que se resuelve sin bloquear y quien lo
    /// use sigue con el provisional.
    /// </summary>
    private async Task<string?> PlantillaAsync(DateOnly fecha, CancellationToken cancelacion)
    {
        var catalogo = await _parametros.CatalogoDeAsync(
            ReglasDelFolio.ClaveDelFormato, cancelacion);

        return catalogo
            .ResolverSiHay(ReglasDelFolio.ClaveDelFormato, fecha, DateTimeOffset.UtcNow)
            ?.Valor;
    }

    private async Task<decimal?> UmbralAsync(DateOnly fecha, CancellationToken cancelacion)
    {
        var catalogo = await _parametros.CatalogoDeAsync(
            ReglasDelFolio.ClaveDelUmbral, cancelacion);

        var valor = catalogo
            .ResolverSiHay(ReglasDelFolio.ClaveDelUmbral, fecha, DateTimeOffset.UtcNow)
            ?.Valor;

        return decimal.TryParse(valor, out var n) && n is > 0 and <= 1 ? n : null;
    }

    private static RangoDeFolios Dominio(FilaDeRango f) =>
        new(f.Id, f.Delegacion, f.TipoDeDocumento, f.Desde, f.Hasta, f.Emitidos,
            f.Dispositivo, f.Asigna, f.AsignadoEl);
}

/// <param name="Texto">
/// <b>Nulo cuando no hay plantilla configurada.</b> El número sí se consumió: el hueco existe y
/// se explica, aunque el folio impreso todavía no se pueda componer.
/// </param>
public sealed record FolioEmitido(Ulid RangoId, int Numero, string? Texto);

public sealed record ControlDeRango(RangoDeFolios Rango, AvisoDeRango Aviso);
