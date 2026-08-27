using Sigti.Aplicacion.M01_Organizacion;
using Sigti.Datos;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.Organizacion;
using Sigti.Pruebas.Datos;

namespace Sigti.Pruebas.M01_Organizacion;

/// <summary>
/// El organigrama persistido — <b>y es un espejo, no un maestro</b>.
///
/// ── La restricción que decide la forma de esta tabla ─────────────────────────
/// `DP-001`: la estructura de puestos es <b>propiedad de ARGOS y Talento Humano</b>.
/// `RN-48`: los datos cuyo dueño es otro sistema se almacenan como <b>espejo marcado como
/// tal</b>, y <b>ninguna pantalla ni operación de SIGTI debe permitir editarlos</b>.
///
/// Por eso esta tabla lleva `ConfirmadoAl`, que un maestro no necesitaría: un espejo
/// **envejece**, y hay que poder decir desde cuándo no se confirma. Es el dato que
/// `HU-009` muestra en la bandeja de autorización y el que `RN-50` usa para degradar.
///
/// ── Y por qué la antigüedad advierte en vez de bloquear ──────────────────────
/// La máquina de estados —autoridad— resuelve `T-05` como **advertencia registrada**, no
/// como bloqueo. `RN-50` decía lo contrario y se corrigió con `HB1-10`. Bloquear la
/// autorización porque un espejo lleva días sin confirmarse paralizaría la institución
/// por un problema de integración, que es exactamente el fallo que no se quiere.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class EspejoDelOrganigramaPruebas(BaseDePruebas baseDePruebas)
{
    private static readonly IdPersona Jefa = new("P-JEFA-CHOLUTECA");
    private static readonly IdPuesto Jefatura = new("PU-JEFATURA-CHOLUTECA");

    [Fact]
    public async Task El_organigrama_se_arma_desde_la_base_y_responde_a_la_fecha_del_hecho()
    {
        await using var contexto = baseDePruebas.Contexto();
        await SembrarAsync(contexto);

        var consulta = new ConsultaDelOrganigrama(contexto);
        var organigrama = await consulta.VigenteAsync();

        // Ocupó el puesto hasta el 28 de febrero.
        Assert.True(organigrama.Ocupa(Jefa, Jefatura, new DateOnly(2026, 2, 10)));
        Assert.False(organigrama.Ocupa(Jefa, Jefatura, new DateOnly(2026, 3, 10)));
    }

    [Fact]
    public async Task El_espejo_dice_desde_cuando_no_se_confirma()
    {
        // Es el dato que `HU-009` muestra en la bandeja: **una jefatura que autoriza sobre
        // un organigrama de hace nueve días tiene derecho a saberlo antes de firmar**, no
        // después. Sin esto, la advertencia de la pantalla es un texto fijo que no mide
        // nada — que es exactamente lo que hay hoy.
        await using var contexto = baseDePruebas.Contexto();
        await SembrarAsync(contexto);

        var consulta = new ConsultaDelOrganigrama(contexto);
        var antiguedad = await consulta.AntiguedadDelEspejoAsync(
            ahora: new DateTimeOffset(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6)));

        Assert.NotNull(antiguedad);
        Assert.Equal(9, antiguedad!.Value.Days);
    }

    [Fact]
    public async Task Un_espejo_que_nunca_se_confirmo_lo_dice_en_vez_de_devolver_cero()
    {
        // Cero días de antigüedad y «nunca se sincronizó» son cosas **opuestas**, y
        // confundirlas haría que una integración que jamás corrió se muestre como recién
        // confirmada. Es la peor forma de fallar: en silencio y con buena cara.
        await using var contexto = baseDePruebas.Contexto();

        // Sin sembrar nada para ese puesto.
        var consulta = new ConsultaDelOrganigrama(contexto);

        var antiguedad = await consulta.AntiguedadDelEspejoAsync(
            ahora: new DateTimeOffset(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6)),
            soloPuesto: new IdPuesto("PU-QUE-NO-EXISTE"));

        Assert.Null(antiguedad);
    }

    private static async Task SembrarAsync(SigtiDbContext contexto)
    {
        if (contexto.AsignacionesDePuesto.Any(a => a.Puesto == Jefatura.Valor)) return;

        contexto.AsignacionesDePuesto.Add(new FilaDeAsignacionDePuesto
        {
            Id = Ulid.NewUlid(),
            Persona = Jefa.Valor,
            Puesto = Jefatura.Valor,
            Desde = new DateOnly(2025, 1, 15),
            Hasta = new DateOnly(2026, 2, 28),
            // Nueve días antes del momento de la prueba.
            ConfirmadoAlUtc = new DateTime(2026, 3, 3, 15, 0, 0, DateTimeKind.Utc),
        });

        await contexto.SaveChangesAsync();
    }
}
