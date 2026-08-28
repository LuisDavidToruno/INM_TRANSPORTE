using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M02_Parametros;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.Organizacion;
using Sigti.Dominio.M05_Motoristas;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Pruebas.M07_ProgramacionYDespacho;

/// <summary>
/// Asignaciones de prueba. <b>La matriz de acá es dato de prueba, no la matriz oficial</b>,
/// que sigue siendo insumo abierto `[C]`.
/// </summary>
internal static class Asignacion
{
    /// <summary>
    /// La ventana arranca DESPUÉS del momento de las pruebas —2026-03-12— a propósito.
    /// Si empezara el mismo día, la aprobación estaría caducada y `T-08` no se podría
    /// ejercer: programar el día de salida ya es tarde.
    /// </summary>
    public static readonly VentanaDeMision Ventana =
        new(new DateOnly(2026, 3, 20), new DateOnly(2026, 3, 22), HolguraDias: 1);

    public static readonly FichaTecnica Pickup =
        new("PICKUP", ClaseNormativa.Automovil, PesoBrutoKg: 2_800, CapacidadPasajeros: 5, LlevaRemolque: false);

    public static readonly MatrizDeLicencias Matriz = MatrizDeLicencias.Con("PRUEBA-01",
    [
        new EntradaDeMatriz(CategoriaDeLicencia.B, ClaseNormativa.Automovil, 3_500, 8, PermiteRemolque: false,
            VigenteDesde: new DateOnly(2026, 1, 1), VigenteHasta: null,
            RegistradoDesde: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.FromHours(-6)),
            RegistradoHasta: null)
    ]);

    /// <summary>
    /// Una custodia vigente y abierta, para las pruebas que <b>no</b> ejercen `BD-13`.
    ///
    /// Desde que el despacho la exige, toda prueba que despache tiene que contestar quién
    /// responde por el vehículo. Las que no prueban la custodia usan ésta; las que sí,
    /// arman la suya. Un valor por omisión en el dominio habría hecho lo contrario:
    /// que olvidarse de contestar pasara desapercibido, que es justo lo que el bloqueo
    /// existe para impedir.
    /// </summary>
    public static readonly CustodiaAlDespachar Custodiado = new(
        [new CustodiaDelVehiculo(new IdPersona("P-CUSTODIO"), new DateOnly(2025, 1, 1), null)],
        // Organigrama VACIO a proposito: el espejo no conoce a P-CUSTODIO, asi que no se
        // puede afirmar que ceso. Es el estado real del sistema hoy, y estas pruebas no van
        // de la custodia vacante -- las que si, arman el suyo.
        new Organigrama([]));

    /// <summary>
    /// Circulación <b>sin ningún día inhábil</b>, para las pruebas que no ejercen `BD-04`.
    ///
    /// ── Por qué un calendario de siete días y no un permiso falso ───────────
    /// Porque un permiso inventado haría que estas pruebas <b>pasaran por BD-04</b> sin que
    /// nadie las hubiera escrito para eso, y el día que el permiso dejara de amparar
    /// fallarían por una razón que no tiene nada que ver con lo que prueban. Un calendario
    /// sin días inhábiles hace que `BD-04` sencillamente <b>no aplique</b> — que es la verdad
    /// de estas pruebas—, y además es una configuración institucional legítima: una
    /// delegación con turnos de fin de semana tiene exactamente ese calendario.
    ///
    /// La ventana de `Asignacion.Ventana` va del <b>viernes 20</b> al <b>lunes 23</b> de marzo:
    /// cruza sábado y domingo. Sin esto, toda prueba que despache chocaría contra `BD-04`.
    /// </summary>
    public static CirculacionEnDiaInhabil SinDiasInhabiles(Ulid? vehiculo = null, Ulid? motorista = null) =>
        new(new CalendarioDeDiasHabiles(
                "PRUEBA-TODOS-HABILES",
                new HashSet<DayOfWeek>(Enum.GetValues<DayOfWeek>()),
                new HashSet<DateOnly>()),
            vehiculo ?? Ulid.NewUlid(),
            motorista ?? Ulid.NewUlid(),
            Excepcion: null,
            Permisos: []);

    public static AsignacionDeMision Valida() => ConLicenciaHasta(new DateOnly(2027, 1, 1));

    public static AsignacionDeMision ConLicenciaHasta(DateOnly vencimiento) => new(
        Licencia: new Licencia("0801-1990-01234", CategoriaDeLicencia.B, vencimiento, []),
        Vehiculo: Pickup,
        Documentacion: new DocumentacionDelVehiculo
        {
            Placa = "PAA1234",
            TieneConstanciaSustitutaDePlaca = false,
            VenceMatricula = new DateOnly(2027, 1, 1),
            VencePoliza = new DateOnly(2027, 1, 1),
            VenceRevisionMecanica = new DateOnly(2027, 1, 1),
            IdentificacionInstitucionalVerificada = true
        });
}
