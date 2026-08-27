using Sigti.Dominio.M05_Motoristas;

namespace Sigti.Aplicacion.M05_Motoristas;

/// <summary>
/// Las condiciones de misión que hoy se pueden contrastar contra una restricción.
///
/// Es una lista corta a propósito: `RN-11` solo admite evaluar lo que la misión
/// <b>declara</b>. Si la misión no declara la condición, no se omite la evaluación — se
/// exige declararla.
/// </summary>
public static class CondicionDeMision
{
    public const string ConduccionNocturna = "conduccion_nocturna";
}

/// <summary>
/// ⚠️ <b>Catálogo provisional de restricciones médicas.</b>
///
/// El catálogo oficial de la DNVT es el <b>insumo #42</b>: se buscó el 2026-08-24 sin
/// resultado y se confirmó que <b>no existe vía documental</b> — es consulta directa a la
/// institución. Hasta obtenerlo, esto tipifica lo único que `RN-11` sostiene que se puede
/// contrastar por sistema, y **no inventa códigos**.
///
/// Todo lo que no esté aquí llega sin clasificar y <b>advierte</b>, que es exactamente lo
/// que `RN-11` exige: *«nunca se ignora por no estar tipificada»*. Mientras el #42 siga
/// abierto ese va a ser el caso mayoritario, y está bien que lo sea: una advertencia con
/// acuse deja rastro; un silencio no.
///
/// Cuando llegue el catálogo real, esta clase se borra y el catálogo entra por `M-02` como
/// parámetro con vigencia por rango de fechas, que es lo que `RN-39` exige de todo dato
/// normativo configurable.
/// </summary>
public sealed class CatalogoProvisionalDeRestricciones
{
    public CatalogoDeRestricciones Vigente { get; } = new([
        // La única que `RN-11` sostiene que se puede contrastar por sistema: se compara
        // contra la ventana horaria que la misión declara.
        new RestriccionTipificada(
            "CONDUCCION DIURNA UNICAMENTE",
            CondicionDeMision.ConduccionNocturna,
            EfectoDeRestriccion.Bloqueo)
    ]);
}
