using Microsoft.EntityFrameworkCore;

using Sigti.Aplicacion.M02_Parametros;
using Sigti.Aplicacion.M07_ProgramacionYDespacho;
using Sigti.Aplicacion.M09_Combustible;
using Sigti.Aplicacion.M18_Peajes;
using Sigti.Datos;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.M13_Cierre;

namespace Sigti.Aplicacion.M13_Cierre;

/// <summary>
/// Reúne los hechos del expediente y deja que §7.2 los juzgue.
///
/// ── ⚠️ Qué se corrigió ──────────────────────────────────────────────────────
/// La detección de criterios <b>vivía en el navegador</b> y evaluaba uno de los trece. El
/// endpoint de cierre recibía la lista <b>del cliente</b>: quien llamara con la lista vacía
/// cerraba `CERRADA`, y el asiento decía que cerró limpio. La precondición de `T-21` —<i>«no se
/// cumple ninguno de los criterios»</i>— la declaraba el propio llamador, que es lo mismo que no
/// tenerla.
///
/// ── Y lo que no se puede evaluar se declara ─────────────────────────────────
/// Cada consulta que falta entra como <b>nulo</b>, no como cero: el dominio distingue «no se
/// cumple» de «nadie lo miró», y esa distinción sólo sirve si acá se respeta. Devolver listas
/// vacías porque una consulta no se hizo produciría exactamente el expediente que cierra limpio
/// afirmando verificaciones que no ocurrieron.
/// </summary>
public sealed class ServicioDeLaPropuestaDeCierre(
    SigtiDbContext contexto,
    ServicioDeMisiones misiones,
    ServicioDeCombustible combustible,
    ConsultaDePermisos permisos,
    ServicioDePeajes peajes,
    IParametrosDeLaInstitucion parametros)
{
    public async Task<PropuestaDeCierre> DeLaMisionAsync(
        Ulid mision, CancellationToken cancelacion = default)
    {
        var expediente = await misiones.BuscarAsync(mision, cancelacion)
            ?? throw new ExpedienteNoEncontrado(mision);

        var vales = await combustible.DeLaMisionAsync(mision, cancelacion);
        var recuento = await combustible.RecuentoDeLaMisionAsync(mision, cancelacion);

        return ReglasDeLaPropuestaDeCierre.Evaluar(new HechosDelCierre(
            ValesConDesviacion:
            [
                .. vales
                    .Where(v => v.Estado == EstadoDeAsignacion.ConciliadaConDesviacion)
                    .Select(v => v.Folio),
            ],

            FondoEntregadoSinDevolver: recuento.EntregadasSinDevolver,

            DiasInhabilesCirculados: InhabilesDe(expediente),
            AmparadaPorPermiso: await AmparadaAsync(mision, expediente, cancelacion),

            IncidentesSinResolver: await IncidentesAbiertosAsync(mision, cancelacion),
            Peajes: await PeajesDeAsync(mision, cancelacion)));
    }

    /// <summary>
    /// Los días inhábiles que la misión tocó, con el calendario vigente <b>a la fecha del
    /// hecho</b> (`P-4`).
    ///
    /// Resolverlo a hoy diría qué días son inhábiles ahora, no cuáles lo eran cuando la misión
    /// salió — y un feriado decretado en junio no vuelve irregular un viaje de marzo.
    /// </summary>
    private IReadOnlyList<DateOnly> InhabilesDe(Dominio.M07_ProgramacionYDespacho.OrdenDeMision expediente)
    {
        var ventana = expediente.Solicitud.Ventana;
        return parametros.CalendarioVigenteAl(ventana.Salida).InhabilesEn(ventana);
    }

    /// <summary>
    /// Si algún permiso firmado ampara el vehículo, el motorista, el destino y la ventana.
    ///
    /// ── ⚠️ Contra los recursos de HOY, no los del despacho ──────────────────
    /// `H-05` existe justamente para atrapar lo que cambió después de que `BD-04` miró: un
    /// relevo de motorista que invalidó el permiso, o una prórroga que metió el sábado.
    /// Compararlo contra los recursos del despacho lo dejaría sin disparar nunca — que es el
    /// defecto que ya costó dos veces en `RN-32`.
    /// </summary>
    private async Task<bool> AmparadaAsync(
        Ulid mision, Dominio.M07_ProgramacionYDespacho.OrdenDeMision expediente,
        CancellationToken cancelacion)
    {
        var fila = await contexto.Expedientes
            .AsNoTracking()
            .Include(e => e.Transiciones)
            .SingleOrDefaultAsync(e => e.Id == mision, cancelacion);

        if (fila is null) return false;

        var (vehiculo, motorista) = ServicioDePermisos.Reserva(fila);

        // Sin vehículo ni motorista no hay nada que un permiso pueda amparar. Al conciliar eso
        // no debería pasar —la misión circuló—, y si pasa, decir «amparada» sería lo peor.
        if (vehiculo is null || motorista is null) return false;

        var firmados = await permisos.DeExpedienteAsync(mision, cancelacion);

        return firmados.Any(p => p.Ampara(
            vehiculo.Value, motorista.Value,
            expediente.Solicitud.Destino, expediente.Solicitud.Ventana));
    }

    /// <summary>
    /// El dictamen de `RN-37` por vehículo, reducido a lo que `H-03` necesita.
    ///
    /// ── ⚠️ La misma velocidad que usa la pantalla de coherencia ─────────────
    /// Si esto la resolviera por su cuenta, el dictamen del cierre y el que se ve en el
    /// expediente podrían diferir — y quien cierra estaría firmando contra un número distinto
    /// del que miró.
    /// </summary>
    private async Task<IReadOnlyList<DictamenDePeajes>> PeajesDeAsync(
        Ulid mision, CancellationToken cancelacion)
    {
        var dictamenes = await peajes.EvaluarCoherenciaAsync(
            mision, parametros.VelocidadMediaMaximaKmH, cancelacion: cancelacion);

        return
        [
            .. dictamenes.Select(d => new DictamenDePeajes(
                d.Dictamen.Hallazgos.Count,
                d.Dictamen.Dimensiones.Todas,
                d.Dictamen.Dimensiones.PorQueNo)),
        ];
    }

    /// <summary>
    /// Los incidentes de la misión que siguen abiertos, descritos como quien lea el acta los va
    /// a buscar: <b>por tipo y fecha del hecho</b>, no por identificador.
    /// </summary>
    private async Task<IReadOnlyList<string>> IncidentesAbiertosAsync(
        Ulid mision, CancellationToken cancelacion) =>
        await contexto.Incidentes
            .AsNoTracking()
            .Where(i => i.MisionId == mision && i.ResueltoEn == null)
            .OrderBy(i => i.FechaDelHecho)
            .Select(i => i.Tipo.ToString() + " del " + i.FechaDelHecho.ToString("yyyy-MM-dd"))
            .ToListAsync(cancelacion);
}
