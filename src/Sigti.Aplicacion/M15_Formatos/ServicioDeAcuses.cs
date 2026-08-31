using Microsoft.EntityFrameworkCore;

using Sigti.Aplicacion.M07_ProgramacionYDespacho;
using Sigti.Datos;
using Sigti.Datos.M15_Formatos;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M15_Formatos;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M15_Formatos;

/// <summary>
/// `RN-65` — <b>emitir, imprimir y entregar contra acuse</b>.
///
/// ── Lo que faltaba, y por qué importa ───────────────────────────────────────
/// El salvoconducto se emitía y se imprimía, y nada registraba <b>que llegara a la mano del
/// motorista</b>. Emitir e imprimir son actos de oficina: entre la impresora y el vehículo el
/// papel se pierde — queda en el escritorio, se despacha antes de que salga la impresión, o se
/// entrega a quien pasaba por ahí.
///
/// `§10.2` describe `DESPACHADA` diciendo que <i>«el motorista ya tiene en la mano … los
/// documentos del vehículo … Firmó la recepción»</i>. Esto es esa firma.
/// </summary>
public sealed class ServicioDeAcuses(SigtiDbContext contexto)
{
    /// <summary>
    /// Registra la recepción de un documento impreso.
    ///
    /// <b>Exige que el documento exista y que lo reciba el motorista de la orden.</b> Un acuse
    /// sobre un papel inexistente es una firma sobre nada, y uno a nombre de otro no prueba
    /// nada: el documento es nominativo.
    /// </summary>
    public async Task<Ulid> AcusarAsync(
        Ulid mision,
        DocumentoEntregado documento,
        IdPersona entrega,
        IdPersona recibe,
        string? observaciones,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var expediente = await contexto.Expedientes
            .AsNoTracking()
            .Include(e => e.Transiciones)
            .SingleOrDefaultAsync(e => e.Id == mision, cancelacion)
            ?? throw new ExpedienteNoEncontrado(mision);

        var reserva = ServicioDePermisos.Reserva(expediente);

        var (emitido, folio) = await EmitidoAsync(mision, documento, reserva.Vehiculo, cancelacion);

        var yaAcusado = await contexto.AcusesDeEntrega
            .AnyAsync(a => a.MisionId == mision && a.Documento == documento, cancelacion);

        // ⚠️ Se compara contra el motorista **de la reserva**, no contra el del acuse: si se
        // comparara consigo mismo el bloqueo no dispararía nunca — que es el defecto que ya
        // costó dos veces en `RN-32`.
        var esElDeLaOrden = reserva.Motorista is { } m && m.ToString() == recibe.Valor;

        var porQue = ReglasDelAcuse.PorQueNoSeAcusa(
            documento, emitido, esElDeLaOrden, yaAcusado);

        if (porQue is not null) throw new BloqueoDuro("RN-65", porQue);

        var id = Ulid.NewUlid();

        contexto.AcusesDeEntrega.Add(new FilaDeAcuse
        {
            Id = id,
            MisionId = mision,
            Documento = documento,
            Folio = folio,
            Entrega = entrega.Valor,
            Recibe = recibe.Valor,
            MomentoUtc = momento.UtcDateTime,
            DesfaseMinutos = (int)momento.Offset.TotalMinutes,
            Observaciones = observaciones,
        });

        await contexto.SaveChangesAsync(cancelacion);
        return id;
    }

    /// <summary>
    /// Si el salvoconducto de la misión <b>está emitido y acusado</b> — la otra mitad de
    /// `INV-19`, que `BD-04` comprobaba a medias.
    ///
    /// <b>Las dos condiciones, no una.</b> Emitido y sin acusar significa que el papel existe en
    /// el sistema y nadie declaró tenerlo, que es exactamente la situación que el invariante
    /// impide: la misión sale y el agente pide un papel que quedó en el escritorio.
    /// </summary>
    public async Task<bool> SalvoconductoEnLaManoAsync(
        Ulid mision, CancellationToken cancelacion = default)
    {
        var emitido = await contexto.Salvoconductos
            .AnyAsync(sc => sc.ExpedienteId == mision && !sc.Anulado, cancelacion);

        if (!emitido) return false;

        return await contexto.AcusesDeEntrega.AnyAsync(
            a => a.MisionId == mision && a.Documento == DocumentoEntregado.Salvoconducto,
            cancelacion);
    }

    /// <summary>Los acuses de una misión, para el expediente.</summary>
    public async Task<IReadOnlyList<AcuseRegistrado>> DeLaMisionAsync(
        Ulid mision, CancellationToken cancelacion = default) =>
        [
            .. (await contexto.AcusesDeEntrega
                    .AsNoTracking()
                    .Where(a => a.MisionId == mision)
                    .OrderBy(a => a.MomentoUtc)
                    .ToListAsync(cancelacion))
                .Select(a => new AcuseRegistrado(
                    a.Documento, a.Folio, a.Entrega, a.Recibe,
                    new DateTimeOffset(a.MomentoUtc, TimeSpan.Zero)
                        .ToOffset(TimeSpan.FromMinutes(a.DesfaseMinutos)),
                    a.Observaciones)),
        ];

    /// <summary>
    /// Si el documento existe, y con qué folio.
    ///
    /// El paquete de identificación <b>no lleva folio</b>: se arma en cada impresión y no se
    /// congela, así que lo que se comprueba es que <b>haga falta</b> — que el vehículo no tenga
    /// lámina.
    /// </summary>
    private async Task<(bool Emitido, string? Folio)> EmitidoAsync(
        Ulid mision, DocumentoEntregado documento, Ulid? vehiculo, CancellationToken cancelacion)
    {
        if (documento == DocumentoEntregado.Salvoconducto)
        {
            var sc = await contexto.Salvoconductos
                .AsNoTracking()
                .Where(x => x.ExpedienteId == mision && !x.Anulado)
                .Select(x => x.Folio)
                .FirstOrDefaultAsync(cancelacion);

            return (sc is not null, sc);
        }

        if (vehiculo is not { } id) return (false, null);

        var sinLamina = await contexto.Vehiculos
            .AsNoTracking()
            .AnyAsync(v => v.Id == id && v.EstadoDePlaca != Dominio.M03_Flota.EstadoDePlaca.ConLamina,
                cancelacion);

        return (sinLamina, null);
    }
}

public sealed record AcuseRegistrado(
    DocumentoEntregado Documento,
    string? Folio,
    string Entrega,
    string Recibe,
    DateTimeOffset Momento,
    string? Observaciones);
