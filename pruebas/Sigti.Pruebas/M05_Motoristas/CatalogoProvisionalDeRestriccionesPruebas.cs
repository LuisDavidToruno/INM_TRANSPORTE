using Sigti.Aplicacion.M05_Motoristas;
using Sigti.Dominio.M05_Motoristas;

namespace Sigti.Pruebas.M05_Motoristas;

/// <summary>
/// ⚠️ El catálogo de restricciones médicas es <b>provisional</b>: el oficial de la DNVT
/// es el insumo #42 y se buscó el 2026-08-24 sin resultado. No hay fuente pública; es
/// consulta directa a la institución.
///
/// Estas pruebas fijan lo único que sí se puede sostener hoy: <b>qué se tipifica como
/// incompatibilizante y qué no</b>. El resto llega sin clasificar y advierte, que es lo
/// que `RN-11` exige — «nunca se ignora por no estar tipificada».
/// </summary>
public class CatalogoProvisionalDeRestriccionesPruebas
{
    private static readonly CatalogoProvisionalDeRestricciones Catalogo = new();

    private static Licencia Con(params string[] restricciones) =>
        new(Numero: "0801-1990-01234",
            Categoria: CategoriaDeLicencia.B,
            Vencimiento: new DateOnly(2028, 4, 30),
            Restricciones: restricciones);

    [Fact]
    public void La_conduccion_diurna_unicamente_bloquea_una_mision_nocturna()
    {
        // Es la única que `RN-11` nombra como contrastable por sistema: se puede comparar
        // contra la ventana horaria declarada. Por eso es la única que se tipifica.
        var resultado = ReglasDeRestriccionMedica.Evaluar(
            Con("CONDUCCION DIURNA UNICAMENTE"),
            condicionesDeclaradas: [CondicionDeMision.ConduccionNocturna],
            Catalogo.Vigente);

        Assert.Equal(EfectoDeRestriccion.Bloqueo, resultado.Efecto);
    }

    [Fact]
    public void La_misma_restriccion_no_bloquea_una_mision_que_no_declara_conduccion_nocturna()
    {
        // El bloqueo es contra la condición, no contra la persona. Una misión diurna con
        // un motorista de restricción diurna no tiene nada que objetar.
        var resultado = ReglasDeRestriccionMedica.Evaluar(
            Con("CONDUCCION DIURNA UNICAMENTE"),
            condicionesDeclaradas: [],
            Catalogo.Vigente);

        Assert.Equal(EfectoDeRestriccion.Ninguno, resultado.Efecto);
    }

    [Fact]
    public void Usar_lentes_correctores_advierte_y_no_bloquea_nunca()
    {
        // `RN-11`: «no se puede verificar por sistema y no debe bloquear». Bajo `BD-02`
        // esto paralizaba el despacho sin excepción posible.
        var resultado = ReglasDeRestriccionMedica.Evaluar(
            Con("USAR LENTES CORRECTORES"),
            condicionesDeclaradas: [CondicionDeMision.ConduccionNocturna],
            Catalogo.Vigente);

        Assert.Equal(EfectoDeRestriccion.Advertencia, resultado.Efecto);
    }
}
