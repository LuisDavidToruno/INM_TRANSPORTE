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
    private readonly AbastecimientosDeLaFlota _abastecimientos = new(contexto);

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

        // **Todos los abastecimientos de la misión, no los del vale** — `RN-83`. Sumar sólo
        // los del fondo dejaría fuera lo que salió del tanque de la sede, de una donación o
        // del bolsillo del motorista, y ese hueco es exactamente lo que produce un
        // rendimiento imposiblemente bueno.
        var abastecimientos = await _abastecimientos.DeLaMisionAsync(vale.Mision, cancelacion);

        var galones = abastecimientos.Sum(a => a.Galones);

        var composicion = abastecimientos
            .GroupBy(a => a.Fuente)
            .ToDictionary(g => g.Key, g => g.Sum(a => a.Galones));

        return ReglasDeConciliacion.Evaluar(
            kilometros,
            galones,
            esperado,
            parametros.UmbralesVigentesAl(fecha),
            ConNivelDeTanque(expediente, reparos),
            composicion);
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
    /// El reparo del <b>nivel de tanque</b>, calculado en vez de declarado — `RN-83`.
    ///
    /// ── Por qué esto deja de ser una casilla ────────────────────────────────
    /// Hasta ahora quien conciliaba marcaba «salió y volvió con niveles muy distintos» a
    /// mano, porque el sistema no tenía el dato. Una casilla que alguien olvida marcar deja
    /// pasar un cálculo que no significa nada, y el conciliador no puede saber que no
    /// significa nada.
    ///
    /// ── Dónde vive el umbral ────────────────────────────────────────────────
    /// En <see cref="NivelDeTanque.MuyDistintoDe"/>, no acá: qué es «muy distinto» es
    /// conocimiento del dominio, y dejarlo en el servicio lo volvería inalcanzable para una
    /// prueba que no monte una misión entera.
    ///
    /// <b>Lo que NO se hace es estimar.</b> Si falta una de las dos lecturas, no hay
    /// diferencia que medir y el reparo no se activa — `RN-80`: el campo no consignado se
    /// declara, no se rellena.
    /// </summary>
    private static ReparosDelCalculo ConNivelDeTanque(
        OrdenDeMision expediente, ReparosDelCalculo? declarados)
    {
        declarados ??= new ReparosDelCalculo();

        // Lo que declaró quien concilia manda: si dice que el tanque estaba dispar, lo estaba
        // — él lo vio y el sistema sólo tiene dos números.
        if (declarados.NivelDeTanqueDispar) return declarados;

        var (salida, retorno) = expediente.NivelesDelTanque;

        // Falta una de las dos lecturas: no hay diferencia que medir, y no se estima.
        if (salida is null || retorno is null) return declarados;

        // Nulo cuando las escalas no se pueden comparar. Se deja como estaba: no saber si el
        // tanque estaba dispar no es lo mismo que saber que no lo estaba.
        return salida.MuyDistintoDe(retorno) is { } dispar
            ? declarados with { NivelDeTanqueDispar = dispar }
            : declarados;
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

            // Sólo misiones ya conciliadas: una carga que todavía nadie revisó no puede ser la
            // referencia contra la que se revisa la siguiente.
            var yaConciliada = vales.Any() && vales.All(
                v => v.Id == excluir ||
                     v.Estado is EstadoDeAsignacion.Conciliada
                              or EstadoDeAsignacion.ConciliadaConDesviacion
                              or EstadoDeAsignacion.Anulada
                              or EstadoDeAsignacion.Devuelta);

            if (!yaConciliada || vales.Any(v => v.Id == excluir)) continue;

            // Y los galones salen de los ABASTECIMIENTOS, igual que en el cálculo: si la
            // referencia se armara sólo con los del fondo y el cálculo contara todos, la
            // media estaría por debajo y toda misión con combustible de otra fuente parecería
            // consumir de más.
            var abastecidos = await _abastecimientos.DeLaMisionAsync(idMision, cancelacion);
            var galones = abastecidos.Sum(a => a.Galones);

            if (galones > 0) historico.Add((kilometros, galones));
        }

        return ReglasDeConciliacion.ProponerDelHistorico(historico);
    }
}
