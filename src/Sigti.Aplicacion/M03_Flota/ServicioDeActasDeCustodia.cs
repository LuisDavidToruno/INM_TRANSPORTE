using Microsoft.EntityFrameworkCore;

using Sigti.Datos;
using Sigti.Datos.M03_Flota;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M03_Flota;

/// <summary>
/// `RN-22` — el <b>traslado temporal de custodia</b> al motorista, con acta en los dos extremos.
///
/// ── Lo que faltaba ──────────────────────────────────────────────────────────
/// `BD-13` ya impedía despachar un vehículo sin custodio vigente. Lo que no existía era el
/// <b>traslado</b>: el acto por el cual ese custodio le entrega la unidad al motorista y se la
/// vuelve a recibir, con odómetro, nivel, accesorios, estado y constancia.
///
/// Sin él, el sistema sabía <b>de quién es</b> el vehículo y no <b>quién lo tenía</b> — y la
/// segunda es la que hace falta cuando falta un gato o aparece un golpe.
/// </summary>
public sealed class ServicioDeActasDeCustodia(SigtiDbContext contexto)
{
    /// <summary>
    /// Registra un acta. <b>La devolución exige que exista la entrega</b>: sin ella no hay
    /// contra qué comparar, y comparar es lo único para lo que el acta sirve.
    /// </summary>
    public async Task<Ulid> RegistrarAsync(
        Ulid mision,
        Ulid vehiculo,
        ActaDeCustodia acta,
        CancellationToken cancelacion = default)
    {
        var existentes = await contexto.ActasDeCustodia
            .AsNoTracking()
            .Where(a => a.MisionId == mision)
            .Select(a => a.Tipo)
            .ToListAsync(cancelacion);

        var porQue = ReglasDelActaDeCustodia.PorQueNoSeRegistra(
            acta.Tipo,
            hayEntregaPrevia: existentes.Contains(TipoDeActa.Entrega),
            yaHayDeLaMismaClase: existentes.Contains(acta.Tipo),
            acta.EstadoDeLaUnidad);

        if (porQue is not null) throw new BloqueoDuro("RN-22", porQue);

        var id = Ulid.NewUlid();

        var fila = new FilaDeActaDeCustodia
        {
            Id = id,
            MisionId = mision,
            VehiculoId = vehiculo,
            Tipo = acta.Tipo,
            Entrega = acta.Entrega.Valor,
            Recibe = acta.Recibe.Valor,
            MomentoUtc = acta.Momento.UtcDateTime,
            DesfaseMinutos = (int)acta.Momento.Offset.TotalMinutes,
            Odometro = acta.Odometro,
            NivelDeTanque = acta.NivelDeTanque,
            EstadoDeLaUnidad = acta.EstadoDeLaUnidad.Trim(),
            Observaciones = acta.Observaciones,
        };

        foreach (var e in acta.Elementos)
        {
            fila.Elementos.Add(new FilaDeElementoDelActa
            {
                Id = Ulid.NewUlid(),
                ActaId = id,
                Nombre = e.Nombre.Trim(),
                Presente = e.Presente,
                Observacion = e.Observacion,
            });
        }

        contexto.ActasDeCustodia.Add(fila);
        await contexto.SaveChangesAsync(cancelacion);

        return id;
    }

    /// <summary>
    /// Las actas de una misión <b>con el cotejo</b>, que es el producto.
    ///
    /// Dos listas por separado no las lee nadie: el gato que no volvió se ve al restarlas.
    /// </summary>
    public async Task<CustodiaDeLaMision> DeLaMisionAsync(
        Ulid mision, CancellationToken cancelacion = default)
    {
        var filas = await contexto.ActasDeCustodia
            .AsNoTracking()
            .Include(a => a.Elementos)
            .Where(a => a.MisionId == mision)
            .ToListAsync(cancelacion);

        var entrega = Convertir(filas.SingleOrDefault(a => a.Tipo == TipoDeActa.Entrega));
        var devolucion = Convertir(filas.SingleOrDefault(a => a.Tipo == TipoDeActa.Devolucion));

        // ⚠️ **El cotejo sólo existe con las dos.** Con una sola no hay nada que restar, y
        // devolver un cotejo vacío se leería como «no faltó nada» — que es una afirmación que
        // nadie hizo.
        var cotejo = entrega is not null && devolucion is not null
            ? ReglasDelActaDeCustodia.Cotejar(entrega, devolucion)
            : null;

        return new CustodiaDeLaMision(entrega, devolucion, cotejo);
    }

    private static ActaDeCustodia? Convertir(FilaDeActaDeCustodia? f) =>
        f is null
            ? null
            : new ActaDeCustodia(
                f.Tipo,
                new IdPersona(f.Entrega),
                new IdPersona(f.Recibe),
                new DateTimeOffset(f.MomentoUtc, TimeSpan.Zero)
                    .ToOffset(TimeSpan.FromMinutes(f.DesfaseMinutos)),
                f.Odometro,
                f.NivelDeTanque,
                f.EstadoDeLaUnidad,
                [
                    .. f.Elementos.Select(e => new ElementoDeLaUnidad(
                        e.Nombre, e.Presente, e.Observacion)),
                ],
                f.Observaciones);
}

/// <param name="Cotejo">
/// ⚠️ <b>Nulo mientras falte una de las dos actas.</b> No es un cotejo sin hallazgos: es que no
/// hay nada que restar todavía, y las dos cosas se leen distinto.
/// </param>
public sealed record CustodiaDeLaMision(
    ActaDeCustodia? Entrega,
    ActaDeCustodia? Devolucion,
    CotejoDeLaDevolucion? Cotejo);
