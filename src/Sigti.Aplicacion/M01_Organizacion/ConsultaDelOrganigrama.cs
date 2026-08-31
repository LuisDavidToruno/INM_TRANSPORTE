using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M01_Organizacion;

/// <summary>
/// Arma el <see cref="Organigrama"/> desde el espejo, y dice <b>desde cuándo no se
/// confirma</b>.
///
/// ── Por qué se traen todas las asignaciones ──────────────────────────────────
/// Porque `RN-100` resuelve los permisos <b>a la fecha del hecho</b>, y filtrar en SQL por
/// «vigentes hoy» impediría reevaluar un expediente de febrero. El organigrama completo de
/// una institución son cientos de filas, no millones: el costo es nulo y la alternativa
/// —una consulta por cada fecha que se quiera evaluar— convertiría cada reevaluación en un
/// viaje a la base.
///
/// ── Por qué la antigüedad va aparte y no dentro del organigrama ──────────────
/// Porque son preguntas distintas. El organigrama responde <i>«¿quién ocupaba qué?»</i>;
/// la antigüedad responde <i>«¿cuánto puedo confiar en esa respuesta?»</i>. Mezclarlas
/// obligaría a que toda regla que consulta competencia cargue también con la política de
/// degradación, que es de `RN-50` y es <b>advertencia, no bloqueo</b>.
/// </summary>
public sealed class ConsultaDelOrganigrama(SigtiDbContext contexto)
{
    /// <summary>
    /// El organigrama completo, para resolver a cualquier fecha del hecho.
    /// </summary>
    public async Task<Organigrama> VigenteAsync(CancellationToken cancelacion = default)
    {
        var filas = await contexto.AsignacionesDePuesto
            .AsNoTracking()
            .ToListAsync(cancelacion);

        return new Organigrama(filas
            .Select(f => new AsignacionDePuesto(
                new IdPersona(f.Persona),
                new IdPuesto(f.Puesto),
                f.Desde,
                f.Hasta))
            .ToList());
    }

    /// <summary>
    /// Cuánto lleva el espejo sin confirmarse.
    ///
    /// <b>Devuelve nulo cuando <i>nunca</i> se confirmó</b>, y esa distinción importa:
    /// cero días de antigüedad y «no hay integración corriendo» son cosas opuestas.
    /// Devolver cero en el segundo caso mostraría como recién sincronizado un espejo que
    /// jamás existió — que es la peor forma de fallar, en silencio y con buena cara.
    ///
    /// Se mide contra la confirmación <b>más reciente</b>: si una sola fila se confirmó
    /// hoy, el espejo está vivo. Lo que envejece es la integración, no la fila.
    /// </summary>
    /// <param name="ahora">
    /// Se recibe, no se lee del reloj — `ADR-007` y la guarda NingunaReglaLeeElReloj.
    /// </param>
    /// <param name="soloPuesto">
    /// Acota la pregunta a un puesto. Sirve para la bandeja de `HU-009`, donde lo que
    /// importa no es el espejo entero sino <b>la jerarquía que decide esta autorización</b>.
    /// </param>
    public async Task<TimeSpan?> AntiguedadDelEspejoAsync(
        DateTimeOffset ahora,
        IdPuesto? soloPuesto = null,
        CancellationToken cancelacion = default)
    {
        // ⚠️ **Solo las espejadas.** Lo que esto mide es cuando se hablo por ultima vez con el
        // sistema dueno del padron. Una asignacion otorgada dentro de SIGTI —el puesto funcional
        // de flota— no dice nada de eso, y contarla haria que el espejo **pareciera recien
        // confirmado** por haber nombrado a un encargado de despacho.
        //
        // El sintoma seria el peor posible para lo que este dato existe: la jefatura que va a
        // firmar veria «confirmado hoy» sobre un organigrama detenido hace tres semanas.
        var consulta = contexto.AsignacionesDePuesto
            .AsNoTracking()
            .Where(a => a.Origen == OrigenDeLaAsignacion.Espejo);

        if (soloPuesto is { } puesto)
            consulta = consulta.Where(a => a.Puesto == puesto.Valor);

        var confirmaciones = await consulta
            .Select(a => a.ConfirmadoAlUtc)
            .ToListAsync(cancelacion);

        if (confirmaciones.Count == 0) return null;

        return ahora.UtcDateTime - confirmaciones.Max();
    }
}
