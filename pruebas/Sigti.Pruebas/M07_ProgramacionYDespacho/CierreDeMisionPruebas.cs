using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M07_ProgramacionYDespacho;

/// <summary>
/// El cierre del expediente — `T-20`, `T-21` y `T-22`.
///
/// <b>El invariante que estas pruebas defienden</b> está en `orden-de-mision.md` §7.2:
/// «`T-22` está disponible <b>si y solo si</b> se cumple al menos un criterio. Y si se
/// cumple alguno, <b>`T-21` no está disponible</b>: quien cierra no elige entre cerrar
/// limpio o con hallazgo, <b>el criterio decide</b> y él lo confirma con su justificación.»
///
/// Es la diferencia entre un estado de seguimiento que significa algo y un cajón de
/// sastre. Si quien cierra pudiera elegir, en seis meses nadie cerraría con hallazgo y el
/// auditor dejaría de mirar ese estado — que es justo lo contrario de lo que se busca.
/// </summary>
public class CierreDeMisionPruebas
{
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

    private static readonly IdPersona Asistente = new("P-ASISTENTE");
    private static readonly IdPersona Jefatura = new("P-JEFATURA");
    private static readonly IdPersona Transporte = new("P-TRANSPORTE");
    private static readonly IdPersona Gerencia = new("P-GERENCIA");

    private static readonly DatosDeLaSolicitud Solicitud = new(
        Dependencia: "Delegación de Choluteca",
        ObjetoDelTraslado: "Traslado de personal y equipo",
        Destino: "Choluteca",
        Ventana: Asignacion.Ventana);

    /// <summary>Un expediente llevado hasta `LIQUIDADA`, que es donde empieza el cierre.</summary>
    private static OrdenDeMision Liquidada()
    {
        var expediente = OrdenDeMision.Crear(Ulid.NewUlid(), Asistente, Asistente, Solicitud, Momento);
        expediente.Enviar(Asistente, Momento);
        expediente.Aprobar(Jefatura, Momento, motivo: null);
        expediente.Programar(Transporte, Asignacion.Valida(),
            Asignacion.Matriz, PoliticaDeDocumentacion.PorDefecto, Momento);
        expediente.Despachar(new IdPersona("P-DESPACHO"), Asignacion.Valida(),
            Asignacion.Matriz, PoliticaDeDocumentacion.PorDefecto, Momento, Asignacion.Custodiado, Asignacion.SinDiasInhabiles());
        expediente.IniciarRuta(new IdPersona("P-MOTORISTA"), Momento, Asignacion.Sale);
        expediente.Retornar(new IdPersona("P-MOTORISTA"), Momento, Asignacion.Vuelve);
        expediente.Liquidar(Transporte, Momento);
        return expediente;
    }

    [Fact]
    public void Con_un_criterio_presente_el_expediente_cierra_con_hallazgo_aunque_nadie_lo_pida()
    {
        // El corazón de §7.2, y la razón de que `Cerrar` no reciba el estado destino:
        // **quien cierra no puede elegir**. Si pudiera, en seis meses nadie cerraría con
        // hallazgo y el auditor dejaría de mirar ese estado.
        //
        // El invariante es estructural, no comprobado: no hay forma de pedir el destino
        // equivocado porque no hay forma de pedir el destino.
        var expediente = Liquidada();

        expediente.Cerrar(
            Gerencia, Momento,
            criterios: [new HallazgoDetectado("H-11", "Diferencia de caja de L 400 sin explicar")],
            justificacion: "Diferencia bajo revisión de Auditoría Interna");

        Assert.Equal(EstadoDeMision.CerradaConHallazgo, expediente.Estado);
    }

    [Fact]
    public void Sin_ningun_criterio_el_expediente_cierra_limpio()
    {
        var expediente = Liquidada();

        expediente.Cerrar(Gerencia, Momento, criterios: [], justificacion: null);

        Assert.Equal(EstadoDeMision.Cerrada, expediente.Estado);
    }

    [Fact]
    public void Quien_liquido_no_puede_cerrar()
    {
        // `BD-06` en `T-21` y `T-22`: **quien cierra ≠ quien liquidó**. Es el último par de
        // la cadena de segregación, y el más fácil de saltarse en una delegación pequeña —
        // donde la misma persona elaboró el descargo y tiene a mano el botón de cerrar.
        var expediente = Liquidada();

        var bloqueo = Assert.Throws<BloqueoDuro>(() =>
            expediente.Cerrar(Transporte, Momento, criterios: [], justificacion: null));

        Assert.Equal("BD-06", bloqueo.Precondicion);
    }

    [Fact]
    public void Un_cierre_con_hallazgo_exige_justificacion()
    {
        // §7.2: «el criterio decide y **él lo confirma con su justificación**». Sin ella el
        // expediente diría qué se detectó y no diría qué se resolvió, que es lo que el
        // control interno tiene que seguir.
        var expediente = Liquidada();

        var bloqueo = Assert.Throws<BloqueoDuro>(() => expediente.Cerrar(
            Gerencia, Momento,
            criterios: [new HallazgoDetectado("H-11", "Diferencia de caja de L 400 sin explicar")],
            justificacion: null));

        Assert.Equal("T-22", bloqueo.Precondicion);
    }

    [Fact]
    public void La_liquidacion_se_puede_devolver_para_rehacerla()
    {
        // `T-20` — `LIQUIDADA` → `RETORNADA`. Existe porque el descargo conciliado se puede
        // haber elaborado mal, y **la alternativa a devolverlo es cerrarlo mal**.
        var expediente = Liquidada();

        expediente.DevolverLiquidacion(Gerencia, Momento, "Faltan los tickets de peaje del retorno");

        Assert.Equal(EstadoDeMision.Retornada, expediente.Estado);
    }
}
