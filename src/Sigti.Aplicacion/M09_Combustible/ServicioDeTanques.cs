using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Datos.M09_Combustible;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M09_Combustible;

namespace Sigti.Aplicacion.M09_Combustible;

/// <summary>
/// El libro de existencias del tanque institucional — `RN-83` punto 5.
///
/// ── Lo que este servicio existe para contestar ──────────────────────────────
/// La pregunta que `CE-23` llama <b>préstamo invisible</b>: <i>«el combustible entra al tanque
/// del vehículo y no pasa por ningún folio»</i>. Con `RN-83` la fuente ya se declaraba; lo que
/// faltaba era el otro lado — <b>que el tanque supiera que salió</b>.
///
/// Sin ese otro lado, declarar `TANQUE_INSTITUCIONAL` era escribir una palabra: nada se
/// descontaba, nadie firmaba el despacho, y el galón seguía siendo tan invisible como antes,
/// sólo que ahora con la apariencia de estar registrado.
/// </summary>
public sealed class ServicioDeTanques(SigtiDbContext contexto)
{
    private readonly TanquesDeLaInstitucion _tanques = new(contexto);

    public Task<IReadOnlyList<TanqueInstitucional>> TodosAsync(
        CancellationToken cancelacion = default) => _tanques.TodosAsync(cancelacion);

    public Task<TanqueInstitucional?> BuscarAsync(
        Ulid id, CancellationToken cancelacion = default) => _tanques.BuscarAsync(id, cancelacion);

    public async Task<Ulid> AbrirAsync(
        Ulid id, string nombre, string ambitoDeclarado, string tipoDeCombustible,
        decimal? capacidadGalones, Autoria abre, decimal existenciaInicial,
        DateTimeOffset momento, CancellationToken cancelacion = default)
    {
        var tanque = TanqueInstitucional.Abrir(
            id, nombre, ambitoDeclarado, tipoDeCombustible, capacidadGalones,
            abre, existenciaInicial, momento);

        await _tanques.GuardarAsync(tanque, cancelacion);
        return tanque.Id;
    }

    /// <summary>Aplica un movimiento sobre un tanque existente.</summary>
    public async Task<decimal> MoverAsync(
        Ulid id, Action<TanqueInstitucional> movimiento,
        CancellationToken cancelacion = default)
    {
        var tanque = await _tanques.BuscarAsync(id, cancelacion)
            ?? throw new TanqueNoEncontrado(id);

        movimiento(tanque);

        await _tanques.GuardarAsync(tanque, cancelacion);
        return tanque.Existencia;
    }

    /// <summary>
    /// `E-02` — despachar a un vehículo, dejando el asiento atado al abastecimiento.
    ///
    /// <b>Son el mismo hecho visto desde dos lados</b>, igual que `V-04` y su abastecimiento: el
    /// vale mueve el instrumento y el abastecimiento cuenta el galón. Acá el tanque descuenta la
    /// existencia y el abastecimiento imputa el galón al vehículo. Contarlos como dos hechos
    /// duplicaría el galón en el denominador de `RN-30`.
    /// </summary>
    public async Task<decimal> DespacharAsync(
        Ulid tanqueId,
        Autoria despacha,
        decimal galones,
        Ulid vehiculo,
        Ulid? mision,
        Ulid abastecimiento,
        string combustibleDelVehiculo,
        IdPersonaDelReceptor recibe,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var tanque = await _tanques.BuscarAsync(tanqueId, cancelacion)
            ?? throw new TanqueNoEncontrado(tanqueId);

        tanque.Despachar(
            despacha, galones, vehiculo, mision, abastecimiento,
            combustibleDelVehiculo, recibe, momento);

        await _tanques.GuardarAsync(tanque, cancelacion);
        return tanque.Existencia;
    }

    /// <summary>
    /// `E-03` y `E-04` — el trasiego, con sus <b>dos</b> asientos en una sola transacción.
    ///
    /// Registrar sólo la salida haría que el combustible se evaporara del sistema entero en vez
    /// de sólo de este tanque, y ésa es exactamente la forma en que un faltante se disfraza de
    /// traslado. Por eso los dos lados los mueve este método y no quien llama.
    /// </summary>
    public async Task TrasegarAsync(
        Ulid origenId, Ulid destinoId, Autoria autoriza, decimal galones,
        DateTimeOffset momento, CancellationToken cancelacion = default)
    {
        var origen = await _tanques.BuscarAsync(origenId, cancelacion)
            ?? throw new TanqueNoEncontrado(origenId);

        var destino = await _tanques.BuscarAsync(destinoId, cancelacion)
            ?? throw new TanqueNoEncontrado(destinoId);

        origen.Trasegar(autoriza, galones, destino, sale: true, momento);
        destino.Trasegar(autoriza, galones, origen, sale: false, momento);

        await _tanques.GuardarTrasiegoAsync(origen, destino, cancelacion);
    }

    /// <summary>
    /// <b>El préstamo invisible, vuelto consulta</b> — los galones que alguien declaró sacados
    /// del tanque institucional y que ningún tanque registró haber despachado.
    ///
    /// ── Por qué no se bloquea en vez de reportarse ──────────────────────────
    /// Porque el abastecimiento declarado desde el campo es un <b>hecho consumado</b> (P-2) y
    /// `RN-83` es taxativo: <i>«el registro del abastecimiento no se omite nunca»</i>. Rechazarlo
    /// no devolvería el combustible al tanque — lo sacaría del denominador de `RN-30`, que es
    /// donde más falta hace.
    ///
    /// Lo que sí se puede hacer es que la contradicción <b>tenga nombre y salga en una lista</b>.
    /// Cada fila de acá es un galón que dice haber salido de un tanque que no lo anotó.
    /// </summary>
    public async Task<IReadOnlyList<DespachoSinRespaldo>> DespachosSinRespaldoAsync(
        CancellationToken cancelacion = default)
    {
        var declarados = await contexto.Abastecimientos
            .Where(a => a.Fuente == FuenteDeAbastecimiento.TanqueInstitucional)
            .Select(a => new
            {
                a.Id,
                a.VehiculoId,
                a.MisionId,
                a.Galones,
                a.MomentoUtc,
                a.Registra,
            })
            .ToListAsync(cancelacion);

        var sinRespaldo = new List<DespachoSinRespaldo>();

        // Punto por punto. Un `Contains` sobre ULID convertido devuelve vacío en silencio bajo
        // `UseCompatibilityLevel(120)`, y acá el silencio reportaría como no respaldado todo
        // galón que sí lo está — un reporte de hallazgos falsos que en tres meses nadie mira.
        foreach (var a in declarados)
        {
            if (await _tanques.TieneDespachoAsync(a.Id, cancelacion)) continue;

            sinRespaldo.Add(new DespachoSinRespaldo(
                a.Id, a.VehiculoId, a.MisionId, a.Galones,
                new DateTimeOffset(a.MomentoUtc, TimeSpan.Zero), a.Registra));
        }

        return [.. sinRespaldo.OrderByDescending(d => d.OcurridoEn)];
    }
}

/// <summary>
/// Un galón que dice haber salido del tanque institucional y que ningún tanque anotó.
///
/// <b>No es necesariamente fraude</b>, y el reporte no lo llama así: lo más común es que el
/// despacho se haya hecho y nadie lo asentara, que es precisamente el procedimiento no modelado
/// que `CE-23` describe. Lo que la fila afirma es que hay dos registros que no se corresponden.
/// </summary>
public sealed record DespachoSinRespaldo(
    Ulid Abastecimiento,
    Ulid Vehiculo,
    Ulid? Mision,
    decimal Galones,
    DateTimeOffset OcurridoEn,
    string Registra);

public sealed class TanqueNoEncontrado(Ulid id)
    : Exception($"No existe el tanque institucional {id}.");
