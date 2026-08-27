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
    IReadOnlyList<string> VehiculosQueHabilita,
    ConflictoDeReserva? Conflicto);

/// <summary>
/// El solape de `BD-11`, con lo que `EF-01` exige mostrar: <b>qué misión, de qué
/// dependencia, en qué franja</b>.
///
/// Va en la vista previa y no sólo en el bloqueo del guardado porque las cuatro salidas
/// que `EF-01` ofrece —consolidar, otro recurso, reprogramar, escalar— <b>se deciden antes
/// de intentar guardar</b>. Descubrir el conflicto recién al apretar el botón obliga a
/// rehacer la elección entera.
/// </summary>
public sealed record ConflictoDeReserva(
    string Folio,
    string Dependencia,
    DateOnly Desde,
    DateOnly Hasta,
    bool Vehiculo,
    bool Conductor);

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
    ConsultaDeConductores padron,
    ConsultaDeFlota flota,
    ConsultaDeOcupacion ocupacion,
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
        var conductor = Ulid.TryParse(idConductor, out var ulidConductor)
            ? await padron.PorIdAsync(ulidConductor, cancelacion)
            : null;

        if (expediente is null || vehiculo is null || conductor is null) return null;

        // La ventana sale de LA SOLICITUD, no de la pantalla: la declara quien pide.
        var ventana = expediente.Solicitud.Ventana;
        var matriz = parametros.MatrizVigenteAl(ventana.Salida);
        var politica = parametros.PoliticaVigenteAl(ventana.Salida);

        // `BD-12` aparte de `BD-02`, y con el efecto que decida el catálogo. La condición
        // la declara la misión; si no la declara, no hay nada que contrastar.
        string[] condiciones = hayConduccionNocturna ? [CondicionDeMision.ConduccionNocturna] : [];

        var restriccion = ReglasDeRestriccionMedica.Evaluar(
            conductor.Licencia(), condiciones, restricciones.Vigente);

        var habilitacion = ReglasDeHabilitacion.Evaluar(
            conductor.Licencia(), vehiculo.Ficha(), ventana, matriz, conocidoAl);

        var documentacion = ReglasDeDocumentacion.Evaluar(
            vehiculo.Documentacion(), ventana, politica);

        // `BD-11`. El solape lo decide el dominio, con la misma `SeSolapaCon` que después
        // bloquea en `T-08`: una segunda implementación acá haría que la pantalla y el
        // guardado pudieran discrepar, que es el fallo que esta clase existe para evitar.
        var conflicto = (await ocupacion.ReservasDeAsync(vehiculo.Id, conductor.Id, idExpediente, cancelacion))
            .FirstOrDefault(r => r.SeSolapaCon(ventana));

        // Qué vehículos están tomados en la franja. Hace falta para que la lista de salida
        // no ofrezca un callejón: un vehículo que la licencia habilita pero que está
        // reservado por otra misión manda a quien programa a chocar contra `BD-11` otra vez.
        var tomados = (await ocupacion.EnVentanaAsync(ventana.Salida, ventana.FinDelRango, cancelacion))
            .Where(c => c.Barras.Count > 0)
            .Select(c => c.Vehiculo)
            .ToHashSet();

        return new ResultadoDeAsignacion(
            // Solo el bloqueo impide. La advertencia se acusa y se sigue.
            Habilita: habilitacion.Habilita
                      && documentacion.Habilita
                      && restriccion.Efecto != EfectoDeRestriccion.Bloqueo
                      // `EF-01`: «no sobre-asigna, ni siquiera con advertencia».
                      && conflicto is null,
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
            ConductoresQueHabilitan: (await padron.TodosAsync(cancelacion))
                .Where(c => c.Id != conductor.Id)
                .Where(c => ReglasDeHabilitacion.Evaluar(
                    c.Licencia(), vehiculo.Ficha(), ventana, matriz, conocidoAl).Habilita
                    && ReglasDeRestriccionMedica.Evaluar(
                        c.Licencia(), condiciones, restricciones.Vigente).Efecto != EfectoDeRestriccion.Bloqueo)
                .Select(c => c.Id.ToString())
                .ToList(),

            // Habilitan **y están libres**. Las dos condiciones, porque la pantalla los
            // ofrece como salida y una salida que vuelve a bloquear no es una salida.
            VehiculosQueHabilita: (await flota.ParaEvaluarAsync(cancelacion))
                .Where(v => v.Id != vehiculo.Id)
                .Where(v => !tomados.Contains(v.Id.ToString()))
                .Where(v => ReglasDeHabilitacion.Evaluar(
                    conductor.Licencia(), v.Ficha(), ventana, matriz, conocidoAl).Habilita)
                .Select(v => v.Id.ToString())
                .ToList(),

            Conflicto: conflicto is null
                ? null
                : new ConflictoDeReserva(
                    conflicto.Folio, conflicto.Dependencia, conflicto.Desde, conflicto.Hasta,
                    conflicto.Vehiculo, conflicto.Conductor));
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
