namespace Sigti.Dominio.M01_Organizacion;

/// <summary>
/// La raíz de cada rol — `mapa-de-navegacion` §1 `R-1` y `R-2`.
///
/// ── Por qué esto es dominio y no una tabla en el menú ───────────────────────
/// <i>«No hay un menú único. Hay una raíz por puesto»</i> (`R-1`), y <i>«la raíz de cada rol es
/// su bandeja de trabajo, no un tablero decorativo»</i> (`R-2`). Son afirmaciones sobre el
/// negocio: dicen a qué entra cada persona a hacer su trabajo. Dejarlas en el componente del
/// menú haría que la próxima pantalla que se agregue las contradiga sin que nada avise.
///
/// ── Transcritas, no inventadas ──────────────────────────────────────────────
/// Cada entrada sale del diagrama del rol en el mapa, donde el nodo está rotulado «raíz». Los
/// roles cuyo diagrama <b>no declara raíz</b> devuelven nulo y lo dicen: elegirles una sería
/// decidir en el código algo que el diseño todavía no decidió.
/// </summary>
public static class ReglasDeLaRaiz
{
    /// <summary>
    /// La raíz declarada para el rol, o <b>nulo cuando el mapa no declara ninguna</b>.
    ///
    /// Nulo no es «este rol no tiene nada que hacer»: es «el mapa de navegación no lo cubre».
    /// Son cosas distintas y la pantalla tiene que poder decir cuál.
    /// </summary>
    public static Raiz? De(Rol rol) => rol switch
    {
        // §2 — «PT-006 Mis solicitudes · raíz del rol».
        Rol.Solicitante => new Raiz("PT-006", "Mis solicitudes",
            "Entra a pedir un traslado y a ver en qué quedó el que pidió."),

        // §3 — «PT-013 Bandeja de autorización · raíz · ordenada por salida más próxima».
        Rol.JefaturaInmediata => new Raiz("PT-013", "Bandeja de autorización",
            "Entra a pronunciarse sobre lo que espera su firma, en lotes."),

        // §4 — «PT-025 Cola de programación · raíz · caducidad de la aprobación visible».
        Rol.JefeDeTransporte => new Raiz("PT-025", "Cola de programación",
            "Es el usuario más intensivo: el sistema es su herramienta, no un trámite."),

        // §5 — «PT-038 Tablero de despacho del día · raíz · salidas y retornos previstos».
        Rol.EncargadoDeDespacho => new Raiz("PT-038", "Tablero de despacho del día",
            "Entra a la ráfaga de la mañana: qué sale y qué vuelve hoy."),

        // §8.1 — «PT-050 Ciclo de vida del vale y arqueo del fondo · raíz».
        Rol.EncargadoDeCombustible => new Raiz("PT-050", "Ciclo del vale y arqueo del fondo",
            "Custodia efectivo o vales: su navegación gira alrededor de ese objeto."),

        // §8.3 — «Pendientes de mi firma · raíz · pocas filas, decisión inmediata». El mapa no
        // le asigna identificador de pantalla, y no se le inventa uno: los `PT-xxx` los declara
        // el inventario.
        Rol.GerenciaAdministrativa or Rol.MaximaAutoridad =>
            new Raiz(null, "Pendientes de mi firma",
                "Pocas filas y decisión inmediata: lo usa desde el celular."),

        // §8.2 — «PT-127 Mi delegación hoy · raíz». Corrección `HB34-70`: es `PT-127`, no
        // `PT-104` —que es del motorista—. Son pantallas de propósito opuesto.
        Rol.EncargadoDeDelegacion => new Raiz("PT-127", "Mi delegación hoy",
            "Misiones, pendientes de envío y la cola de papeles por digitar. Cliente de campo."),

        // §8.4 — «PT-088 Consulta de la pista de auditoría · raíz».
        Rol.AuditorInterno => new Raiz("PT-088", "Pista de auditoría",
            "Sólo lectura, y cada consulta queda registrada."),

        // §6 — el motorista es cliente de campo: una misión, sin menú.
        Rol.Motorista => new Raiz("PT-104", "Mi misión",
            "Una misión, sin menú. Es cliente de campo, no oficina."),

        // ⚠️ El mapa **no declara raíz** para estos. No se les elige una acá.
        Rol.EncargadoDeMantenimiento or Rol.CustodioDelVehiculo or Rol.EncargadoDeBienes
            or Rol.Administrador or Rol.VerificadorEnCarretera => null,

        _ => null,
    };

    /// <summary>
    /// Las raíces de un puesto, una por cada rol que le compete.
    ///
    /// ── Cuando hay más de una, no se elige ──────────────────────────────────
    /// Un puesto con dos competencias tiene dos raíces, y <b>cuál manda es política, no
    /// código</b>. Poner una precedencia acá —«transporte antes que custodia»— inventaría una
    /// decisión de la institución y se aplicaría en silencio a todos los puestos.
    /// </summary>
    public static IReadOnlyList<Raiz> DeTodos(IEnumerable<Rol> roles) =>
        [.. roles.Select(De).OfType<Raiz>().DistinctBy(r => r.Nombre)];
}

/// <param name="Pantalla">
/// El identificador del inventario. <b>Nulo cuando el mapa describe la raíz sin darle uno</b> —
/// pasa con «Pendientes de mi firma». Inventarle un `PT-xxx` crearía un identificador que el
/// inventario no reconoce, y los identificadores no se reciclan.
/// </param>
public sealed record Raiz(string? Pantalla, string Nombre, string PorQue);
