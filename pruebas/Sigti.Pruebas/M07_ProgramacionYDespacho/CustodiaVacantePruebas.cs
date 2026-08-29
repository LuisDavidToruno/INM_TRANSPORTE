using Sigti.Dominio.M11_Mantenimiento;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M07_ProgramacionYDespacho;

/// <summary>
/// La <b>custodia vacante</b> de `RN-22`: *«custodio que cesa en el cargo dejando el vehículo
/// asignado»*.
///
/// ── El hueco que `BD-13` sola no cubre ───────────────────────────────────────
/// `BD-13` mira la tarjeta de responsabilidad y la encuentra <b>abierta</b> — nadie la cerró,
/// porque la persona ya no está para firmarla. Y despacha. El vehículo sale a nombre de
/// alguien que ya no trabaja en la institución.
///
/// Es el mismo daño que `BD-13` existe para evitar, por otro camino: cuando aparezca el
/// golpe o la multa, <b>no hay a quién imputarla</b>, porque la persona ya no está y nadie
/// recibió formalmente el bien. `RN-101` lo dice sin rodeos: <i>«la institución pierde la
/// deducción de responsabilidad por un trámite que no se hizo»</i>.
///
/// ── La distinción que decide si esto sirve o es ruido ────────────────────────
/// <b>Ausencia de dato no es dato de ausencia.</b> Una persona sin puestos vigentes puede
/// haber cesado <b>o</b> puede ser que el espejo no sepa de ella. Tratarlas igual haría que
/// un espejo vacío —que es el estado de hoy— declarara cesada a toda la institución y
/// pusiera la advertencia en cada despacho, hasta que nadie la lea.
/// </summary>
public class CustodiaVacantePruebas
{
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

    private static readonly IdPersona Asistente = new("P-ASISTENTE");
    private static readonly IdPersona Jefe = new("P-JEFE");
    private static readonly IdPersona Jefatura = new("P-JEFATURA");
    private static readonly IdPersona Transporte = new("P-TRANSPORTE");
    private static readonly IdPersona Encargado = new("P-ENCARGADO");
    private static readonly IdPersona Custodio = new("P-CUSTODIO");
    private static readonly IdPuesto Bodega = new("PU-BODEGA-CHOLUTECA");

    private static readonly DatosDeLaSolicitud Solicitud = new(
        "Delegacion de Choluteca", "Traslado de personal", "Choluteca", Asignacion.Ventana);

    /// <summary>Tarjeta de responsabilidad abierta: nadie la cerró.</summary>
    private static readonly CustodiaDelVehiculo Abierta =
        new(Custodio, new DateOnly(2025, 1, 1), null);

    [Fact]
    public void El_custodio_que_ceso_deja_el_vehiculo_en_custodia_vacante()
    {
        // **El caso.** La tarjeta sigue abierta —nadie la cerró, porque la persona ya no está
        // para firmarla— y el puesto se cerró el 28 de febrero.
        var expediente = Programada();

        var ceso = new Organigrama([
            new AsignacionDePuesto(Custodio, Bodega, new DateOnly(2024, 1, 1), new DateOnly(2026, 2, 28)),
        ]);

        Despachar(expediente, new CustodiaAlDespachar([Abierta], ceso));

        // **Advierte y NO bloquea**: `RN-22` pone el bloqueo «tras un plazo configurable», y
        // el plazo es `[C]`. Inventarlo dejaría vehículos varados contra un número que nadie
        // acordó.
        Assert.Equal(EstadoDeMision.Despachada, expediente.Estado);

        var asiento = expediente.Diario[^1].Motivo!;
        Assert.Contains("CUSTODIA VACANTE", asiento);
        Assert.Contains("P-CUSTODIO", asiento);
        // Y dice qué hacer: el acta de entrega, o la unilateral si ya no está.
        Assert.Contains("unilateral", asiento);
    }

    [Fact]
    public void Un_custodio_que_sigue_en_su_puesto_no_produce_advertencia()
    {
        // El recíproco. Sin él, la advertencia podría dispararse siempre y las otras pruebas
        // seguirían en verde — y una advertencia que sale en todos los despachos deja de
        // leerse en una semana.
        var expediente = Programada();

        var activo = new Organigrama([
            new AsignacionDePuesto(Custodio, Bodega, new DateOnly(2024, 1, 1), null),
        ]);

        Despachar(expediente, new CustodiaAlDespachar([Abierta], activo));

        Assert.DoesNotContain("CUSTODIA VACANTE", expediente.Diario[^1].Motivo!);
        Assert.Contains("BD-13 verificada", expediente.Diario[^1].Motivo!);
    }

    [Fact]
    public void Un_espejo_que_no_conoce_a_la_persona_NO_la_declara_cesada()
    {
        // **La distinción que evita convertir esto en ruido.** Hoy el espejo del organigrama
        // está prácticamente vacío: si «sin puestos vigentes» se leyera como «cesó», la
        // advertencia saldría en cada despacho de la institución.
        //
        // Ausencia de dato no es dato de ausencia — la misma razón por la que la antigüedad
        // del espejo devuelve nulo en vez de cero.
        var expediente = Programada();

        Despachar(expediente, new CustodiaAlDespachar([Abierta], new Organigrama([])));

        Assert.DoesNotContain("CUSTODIA VACANTE", expediente.Diario[^1].Motivo!);
    }

    [Fact]
    public void El_espejo_que_conoce_a_OTROS_pero_no_al_custodio_tampoco_lo_declara_cesado()
    {
        // El caso intermedio, y el que más fácil se cuela: el espejo trajo la delegación de
        // Tegucigalpa y no la de Choluteca. Conoce gente — así que no está «vacío»—, pero no
        // a este custodio. Sigue sin poder afirmarse que cesó.
        var expediente = Programada();

        var otraDelegacion = new Organigrama([
            new AsignacionDePuesto(new IdPersona("P-OTRA"), Bodega, new DateOnly(2024, 1, 1), null),
        ]);

        Despachar(expediente, new CustodiaAlDespachar([Abierta], otraDelegacion));

        Assert.DoesNotContain("CUSTODIA VACANTE", expediente.Diario[^1].Motivo!);
    }

    [Fact]
    public void La_vacancia_se_juzga_a_la_fecha_del_HECHO()
    {
        // P-4, igual que `BD-13`. El puesto se cerró **después** del despacho: cuando el
        // vehículo salió, el custodio estaba en su cargo. Una rotación posterior no puede
        // marcar retroactivamente como vacante una custodia que no lo era.
        var expediente = Programada();

        var cesoDespues = new Organigrama([
            new AsignacionDePuesto(Custodio, Bodega, new DateOnly(2024, 1, 1), new DateOnly(2026, 3, 31)),
        ]);

        Despachar(expediente, new CustodiaAlDespachar([Abierta], cesoDespues));

        Assert.DoesNotContain("CUSTODIA VACANTE", expediente.Diario[^1].Motivo!);
    }

    private static void Despachar(OrdenDeMision expediente, CustodiaAlDespachar custodias) =>
        expediente.Despachar(Encargado, Asignacion.Valida(), Asignacion.Matriz,
                             PoliticaDeDocumentacion.PorDefecto, Momento,
                             custodias, Asignacion.SinDiasInhabiles(), ConflictoPorIndisponibilidad.Ninguno);

    private static OrdenDeMision Programada()
    {
        var expediente = OrdenDeMision.Crear(Ulid.NewUlid(), Asistente, Jefe, Solicitud, Momento);
        expediente.Enviar(Asistente, Momento);
        expediente.Aprobar(Jefatura, Momento);
        expediente.Programar(Transporte, Asignacion.Valida(), Asignacion.Matriz,
                             PoliticaDeDocumentacion.PorDefecto, Momento,
                             new RecursosTomados(Ulid.NewUlid(), Ulid.NewUlid()), []);
        return expediente;
    }
}
