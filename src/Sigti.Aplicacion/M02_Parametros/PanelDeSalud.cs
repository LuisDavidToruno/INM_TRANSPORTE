using Microsoft.EntityFrameworkCore;

using Sigti.Datos;
using Sigti.Datos.M02_Parametros;
using Sigti.Dominio.M02_Parametros;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M06_Solicitudes;
using Sigti.Dominio.M16_Sincronizacion;
using Sigti.Dominio.M19_Seguimiento;
using Sigti.Dominio.M17_PersonasExternas;

namespace Sigti.Aplicacion.M02_Parametros;

/// <summary>
/// `PT-101` — qué está mal y qué hacer.
///
/// ── Por qué esta pantalla tiene que existir ─────────────────────────────────
/// El sistema está lleno de controles que <b>se apagan solos</b> cuando falta su parámetro, y
/// cada uno lo declara donde aparece: el tablero de seguimiento dice que no puede degradar, el
/// control de folios dice que no habrá aviso previo, la depuración dice que no depura.
///
/// Cada aviso por separado es correcto y <b>ninguno alcanza</b>: nadie recorre once pantallas
/// para saber qué le falta configurar. Sin este resumen, un control apagado se descubre el día
/// que hacía falta.
///
/// ── Y dice qué hacer, no sólo qué falta ─────────────────────────────────────
/// Es la mitad que convierte un tablero en algo accionable. «Umbral no configurado» manda a
/// preguntar; «acuérdelo con Auditoría Interna y cárguelo en Parámetros» se puede hacer.
/// </summary>
public sealed class PanelDeSalud(SigtiDbContext contexto)
{
    /// <summary>
    /// Cada control del sistema que depende de algo que la institución tiene que decidir.
    ///
    /// <b>La lista es explícita y no derivada</b>: recorrer las claves cargadas diría qué hay,
    /// no qué falta — y lo que falta es justamente lo que nunca se cargó.
    /// </summary>
    private static readonly IReadOnlyList<ControlEsperado> Esperados =
    [
        new(ReglasDeLaFrescura.ClaveDelUmbral, "Umbral de degradación del seguimiento",
            "El tablero muestra la antigüedad de cada dato y no puede decir si es mucha.",
            "Acuérdelo con quien opera el seguimiento y cárguelo acá.", "#68"),

        new(Aplicacion.M19_Seguimiento.ServicioDeSeguimiento.ClaveDeEstados,
            "Catálogo de estados en ruta",
            "El motorista no puede declarar en qué está: la captura se rechaza.",
            "Cargue los estados del catálogo `estado_en_ruta` de RN-76.", null),

        new(Aplicacion.M19_Seguimiento.ServicioDeSeguimiento.ClaveDeCausasImproductivas,
            "Causas de espera improductiva",
            "Ninguna espera se clasifica, y el indicador no puede calcularse.",
            "Decida cuáles causas cuentan como improductivas — asigna responsabilidad a una " +
            "dependencia, así que es decisión de la institución.", "#103"),

        new(ReglasDelFolio.ClaveDelFormato, "Formato del folio institucional",
            "Las órdenes de misión salen con folio provisional, no oficial.",
            "Defina el formato del correlativo. RNF-21: no se decide por inferencia.", "#34"),

        new(ReglasDelFolio.ClaveDelUmbral, "Umbral de aviso de agotamiento de folios",
            "No habrá aviso previo cuando a una delegación se le acaben los folios — y " +
            "reponerlos exige conectividad.",
            "Fije a partir de qué saldo se avisa.", "#34"),

        new(ReglasDelAviso.ClaveDelCanal, "Canal de aviso",
            "Los avisos quedan sólo en la bandeja: nadie recibe nada fuera del sistema.",
            "Elija el canal. `SoloBandeja` es respuesta legítima en delegaciones sin señal.",
            "#102"),

        new(ReglasDeLaDepuracion.ClaveDelPlazo, "Plazo de depuración de datos personales",
            "Los datos personales de los manifiestos se acumulan indefinidamente.",
            "Acuérdelo con Auditoría Interna y el Oficial de Información Pública.", null),
    ];

    public async Task<Salud> EvaluarAsync(
        DateOnly fecha, CancellationToken cancelacion = default)
    {
        var parametros = new ParametrosNormativos(contexto);
        var controles = new List<EstadoDelControl>();

        foreach (var e in Esperados)
        {
            var catalogo = await parametros.CatalogoDeAsync(e.Clave, cancelacion);
            var vigente = catalogo.ResolverSiHay(e.Clave, fecha, DateTimeOffset.UtcNow);

            // ⚠️ **El respaldo documental va al panel.** Un valor cargado y uno DECIDIDO se ven
            // iguales en una casilla marcada, y no son lo mismo: sin ver de dónde salió, un
            // valor puesto para poder probar el sistema pasa por una decisión de la institución
            // — y nadie vuelve a preguntar por él.
            var respaldo = await contexto.Parametros
                .AsNoTracking()
                .Where(v => v.Clave == e.Clave && v.AprobadoPor != null)
                .OrderByDescending(v => v.VigenteDesde)
                .Select(v => v.Respaldo.Fuente)
                .FirstOrDefaultAsync(cancelacion);

            controles.Add(new EstadoDelControl(
                e.Clave, e.Nombre, vigente is not null,
                vigente?.Valor, e.QueSeApaga, e.QueHacer, e.Insumo, respaldo));
        }

        // ── Lo que no es un parámetro pero también apaga un control ─────────
        //
        // Van en la misma lista a propósito: a quien administra el sistema le da igual si lo
        // que falta es una clave o una integración. Separarlos en dos pantallas haría que la
        // segunda no la mire nadie.
        var sinFundamento = await contexto.CamposDelManifiesto
            .CountAsync(c => c.Activo && c.Clase != ClaseDelCampo.Minimo &&
                             (c.BaseLegal == null || c.NecesidadOperativa == null), cancelacion);

        var rangos = await contexto.RangosDeFolio.CountAsync(cancelacion);

        var otros = new List<EstadoDelControl>
        {
            new("espejo.talento-humano", "Espejo de Talento Humano", false, null,
                "La disponibilidad del motorista no se verifica contra vacaciones, permisos " +
                "ni incapacidades: `BD-10` la evalúa sólo contra el padrón.",
                "Requiere construir la integración. No es una configuración.", null, null),

            new("folio.rangos", "Rangos de folio por delegación", rangos > 0,
                rangos > 0 ? $"{rangos} rango(s) asignado(s)" : null,
                "Ninguna delegación puede emitir un folio oficial, y las que no tienen enlace " +
                "no podrían emitir nada.",
                "Asigne un rango por delegación — y un subrango por equipo donde haya más de uno.",
                null, null),

            new("manifiesto.campos-sin-fundamento", "Campos sensibles sin fundamentar",
                sinFundamento == 0,
                sinFundamento == 0 ? "ninguno" : $"{sinFundamento} campo(s)",
                "Se están capturando datos de salud, etnia, situación migratoria o " +
                "vulnerabilidad sin que conste por qué.",
                "Registre la base legal y la necesidad operativa de cada uno, o desactívelos.",
                null, null),
        };

        var todos = controles.Concat(otros).ToList();

        return new Salud(
            // Lo que falta primero. Es a lo que se entra.
            [.. todos.OrderBy(c => c.Configurado).ThenBy(c => c.Nombre)],
            todos.Count(c => !c.Configurado));
    }

    private sealed record ControlEsperado(
        string Clave, string Nombre, string QueSeApaga, string QueHacer, string? Insumo);
}

/// <param name="Valor">
/// Lo que está configurado. <b>Nulo cuando no lo está</b> — y esa es la razón de que el control
/// esté apagado, no un dato que falte mostrar.
/// </param>
/// <param name="QueSeApaga">
/// Qué deja de funcionar mientras falte. <b>Es lo que hace útil el panel</b>: sin esto, una
/// lista de claves sin valor no le dice a nadie qué está en riesgo.
/// </param>
/// <param name="Insumo">
/// El número del insumo pendiente, cuando lo tiene. Nulo no significa que esté decidido:
/// significa que no se levantó como insumo.
/// </param>
/// <param name="Respaldo">
/// De dónde salió el valor. <b>Nulo cuando no es un parámetro cargado.</b>
///
/// ⚠️ Va al panel porque <b>un valor cargado y uno decidido se ven iguales</b> en una casilla
/// marcada. Sin ver el respaldo, un valor puesto para poder probar el sistema pasa por una
/// decisión de la institución, y nadie vuelve a preguntar por él.
/// </param>
public sealed record EstadoDelControl(
    string Clave, string Nombre, bool Configurado, string? Valor,
    string QueSeApaga, string QueHacer, string? Insumo, string? Respaldo);

public sealed record Salud(IReadOnlyList<EstadoDelControl> Controles, int SinConfigurar);
