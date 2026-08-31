using Microsoft.EntityFrameworkCore;

using Sigti.Aplicacion.M01_Organizacion;
using Sigti.Aplicacion.M07_ProgramacionYDespacho;
using Sigti.Datos;
using Sigti.Datos.M03_Flota;
using Sigti.Datos.M04_Documentacion;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M04_Documentacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M04_Documentacion;

/// <summary>
/// `HU-020` — el reporte previo al feriado largo y la <b>firma en lote</b>.
///
/// ── Por qué esto no es una comodidad ────────────────────────────────────────
/// El Tribunal Superior de Cuentas hace operativos de fiscalización vehicular <b>específicamente
/// en Semana Santa</b> `[V]`. Es el pico anual de riesgo, y es <b>predecible</b>.
///
/// Un flujo que le exige a la máxima autoridad abrir veinte expedientes uno por uno a las cinco
/// de la tarde del jueves santo produce una de dos cosas: <b>permisos que no se firman y
/// misiones que salen sin amparo, o la clave prestada a un asistente</b>. La segunda es la que
/// el sistema entero está diseñado para evitar.
/// </summary>
public sealed class ServicioDelPeriodo(
    SigtiDbContext contexto,
    ServicioDePermisos permisos,
    ServicioDeCompetencias competencias)
{
    /// <summary>
    /// El reporte del período. <b>Tres listas que suman la flota entera.</b>
    /// </summary>
    /// <param name="corte">
    /// La fecha de corte de conocimiento — `RN-94`. Va declarada <b>en</b> el reporte: una
    /// consulta con la misma fecha tiene que reproducir el mismo resultado, y sin declararla
    /// nadie puede saber contra qué se comparó.
    /// </param>
    public async Task<ReporteDelPeriodo> ReporteAsync(
        DateOnly desde, DateOnly hasta, DateTimeOffset corte,
        CancellationToken cancelacion = default)
    {
        var flota = await FlotaVigenteAsync(cancelacion);

        // Las misiones que tocan el período. Se traen las que se solapan, no las que empiezan
        // dentro: una que sale el 28 y vuelve el 2 circula en Semana Santa igual.
        var expedientes = await contexto.Expedientes
            .AsNoTracking()
            .Include(e => e.Transiciones)
            .Where(e => e.Salida <= hasta && e.Retorno >= desde)
            .ToListAsync(cancelacion);

        var enElPeriodo = new List<VehiculoEnElPeriodo>();

        foreach (var v in flota)
        {
            var mision = expedientes.FirstOrDefault(
                e => ServicioDePermisos.Reserva(e).Vehiculo == v.Id);

            var situacion = ReglasDelReporteDelPeriodo.Situar(
                v.Excepcion(), desde, mision is not null);

            var (permiso, folio, porQueNo, firmado) =
                situacion == SituacionEnElPeriodo.ConPermisoPropuesto
                    ? await PermisoDeAsync(mision!.Id, cancelacion)
                    : (null, null, null, false);

            var resguardo = situacion == SituacionEnElPeriodo.AResguardar
                ? await ResguardoDeAsync(v.Id, desde, hasta, cancelacion)
                : null;

            enElPeriodo.Add(new VehiculoEnElPeriodo(
                v.Id,
                v.CorrelativoInstitucional ?? v.Siglas,
                situacion,
                permiso,
                folio,
                mision?.Id.ToString(),

                // Nulo en los que circulan y en los exceptuados: un vehículo que sale no tiene
                // resguardo que confirmar, y pedirlo produciría una tarea imposible.
                situacion == SituacionEnElPeriodo.AResguardar
                    ? resguardo is null
                        ? EstadoDelResguardo.NoConfirmado
                        : EstadoDelResguardo.Confirmado
                    : null,
                resguardo?.ConfirmadoEl,
                resguardo?.Predio,
                porQueNo,
                firmado));
        }

        return new ReporteDelPeriodo(desde, hasta, corte, enElPeriodo, []);
    }

    /// <summary>
    /// Firma en lote los permisos del período.
    ///
    /// ── ⚠️ Los incompletos NO detienen a los completos ──────────────────────
    /// `HU-020` es explícita: <i>«el sistema firma los 4 permisos completos y no firma el
    /// incompleto»</i>. Abortar el lote entero por uno haría que la máxima autoridad tuviera que
    /// volver, y volver a las cinco de la tarde del jueves santo es lo que no ocurre.
    ///
    /// ── Y cada firma se registra individualmente ────────────────────────────
    /// `RN-03` y `RN-23`: cada permiso conserva su individualidad aunque la firma se ejecute en
    /// lote. <b>No hay un «acto de firma del lote»</b>: hay cinco actos que ocurrieron juntos.
    /// </summary>
    public async Task<ResultadoDelLote> FirmarLoteAsync(
        IReadOnlyList<Ulid> permisosDelLote,
        IdPersona quienFirma,
        DateOnly fechaDelHecho,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        // ⚠️ **El rol se comprueba UNA vez, antes de tocar nada.** Si se comprobara permiso por
        // permiso, quien no es la máxima autoridad recibiría veinte rechazos idénticos en vez
        // de uno claro — y el intento quedaría veinte veces en la bitácora.
        var suyas = await competencias.DeLaPersonaAsync(quienFirma, fechaDelHecho, cancelacion);

        if (!suyas.Roles.Contains(Rol.MaximaAutoridad))
        {
            return new ResultadoDelLote(
                [], [],
                "Esta facultad es de la máxima autoridad y se trata como indelegable mientras " +
                "no se confirme lo contrario (insumo #29). No se firmó ningún permiso del lote.");
        }

        var firmados = new List<string>();
        var noFirmados = new List<PermisoNoFirmado>();

        foreach (var id in permisosDelLote)
        {
            var intento = await permisos.FirmarAsync(id, quienFirma, momento, cancelacion);

            if (intento.Concedida) firmados.Add(intento.Folio);
            else noFirmados.Add(new PermisoNoFirmado(id, intento.Folio, intento.Motivo!));
        }

        return new ResultadoDelLote(firmados, noFirmados, null);
    }

    /// <summary>
    /// Confirma el resguardo de un vehículo. <b>Con evidencia fechada o no vale.</b>
    /// </summary>
    public async Task<Ulid> ConfirmarResguardoAsync(
        Ulid vehiculo, DateOnly desde, DateOnly hasta, string predio,
        Ulid evidencia, DateOnly confirmadoEl, IdPersona confirma, DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var existe = await contexto.Adjuntos.AnyAsync(a => a.Id == evidencia, cancelacion);

        var porQue = ReglasDelReporteDelPeriodo.PorQueNoSeConfirma(existe, predio);
        if (porQue is not null) throw new BloqueoDuro("HU-020", porQue);

        var id = Ulid.NewUlid();

        contexto.ResguardosDeFeriado.Add(new FilaDeResguardo
        {
            Id = id,
            VehiculoId = vehiculo,
            Desde = desde,
            Hasta = hasta,
            Predio = predio.Trim(),
            Evidencia = evidencia,
            ConfirmadoEl = confirmadoEl,
            ConfirmadoPor = confirma.Valor,
            RegistradoEnUtc = momento.UtcDateTime,
        });

        await contexto.SaveChangesAsync(cancelacion);
        return id;
    }

    /// <summary>
    /// Cuántos vehículos tiene que cubrir el reporte.
    ///
    /// ⚠️ <b>Con el mismo criterio que arma las listas</b>, y por eso vive acá y no en la ruta:
    /// contar la flota de una manera y clasificarla de otra haría que la comprobación fallara
    /// siempre —o, peor, que pasara escondiendo un vehículo.
    /// </summary>
    public async Task<int> VehiculosDeLaFlotaAsync(CancellationToken cancelacion = default) =>
        (await FlotaVigenteAsync(cancelacion)).Count;

    /// <summary>
    /// La flota que sigue siendo flota — <c>ReglasDelReporteDelPeriodo.EstaEnLaFlota</c>.
    ///
    /// El estado operativo es <b>proyección del diario</b> (`P-1`), no una columna: se resuelve
    /// por el último cambio de cada vehículo. Se trae el diario entero de una vez en lugar de
    /// preguntar vehículo por vehículo — con una flota de doscientas unidades serían doscientas
    /// consultas para armar una pantalla que se abre una vez al año.
    /// </summary>
    private async Task<List<FilaDeVehiculo>> FlotaVigenteAsync(CancellationToken cancelacion)
    {
        var flota = await contexto.Vehiculos.AsNoTracking().ToListAsync(cancelacion);

        var cambios = await contexto.CambiosDeEstado.AsNoTracking().ToListAsync(cancelacion);

        var ultimo = cambios
            .GroupBy(c => c.VehiculoId)

            // Por `Orden` y no por marca de tiempo: dos cambios pueden compartir instante
            // cuando uno lo fija el sistema por una transición y otro una persona.
            .ToDictionary(g => g.Key, g => g.MaxBy(c => c.Orden)!.Estado);

        return [.. flota.Where(v => ReglasDelReporteDelPeriodo.EstaEnLaFlota(
            ultimo.TryGetValue(v.Id, out var estado) ? estado : null))];
    }

    /// <summary>
    /// El permiso de la misión y por qué no se puede firmar todavía.
    ///
    /// El motivo sale de <c>ReglasDelPermiso</c>, no se reescribe acá: una segunda copia de la
    /// regla diría cosas distintas el día que una de las dos cambie.
    /// </summary>
    private async Task<(Ulid? Id, string? Folio, string? PorQueNo, bool Firmado)> PermisoDeAsync(
        Ulid mision, CancellationToken cancelacion)
    {
        var diagnosticados = await permisos.DiagnosticoDelExpedienteAsync(mision, cancelacion);

        var vivo = diagnosticados.FirstOrDefault(
            d => d.Permiso.Estado != EstadoDelPermiso.Desistido);

        if (vivo is null)
        {
            return (null, null,
                "No hay permiso tramitado para esta misión. Ábralo desde el expediente antes " +
                "de la sesión de firma.", false);
        }

        // ⚠️ **Firmado NO es firmable, y tampoco es un problema.** Los dos motivos se separan a
        // propósito:
        //
        //   - En el firmado, el motivo es <c>PorQueYaNoCubre</c> — nulo mientras siga amparando.
        //     <c>PorQueNoSeFirma</c> también lo rechazaría, pero con «ya está firmado», y eso
        //     dejaría al permiso resuelto mostrándose como un bloqueo que hay que ir a arreglar.
        //   - En el que no está firmado, el motivo es lo que falta para poder firmarlo.
        //
        // Que la bandera vaya aparte es lo que permite descontarlo de «puede firmar hoy» sin
        // inventarle un problema: si no se descontara, la cifra no bajaría al firmar y la sesión
        // de firma no terminaría nunca.
        var firmado = vivo.Permiso.Estado == EstadoDelPermiso.Firmado;

        return (vivo.Permiso.Id, vivo.Permiso.Folio,
            firmado ? vivo.PorQueYaNoCubre : ReglasDelPermiso.PorQueNoSeFirma(
                vivo.Permiso, [Rol.MaximaAutoridad]),
            firmado);
    }

    /// <summary>
    /// El resguardo confirmado que cubre el período. <b>Nulo es que nadie fue a mirar.</b>
    ///
    /// Se exige que cubra el período entero, no que se solape: una confirmación del 27 de marzo
    /// para un feriado que va hasta el 5 de abril dice dónde estaba el vehículo el 27, y nada
    /// del resto.
    /// </summary>
    private async Task<FilaDeResguardo?> ResguardoDeAsync(
        Ulid vehiculo, DateOnly desde, DateOnly hasta, CancellationToken cancelacion) =>
        await contexto.ResguardosDeFeriado
            .AsNoTracking()
            .Where(r => r.VehiculoId == vehiculo && r.Desde <= desde && r.Hasta >= hasta)
            .OrderByDescending(r => r.ConfirmadoEl)
            .FirstOrDefaultAsync(cancelacion);
}

/// <param name="Rechazado">
/// Por qué no se firmó <b>nada</b>. Nulo cuando el lote se procesó — aunque algún permiso
/// individual no se haya firmado, que es otra cosa.
/// </param>
public sealed record ResultadoDelLote(
    IReadOnlyList<string> Firmados,
    IReadOnlyList<PermisoNoFirmado> NoFirmados,
    string? Rechazado);

public sealed record PermisoNoFirmado(Ulid Id, string Folio, string Motivo);
