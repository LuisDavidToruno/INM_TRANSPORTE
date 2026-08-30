using Sigti.Dominio.M01_Organizacion;

namespace Sigti.Pruebas.M01_Organizacion;

/// <summary>
/// `mapa-de-navegacion` §1 — `R-1` y `R-2`.
///
/// <i>«No hay un menú único. Hay una raíz por puesto»</i> · <i>«La raíz de cada rol es su
/// bandeja de trabajo, no un tablero decorativo»</i>.
/// </summary>
public class ReglasDeLaRaizPruebas
{
    [Theory]
    [InlineData(Rol.Solicitante, "PT-006")]
    [InlineData(Rol.JefaturaInmediata, "PT-013")]
    [InlineData(Rol.JefeDeTransporte, "PT-025")]
    [InlineData(Rol.EncargadoDeDespacho, "PT-038")]
    [InlineData(Rol.EncargadoDeCombustible, "PT-050")]
    [InlineData(Rol.EncargadoDeDelegacion, "PT-127")]
    [InlineData(Rol.AuditorInterno, "PT-088")]
    [InlineData(Rol.Motorista, "PT-104")]
    public void Cada_raiz_es_la_que_el_mapa_rotula_como_raiz(Rol rol, string pantalla)
    {
        Assert.Equal(pantalla, ReglasDeLaRaiz.De(rol)?.Pantalla);
    }

    [Fact]
    public void La_raiz_del_encargado_de_delegacion_es_PT_127_y_no_PT_104()
    {
        // Corrección `HB34-70`: compartían identificador y son pantallas de propósito opuesto.
        // `PT-104` «Mi misión» es del motorista —una misión, sin menú—; `PT-127` es lo
        // contrario: varias misiones y la cola de papeles por digitar.
        Assert.Equal("PT-127", ReglasDeLaRaiz.De(Rol.EncargadoDeDelegacion)?.Pantalla);
        Assert.Equal("PT-104", ReglasDeLaRaiz.De(Rol.Motorista)?.Pantalla);
    }

    [Fact]
    public void La_raiz_de_firma_tiene_nombre_y_no_tiene_identificador()
    {
        // El mapa la describe —«Pendientes de mi firma · raíz»— y no le da un `PT-xxx`.
        // Inventarle uno crearía un identificador que el inventario no reconoce, y los
        // identificadores no se reciclan.
        var raiz = ReglasDeLaRaiz.De(Rol.GerenciaAdministrativa);

        Assert.NotNull(raiz);
        Assert.Null(raiz.Pantalla);
        Assert.Equal("Pendientes de mi firma", raiz.Nombre);
    }

    [Theory]
    [InlineData(Rol.EncargadoDeMantenimiento)]
    [InlineData(Rol.CustodioDelVehiculo)]
    [InlineData(Rol.EncargadoDeBienes)]
    public void Los_roles_que_el_mapa_no_cubre_devuelven_nulo(Rol rol)
    {
        // Nulo es «el mapa de navegación no lo cubre», no «este rol no tiene nada que hacer».
        // Elegirles una raíz acá decidiría en el código algo que el diseño no decidió.
        Assert.Null(ReglasDeLaRaiz.De(rol));
    }

    [Fact]
    public void Un_puesto_con_dos_competencias_tiene_dos_raices_y_no_se_elige_por_el()
    {
        // El caso de `R-1`, textual: «el Jefe de Transporte que además es custodio de tres
        // vehículos ve dos raíces distintas, no una mezclada».
        var raices = ReglasDeLaRaiz.DeTodos(
            [Rol.JefeDeTransporte, Rol.EncargadoDeDespacho]);

        Assert.Equal(2, raices.Count);

        // Poner una precedencia acá inventaría una decisión de la institución, y se aplicaría
        // en silencio a todos los puestos.
        Assert.Contains(raices, r => r.Pantalla == "PT-025");
        Assert.Contains(raices, r => r.Pantalla == "PT-038");
    }

    [Fact]
    public void El_rol_sin_raiz_no_aporta_una_entrada_vacia()
    {
        // Un custodio que además es jefe de transporte tiene UNA raíz, no dos con un hueco.
        var raices = ReglasDeLaRaiz.DeTodos([Rol.CustodioDelVehiculo, Rol.JefeDeTransporte]);

        Assert.Equal("PT-025", Assert.Single(raices).Pantalla);
    }

    [Fact]
    public void Sin_competencias_no_hay_ninguna_raiz()
    {
        // Una persona sin puesto vigente es un usuario sin permisos: no se le abre nada.
        Assert.Empty(ReglasDeLaRaiz.DeTodos([]));
    }

    [Fact]
    public void Toda_raiz_dice_a_que_entra_la_persona()
    {
        // `R-2`: nadie entra a «ver indicadores». Una raíz sin porqué es un ítem de menú, que
        // es exactamente lo que la regla rechaza.
        foreach (var rol in Enum.GetValues<Rol>())
        {
            var raiz = ReglasDeLaRaiz.De(rol);
            if (raiz is null) continue;

            Assert.False(string.IsNullOrWhiteSpace(raiz.PorQue));
            Assert.False(string.IsNullOrWhiteSpace(raiz.Nombre));
        }
    }
}
