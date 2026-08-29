using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Pruebas.M03_Flota;

/// <summary>
/// `RN-63` — el préstamo de un vehículo es un expediente del bien, nunca una Orden de Misión.
/// </summary>
public class ReglasDelPrestamoPruebas
{
    private static readonly Ulid Vehiculo = Ulid.NewUlid();
    private static readonly DateOnly Desde = new(2026, 4, 6);
    private static readonly DateOnly Comprometida = new(2026, 5, 6);

    // ── Préstamo o misión: la diferencia es la tenencia ─────────────────────

    /// <summary>
    /// `RN-63`: <i>«cuando el vehículo se cede <b>con motorista de la institución propietaria</b>,
    /// sí es una Orden de Misión con motivo apoyo institucional: ahí no se cedió la tenencia, se
    /// prestó un servicio»</i>.
    /// </summary>
    [Fact]
    public void Ceder_el_vehiculo_con_motorista_propio_NO_es_un_prestamo()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelPrestamo.ExigirCesionDeTenencia(conMotoristaPropio: true));

        Assert.Equal("RN-63", error.Precondicion);
        Assert.Contains("apoyo institucional", error.Message);
    }

    [Fact]
    public void Ceder_la_tenencia_sin_motorista_propio_si_es_prestamo() =>
        ReglasDelPrestamo.ExigirCesionDeTenencia(conMotoristaPropio: false);

    // ── Lo que el expediente exige para existir ─────────────────────────────

    [Fact]
    public void Un_expediente_completo_pasa() =>
        ReglasDelPrestamo.ExigirElExpediente(
            Acto(), Receptor(), Desde, Comprometida, "Apoyo a jornada de vacunación");

    [Fact]
    public void Sin_acto_autorizante_con_folio_y_firmante_no_pasa()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelPrestamo.ExigirElExpediente(
                Acto(folio: ""), Receptor(), Desde, Comprometida, "Apoyo"));

        Assert.Contains("hay un vehículo que se fue", error.Message);
    }

    /// <summary>
    /// `RN-63` punto 2 exige cargo e institución porque el punto 7 pide poder responder
    /// <b>quién respondía por la unidad</b>. Un nombre suelto no contesta esa pregunta.
    /// </summary>
    [Theory]
    [InlineData("", "Jefe de Transporte", "Secretaría de Salud")]
    [InlineData("Ana Discua", "", "Secretaría de Salud")]
    [InlineData("Ana Discua", "Jefe de Transporte", "")]
    public void Un_receptor_sin_nombre_cargo_o_institucion_no_pasa(
        string persona, string cargo, string institucion)
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelPrestamo.ExigirElExpediente(
                Acto(), Receptor(persona, cargo, institucion), Desde, Comprometida, "Apoyo"));

        Assert.Contains("quién respondía por la unidad", error.Message);
    }

    /// <summary>
    /// Sin fecha de devolución posterior al inicio, el préstamo no vence nunca — y un préstamo
    /// que no vence es una baja encubierta.
    /// </summary>
    [Fact]
    public void Una_devolucion_comprometida_anterior_al_inicio_no_pasa() =>
        Assert.Throws<BloqueoDuro>(() =>
            ReglasDelPrestamo.ExigirElExpediente(
                Acto(), Receptor(), Desde, Desde.AddDays(-1), "Apoyo"));

    // ── La segregación de `RN-63` punto 2 ───────────────────────────────────

    /// <summary>
    /// <b>Quien autoriza no puede ser el receptor.</b> Sería la misma persona decidiendo
    /// entregarse a sí misma un vehículo del Estado.
    /// </summary>
    [Fact]
    public void Quien_autoriza_no_puede_ser_el_receptor()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelPrestamo.ExigirSegregacion("Ana Discua", Receptor(persona: "Ana Discua")));

        Assert.Contains("entregarse a sí misma", error.Message);
    }

    /// <summary>La comparación ignora mayúsculas y espacios: el rodeo no puede ser tipográfico.</summary>
    [Fact]
    public void La_segregacion_no_se_esquiva_con_mayusculas_ni_espacios() =>
        Assert.Throws<BloqueoDuro>(() =>
            ReglasDelPrestamo.ExigirSegregacion(
                "  ana discua ", Receptor(persona: "ANA DISCUA")));

    [Fact]
    public void Dos_personas_distintas_pasan() =>
        ReglasDelPrestamo.ExigirSegregacion("Rolando Discua", Receptor(persona: "Ana Discua"));

    /// <summary>
    /// <b>Quien firma la devolución no puede ser quien recibió.</b> El acta dejaría de ser una
    /// constatación para volverse una autodeclaración de que devolvió en orden.
    /// </summary>
    [Fact]
    public void Quien_recibio_no_puede_firmar_su_propia_devolucion()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelPrestamo.ExigirQuienRecibeLaDevolucion(
                Receptor(persona: "Ana Discua"), "Ana Discua"));

        Assert.Contains("autodeclaración", error.Message);
    }

    [Fact]
    public void El_acta_de_devolucion_sin_quien_la_firma_no_pasa() =>
        Assert.Throws<BloqueoDuro>(() =>
            ReglasDelPrestamo.ExigirQuienRecibeLaDevolucion(Receptor(), "  "));

    // ── No vuelve a DISPONIBLE sin acta ─────────────────────────────────────

    [Fact]
    public void Un_prestamo_vigente_impide_devolver_el_vehiculo_a_disponible()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelPrestamo.ExigirActaDeDevolucion(Prestamo()));

        Assert.Contains("No vuelve a DISPONIBLE sin ella", error.Message);
    }

    [Fact]
    public void Con_acta_de_devolucion_el_vehiculo_puede_volver() =>
        ReglasDelPrestamo.ExigirActaDeDevolucion(Prestamo(devuelto: new DateOnly(2026, 5, 4)));

    // ── La mora, que bloquea el cierre del período ──────────────────────────

    [Fact]
    public void La_mora_se_cuenta_desde_la_fecha_comprometida()
    {
        var prestamo = Prestamo();

        Assert.Equal(0, prestamo.DiasDeMora(Comprometida));
        Assert.Equal(30, prestamo.DiasDeMora(Comprometida.AddDays(30)));
        Assert.True(prestamo.EstaVencido(Comprometida.AddDays(1)));
    }

    /// <summary>Un préstamo devuelto no acumula mora: dejó de estar afuera.</summary>
    [Fact]
    public void El_prestamo_devuelto_no_acumula_mora()
    {
        var prestamo = Prestamo(devuelto: new DateOnly(2026, 5, 4));

        Assert.Equal(0, prestamo.DiasDeMora(Comprometida.AddDays(300)));
        Assert.False(prestamo.EstaVencido(Comprometida.AddDays(300)));
    }

    /// <summary>
    /// `RN-97` punto 4 — <b>no se cierra el período con préstamos vencidos</b>. Es la fuente que
    /// faltaba para que ese bloqueo quedara completo.
    /// </summary>
    [Fact]
    public void Un_prestamo_vencido_impide_cerrar_el_periodo()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelPrestamo.ExigirDevolucionAntesDelCierre(
                [Prestamo()], new DateOnly(2026, 12, 31), declaracionExplicita: null));

        Assert.Equal("RN-63", error.Precondicion);
        Assert.Contains("días de mora", error.Message);
        Assert.Contains("Secretaría de Salud", error.Message);
    }

    [Fact]
    public void Declarado_explicitamente_el_prestamo_vencido_deja_cerrar() =>
        ReglasDelPrestamo.ExigirDevolucionAntesDelCierre(
            [Prestamo()], new DateOnly(2026, 12, 31),
            "El acto de prórroga está en trámite en la Secretaría receptora.");

    [Fact]
    public void Un_prestamo_en_plazo_no_bloquea_el_cierre() =>
        ReglasDelPrestamo.ExigirDevolucionAntesDelCierre(
            [Prestamo()], new DateOnly(2026, 4, 20), declaracionExplicita: null);

    // ── El entregable de la regla ───────────────────────────────────────────

    /// <summary>
    /// `RN-63` punto 7: <i>«en cualquier fecha del período, el sistema responde <b>quién
    /// respondía por la unidad</b>. Esa consulta es el entregable de la regla»</i>.
    ///
    /// Se resuelve <b>por la fecha</b>, no por el estado de hoy: un vehículo que hoy está
    /// disponible pudo estar prestado el día que se cometió la infracción.
    /// </summary>
    [Fact]
    public void El_sistema_responde_quien_respondia_por_la_unidad_en_una_fecha()
    {
        var prestamos = new[] { Prestamo(devuelto: new DateOnly(2026, 5, 4)) };

        // Antes del préstamo: responde la institución propietaria.
        var antes = ReglasDelPrestamo.QuienRespondiaPor(prestamos, new DateOnly(2026, 4, 1));
        Assert.False(antes.EsTenenciaAjena);
        Assert.Null(antes.Persona);

        // Durante: responde el receptor, con cargo e institución.
        var durante = ReglasDelPrestamo.QuienRespondiaPor(prestamos, new DateOnly(2026, 4, 20));
        Assert.True(durante.EsTenenciaAjena);
        Assert.Equal("Ana Discua", durante.Persona);
        Assert.Equal("Secretaría de Salud", durante.Institucion);

        // Después de devuelto: vuelve a responder la institución propietaria.
        var despues = ReglasDelPrestamo.QuienRespondiaPor(prestamos, new DateOnly(2026, 6, 1));
        Assert.False(despues.EsTenenciaAjena);
    }

    /// <summary>
    /// El día mismo de la devolución <b>todavía respondía el receptor</b>: el acta se firma ese
    /// día y hasta entonces la unidad estuvo en su poder.
    /// </summary>
    [Fact]
    public void El_dia_de_la_devolucion_todavia_responde_el_receptor()
    {
        var devuelto = new DateOnly(2026, 5, 4);

        var quien = ReglasDelPrestamo.QuienRespondiaPor([Prestamo(devuelto: devuelto)], devuelto);

        Assert.True(quien.EsTenenciaAjena);
    }

    // ── Lo que el expediente calcula ────────────────────────────────────────

    /// <summary>
    /// `RN-63` punto 3 — los kilómetros bajo tenencia ajena <b>no entran</b> en la conciliación
    /// galonaje–kilometraje (`RN-30`): no hubo consumo nuestro contra esos kilómetros.
    ///
    /// Nulo mientras no haya devolución: con una sola lectura no hay recorrido que medir.
    /// </summary>
    [Fact]
    public void Los_kilometros_bajo_tenencia_ajena_salen_de_las_dos_lecturas()
    {
        Assert.Null(Prestamo().KilometrosBajoTenenciaAjena);

        var devuelto = Prestamo(devuelto: new DateOnly(2026, 5, 4), odometroDevolucion: 92_800);
        Assert.Equal(1_400, devuelto.KilometrosBajoTenenciaAjena);
    }

    /// <summary>
    /// La identificación del vehículo del Estado es hallazgo frecuente de auditoría, y `RN-63`
    /// punto 6 manda <b>reconstatarla</b> al devolver justamente por eso.
    /// </summary>
    [Fact]
    public void Un_vehiculo_que_vuelve_sin_rotulacion_queda_marcado()
    {
        Assert.True(Prestamo(
            devuelto: new DateOnly(2026, 5, 4), rotulacionAlVolver: false).VolvioSinRotulacion);

        Assert.False(Prestamo(
            devuelto: new DateOnly(2026, 5, 4), rotulacionAlVolver: true).VolvioSinRotulacion);
    }

    /// <summary>
    /// `RN-63` punto 5 — un rubro sin pactar es el que aparece cuando llega la multa. Va nombrado
    /// desde el principio, no supuesto.
    /// </summary>
    [Fact]
    public void Los_rubros_sin_pactar_van_nombrados()
    {
        var rubros = new RubrosPactados("Receptor", null, "Propietaria", null, null);

        Assert.Equal(["peajes", "multas", "daños"], rubros.SinPactar);
        Assert.Empty(new RubrosPactados("R", "R", "P", "R", "R").SinPactar);
    }

    // ── Andamio ─────────────────────────────────────────────────────────────

    private static ActoAutorizante Acto(string folio = "ACU-2026-31") =>
        new(folio, "Rolando Discua", new DateOnly(2026, 4, 2));

    private static ResponsableReceptor Receptor(
        string persona = "Ana Discua",
        string cargo = "Jefe de Transporte",
        string institucion = "Secretaría de Salud") =>
        new(persona, cargo, institucion, "Acta de recepción firmada el 06/04/2026");

    private static ExpedienteDePrestamo Prestamo(
        DateOnly? devuelto = null,
        int odometroDevolucion = 92_000,
        bool rotulacionAlVolver = true) =>
        new(Ulid.NewUlid(),
            Vehiculo,
            Acto(),
            "Rolando Discua",
            Receptor(),
            "Apoyo a jornada de vacunación",
            Desde,
            Comprometida,
            new ActaDeTenencia(Desde, 91_400, "P-TRANSPORTE", "3/4", "Llanta de repuesto, gato",
                "Matrícula y tarjeta de circulación", true, null),
            new RubrosPactados("Receptor", "Receptor", "Propietaria", "Receptor", "Receptor"),
            devuelto is null
                ? null
                : new ActaDeTenencia(devuelto.Value, odometroDevolucion, "P-TRANSPORTE", "1/2",
                    null, null, rotulacionAlVolver, null),
            devuelto is null ? null : "P-TRANSPORTE");
}
