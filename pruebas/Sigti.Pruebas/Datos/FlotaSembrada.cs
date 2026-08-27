using Sigti.Datos;
using Sigti.Dominio.M03_Flota;

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
    /// Siembra la flota si no está. Idempotente: las pruebas comparten la base y ninguna
    /// puede asumir que corre primero.
    /// </summary>
    public static async Task SembrarAsync(SigtiDbContext contexto)
    {
        if (contexto.Vehiculos.Any(v => v.Id == Pickup)) return;

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
