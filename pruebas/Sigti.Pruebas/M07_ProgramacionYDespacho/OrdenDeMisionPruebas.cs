using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M07_ProgramacionYDespacho;

/// <summary>
/// La máquina de estados del expediente de misión. La autoridad sobre transiciones,
/// precondiciones e invariantes es docs/03-arquitectura/estados/orden-de-mision.md;
/// estas pruebas la ejercen, no la redefinen.
/// </summary>
public class OrdenDeMisionPruebas
{
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

    private static readonly IdPersona Asistente = new("P-ASISTENTE");
    private static readonly IdPersona Jefe = new("P-JEFE");
    private static readonly IdPersona Jefatura = new("P-JEFATURA");
    private static readonly IdPersona Transporte = new("P-TRANSPORTE");
    private static readonly IdPersona Encargado = new("P-ENCARGADO");
    private static readonly IdPersona Motorista = new("P-MOTORISTA");

    [Fact]
    public void Un_expediente_recien_creado_esta_en_borrador()
    {
        var expediente = OrdenDeMision.Crear(
            capturadaPor: Asistente,
            solicitanteDeDerecho: Jefe,
            momento: Momento);

        Assert.Equal(EstadoDeMision.Borrador, expediente.Estado);
    }

    [Fact]
    public void El_solicitante_de_derecho_no_puede_autorizar_su_propia_solicitud()
    {
        // El caso cotidiano que BD-01 no cubría antes del hallazgo HB3-01: la asistente
        // captura la solicitud para su jefe. Formalmente el jefe no creó ni envió nada
        // — pero es el solicitante, y la incompatibilidad I-01 sí se está violando.
        var expediente = OrdenDeMision.Crear(Asistente, Jefe, Momento);
        expediente.Enviar(Asistente, Momento);

        var bloqueo = Assert.Throws<BloqueoDuro>(() => expediente.Aprobar(Jefe, Momento));

        Assert.Equal("BD-01", bloqueo.Precondicion);
        Assert.Equal(EstadoDeMision.Solicitada, expediente.Estado);
    }

    [Fact]
    public void No_se_puede_despachar_una_mision_que_no_fue_programada()
    {
        // §3.4: APROBADA → DESPACHADA no existe. Sin programación no hay verificación de
        // licencia, documentación ni reserva.
        var expediente = Aprobada();

        var fallo = Assert.Throws<TransicionInvalida>(
            () => expediente.Despachar(Encargado, Momento));

        Assert.Equal(EstadoDeMision.Programada, fallo.EstadoRequerido);
        Assert.Equal(EstadoDeMision.Aprobada, expediente.Estado);
    }

    [Fact]
    public void El_estado_se_reconstruye_desde_el_diario()
    {
        // P-1: dos dispositivos no negocian «el estado», intercambian transiciones. Si el
        // estado no se puede reconstruir del diario, la sincronización desconectada no
        // tiene solución.
        var original = HiloCompleto();

        var reconstruido = OrdenDeMision.Reconstruir(
            original.CapturadaPor,
            original.SolicitanteDeDerecho,
            original.Diario);

        Assert.Equal(EstadoDeMision.Liquidada, original.Estado);
        Assert.Equal(original.Estado, reconstruido.Estado);
        Assert.Equal(original.Diario, reconstruido.Diario);
    }

    /// <summary>El hilo completo: solicitud → despacho → ejecución → liquidación.</summary>
    private static OrdenDeMision HiloCompleto()
    {
        var expediente = Aprobada();
        expediente.Programar(Transporte, Momento);
        expediente.Despachar(Encargado, Momento);
        expediente.IniciarRuta(Motorista, Momento);
        expediente.Retornar(Motorista, Momento);
        expediente.Liquidar(Transporte, Momento);
        return expediente;
    }

    /// <summary>Un expediente aprobado por la jefatura, que no es ni capturador ni solicitante.</summary>
    private static OrdenDeMision Aprobada()
    {
        var expediente = OrdenDeMision.Crear(Asistente, Jefe, Momento);
        expediente.Enviar(Asistente, Momento);
        expediente.Aprobar(Jefatura, Momento);
        return expediente;
    }
}
