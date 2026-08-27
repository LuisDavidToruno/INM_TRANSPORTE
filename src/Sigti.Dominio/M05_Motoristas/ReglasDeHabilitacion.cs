using Sigti.Dominio.M03_Flota;

namespace Sigti.Dominio.M05_Motoristas;

public enum MotivoDeNoHabilitacion
{
    Ninguno,
    CategoriaNoHabilitaElVehiculo,
    LicenciaVenceDentroDelRango
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
/// <b>Dos condiciones</b>, y ambas <b>sin excepción configurable</b> (`DP-001, D-12`):
/// categoría que no cubre el vehículo, y licencia vencida dentro del rango. Son las dos
/// que el PO decidió, palabra por palabra.
///
/// <b>Las restricciones médicas ya no están aquí</b> (hallazgo `HN1-13`). Eran la condición
/// 3 y heredaban un «sin excepción» que nadie les dio. Viven en <c>BD-12</c>, evaluadas por
/// <see cref="ReglasDeRestriccionMedica"/>, donde el efecto lo decide el catálogo: bloqueo
/// solo para las tipificadas como incompatibilizantes, advertencia con acuse para el resto
/// (`RN-11`).
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
        DateTimeOffset conocidoAl)
    {
        var motivo = Determinar(licencia, vehiculo, ventana, matriz, conocidoAl);

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
        DateTimeOffset conocidoAl)
    {
        // 1. Habilitación por categoría, contra los atributos de la ficha técnica y la
        //    matriz vigente A LA FECHA DE SALIDA PREVISTA, no a la de captura.
        if (!matriz.Habilita(licencia.Categoria, vehiculo, ventana.Salida, conocidoAl))
            return MotivoDeNoHabilitacion.CategoriaNoHabilitaElVehiculo;

        // 2. Vigencia en TODO el rango, no solo el día de salida.
        if (licencia.Vencimiento < ventana.FinDelRango)
            return MotivoDeNoHabilitacion.LicenciaVenceDentroDelRango;

        return MotivoDeNoHabilitacion.Ninguno;
    }
}
