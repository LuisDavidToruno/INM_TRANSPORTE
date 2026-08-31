using Microsoft.EntityFrameworkCore;

using Sigti.Aplicacion.M02_Parametros;
using Sigti.Aplicacion.M07_ProgramacionYDespacho;
using Sigti.Aplicacion.M09_Combustible;
using Sigti.Aplicacion.M18_Peajes;
using Sigti.Datos;
using Sigti.Datos.M07_ProgramacionYDespacho;
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
    /// <summary>
    /// La propuesta y <b>la cadena entera</b>.
    ///
    /// Van juntas y en una sola consulta porque `RN-08` manda presentar al liquidador la lista de
    /// verificación eslabón por eslabón, y `H-09` sale de esa misma lista: resolverlas por
    /// separado dejaría la pantalla mostrando una cadena y el criterio juzgando otra.
    /// </summary>
    public async Task<(PropuestaDeCierre Propuesta, CadenaDeTrazabilidad? Cadena)> DeLaMisionAsync(
        Ulid mision, CancellationToken cancelacion = default)
    {
        var expediente = await misiones.BuscarAsync(mision, cancelacion)
            ?? throw new ExpedienteNoEncontrado(mision);

        var vales = await combustible.DeLaMisionAsync(mision, cancelacion);
        var recuento = await combustible.RecuentoDeLaMisionAsync(mision, cancelacion);

        // El diario entero, una sola vez: la cadena y `H-05` lo miran los dos, y dos consultas
        // del mismo expediente pueden devolver cosas distintas si algo cambia en medio.
        var fila = await contexto.Expedientes
            .AsNoTracking()
            .Include(e => e.Transiciones)
            .SingleOrDefaultAsync(e => e.Id == mision, cancelacion);

        var cadena = fila is null ? null : await CadenaDeAsync(mision, fila, vales.Count, cancelacion);

        var propuesta = ReglasDeLaPropuestaDeCierre.Evaluar(new HechosDelCierre(
            ValesConDesviacion:
            [
                .. vales
                    .Where(v => v.Estado == EstadoDeAsignacion.ConciliadaConDesviacion)
                    .Select(v => v.Folio),
            ],

            FondoEntregadoSinDevolver: recuento.EntregadasSinDevolver,

            DiasInhabilesCirculados: InhabilesDe(expediente),
            AmparadaPorPermiso: fila is not null && await AmparadaAsync(fila, expediente, cancelacion),

            IncidentesSinResolver: await IncidentesAbiertosAsync(mision, cancelacion),
            Peajes: await PeajesDeAsync(mision, cancelacion),

            Cadena: cadena,

            // Nula cuando no hay expediente que consultar: sin recursos tomados no hay contra
            // qué comparar, y una lista vacía diría que todos los vales coinciden.
            ValesFueraDeLaOrden: fila is null ? null : ValesFueraDeLaOrden(vales, Tomados(fila))));

        return (propuesta, cadena);
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
        FilaDeExpediente fila, Dominio.M07_ProgramacionYDespacho.OrdenDeMision expediente,
        CancellationToken cancelacion)
    {
        var (vehiculo, motorista) = Tomados(fila);

        // Sin vehículo ni motorista no hay nada que un permiso pueda amparar. Al conciliar eso
        // no debería pasar —la misión circuló—, y si pasa, decir «amparada» sería lo peor.
        if (vehiculo is null || motorista is null) return false;

        var firmados = await permisos.DeExpedienteAsync(fila.Id, cancelacion);

        return firmados.Any(p => p.Ampara(
            vehiculo.Value, motorista.Value,
            expediente.Solicitud.Destino, expediente.Solicitud.Ventana));
    }

    /// <summary>
    /// La cadena de `RN-08` del expediente, eslabón por eslabón.
    ///
    /// ── ⚠️ Los hechos se leen del diario, no de una columna ─────────────────
    /// `P-1`: el estado es proyección del diario. Preguntarle a una columna «¿está autorizada?»
    /// funcionaría hasta el día que una anulación la deje desincronizada, y el eslabón diría que
    /// sí sobre un expediente que no.
    /// </summary>
    private async Task<CadenaDeTrazabilidad> CadenaDeAsync(
        Ulid mision, FilaDeExpediente fila, int vales, CancellationToken cancelacion)
    {
        var reserva = Tomados(fila);

        // Los cruces salen de la **ruta autorizada congelada**, no de los pasos: deducir «no hay
        // casetas» de que nadie registró ninguna haría que una misión que cruzó tres y no
        // registró ninguna se declarara sola como ruta sin peajes.
        var cruces = await contexto.RutasAutorizadasDePeaje
            .AsNoTracking()
            .Where(r => r.MisionId == mision && r.SupersedidaPor == null)
            .SumAsync(r => r.Cruces, cancelacion);

        var pasos = await contexto.PasosPorCaseta
            .AsNoTracking()
            .CountAsync(p => p.MisionId == mision, cancelacion);

        // `RN-50` — lo que todavía viaja. Se cuentan las dos cosas: lo retenido esperando su
        // antecedente y las divergencias sin resolver. Las dos significan «hay datos de campo de
        // esta misión que no están completos», que es lo que decide entre ausente y en camino.
        var retenidos = await contexto.HechosRetenidos
            .AsNoTracking()
            .CountAsync(h => h.EsperaExpediente == mision, cancelacion);

        var conflictos = await contexto.ConflictosDeSincronizacion
            .AsNoTracking()
            .CountAsync(c => c.ExpedienteId == mision && c.ResueltoUtc == null, cancelacion);

        return ReglasDeLaCadena.Evaluar(new HechosDeLaCadena(
            Autorizada: fila.Transiciones.Any(t => t.Transicion == "T-05"),
            // El provisional sigue identificando el documento. Lo que se marca aparte es si
            // consumio folio del rango, que es configuracion pendiente y no omision de nadie.
            Folio: ConsultaDeMisiones.Folio(fila),
            FolioOficial: fila.FolioTexto is not null,
            ConVehiculoYMotorista: reserva is { Vehiculo: not null, Motorista: not null },

            OdometroDeSalida: fila.Transiciones
                .FirstOrDefault(t => t.Transicion == "T-14")?.Odometro,
            OdometroDeRetorno: fila.Transiciones
                .FirstOrDefault(t => t.Transicion == "T-18")?.Odometro,

            ValesDeLaMision: vales,
            CrucesAutorizados: cruces,
            PasosRegistrados: pasos,
            Liquidada: fila.Transiciones.Any(t => t.Transicion == "T-19"),
            HechosSinSincronizar: retenidos + conflictos));
    }

    /// <summary>
    /// Los vales cuyo vehículo o receptor <b>no es el que la misión tomó</b> — `H-13`.
    ///
    /// ── ⚠️ Se compara contra lo que la misión TOMÓ ──────────────────────────
    /// No contra lo que el propio vale trae, que sería compararlo consigo mismo y no dispararía
    /// nunca. Y no contra la reserva vigente, que al liquidar ya no existe.
    ///
    /// ── Y se dice QUÉ difiere, no que difiere ───────────────────────────────
    /// Vehículo y motorista son dos hechos distintos con dos explicaciones distintas: uno huele
    /// a sustitución que no se propagó al vale, el otro a combustible entregado en la ventanilla
    /// a quien pasaba por ahí. Un mensaje que sólo diga «no coincide» convierte la diferencia en
    /// una investigación.
    /// </summary>
    private static IReadOnlyList<string> ValesFueraDeLaOrden(
        IReadOnlyList<AsignacionDeCombustible> vales, (Ulid? Vehiculo, Ulid? Motorista) tomados)
    {
        // Sin recursos tomados no hay contra qué comparar. Devolver vacía diría que todos
        // coinciden, y lo que pasa es que no se pudo mirar — eso lo dice el nulo.
        if (tomados is not { Vehiculo: { } vehiculo, Motorista: { } motorista }) return [];

        var fuera = new List<string>();

        foreach (var vale in vales)
        {
            var diferencias = new List<string>();

            if (vale.Vehiculo != vehiculo) diferencias.Add("vehículo distinto del de la orden");
            if (vale.Receptor != motorista) diferencias.Add("receptor distinto del motorista de la orden");

            if (diferencias.Count > 0)
                fuera.Add($"{vale.Folio} ({string.Join(" y ", diferencias)})");
        }

        return fuera;
    }

    /// <summary>
    /// Los recursos que la misión <b>tomó</b>, leídos del diario.
    ///
    /// ── ⚠️ Por qué no sirve <c>ServicioDePermisos.Reserva</c> ───────────────
    /// Esa contesta otra pregunta: <i>«¿qué tiene tomado este expediente <b>ahora</b>?»</i>, y
    /// por eso devuelve nulo en cuanto la misión deja de sostener la reserva — que es
    /// exactamente lo que pasa al retornar. Al cierre <b>toda</b> misión daría «sin vehículo ni
    /// motorista»: el eslabón de la cadena faltaría siempre, y `H-05` diría que ningún permiso
    /// ampara nada.
    ///
    /// La pregunta del cierre es la otra: <i>«¿qué tomó esta misión mientras corría?»</i>, y eso
    /// no caduca. Reutilizar la primera para contestar la segunda es el defecto que ya costó dos
    /// veces en `RN-32`, y volvió a costar acá.
    /// </summary>
    private static (Ulid? Vehiculo, Ulid? Motorista) Tomados(FilaDeExpediente fila)
    {
        var reserva = fila.Transiciones
            .Where(t => t.VehiculoTomado is not null)
            .MaxBy(t => t.Orden);

        return (reserva?.VehiculoTomado, reserva?.ConductorTomado);
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
