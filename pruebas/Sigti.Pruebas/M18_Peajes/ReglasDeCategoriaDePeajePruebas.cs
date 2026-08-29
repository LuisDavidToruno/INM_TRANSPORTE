using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M18_Peajes;

namespace Sigti.Pruebas.M18_Peajes;

/// <summary>
/// `RN-33` — la categoría se deriva de la ficha técnica, <b>nunca del número de ejes solo</b>.
///
/// ── El error que estas pruebas existen para impedir ──────────────────────────
/// `NRM-10`, con evidencia `[V]`: <i>«un vehículo liviano tiene 2 ejes y paga L. 22. Un
/// "Vehículo de 2 Ejes" paga L. 90. <b>Ambos tienen dos ejes</b>»</i>. Y la consecuencia,
/// textual: <i>«cualquier modelo que use `numero_ejes` como única llave para resolver la tarifa
/// está mal y va a cobrar cuatro veces de más a cada pickup de la flota»</i>.
/// </summary>
public class ReglasDeCategoriaDePeajePruebas
{
    private static readonly DateOnly Hoy = new(2026, 3, 16);
    private static readonly DateTimeOffset Ahora = new(2026, 3, 16, 8, 0, 0, TimeSpan.FromHours(-6));

    private static readonly Dictionary<string, string> Nombres = new()
    {
        ["LIVIANO"] = "Liviano/Turismo",
        ["EJES-2"] = "Vehículo de 2 Ejes",
        ["EJES-3"] = "Vehículo de 3 Ejes",
    };

    /// <summary>
    /// Una matriz mínima con la forma que `RN-33` describe: el liviano se resuelve por clase y
    /// peso —no por ejes—, y las categorías por eje sólo alcanzan a lo que pesa más.
    /// </summary>
    private static ReglaDeCategoria[] Matriz() =>
    [
        // Prioridad 10: la excepción nominal que la SAPP tuvo que resolver el 17/09/2025.
        new(Ulid.NewUlid(), "LIVIANO", 10,
            "Resolución SAPP 17/09/2025: panel Hyundai H-100 clasifica como liviano conforme " +
            "al Artículo 51 de la Ley de Tránsito.",
            TipoDeVehiculo: "Panel H-100",
            VigenteDesde: new DateOnly(2025, 9, 17), RegistradoDesde: Ahora),

        // Prioridad 20: la general de livianos, por clase y peso.
        new(Ulid.NewUlid(), "LIVIANO", 20,
            "Clasificación general de automóviles livianos por peso bruto.",
            Clase: ClaseNormativa.Automovil, PesoBrutoHastaKg: 3_500,
            VigenteDesde: new DateOnly(2025, 1, 1), RegistradoDesde: Ahora),

        // Prioridad 30 y 40: las de carga, que sí miran ejes.
        new(Ulid.NewUlid(), "EJES-2", 30, "Vehículo de carga de 2 ejes.",
            Clase: ClaseNormativa.Camion, EjesDesde: 2, EjesHasta: 2,
            VigenteDesde: new DateOnly(2025, 1, 1), RegistradoDesde: Ahora),

        new(Ulid.NewUlid(), "EJES-3", 40, "Vehículo de carga de 3 ejes.",
            Clase: ClaseNormativa.Camion, EjesDesde: 3, EjesHasta: 3,
            VigenteDesde: new DateOnly(2025, 1, 1), RegistradoDesde: Ahora),
    ];

    private static CategoriaResuelta Derivar(FichaTecnica ficha, bool provisional = false) =>
        ReglasDeCategoriaDePeaje.Derivar(ficha, Matriz(), Nombres, Hoy, Ahora, provisional);

    // ── El caso que da nombre a la regla ────────────────────────────────────

    [Fact]
    public void Un_pickup_de_DOS_ejes_es_LIVIANO_y_no_Vehiculo_de_2_Ejes()
    {
        // El pickup y el camión de dos ejes tienen los mismos dos ejes y pagan L 22 y L 90.
        // Resolver por ejes le cobraría al pickup cuatro veces de más.
        var pickup = new FichaTecnica(
            "Pickup doble cabina", ClaseNormativa.Automovil, PesoBrutoKg: 2_800,
            CapacidadPasajeros: 5, LlevaRemolque: false, NumeroDeEjes: 2);

        var r = Derivar(pickup);

        Assert.True(r.EstaResuelta);
        Assert.Equal("LIVIANO", r.Categoria!.Codigo);
    }

    [Fact]
    public void Un_camion_de_DOS_ejes_NO_es_liviano()
    {
        var camion = new FichaTecnica(
            "Camión de carga", ClaseNormativa.Camion, PesoBrutoKg: 12_000,
            CapacidadPasajeros: 3, LlevaRemolque: false, NumeroDeEjes: 2);

        var r = Derivar(camion);

        Assert.Equal("EJES-2", r.Categoria!.Codigo);
    }

    [Fact]
    public void La_excepcion_nominal_de_la_SAPP_le_GANA_a_la_regla_general()
    {
        // El panel H-100 es exactamente el vehículo que COVI-H reclasificó a categoría superior
        // y que la SAPP mandó suspender. La fila de prioridad alta es la que lo protege, y su
        // fundamento viaja con el resultado — porque volverán a cobrarlo mal.
        var panel = new FichaTecnica(
            "Panel H-100", ClaseNormativa.Camion, PesoBrutoKg: 5_000,
            CapacidadPasajeros: 3, LlevaRemolque: false, NumeroDeEjes: 2);

        var r = Derivar(panel);

        Assert.Equal("LIVIANO", r.Categoria!.Codigo);
        Assert.Contains("SAPP 17/09/2025", r.Explicacion);
    }

    [Fact]
    public void La_categoria_dice_QUE_atributos_la_determinaron()
    {
        // `RN-33` punto 2: una categoría sin explicación no se puede defender ante la SAPP ni
        // ante un auditor.
        var camion = new FichaTecnica(
            "Camión de carga", ClaseNormativa.Camion, 12_000, 3, false, NumeroDeEjes: 3);

        var r = Derivar(camion);

        Assert.Contains("clase normativa", r.Explicacion);
        Assert.Contains("número de ejes", r.Explicacion);
    }

    [Fact]
    public void La_CLASE_por_si_sola_separa_dos_vehiculos_identicos_en_todo_lo_demas()
    {
        // Esta prueba existe porque la anterior no bastaba: en la matriz de arriba el peso
        // discrimina antes que la clase, así que quitar la comprobación de clase no rompía
        // nada. Acá las dos filas coinciden en peso y ejes, y **sólo la clase las separa** --
        // que es la forma en que un microbús y un panel del mismo tonelaje terminan en
        // categorías distintas.
        ReglaDeCategoria[] soloClase =
        [
            new(Ulid.NewUlid(), "LIVIANO", 10, "Automóviles.",
                Clase: ClaseNormativa.Automovil, PesoBrutoHastaKg: 6_000,
                EjesDesde: 2, EjesHasta: 2,
                VigenteDesde: new DateOnly(2025, 1, 1), RegistradoDesde: Ahora),

            new(Ulid.NewUlid(), "EJES-2", 20, "Carga.",
                Clase: ClaseNormativa.Camion, PesoBrutoHastaKg: 6_000,
                EjesDesde: 2, EjesHasta: 2,
                VigenteDesde: new DateOnly(2025, 1, 1), RegistradoDesde: Ahora),
        ];

        CategoriaResuelta Con(ClaseNormativa clase) =>
            ReglasDeCategoriaDePeaje.Derivar(
                new FichaTecnica("Unidad", clase, 5_000, 3, false, NumeroDeEjes: 2),
                soloClase, Nombres, Hoy, Ahora, false);

        Assert.Equal("LIVIANO", Con(ClaseNormativa.Automovil).Categoria!.Codigo);
        Assert.Equal("EJES-2", Con(ClaseNormativa.Camion).Categoria!.Codigo);
    }

    // ── Cuando no se puede ──────────────────────────────────────────────────

    [Fact]
    public void Sin_numero_de_ejes_la_categoria_queda_NO_RESUELTA_y_dice_que_falta()
    {
        // `RN-33` punto 3: el sistema no adivina. Y decir cuál es el dato faltante convierte un
        // «no se pudo» en algo que alguien puede ir a cargar.
        var camion = new FichaTecnica(
            "Camión de carga", ClaseNormativa.Camion, 12_000, 3, false, NumeroDeEjes: null);

        var r = Derivar(camion);

        Assert.False(r.EstaResuelta);
        Assert.Null(r.Categoria);
        Assert.Equal("el número de ejes", r.AtributoQueFalta);
        Assert.Contains("no adivina", r.Explicacion);
    }

    [Fact]
    public void Sin_matriz_cargada_no_se_inventa_ningun_criterio_de_corte()
    {
        // `[C]` insumo #23: el Artículo 51 es un escaneo sin capa de texto. La matriz no se
        // puede fijar hasta obtenerlo, y mientras tanto no se supone nada.
        var pickup = new FichaTecnica("Pickup", ClaseNormativa.Automovil, 2_800, 5, false, 2);

        var r = ReglasDeCategoriaDePeaje.Derivar(pickup, [], Nombres, Hoy, Ahora, false);

        Assert.False(r.EstaResuelta);
        Assert.Contains("no se inventa ningún criterio de corte", r.Explicacion);
    }

    [Fact]
    public void La_matriz_PROVISIONAL_lo_dice_en_el_resultado()
    {
        // Una categoría provisional que se muestra igual que una firme se cita después como si
        // lo fuera.
        var pickup = new FichaTecnica("Pickup", ClaseNormativa.Automovil, 2_800, 5, false, 2);

        Assert.True(Derivar(pickup, provisional: true).Provisional);
        Assert.False(Derivar(pickup, provisional: false).Provisional);
    }

    [Fact]
    public void Una_ficha_que_la_matriz_no_cubre_pide_cargar_la_fila_que_falta()
    {
        var montacargas = new FichaTecnica(
            "Montacargas", ClaseNormativa.Motocicleta, 3_000, 1, false, NumeroDeEjes: 2);

        var r = Derivar(montacargas);

        Assert.False(r.EstaResuelta);
        Assert.Null(r.AtributoQueFalta);
        Assert.Contains("cargar la fila que lo clasifica", r.Explicacion);
    }

    // ── La derivación por TIPO, para la estimación previa ───────────────────

    [Fact]
    public void El_estimado_previo_usa_la_categoria_del_TIPO_y_lo_dice()
    {
        // Antes de asignar el vehículo no hay ficha técnica contra la cual derivar, y `T-02`
        // estima igual — hallazgos `HB1-09` y `HN1-10`.
        var r = ReglasDeCategoriaDePeaje.DelTipoRequerido(
            new CategoriaDePeaje("LIVIANO", "Liviano/Turismo"), "Pickup doble cabina");

        Assert.True(r.EstaResuelta);
        Assert.Equal(BaseDeLaCategoria.TipoDeVehiculoRequerido, r.Base);

        // «Un estimado que no dice sobre qué base se calculó no se puede defender ante quien lo
        // autorizó».
        Assert.Contains("Estimativa", r.Explicacion);
        Assert.Contains("no de una unidad concreta", r.Explicacion);
    }

    [Fact]
    public void Un_tipo_sin_categoria_declarada_no_produce_un_supuesto()
    {
        var r = ReglasDeCategoriaDePeaje.DelTipoRequerido(null, "Grúa de plataforma");

        Assert.False(r.EstaResuelta);
        Assert.Contains("no declara categoría de peaje", r.Explicacion);
    }

    // ── La tabla abierta ────────────────────────────────────────────────────

    [Fact]
    public void La_categoria_se_compara_sin_importar_la_caja_de_las_letras()
    {
        // La tabla la carga una persona. «LIVIANO» y «Liviano» son la misma categoría, y
        // tratarlas distinto produciría un «no hay tarifa» sobre una que sí está.
        var cat = new CategoriaDePeaje("LIVIANO", "Liviano/Turismo");

        Assert.True(cat.Es("liviano"));
        Assert.False(cat.Es("EJES-2"));
    }
}
