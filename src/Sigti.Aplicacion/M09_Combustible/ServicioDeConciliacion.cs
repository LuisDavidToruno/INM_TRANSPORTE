using Microsoft.EntityFrameworkCore;
using Sigti.Aplicacion.M02_Parametros;
using Sigti.Datos;
using Sigti.Datos.M07_ProgramacionYDespacho;
using Sigti.Datos.M09_Combustible;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M09_Combustible;
using Sigti.Aplicacion.M07_ProgramacionYDespacho;

namespace Sigti.Aplicacion.M09_Combustible;

/// <summary>
/// Arma la conciliación de `RN-30` con los datos que están repartidos por tres agregados.
///
/// ── Qué junta, y de dónde ───────────────────────────────────────────────────
/// | Dato | De dónde sale |
/// |---|---|
/// | Kilómetros | El diario de la misión: odómetro de `T-14` contra el de `T-18` |
/// | Galones | Los asientos `V-04` del vale |
/// | Esperado | Los parámetros de la institución, o la propuesta del histórico del vehículo |
/// | Umbrales | Los parámetros, vigentes a la fecha del hecho |
///
/// El cálculo en sí <b>no está acá</b>: vive en <see cref="ReglasDeConciliacion"/>, puro, para
/// que los bordes se puedan ejercer sin montar una misión con tres consumos.
/// </summary>
public sealed class ServicioDeConciliacion(
    SigtiDbContext contexto,
    IParametrosDeLaInstitucion parametros)
{
    private readonly CombustibleDeLaInstitucion _combustible = new(contexto);
    private readonly ExpedientesDeMision _expedientes = new(contexto);

    /// <summary>
    /// Calcula, sin escribir nada. Se puede llamar para <b>mostrar</b> el dictamen antes de
    /// aplicarlo — que es lo que quien concilia necesita: ver contra qué se le va a juzgar.
    /// </summary>
    public async Task<Conciliacion> EvaluarAsync(
        Ulid idAsignacion,
        ReparosDelCalculo? reparos = null,
        CancellationToken cancelacion = default)
    {
        var vale = await _combustible.BuscarAsignacionAsync(idAsignacion, cancelacion)
            ?? throw new AsignacionNoEncontrada(idAsignacion);

        var expediente = await _expedientes.BuscarAsync(vale.Mision, cancelacion)
            ?? throw new ExpedienteNoEncontrado(vale.Mision);

        var kilometros = KilometrosDe(expediente);

        // La fecha del HECHO, no la de hoy: `RN-30` se evalúa contra el esperado vigente cuando
        // ocurrió el viaje, y una misión de marzo no se juzga con la tabla de septiembre (P-4).
        var fecha = expediente.Solicitud.Ventana.Salida;

        var vehiculo = VehiculoDe(expediente);

        var esperado = vehiculo is { } id
            ? parametros.RendimientoEsperadoDe(id, fecha)
              // `RN-30` punto 1 autoriza proponerlo del histórico del propio vehículo cuando la
              // institución no lo fijó. Sin esto la conciliación no correría **nunca**, y el
              // control existiría sin funcionar.
              ?? await ProponerDelHistoricoAsync(id, idAsignacion, cancelacion)
            : null;

        return ReglasDeConciliacion.Evaluar(
            kilometros,
            vale.GalonesConsumidos,
            esperado,
            parametros.UmbralesVigentesAl(fecha),
            reparos);
    }

    /// <summary>
    /// `V-09` o `V-10`, según lo que el cálculo diga. <b>Quien concilia no elige.</b>
    /// </summary>
    public async Task<EstadoDeAsignacion> ConciliarAsync(
        Ulid idAsignacion,
        Sigti.Dominio.Organizacion.IdPersona concilia,
        string? causa,
        DateTimeOffset momento,
        ReparosDelCalculo? reparos = null,
        CancellationToken cancelacion = default)
    {
        var resultado = await EvaluarAsync(idAsignacion, reparos, cancelacion);

        var vale = await _combustible.BuscarAsignacionAsync(idAsignacion, cancelacion)
            ?? throw new AsignacionNoEncontrada(idAsignacion);

        vale.Conciliar(concilia, resultado, causa, momento);

        await _combustible.GuardarAsignacionAsync(vale, cancelacion);
        return vale.Estado;
    }

    /// <summary>
    /// Los kilómetros del <b>vehículo en esta misión</b> — `T-18` menos `T-14`.
    ///
    /// ⚠️ <b>Con sustitución de vehículo esto da un número que no significa nada.</b> `RN-30` es
    /// explícita: <i>«cada vehículo se concilia por separado, con sus propios cortes de odómetro;
    /// un cálculo agregado de la misión mezclaría dos rendimientos»</i>. Hoy `T-10` reasigna sin
    /// registrar corte de odómetro, así que el corte no existe y no se puede partir. Queda dicho
    /// en vez de partir por un punto inventado.
    /// </summary>
    private static int KilometrosDe(OrdenDeMision expediente)
    {
        var salida = expediente.Diario.LastOrDefault(t => t.Id == "T-14")?.Odometro;
        var retorno = expediente.Diario.LastOrDefault(t => t.Id == "T-18")?.Odometro;

        // Sin las dos lecturas no hay recorrido. Cero deja el dictamen en «no evaluable», que es
        // la verdad: `RN-30` aplica «a toda misión con odómetro de salida y retorno».
        if (salida is not { } desde || retorno is not { } hasta) return 0;

        return Math.Max(0, hasta - desde);
    }

    private static Ulid? VehiculoDe(OrdenDeMision expediente) =>
        expediente.Diario.LastOrDefault(t => t.Recursos is not null)?.Recursos?.Vehiculo;

    /// <summary>
    /// La propuesta del histórico del propio vehículo — `RN-30` punto 1.
    ///
    /// ── Qué cuenta como histórico ───────────────────────────────────────────
    /// Sólo misiones <b>ya conciliadas</b> de ese vehículo, y <b>nunca la que se está
    /// conciliando</b>: incluirla haría que cada misión ayudara a definir su propio umbral, y una
    /// desviación grande se justificaría a sí misma.
    /// </summary>
    private async Task<RendimientoEsperado?> ProponerDelHistoricoAsync(
        Ulid vehiculo, Ulid excluir, CancellationToken cancelacion)
    {
        // Las misiones que ese vehículo tomó. La reserva vive en el diario, no en una segunda
        // tabla, así que se pregunta por ahí — el mismo índice que usa la ocupación de flota.
        var misiones = await contexto.Set<FilaDeTransicion>()
            .Where(t => t.VehiculoTomado == vehiculo)
            .Select(t => t.ExpedienteId)
            .Distinct()
            .ToListAsync(cancelacion);

        var historico = new List<(int, decimal)>();

        foreach (var idMision in misiones)
        {
            var expediente = await _expedientes.BuscarAsync(idMision, cancelacion);
            if (expediente is null) continue;

            var kilometros = KilometrosDe(expediente);
            if (kilometros <= 0) continue;

            var vales = await _combustible.DeLaMisionAsync(idMision, cancelacion);

            var galones = vales
                .Where(v => v.Id != excluir)
                // Sólo lo ya conciliado: una carga que todavía nadie revisó no puede ser la
                // referencia contra la que se revisa la siguiente.
                .Where(v => v.Estado is EstadoDeAsignacion.Conciliada
                                     or EstadoDeAsignacion.ConciliadaConDesviacion)
                .Sum(v => v.GalonesConsumidos);

            if (galones > 0) historico.Add((kilometros, galones));
        }

        return ReglasDeConciliacion.ProponerDelHistorico(historico);
    }
}
