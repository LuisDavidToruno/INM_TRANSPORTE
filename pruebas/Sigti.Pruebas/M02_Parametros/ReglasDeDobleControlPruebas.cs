using Sigti.Dominio.M02_Parametros;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M02_Parametros;

/// <summary>
/// `HU-146` — Impedir que quien carga un parámetro lo apruebe, <b>y registrar cada
/// intento</b>.
///
/// Las dos mitades importan. Bloquear sin registrar deja al auditor sin saber que
/// alguien lo intentó, y ese intento es justamente lo que un control interno quiere ver.
/// </summary>
public class ReglasDeDobleControlPruebas
{
    private static readonly IdPersona Carlos = new("P-CARLOS");
    private static readonly IdPersona Gerencia = new("P-GERENCIA");

    private static readonly RespaldoDocumental Respaldo = new(
        Adjunto: Ulid.NewUlid(),
        Fuente: "Fuente de prueba",
        FechaDeVerificacion: new DateOnly(2026, 1, 1));

    private static readonly DateTimeOffset Momento =
        new(2026, 9, 18, 11, 0, 0, TimeSpan.FromHours(-6));

    [Fact]
    public void Quien_carga_no_puede_aprobar_su_propia_carga_y_el_intento_queda_registrado()
    {
        var pendiente = Pendiente();

        var intento = ReglasDeDobleControl.Evaluar(pendiente, quienAprueba: Carlos, Momento);

        Assert.False(intento.Concedida);
        Assert.Equal(Carlos, intento.Quien);
        Assert.Equal(Momento, intento.Momento);
        Assert.NotNull(intento.MotivoDelRechazo);
    }

    [Fact]
    public void Otra_persona_si_puede_aprobar_y_queda_como_aprobadora()
    {
        var pendiente = Pendiente();

        var intento = ReglasDeDobleControl.Evaluar(pendiente, quienAprueba: Gerencia, Momento);
        var aprobada = ReglasDeDobleControl.Aplicar(pendiente, intento);

        Assert.True(intento.Concedida);
        Assert.NotNull(aprobada);
        Assert.Equal(Gerencia, aprobada.AprobadoPor);
        Assert.Equal(Carlos, aprobada.CargadoPor);
    }

    [Fact]
    public void Una_version_ya_aprobada_no_se_vuelve_a_aprobar()
    {
        // Una segunda aprobación no agrega control: lo simula. Y dejaría dos registros
        // de aprobación sobre el mismo hecho, que es peor que ninguno.
        var aprobada = Pendiente() with { AprobadoPor = Gerencia };

        var intento = ReglasDeDobleControl.Evaluar(aprobada, quienAprueba: new IdPersona("P-OTRA"), Momento);

        Assert.False(intento.Concedida);
        Assert.Null(ReglasDeDobleControl.Aplicar(aprobada, intento));
    }

    [Fact]
    public void El_intento_rechazado_no_modifica_la_version()
    {
        var pendiente = Pendiente();

        var intento = ReglasDeDobleControl.Evaluar(pendiente, quienAprueba: Carlos, Momento);

        Assert.Null(ReglasDeDobleControl.Aplicar(pendiente, intento));
        Assert.Null(pendiente.AprobadoPor);
    }

    private static VersionDeParametro Pendiente() => new(
        Clave: "umbral_desviacion_consumo",
        Valor: "25",
        VigenteDesde: new DateOnly(2026, 10, 1),
        VigenteHasta: null,
        RegistradoDesde: Momento,
        RegistradoHasta: null,
        CargadoPor: Carlos,
        AprobadoPor: null)
    { Respaldo = Respaldo };
}
