namespace Sigti.Dominio.M03_Flota;

/// <summary>
/// La documentación del vehículo que `BD-03` evalúa.
///
/// <b>La placa es opcional, y eso no es un descuido.</b> Hay desabastecimiento nacional:
/// «sin placa metálica» es un estado válido, y un campo `placa` obligatorio y único
/// rompería el sistema para la flota real.
/// </summary>
public sealed record DocumentacionDelVehiculo
{
    /// <summary>Nula cuando el vehículo no tiene placa metálica asignada. Es válido.</summary>
    public string? Placa { get; init; }

    /// <summary>Constancia o documento sustitutivo del IP, exigido cuando no hay placa.</summary>
    public required bool TieneConstanciaSustitutaDePlaca { get; init; }

    /// <summary>Único documento que bloquea de forma dura, sin configuración de por medio.</summary>
    public required DateOnly VenceMatricula { get; init; }

    /// <summary>No es obligatoria por ley vigente. Rastreable y alertable siempre.</summary>
    public DateOnly? VencePoliza { get; init; }

    /// <summary>Igual que la póliza: se rastrea, y bloquear es decisión de la institución.</summary>
    public DateOnly? VenceRevisionMecanica { get; init; }

    /// <summary>
    /// Franjas azul–blanco–azul, leyenda, siglas y correlativo. <b>No bloquea</b>, pero se
    /// exige constatar con fecha y foto: es hallazgo frecuente de auditoría.
    /// </summary>
    public required bool IdentificacionInstitucionalVerificada { get; init; }
}

/// <summary>
/// Qué bloquea, por institución.
///
/// La póliza y la revisión mecánica vienen <b>apagadas</b>: `DP-001, D-13` es explícito en
/// que no son obligatorias por ley vigente. Encenderlas es decisión de cada institución,
/// no una obligación que SIGTI imponga por su cuenta.
/// </summary>
public sealed record PoliticaDeDocumentacion(
    bool BloquearPorPolizaVencida,
    bool BloquearPorRevisionVencida)
{
    public static PoliticaDeDocumentacion PorDefecto => new(false, false);
}

public enum MotivoDeDocumentacionInsuficiente
{
    Ninguno,
    MatriculaVenceDentroDelRango,
    SinPlacaNiConstanciaSustituta,
    PolizaVenceDentroDelRango,
    RevisionMecanicaVenceDentroDelRango
}

/// <param name="Advertencias">
/// Lo que no bloquea pero sí se registra y se alerta. Que la póliza esté vencida importa
/// aunque la institución no bloquee por ello — y tiene que quedar por escrito.
/// </param>
/// <param name="VenceElQueBloquea">
/// Cuándo vence el documento que bloqueó. <b>Nula si no bloqueó por vencimiento</b> —el
/// caso de «sin placa ni constancia» no tiene fecha—.
///
/// Existe porque «documentación vencida» a secas no le sirve a quien programa: con la
/// fecha sabe si le alcanza con esperar, y sin ella tiene que ir a buscarla al
/// expediente del vehículo. `BD-02` ya decía el vencimiento de la licencia; esto cierra
/// la asimetría.
/// </param>
public sealed record ResultadoDeDocumentacion(
    bool Habilita,
    MotivoDeDocumentacionInsuficiente Motivo,
    IReadOnlyList<MotivoDeDocumentacionInsuficiente> Advertencias,
    DateOnly FinDeRangoEvaluado,
    DateOnly? VenceElQueBloquea = null);

/// <summary>
/// `BD-03` — Documentación del vehículo vigente.
///
/// La regla de vigencia es la misma de `BD-02`: <b>durante todo el rango</b>, no solo el
/// día de salida. Pura, con las fechas recibidas como parámetro.
/// </summary>
public static class ReglasDeDocumentacion
{
    public static ResultadoDeDocumentacion Evaluar(
        DocumentacionDelVehiculo documentacion,
        VentanaDeMision ventana,
        PoliticaDeDocumentacion politica)
    {
        var fin = ventana.FinDelRango;
        var advertencias = new List<MotivoDeDocumentacionInsuficiente>();
        var bloqueo = MotivoDeDocumentacionInsuficiente.Ninguno;
        DateOnly? venceElQueBloquea = null;

        if (documentacion.VenceMatricula < fin)
        {
            bloqueo = MotivoDeDocumentacionInsuficiente.MatriculaVenceDentroDelRango;
            venceElQueBloquea = documentacion.VenceMatricula;
        }

        // Sin placa no bloquea; sin placa Y sin constancia sustituta, sí: entonces no hay
        // ningún documento que identifique al vehículo en carretera.
        if (bloqueo is MotivoDeDocumentacionInsuficiente.Ninguno &&
            string.IsNullOrWhiteSpace(documentacion.Placa) &&
            !documentacion.TieneConstanciaSustitutaDePlaca)
            bloqueo = MotivoDeDocumentacionInsuficiente.SinPlacaNiConstanciaSustituta;

        Evaluar(documentacion.VencePoliza, politica.BloquearPorPolizaVencida,
            MotivoDeDocumentacionInsuficiente.PolizaVenceDentroDelRango);

        Evaluar(documentacion.VenceRevisionMecanica, politica.BloquearPorRevisionVencida,
            MotivoDeDocumentacionInsuficiente.RevisionMecanicaVenceDentroDelRango);

        return new ResultadoDeDocumentacion(
            Habilita: bloqueo == MotivoDeDocumentacionInsuficiente.Ninguno,
            Motivo: bloqueo,
            Advertencias: advertencias,
            FinDeRangoEvaluado: fin,
            VenceElQueBloquea: venceElQueBloquea);

        void Evaluar(DateOnly? vence, bool bloquea, MotivoDeDocumentacionInsuficiente motivo)
        {
            // Ausente o vencido dentro del rango: las dos cosas importan. Un vehículo sin
            // póliza registrada no está en mejor situación que uno con la póliza vencida.
            if (vence is { } fecha && fecha >= fin) return;

            if (bloquea && bloqueo == MotivoDeDocumentacionInsuficiente.Ninguno)
            {
                bloqueo = motivo;
                venceElQueBloquea = vence;
            }
            else
                advertencias.Add(motivo);
        }
    }
}
