using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Datos.Bitacora;
using Sigti.Datos.M02_Parametros;
using Sigti.Dominio.M02_Parametros;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M02_Parametros;

/// <summary>Una carga que las reglas de `HU-144` no admiten.</summary>
public sealed class CargaRechazada(ResultadoDeCarga resultado) : Exception(resultado.Mensaje)
{
    public MotivoDeRechazoDeCarga Motivo { get; } = resultado.Motivo;
}

public sealed class VersionNoEncontrada(Ulid id)
    : Exception($"No existe la versión de parámetro {id}.");

/// <summary>
/// El circuito de carga y aprobación de parámetros normativos.
///
/// Coordina tres cosas que ningún módulo posee solo: las reglas de `M-02`, la
/// persistencia y la bitácora. <b>Cada acto deja su asiento</b> — también el intento de
/// aprobación que se rechaza.
/// </summary>
public sealed class ServicioDeParametros(SigtiDbContext contexto)
{
    private readonly ParametrosNormativos _parametros = new(contexto);
    private readonly EscritorDeBitacora _bitacora = new(contexto);

    public Task<CatalogoDeParametros> CatalogoDeAsync(string clave, CancellationToken cancelacion = default) =>
        _parametros.CatalogoDeAsync(clave, cancelacion);

    /// <summary>
    /// `HU-144` — Carga una versión <b>pendiente de aprobación</b>. Nace sin aprobar a
    /// propósito: una carga que ya resolviera volvería decorativo el doble control.
    /// </summary>
    public async Task<Ulid> CargarAsync(
        SolicitudDeCarga solicitud, DateTimeOffset momento, CancellationToken cancelacion = default)
    {
        var existentes = await contexto.Parametros
            .Where(p => p.Clave == solicitud.Clave)
            .AsNoTracking()
            .ToListAsync(cancelacion);

        var resultado = ReglasDeCarga.Evaluar(solicitud, existentes);
        if (!resultado.Aceptada) throw new CargaRechazada(resultado);

        var version = new VersionDeParametro(
            Clave: solicitud.Clave,
            Valor: solicitud.Valor,
            VigenteDesde: solicitud.VigenteDesde,
            VigenteHasta: solicitud.VigenteHasta,
            RegistradoDesde: momento,
            RegistradoHasta: null,
            CargadoPor: solicitud.CargadoPor,
            AprobadoPor: null)
        {
            // No puede ser nulo: ReglasDeCarga ya lo rechazó si lo fuera.
            Respaldo = solicitud.Respaldo!
        };

        await EnUnaSolaTransaccion(async () =>
        {
            await _parametros.GuardarAsync(version, cancelacion);

            await _bitacora.EscribirAsync(
                Cola(solicitud.Clave),
                $"CARGA de '{solicitud.Clave}' = {solicitud.Valor} " +
                $"vigente desde {solicitud.VigenteDesde:yyyy-MM-dd} · " +
                $"por {solicitud.CargadoPor} · fuente: {solicitud.Respaldo!.Fuente} · " +
                $"pendiente de aprobación",
                momento, cancelacion);
        }, cancelacion);

        return version.Id;
    }

    /// <summary>
    /// `HU-145` y `HU-146` — Intenta aprobar. <b>Registra el intento en los dos casos</b>:
    /// bloquear sin registrar dejaría al auditor sin saber que alguien lo intentó.
    /// </summary>
    public async Task<IntentoDeAprobacion> AprobarAsync(
        Ulid id, IdPersona quienAprueba, DateTimeOffset momento, CancellationToken cancelacion = default)
    {
        var version = await contexto.Parametros.SingleOrDefaultAsync(p => p.Id == id, cancelacion)
            ?? throw new VersionNoEncontrada(id);

        var intento = ReglasDeDobleControl.Evaluar(version, quienAprueba, momento);
        var aprobada = ReglasDeDobleControl.Aplicar(version, intento);

        await EnUnaSolaTransaccion(async () =>
        {
            if (aprobada is not null)
            {
                // Aprobar es fijar quién aprobó sobre la misma versión, no crear otra:
                // la carga y su aprobación son el mismo hecho en dos actos.
                contexto.Entry(version).Property(p => p.AprobadoPor).CurrentValue = quienAprueba;
                await contexto.SaveChangesAsync(cancelacion);
            }

            await _bitacora.EscribirAsync(
                Cola(version.Clave),
                intento.Concedida
                    ? $"APROBACIÓN CONCEDIDA de '{version.Clave}' = {version.Valor} · por {quienAprueba}"
                    : $"APROBACIÓN RECHAZADA de '{version.Clave}' · intentó {quienAprueba} · " +
                      $"motivo: {intento.MotivoDelRechazo}",
                momento, cancelacion);
        }, cancelacion);

        return intento;
    }

    private static string Cola(string clave) => $"parametro:{clave}";

    /// <summary>
    /// El cambio y su asiento se confirman juntos. Si fueran dos transacciones, una caída
    /// entre ambas dejaría o un parámetro sin rastro o un asiento de algo que no ocurrió.
    /// </summary>
    private async Task EnUnaSolaTransaccion(Func<Task> trabajo, CancellationToken cancelacion) =>
        await contexto.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaccion = await contexto.Database.BeginTransactionAsync(cancelacion);
            await trabajo();
            await transaccion.CommitAsync(cancelacion);
        });
}
