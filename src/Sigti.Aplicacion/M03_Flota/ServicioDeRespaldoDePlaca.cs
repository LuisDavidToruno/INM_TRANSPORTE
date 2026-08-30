using Microsoft.EntityFrameworkCore;

using Sigti.Datos;
using Sigti.Datos.M03_Flota;
using Sigti.Dominio;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M03_Flota;

/// <summary>
/// `RN-64` y `RN-65` — el estado de la lámina y los documentos que sostienen la circulación
/// sin ella.
///
/// ── ⚠️ Por qué esto no puede faltar ─────────────────────────────────────────
/// El bloqueo de `RN-65` ya opera: sin lámina y sin respaldo vigente, no se despacha. Un
/// bloqueo sin forma de levantarlo es una puerta sin llave — <b>ya pasó una vez este mismo día
/// con el permiso de circulación</b>, donde `BD-04` bloqueaba y nadie podía emitir el permiso.
/// </summary>
public sealed class ServicioDeRespaldoDePlaca(SigtiDbContext contexto)
{
    /// <summary>
    /// Declara el estado de la lámina — `RN-64`.
    ///
    /// <b>El número de placa no se toca acá</b>: son dos datos distintos y no intercambiables.
    /// Un vehículo puede tener número asignado y no tener lámina, y confundirlos es lo que
    /// hace que las tres situaciones administrativas se vean iguales.
    /// </summary>
    public async Task DeclararEstadoAsync(
        Ulid vehiculo, EstadoDePlaca estado, CancellationToken cancelacion = default)
    {
        var fila = await contexto.Vehiculos.SingleOrDefaultAsync(v => v.Id == vehiculo, cancelacion)
            ?? throw new BloqueoDuro("RN-64", $"No existe el vehículo {vehiculo}.");

        fila.EstadoDePlaca = estado;
        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>
    /// Registra un documento de respaldo — `RN-65`.
    ///
    /// ── No se pisa el anterior ──────────────────────────────────────────────
    /// `RN-64`: los datos de placa se conservan con <b>rangos de vigencia</b>. La pregunta que
    /// el auditor hace de verdad es <i>«¿con qué documento circulaba este vehículo en
    /// marzo?»</i>, y sobreescribir el respaldo la vuelve incontestable.
    /// </summary>
    public async Task<Ulid> RegistrarAsync(
        Ulid vehiculo,
        string tipo,
        string emisor,
        string folio,
        Ulid? adjunto,
        DateOnly vigenteDesde,
        DateOnly? vigenteHasta,
        IdPersona registra,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var fila = await contexto.Vehiculos.SingleOrDefaultAsync(v => v.Id == vehiculo, cancelacion)
            ?? throw new BloqueoDuro("RN-65", $"No existe el vehículo {vehiculo}.");

        if (string.IsNullOrWhiteSpace(folio))
        {
            throw new BloqueoDuro("RN-65",
                "El respaldo necesita su folio. Un documento sin folio no se puede citar ante " +
                "un operativo ni cotejar con el emisor.");
        }

        if (vigenteHasta is { } hasta && hasta < vigenteDesde)
            throw new BloqueoDuro("RN-65", "La vigencia del respaldo termina antes de empezar.");

        // ⚠️ **El adjunto se comprueba contra la tabla**, no contra el tipo. Es la misma
        // distinción que costó el respaldo del parámetro normativo: el identificador de un
        // adjunto no es el adjunto, y uno que apunta a nada se ve igual que uno que existe.
        if (adjunto is { } id && !await contexto.Adjuntos.AnyAsync(a => a.Id == id, cancelacion))
        {
            throw new BloqueoDuro("RN-65",
                "El documento adjunto no existe. Súbalo antes de registrar el respaldo: el " +
                "agente en carretera pide el papel, y uno que sólo existe como referencia no " +
                "se le puede mostrar.");
        }

        var nuevo = Ulid.NewUlid();

        contexto.RespaldosDePlaca.Add(new FilaDeRespaldoDePlaca
        {
            Id = nuevo,
            VehiculoId = vehiculo,
            Tipo = tipo,
            Emisor = emisor,
            Folio = folio.Trim(),
            Adjunto = adjunto,
            VigenteDesde = vigenteDesde,
            VigenteHasta = vigenteHasta,
            EstadoDePlaca = fila.EstadoDePlaca,
            Registra = registra.Valor,
            RegistradoEnUtc = momento.UtcDateTime,
        });

        await contexto.SaveChangesAsync(cancelacion);
        return nuevo;
    }

    /// <summary>
    /// El historial de respaldos de un vehículo, del más nuevo al más viejo, <b>con el
    /// veredicto de cada uno a una fecha</b>.
    ///
    /// El veredicto va acá y no en la pantalla porque es la regla: una lista de documentos con
    /// fechas obliga a quien la mira a hacer la resta a mano, y ésa es exactamente la resta que
    /// el sistema existe para no equivocar.
    /// </summary>
    public async Task<IReadOnlyList<RespaldoRegistrado>> HistorialAsync(
        Ulid vehiculo, DateOnly salida, DateOnly finDelRango,
        CancellationToken cancelacion = default)
    {
        var fila = await contexto.Vehiculos
            .AsNoTracking()
            .SingleOrDefaultAsync(v => v.Id == vehiculo, cancelacion);

        if (fila is null) return [];

        var filas = await contexto.RespaldosDePlaca
            .AsNoTracking()
            .Where(r => r.VehiculoId == vehiculo)
            .OrderByDescending(r => r.VigenteDesde)
            .ToListAsync(cancelacion);

        return
        [
            .. filas.Select(r =>
            {
                var respaldo = new RespaldoDePlaca(
                    r.Tipo, r.Emisor, r.Folio, r.Adjunto, r.VigenteDesde, r.VigenteHasta);

                var veredicto = ReglasDelRespaldoDePlaca.Evaluar(
                    fila.EstadoDePlaca, respaldo, salida, finDelRango);

                return new RespaldoRegistrado(
                    r.Id, respaldo, r.Registra,
                    new DateTimeOffset(r.RegistradoEnUtc, TimeSpan.Zero),
                    veredicto.Habilita, veredicto.Detalle);
            }),
        ];
    }
}

/// <param name="Cubre">
/// Si <b>este</b> respaldo cubre la ventana consultada. Va resuelto para que la pantalla no
/// tenga que rehacer la resta de fechas — que es justamente la que el sistema existe para no
/// equivocar.
/// </param>
public sealed record RespaldoRegistrado(
    Ulid Id,
    RespaldoDePlaca Respaldo,
    string Registra,
    DateTimeOffset RegistradoEn,
    bool Cubre,
    string Veredicto);
