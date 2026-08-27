using Sigti.Datos;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M05_Motoristas;

namespace Sigti.Pruebas.Datos;

/// <summary>
/// La flota de las pruebas de punta a punta.
///
/// ── Por qué existe y por qué los identificadores son fijos ───────────────────
/// Cuando la flota vivía en código, las pruebas citaban `v-001` y `v-002` y esos
/// identificadores existían siempre. Ahora la flota sale de la base, así que cada prueba
/// que ejerza `BD-02` o `BD-03` tiene que sembrarla — y con ULIDs fijos, para poder
/// citarlos igual que antes sin pasarlos de un lado a otro.
///
/// <b>Los cuatro vehículos son los mismos de antes</b>, y cada uno está por una razón:
/// el pick-up habilita con `B`, el camión no, el de remolque exige `BE`, y la motocicleta
/// exige `A` — que es lo que obligó a introducir la clase normativa.
/// </summary>
internal static class FlotaSembrada
{
    /// <summary>Pick-up de 2.800 kg. Una licencia `B` lo habilita.</summary>
    public static readonly Ulid Pickup = Ulid.Parse("01JQ8Z000000000000000VEH01");

    /// <summary>Camión de 12.000 kg. <b>Una `B` no lo habilita</b> — es el rechazo que se prueba.</summary>
    public static readonly Ulid Camion = Ulid.Parse("01JQ8Z000000000000000VEH02");

    /// <summary>
    /// Pick-up <b>con plataforma enganchada</b>. Exige `BE`, y <b>no es articulado</b>: es el
    /// caso que pasaba el bloqueo cuando el atributo decía «articulado» (corrección de
    /// `BD-02` del 2026-08-26).
    /// </summary>
    public static readonly Ulid PickupConRemolque = Ulid.Parse("01JQ8Z000000000000000VEH03");

    /// <summary>Motocicleta. Exige `A`, y por eso la clase normativa no es opcional.</summary>
    public static readonly Ulid Motocicleta = Ulid.Parse("01JQ8Z000000000000000VEH04");

    /// <summary>
    /// Licencia `B` vigente hasta 2028. Habilita el pick-up, no el camión.
    ///
    /// ⚠️ <b>Es COMPARTIDO. Sirve para probar `BD-02`, no para programar.</b> Desde que
    /// `BD-11` bloquea el solapamiento, dos pruebas que lo usen sobre ventanas que se cruzan
    /// chocan entre sí — y con razón: es el mismo motorista en dos misiones a la vez. Una
    /// prueba que <b>programe</b> tiene que pedir el suyo con <see cref="NuevoConductorAsync"/>.
    /// </summary>
    public static readonly Ulid Conductor = Ulid.Parse("01JQ8Z000000000000000CON01");

    /// <summary>
    /// Un motorista propio de la prueba, con la misma licencia `B` vigente.
    ///
    /// ── Por qué hizo falta ──────────────────────────────────────────────────
    /// Nueve pruebas de punta a punta programaban misiones sobre <b>el mismo</b> motorista
    /// en <b>la misma franja</b>, y pasaban porque `BD-11` no estaba implementada. Al
    /// implementarla, empezaron a fallar con razón: eran nueve dobles asignaciones que el
    /// sistema no debía aceptar. <b>El arreglo no fue debilitar la regla</b> — fue que cada
    /// prueba tenga su motorista, que es además lo que ya hacían con el vehículo.
    /// </summary>
    public static async Task<Ulid> NuevoConductorAsync(SigtiDbContext contexto, string nombre)
    {
        var id = Ulid.NewUlid();

        contexto.Conductores.Add(new FilaDeConductor
        {
            Id = id,
            Nombre = nombre,
            EsDelPadron = true,
            NumeroDeLicencia = $"08-1988-{id.ToString()[^5..]}",
            Categoria = CategoriaDeLicencia.B,
            VenceLicencia = new DateOnly(2028, 4, 30),
            Restricciones = null,
        });

        await contexto.SaveChangesAsync();
        return id;
    }

    /// <summary>Un pick-up y un motorista habilitado, los dos propios de la prueba.</summary>
    public sealed record ParaProgramar(string Vehiculo, string Conductor);

    /// <summary>
    /// El par que necesita cualquier prueba que <b>programe</b> de verdad.
    ///
    /// Los dos son nuevos porque `BD-11` bloquea el solapamiento <b>de vehículo Y de
    /// motorista</b>, y la base de pruebas es compartida: reutilizar cualquiera de los dos
    /// sobre ventanas que se cruzan es una doble asignación real, no un artefacto.
    /// </summary>
    /// <param name="prefijo">
    /// Sólo para leer el fallo cuando algo se rompe. <b>Las siglas reales llevan sufijo
    /// único</b>: `flota.Vehiculo` tiene índice único sobre ellas, y dos pruebas que pidan
    /// el mismo prefijo chocarían en la base por un detalle que no tiene nada que ver con
    /// lo que estaban probando.
    /// </param>
    public static async Task<ParaProgramar> ParaProgramarAsync(SigtiDbContext contexto, string prefijo)
    {
        await SembrarAsync(contexto);

        var vehiculo = Ulid.NewUlid();
        var siglas = $"{prefijo}-{vehiculo.ToString()[^5..]}";

        contexto.Vehiculos.Add(Vehiculo(vehiculo, siglas, null, "Pick-up doble cabina",
            ClaseNormativa.Automovil, 2_800, 5, remolque: false));

        await contexto.SaveChangesAsync();
        await CustodiarAsync(contexto, vehiculo);

        var conductor = await NuevoConductorAsync(contexto, $"Motorista de {siglas}");

        return new ParaProgramar(vehiculo.ToString(), conductor.ToString());
    }

    /// <summary>
    /// Siembra la flota si no está. Idempotente: las pruebas comparten la base y ninguna
    /// puede asumir que corre primero.
    /// </summary>
    public static async Task SembrarAsync(SigtiDbContext contexto)
    {
        if (contexto.Vehiculos.Any(v => v.Id == Pickup)) return;

        contexto.Conductores.Add(new FilaDeConductor
        {
            Id = Conductor,
            Nombre = "José Ramón Cruz",
            EsDelPadron = true,
            NumeroDeLicencia = "08-1988-77120",
            Categoria = CategoriaDeLicencia.B,
            VenceLicencia = new DateOnly(2028, 4, 30),
            Restricciones = null,
        });

        contexto.Vehiculos.AddRange(
            Vehiculo(Pickup, "INS-P-014", "PBM8842", "Pick-up doble cabina",
                ClaseNormativa.Automovil, 2_800, 5, remolque: false),

            // Sin placa metálica: estado válido por el desabastecimiento nacional.
            Vehiculo(Camion, "INS-C-002", null, "Camión de carga",
                ClaseNormativa.Camion, 12_000, 3, remolque: false),

            Vehiculo(PickupConRemolque, "INS-P-021", "PCH1190", "Pick-up con plataforma enganchada",
                ClaseNormativa.Automovil, 3_100, 5, remolque: true),

            Vehiculo(Motocicleta, "INS-M-007", "MHA221", "Motocicleta de mensajería",
                ClaseNormativa.Motocicleta, 180, 1, remolque: false));

        await contexto.SaveChangesAsync();

        // Cada vehiculo con su custodio: desde `BD-13`, un vehiculo sin custodio vigente no
        // se despacha. Las cuatro pruebas de punta a punta que despachaban empezaron a fallar
        // al implementarlo, y tenian razon en fallar -- eran cuatro despachos de bienes del
        // Estado sin nadie que respondiera por ellos.
        await CustodiarAsync(contexto, Pickup, Camion, PickupConRemolque, Motocicleta);
    }

    /// <summary>
    /// Registra una custodia vigente y abierta para cada vehiculo.
    ///
    /// <b>Abierta</b> -- `Hasta` nulo -- porque asi es como se registra una tarjeta de
    /// responsabilidad: se firma con fecha de inicio y sin fecha de cese, que llega el dia
    /// que llega.
    /// </summary>
    public static async Task CustodiarAsync(SigtiDbContext contexto, params Ulid[] vehiculos)
    {
        foreach (var vehiculo in vehiculos)
        {
            if (contexto.Custodias.Any(c => c.VehiculoId == vehiculo)) continue;

            contexto.Custodias.Add(new FilaDeCustodia
            {
                Id = Ulid.NewUlid(),
                VehiculoId = vehiculo,
                Custodio = "P-CUSTODIO",
                Desde = new DateOnly(2025, 1, 1),
                Hasta = null,
                Acta = "Acta de prueba",
            });
        }

        await contexto.SaveChangesAsync();
    }

    private static FilaDeVehiculo Vehiculo(
        Ulid id, string siglas, string? placa, string tipo,
        ClaseNormativa clase, int kg, int pasajeros, bool remolque) => new()
    {
        Id = id,
        Siglas = siglas,
        Placa = placa,
        TieneConstanciaSustitutaDePlaca = placa is null,
        TipoDeVehiculo = tipo,
        Clase = clase,
        PesoBrutoKg = kg,
        CapacidadPasajeros = pasajeros,
        LlevaRemolque = remolque,
        // Documentación al día: estas pruebas ejercen `BD-02`, no `BD-03`. Las de `BD-03`
        // siembran su propio vehículo con la fecha que necesitan.
        VenceMatricula = new DateOnly(2030, 12, 31),
        VencePoliza = null,
        VenceRevisionMecanica = null,
        IdentificacionInstitucionalVerificada = true,
    };
}
