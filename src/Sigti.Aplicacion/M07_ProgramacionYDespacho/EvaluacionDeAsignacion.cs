using Sigti.Aplicacion.M02_Parametros;
using Sigti.Aplicacion.M03_Flota;
using Sigti.Aplicacion.M05_Motoristas;
using Sigti.Datos;
using Sigti.Datos.M07_ProgramacionYDespacho;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M05_Motoristas;

namespace Sigti.Aplicacion.M07_ProgramacionYDespacho;

/// <summary>Lo que la pantalla de asignación necesita saber antes de comprometer nada.</summary>
/// <param name="CategoriaRequerida">
/// Qué categoría <b>sí</b> habilitaría este vehículo. Nombrar lo que falta, no solo lo
/// que sobra: «la B no habilita 12,000 kg» deja al usuario igual de perdido que antes.
/// </param>
/// <param name="EfectoDeLaRestriccion">
/// `BD-12`, no `BD-02`. <b>Puede ser advertencia</b>, y entonces no impide programar: lo
/// que exige es acuse. Era la condición 3 de `BD-02` y bloqueaba siempre — corregido con
/// el hallazgo `HN1-13`.
/// </param>
public sealed record ResultadoDeAsignacion(
    bool Habilita,
    string Motivo,
    string NumeroDeLicencia,
    string Categoria,
    DateOnly VenceLicencia,
    string VersionDeMatriz,
    DateOnly FinDeRangoEvaluado,
    string? CategoriaRequerida,
    string EfectoDeLaRestriccion,
    string? RestriccionEnConflicto,
    string? CondicionQueActivaLaRestriccion,
    string MotivoDeDocumentacion,
    IReadOnlyList<string> AdvertenciasDeDocumentacion,
    IReadOnlyList<string> ConductoresQueHabilitan,
    IReadOnlyList<string> VehiculosQueHabilita);

/// <summary>
/// Evalúa una asignación <b>sin ejecutar la transición</b>.
///
/// Existe para que `PT-026` muestre el resultado al elegir y no al guardar, y para que
/// esa evaluación salga del <b>mismo</b> dominio que después bloquea `T-08`. Sin este
/// endpoint el cliente tendría que reimplementar `BD-02`, y dos implementaciones de la
/// precondición que traslada responsabilidad legal es la duplicación que este sistema
/// menos puede permitirse.
///
/// No abre transacción ni escribe bitácora: no ocurrió nada todavía.
/// </summary>
public sealed class EvaluacionDeAsignacion(
    SigtiDbContext contexto,
    CatalogoProvisionalDeFlota padron,
    ConsultaDeFlota flota,
    CatalogoProvisionalDeRestricciones restricciones,
    IParametrosDeLaInstitucion parametros)
{
    private readonly ExpedientesDeMision _expedientes = new(contexto);

    public async Task<ResultadoDeAsignacion?> EvaluarAsync(
        Ulid idExpediente,
        string idVehiculo,
        string idConductor,
        bool hayConduccionNocturna,
        DateTimeOffset conocidoAl,
        CancellationToken cancelacion = default)
    {
        var expediente = await _expedientes.BuscarAsync(idExpediente, cancelacion);
        var vehiculo = Ulid.TryParse(idVehiculo, out var ulidVehiculo)
            ? await flota.PorIdAsync(ulidVehiculo, cancelacion)
            : null;
        var conductor = padron.Conductor(idConductor);

        if (expediente is null || vehiculo is null || conductor is null) return null;

        // La ventana sale de LA SOLICITUD, no de la pantalla: la declara quien pide.
        var ventana = expediente.Solicitud.Ventana;
        var matriz = parametros.MatrizVigenteAl(ventana.Salida);
        var politica = parametros.PoliticaVigenteAl(ventana.Salida);

        // `BD-12` aparte de `BD-02`, y con el efecto que decida el catálogo. La condición
        // la declara la misión; si no la declara, no hay nada que contrastar.
        string[] condiciones = hayConduccionNocturna ? [CondicionDeMision.ConduccionNocturna] : [];

        var restriccion = ReglasDeRestriccionMedica.Evaluar(
            conductor.Licencia, condiciones, restricciones.Vigente);

        var habilitacion = ReglasDeHabilitacion.Evaluar(
            conductor.Licencia, vehiculo.Ficha(), ventana, matriz, conocidoAl);

        var documentacion = ReglasDeDocumentacion.Evaluar(
            vehiculo.Documentacion(), ventana, politica);

        return new ResultadoDeAsignacion(
            // Solo el bloqueo impide. La advertencia se acusa y se sigue.
            Habilita: habilitacion.Habilita
                      && documentacion.Habilita
                      && restriccion.Efecto != EfectoDeRestriccion.Bloqueo,
            Motivo: habilitacion.Motivo.ToString(),
            NumeroDeLicencia: habilitacion.NumeroDeLicencia,
            Categoria: habilitacion.Categoria.ToString(),
            VenceLicencia: habilitacion.VencimientoDeLicencia,
            VersionDeMatriz: habilitacion.VersionDeMatriz,
            FinDeRangoEvaluado: habilitacion.FinDeRangoEvaluado,
            CategoriaRequerida: CategoriaQueHabilita(vehiculo.Ficha(), matriz, ventana, conocidoAl)?.ToString(),
            EfectoDeLaRestriccion: restriccion.Efecto.ToString(),
            RestriccionEnConflicto: restriccion.RestriccionEnConflicto,
            CondicionQueActivaLaRestriccion: restriccion.CondicionQueLaActiva,
            MotivoDeDocumentacion: documentacion.Motivo.ToString(),
            AdvertenciasDeDocumentacion: documentacion.Advertencias.Select(a => a.ToString()).ToList(),

            // Las salidas van en la misma respuesta porque van en la misma pantalla:
            // el usuario no puede resolver un rechazo de BD-02 reintentando.
            ConductoresQueHabilitan: padron.Conductores
                .Where(c => c.Id != conductor.Id)
                .Where(c => ReglasDeHabilitacion.Evaluar(
                    c.Licencia, vehiculo.Ficha(), ventana, matriz, conocidoAl).Habilita
                    && ReglasDeRestriccionMedica.Evaluar(
                        c.Licencia, condiciones, restricciones.Vigente).Efecto != EfectoDeRestriccion.Bloqueo)
                .Select(c => c.Id)
                .ToList(),

            VehiculosQueHabilita: (await flota.ParaEvaluarAsync(cancelacion))
                .Where(v => v.Id != vehiculo.Id)
                .Where(v => ReglasDeHabilitacion.Evaluar(
                    conductor.Licencia, v.Ficha(), ventana, matriz, conocidoAl).Habilita)
                .Select(v => v.Id.ToString())
                .ToList());
    }

    /// <summary>
    /// Prueba cada categoría contra la matriz en vez de deducirla con una tabla propia.
    /// Deducirla acá sería una segunda copia de la regla, con otra oportunidad de diverger.
    /// </summary>
    private static CategoriaDeLicencia? CategoriaQueHabilita(
        FichaTecnica ficha, MatrizDeLicencias matriz, VentanaDeMision ventana, DateTimeOffset conocidoAl) =>
        Enum.GetValues<CategoriaDeLicencia>()
            .Cast<CategoriaDeLicencia?>()
            .FirstOrDefault(c => matriz.Habilita(c!.Value, ficha, ventana.Salida, conocidoAl));

}
