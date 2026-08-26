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

    private static readonly MatrizDeLicencias Matriz = MatrizDeLicencias.Con("ACUERDO-1012-2021-ART-4",
    [
        // TIPO B: livianos, masa máxima autorizada ≤ 3,500 kg, diseñados para no más de
        // ocho (8) personas además del conductor.
        Entrada(CategoriaDeLicencia.B, kg: 3_500, pasajeros: 8),

        // TIPO BE: automóviles de la categoría B enganchados a un remolque.
        Entrada(CategoriaDeLicencia.BE, kg: 3_500, pasajeros: 8, remolque: true),

        // TIPO C1: automóviles no comprendidos en B, masa máxima autorizada ≤ 7,500 kg.
        Entrada(CategoriaDeLicencia.C1, kg: 7_500, pasajeros: 8),

        // TIPO C: vehículos de carga superiores a 7,500 kg, no articulados. El Acuerdo no
        // fija techo superior; el techo real lo pone la ficha técnica del vehículo.
        Entrada(CategoriaDeLicencia.C, kg: int.MaxValue, pasajeros: 8),

        // TIPO CE: categoría C enganchada a remolque o semirremolque (cisternas,
        // plataformas, furgones).
        Entrada(CategoriaDeLicencia.CE, kg: int.MaxValue, pasajeros: 8, remolque: true),

        // TIPO D1: autobuses hasta 25 pasajeros. TIPO D: superiores a 26.
        Entrada(CategoriaDeLicencia.D1, kg: int.MaxValue, pasajeros: 25),
        Entrada(CategoriaDeLicencia.D, kg: int.MaxValue, pasajeros: int.MaxValue)
    ]);

    // ⚠️ FALTAN `A` y `B1`, y no es un olvido.
    //
    // El Artículo 4 define esas dos por CLASE DE VEHÍCULO, no por umbral numérico:
    // «A: ciclomotores y motocicletas», «B1: triciclos y cuadriciclos de motor». Esta
    // matriz resuelve por masa, pasajeros y remolque, y con esos tres atributos no hay
    // forma de distinguir una motocicleta de un automóvil liviano.
    //
    // Inventarles un umbral seria inventar norma. Consecuencia mientras tanto: una
    // licencia `A` o `B1` NO HABILITA NADA, porque la ausencia de entrada se trata como
    // negativa. SIGTI cubre motos explícitamente, así que esto bloquea el despacho de
    // motocicletas y hay que cerrarlo antes de operar. Registrado en HANDOFF.md.

    public MatrizDeLicencias MatrizVigenteAl(DateOnly fecha) => Matriz;

    /// <summary>Póliza y revisión apagadas: no son obligatorias por ley vigente (`DP-001, D-13`).</summary>
    public PoliticaDeDocumentacion PoliticaVigenteAl(DateOnly fecha) => PoliticaDeDocumentacion.PorDefecto;

    private static EntradaDeMatriz Entrada(
        CategoriaDeLicencia categoria, int kg, int pasajeros, bool remolque = false) =>
        new(categoria,
            PesoBrutoMaximoKg: kg,
            CapacidadMaximaPasajeros: pasajeros,
            PermiteRemolque: remolque,
            VigenteDesde: EnVigencia,
            VigenteHasta: null,
            RegistradoDesde: Publicacion,
            RegistradoHasta: null);
}
