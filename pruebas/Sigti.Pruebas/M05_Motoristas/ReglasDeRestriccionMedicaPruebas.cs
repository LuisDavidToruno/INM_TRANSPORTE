using Sigti.Dominio.M05_Motoristas;

namespace Sigti.Pruebas.M05_Motoristas;

/// <summary>
/// `BD-12` — Restricciones médicas compatibles con las condiciones de la misión.
///
/// <b>Era la condición 3 de `BD-02` y no debía serlo</b> (hallazgo `HN1-13`). `BD-02` es
/// «sin excepción configurable» porque `DP-001, D-12` lo decidió así para dos cosas:
/// categoría que no cubre el vehículo, y licencia vencida dentro del rango. Las
/// restricciones médicas nunca estuvieron en esa decisión, y heredaron la etiqueta.
///
/// `RN-11` razona mejor: <b>las restricciones no son homogéneas.</b> «Usar lentes
/// correctores» no se puede verificar por sistema y no debe bloquear; «conducción diurna
/// únicamente» sí es contrastable contra la ventana horaria de la misión.
///
/// <b>El catálogo oficial de la DNVT no existe como fuente pública</b> — insumo #42. Por
/// eso el efecto es dato del catálogo y no constante del código.
/// </summary>
public class ReglasDeRestriccionMedicaPruebas
{
    private static Licencia ConRestricciones(params string[] restricciones) =>
        new(Numero: "0801-1990-01234",
            Categoria: CategoriaDeLicencia.B,
            Vencimiento: new DateOnly(2028, 4, 30),
            Restricciones: restricciones);

    [Fact]
    public void Una_restriccion_no_tipificada_como_incompatibilizante_advierte_pero_no_bloquea()
    {
        // «Usar lentes correctores» no se puede verificar por sistema. Bajo `BD-02` esto
        // bloqueaba el despacho sin excepción posible; bajo `BD-12` advierte y se acusa.
        var catalogo = new CatalogoDeRestricciones([
            new RestriccionTipificada("LENTES", "conduccion", EfectoDeRestriccion.Advertencia)
        ]);

        var resultado = ReglasDeRestriccionMedica.Evaluar(
            ConRestricciones("LENTES"),
            condicionesDeclaradas: ["conduccion"],
            catalogo);

        Assert.Equal(EfectoDeRestriccion.Advertencia, resultado.Efecto);
        Assert.Equal("LENTES", resultado.RestriccionEnConflicto);
    }

    [Fact]
    public void Una_restriccion_tipificada_como_incompatibilizante_bloquea_y_nombra_la_condicion()
    {
        // «Conducción diurna únicamente» sí es contrastable contra la ventana horaria, y
        // el bloqueo tiene que decir contra qué condición se activó: quien programa no
        // puede resolverlo reintentando.
        var catalogo = new CatalogoDeRestricciones([
            new RestriccionTipificada("DIURNA", "conduccion_nocturna", EfectoDeRestriccion.Bloqueo)
        ]);

        var resultado = ReglasDeRestriccionMedica.Evaluar(
            ConRestricciones("DIURNA"),
            condicionesDeclaradas: ["conduccion_nocturna"],
            catalogo);

        Assert.Equal(EfectoDeRestriccion.Bloqueo, resultado.Efecto);
        Assert.Equal("conduccion_nocturna", resultado.CondicionQueLaActiva);
    }

    [Fact]
    public void Una_restriccion_que_no_esta_en_el_catalogo_advierte_en_vez_de_ignorarse()
    {
        // `RN-11`: «Restricción registrada como texto libre en la licencia escaneada. No es
        // evaluable automáticamente. Se registra y produce advertencia genérica al asignar,
        // hasta que alguien la clasifique en el catálogo. **Nunca se ignora por no estar
        // tipificada.**»
        //
        // Es el caso que más va a ocurrir mientras el insumo #42 siga abierto: no hay
        // catálogo oficial de la DNVT, así que casi toda restricción real llega sin
        // clasificar. Devolver `Ninguno` haría que el sistema calle un dato que tiene.
        var resultado = ReglasDeRestriccionMedica.Evaluar(
            ConRestricciones("APTO SOLO PARA VEHICULO ADAPTADO"),
            condicionesDeclaradas: ["conduccion_nocturna"],
            new CatalogoDeRestricciones([]));

        Assert.Equal(EfectoDeRestriccion.Advertencia, resultado.Efecto);
        Assert.Equal("APTO SOLO PARA VEHICULO ADAPTADO", resultado.RestriccionEnConflicto);
    }
}
