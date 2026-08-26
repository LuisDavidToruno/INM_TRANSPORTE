using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M05_Motoristas;

namespace Sigti.Aplicacion.M02_Parametros;

/// <summary>
/// Resuelve los parámetros normativos vigentes <b>a la fecha del hecho</b>, no a la de
/// captura (`P-4`, `RNF-05`).
/// </summary>
public interface IParametrosDeLaInstitucion
{
    MatrizDeLicencias MatrizVigenteAl(DateOnly fecha);
    PoliticaDeDocumentacion PoliticaVigenteAl(DateOnly fecha);
}

/// <summary>
/// La matriz licencia↔vehículo del <b>Artículo 4 del Acuerdo 1012-2021</b> `[V]`,
/// La Gaceta No. 35,661 del 19 de julio de 2021.
///
/// Fuente en el repositorio:
/// `docs/01-negocio/normativa/fuentes/acuerdo-1012-2021-permisos-de-conducir.pdf`.
///
/// <b>Sigue siendo provisional en un sentido:</b> los valores ya son normativos, pero
/// están escritos acá en lugar de cargarse por el circuito de `HU-144` con su doble
/// control. Cuando se carguen por ahí, esta clase se borra.
/// </summary>
public sealed class ParametrosProvisionales : IParametrosDeLaInstitucion
{
    private static readonly DateTimeOffset Publicacion =
        new(2021, 7, 19, 0, 0, 0, TimeSpan.FromHours(-6));

    private static readonly DateOnly EnVigencia = new(2021, 7, 19);

    /// <summary>
    /// Las nueve categorías del Artículo 4. Ninguna lleva umbral inventado: donde el
    /// Acuerdo no fija techo, la entrada no lo fija tampoco y el límite real lo pone la
    /// ficha técnica del vehículo.
    /// </summary>
    private static readonly MatrizDeLicencias Matriz = MatrizDeLicencias.Con("ACUERDO-1012-2021-ART-4",
    [
        // TIPO A: ciclomotores y motocicletas, de motor o eléctricas. La norma no fija
        // masa ni pasajeros: la clase es todo el criterio.
        Entrada(CategoriaDeLicencia.A, ClaseNormativa.Motocicleta),

        // TIPO B1: todo tipo de triciclos y cuadriciclos de motor. Igual que A.
        Entrada(CategoriaDeLicencia.B1, ClaseNormativa.TricicloCuadriciclo),

        // TIPO B: livianos, masa máxima autorizada ≤ 3,500 kg, diseñados para no más de
        // ocho (8) personas además del conductor. «No comprendidos en la categoría A y B1».
        Entrada(CategoriaDeLicencia.B, ClaseNormativa.Automovil, kg: 3_500, pasajeros: 8),

        // TIPO BE: automóviles de la categoría B enganchados a un remolque.
        Entrada(CategoriaDeLicencia.BE, ClaseNormativa.Automovil, kg: 3_500, pasajeros: 8, remolque: true),

        // TIPO C1: no comprendidos en B, masa máxima autorizada ≤ 7,500 kg.
        Entrada(CategoriaDeLicencia.C1, ClaseNormativa.Camion, kg: 7_500),

        // TIPO C: vehículos de carga superiores a 7,500 kg, no articulados.
        Entrada(CategoriaDeLicencia.C, ClaseNormativa.Camion),

        // TIPO CE: categoría C enganchada a remolque o semirremolque (cisternas,
        // plataformas, furgones).
        Entrada(CategoriaDeLicencia.CE, ClaseNormativa.Camion, remolque: true),

        // TIPO D1: autobuses hasta 25 pasajeros. TIPO D: superiores a 26.
        Entrada(CategoriaDeLicencia.D1, ClaseNormativa.Autobus, pasajeros: 25),
        Entrada(CategoriaDeLicencia.D, ClaseNormativa.Autobus)
    ]);

    public MatrizDeLicencias MatrizVigenteAl(DateOnly fecha) => Matriz;

    /// <summary>Póliza y revisión apagadas: no son obligatorias por ley vigente (`DP-001, D-13`).</summary>
    public PoliticaDeDocumentacion PoliticaVigenteAl(DateOnly fecha) => PoliticaDeDocumentacion.PorDefecto;

    /// <summary>
    /// Omitir <c>kg</c> o <c>pasajeros</c> significa <b>que el Acuerdo no fija ese
    /// techo</b>, no que sea infinito por descuido. El límite real lo pone la ficha
    /// técnica del vehículo que se asigne.
    /// </summary>
    private static EntradaDeMatriz Entrada(
        CategoriaDeLicencia categoria,
        ClaseNormativa clase,
        int kg = int.MaxValue,
        int pasajeros = int.MaxValue,
        bool remolque = false) =>
        new(categoria,
            Clase: clase,
            PesoBrutoMaximoKg: kg,
            CapacidadMaximaPasajeros: pasajeros,
            PermiteRemolque: remolque,
            VigenteDesde: EnVigencia,
            VigenteHasta: null,
            RegistradoDesde: Publicacion,
            RegistradoHasta: null);
}
