using Sigti.Dominio.M14_Auditoria;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M14_Auditoria;

/// <summary>
/// El rastro del expediente <b>con sus huecos visibles</b> — `PT-089`.
///
/// ── Lo que estas pruebas defienden ──────────────────────────────────────────
/// Que <i>«falta»</i>, <i>«no correspondía»</i> y <i>«todavía no toca»</i> <b>no se confundan</b>.
/// Las tres se ven igual en una casilla vacía, y juntarlas produce los dos daños a la vez:
/// alarma sobre lo que está bien —y una pista con alarmas falsas se deja de mirar— y silencio
/// sobre lo que está mal.
/// </summary>
public class ReglasDeLaCadenaPruebas
{
    private static readonly IdPersona Karla = new("P-KARLA");
    private static readonly DateOnly Fecha = new(2026, 8, 3);

    /// <summary>
    /// Presente: hay asiento, con su autor y su fecha. <b>Sin motivo</b>, que ahí sería ruido.
    /// </summary>
    [Fact]
    public void Un_eslabon_con_asiento_esta_presente()
    {
        var e = ReglasDeLaCadena.Resolver(
            Eslabon.Autorizacion, corresponde: true, alcanzado: true,
            "T-05 del 03/08/2026", Karla, Fecha);

        Assert.Equal(EstadoDelEslabon.Presente, e.Estado);
        Assert.Equal(Karla, e.Quien);
        Assert.Null(e.PorQue);
    }

    /// <summary>
    /// <b>El hallazgo.</b> Correspondía, el expediente pasó por la etapa, y no hay asiento: la
    /// cadena se cortó ahí. Es lo que el TSC busca, y lo que un rastro que sólo muestra lo
    /// presente nunca enseña.
    /// </summary>
    [Fact]
    public void Lo_que_correspondia_y_no_esta_es_un_hueco()
    {
        var e = ReglasDeLaCadena.Resolver(
            Eslabon.Bitacora, corresponde: true, alcanzado: true, null, null, null);

        Assert.Equal(EstadoDelEslabon.Ausente, e.Estado);
        Assert.Contains("La cadena se cortó", e.PorQue);

        // Sin autor inventado: llenar un reporte de auditoría con un responsable que no actuó
        // es peor que dejarlo vacío.
        Assert.Null(e.Quien);
    }

    /// <summary>
    /// <b>«No correspondía» NO es un hueco.</b> Una misión sin fondo asignado no tiene vale, y
    /// pintarlo como faltante llenaría la pista de alarmas falsas.
    /// </summary>
    [Fact]
    public void Lo_que_no_correspondia_no_es_un_hueco()
    {
        var e = ReglasDeLaCadena.Resolver(
            Eslabon.Vale, corresponde: false, alcanzado: true, null, null, null,
            "La misión no llevó fondo de combustible asignado.");

        Assert.Equal(EstadoDelEslabon.NoAplica, e.Estado);
        Assert.Contains("no llevó fondo", e.PorQue);
    }

    /// <summary>
    /// <b>«Todavía no toca» tampoco.</b> Una misión programada no tiene liquidación, y llamarlo
    /// hueco diría que algo se perdió cuando lo que pasa es que la misión sigue su curso.
    /// </summary>
    [Fact]
    public void Lo_que_todavia_no_toca_no_es_un_hueco()
    {
        var e = ReglasDeLaCadena.Resolver(
            Eslabon.Liquidacion, corresponde: true, alcanzado: false, null, null, null);

        Assert.Equal(EstadoDelEslabon.Pendiente, e.Estado);
        Assert.Contains("todavía no llegó", e.PorQue);
    }

    /// <summary>
    /// Los cuatro estados <b>se distinguen entre sí</b>. Sin esta prueba, una implementación que
    /// devolviera siempre `Ausente` para todo lo que no está pasaría tres de las anteriores.
    /// </summary>
    [Fact]
    public void Los_cuatro_estados_son_distintos_entre_si()
    {
        var estados = new[]
        {
            ReglasDeLaCadena.Resolver(Eslabon.Solicitud, true, true, "SOL-1", Karla, Fecha).Estado,
            ReglasDeLaCadena.Resolver(Eslabon.Bitacora, true, true, null, null, null).Estado,
            ReglasDeLaCadena.Resolver(Eslabon.Vale, false, true, null, null, null).Estado,
            ReglasDeLaCadena.Resolver(Eslabon.Liquidacion, true, false, null, null, null).Estado,
        };

        Assert.Equal(4, estados.Distinct().Count());
    }

    // ── La cadena entera ────────────────────────────────────────────────────

    /// <summary>
    /// <b>Los huecos son sólo los ausentes.</b> Contar también lo pendiente o lo que no aplica
    /// convertiría toda misión en curso en un hallazgo.
    /// </summary>
    [Fact]
    public void Los_huecos_no_incluyen_lo_pendiente_ni_lo_que_no_aplica()
    {
        var cadena = new CadenaDelExpediente("MIS-1", "PROV-000001",
        [
            ReglasDeLaCadena.Resolver(Eslabon.Solicitud, true, true, "SOL-1", Karla, Fecha),
            ReglasDeLaCadena.Resolver(Eslabon.Bitacora, true, true, null, null, null),
            ReglasDeLaCadena.Resolver(Eslabon.Vale, false, true, null, null, null),
            ReglasDeLaCadena.Resolver(Eslabon.Liquidacion, true, false, null, null, null),
        ]);

        var hueco = Assert.Single(cadena.Huecos);

        Assert.Equal(Eslabon.Bitacora, hueco.Eslabon);
        Assert.Equal(1, cadena.NoAplican);
    }

    /// <summary>
    /// <b>Una misión en curso no tiene huecos y tampoco está completa.</b>
    ///
    /// Se exigen las dos cosas a propósito: dar por completa una cadena con eslabones pendientes
    /// cerraría un expediente vivo en el reporte de auditoría.
    /// </summary>
    [Fact]
    public void Sin_huecos_pero_con_pendientes_la_cadena_no_esta_completa()
    {
        var enCurso = new CadenaDelExpediente("MIS-1", "PROV-000001",
        [
            ReglasDeLaCadena.Resolver(Eslabon.Solicitud, true, true, "SOL-1", Karla, Fecha),
            ReglasDeLaCadena.Resolver(Eslabon.Liquidacion, true, false, null, null, null),
        ]);

        Assert.Empty(enCurso.Huecos);
        Assert.False(enCurso.Completa);
    }

    /// <summary>
    /// Y el recíproco: presente más «no aplica» <b>sí</b> es una cadena completa. Sin él, una
    /// regla que nunca diera por completa nada pasaría la prueba anterior.
    /// </summary>
    [Fact]
    public void Presente_mas_no_aplica_es_una_cadena_completa()
    {
        var cerrada = new CadenaDelExpediente("MIS-1", "PROV-000001",
        [
            ReglasDeLaCadena.Resolver(Eslabon.Solicitud, true, true, "SOL-1", Karla, Fecha),
            ReglasDeLaCadena.Resolver(Eslabon.Vale, false, true, null, null, null),
        ]);

        Assert.True(cerrada.Completa);
    }

    /// <summary>
    /// El orden del enum <b>es el de la cadena</b>, no alfabético: un hueco en el medio se ve
    /// porque los de después están y el de antes también.
    /// </summary>
    [Fact]
    public void El_orden_del_enum_es_el_de_la_cadena()
    {
        Assert.Equal(
            [
                Eslabon.Solicitud, Eslabon.Autorizacion, Eslabon.OrdenDeMision, Eslabon.Bitacora,
                Eslabon.Vale, Eslabon.Comprobante, Eslabon.Liquidacion,
            ],
            Enum.GetValues<Eslabon>());
    }
}
