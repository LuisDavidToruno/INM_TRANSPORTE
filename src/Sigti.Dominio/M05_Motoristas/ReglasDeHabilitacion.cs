using Sigti.Dominio.M03_Flota;

namespace Sigti.Dominio.M05_Motoristas;

public enum MotivoDeNoHabilitacion
{
    Ninguno,
    CategoriaNoHabilitaElVehiculo,
    LicenciaVenceDentroDelRango,
    RestriccionMedicaIncompatible
}

/// <summary>
/// El resultado de evaluar `BD-02`, con <b>todos sus insumos</b>.
///
/// `BD-02` lo exige así: número de licencia, categoría, vencimiento, versión de la
/// matriz, atributos del vehículo usados y fecha de fin de rango evaluada. <b>«Guardar
/// solo "verificado" no defiende a nadie.»</b> Este registro es lo que se muestra ante un
/// siniestro, y por eso se conserva aunque la evaluación haya sido favorable.
/// </summary>
public sealed record ResultadoDeHabilitacion(
    bool Habilita,
    MotivoDeNoHabilitacion Motivo,
    string NumeroDeLicencia,
    CategoriaDeLicencia Categoria,
    DateOnly VencimientoDeLicencia,
    string VersionDeMatriz,
    FichaTecnica AtributosDelVehiculo,
    DateOnly FinDeRangoEvaluado);

/// <summary>
/// `BD-02` — Licencia habilitante y vigente durante todo el rango.
///
/// Pura: sin base de datos, sin reloj. Las fechas entran como parámetro, que es lo que
/// `ADR-006` y `ADR-007` necesitan y lo que la guarda NingunaReglaLeeElReloj exige.
///
/// <b>Sin excepción configurable</b> (`DP-001, D-12`) — para las condiciones 1 y 2.
///
/// <b>⚠️ Esta clase todavía no refleja la corrección de `HN1-13`.</b> Sigue evaluando las
/// restricciones médicas como condición 3 de `BD-02`, con bloqueo duro y sin excepción.
/// La máquina de estados —autoridad— ya las sacó a <c>BD-12</c>, donde el efecto lo decide
/// el catálogo: bloqueo solo para las tipificadas como incompatibilizantes, advertencia con
/// acuse para el resto (`RN-11`). La evaluación nueva vive en
/// <see cref="ReglasDeRestriccionMedica"/> y está probada; falta <b>retirar la condición 3
/// de aquí</b> y propagar el cambio a <c>EvaluacionDeAsignacion</c>, a la API y a la
/// pantalla de asignación. No se hizo en el mismo paso porque Smart App Control impidió
/// correr la suite, y ese cambio rompe tres capas si sale mal.
/// </summary>
public static class ReglasDeHabilitacion
{
    /// <param name="conocidoAl">
    /// Desde qué momento se mira la matriz. Reevaluar una misión vieja con el instante de
    /// entonces reproduce la decisión que se tomó; con el instante actual da la decisión
    /// correcta a la luz de una corrección posterior. Las dos preguntas son legítimas.
    /// </param>
    public static ResultadoDeHabilitacion Evaluar(
        Licencia licencia,
        FichaTecnica vehiculo,
        VentanaDeMision ventana,
        MatrizDeLicencias matriz,
        DateTimeOffset conocidoAl,
        IReadOnlyList<string>? restriccionesQueLaMisionContradice = null)
    {
        var motivo = Determinar(
            licencia, vehiculo, ventana, matriz, conocidoAl, restriccionesQueLaMisionContradice ?? []);

        return new ResultadoDeHabilitacion(
            Habilita: motivo == MotivoDeNoHabilitacion.Ninguno,
            Motivo: motivo,
            NumeroDeLicencia: licencia.Numero,
            Categoria: licencia.Categoria,
            VencimientoDeLicencia: licencia.Vencimiento,
            VersionDeMatriz: matriz.Version,
            AtributosDelVehiculo: vehiculo,
            FinDeRangoEvaluado: ventana.FinDelRango);
    }

    private static MotivoDeNoHabilitacion Determinar(
        Licencia licencia,
        FichaTecnica vehiculo,
        VentanaDeMision ventana,
        MatrizDeLicencias matriz,
        DateTimeOffset conocidoAl,
        IReadOnlyList<string> contradichas)
    {
        // 1. Habilitación por categoría, contra los atributos de la ficha técnica y la
        //    matriz vigente A LA FECHA DE SALIDA PREVISTA, no a la de captura.
        if (!matriz.Habilita(licencia.Categoria, vehiculo, ventana.Salida, conocidoAl))
            return MotivoDeNoHabilitacion.CategoriaNoHabilitaElVehiculo;

        // 2. Vigencia en TODO el rango, no solo el día de salida.
        if (licencia.Vencimiento < ventana.FinDelRango)
            return MotivoDeNoHabilitacion.LicenciaVenceDentroDelRango;

        // 3. Restricciones médicas compatibles con lo que la misión exige.
        if (licencia.Restricciones.Intersect(contradichas, StringComparer.OrdinalIgnoreCase).Any())
            return MotivoDeNoHabilitacion.RestriccionMedicaIncompatible;

        return MotivoDeNoHabilitacion.Ninguno;
    }
}
