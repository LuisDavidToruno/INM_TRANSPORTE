using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M17_PersonasExternas;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M17_PersonasExternas;

/// <summary>
/// `RN-52` — el registro de consultas a manifiestos.
///
/// <i>«Si una persona pregunta quién accedió a sus datos, la única respuesta defendible es el
/// registro de consultas. Sin él, la institución no puede afirmar nada.»</i>
/// </summary>
public class ReglasDeLaConsultaPruebas
{
    private static readonly DateTimeOffset Lunes = new(2026, 9, 7, 8, 0, 0, TimeSpan.Zero);

    private static ConsultaRegistrada Consulta(
        string quien = "P-DESPACHO",
        string registro = "OM-2026-0451",
        AlcanceDeLaConsulta alcance = AlcanceDeLaConsulta.ListaDeNombres,
        string? necesidad = "despacho de la misión",
        int horas = 0) =>
        new(Ulid.NewUlid(), new IdPersona(quien), "EncargadoDeDespacho",
            Lunes.AddHours(horas), registro, alcance, necesidad, "10.0.0.5");

    // ── La necesidad de conocer ─────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ver")]
    public void Ver_datos_personales_exige_decir_para_que(string? necesidad)
    {
        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaConsulta.ExigirNecesidadDeConocer(
                AlcanceDeLaConsulta.ListaDeNombres, necesidad));

        // El mensaje dice a quién le puede llegar. Escribirlo también hace pensar: quien no
        // puede completar la frase suele no necesitar el dato.
        Assert.Contains("hábeas data", e.Message);
    }

    [Fact]
    public void El_recuento_no_exige_justificacion()
    {
        // Cuántas personas van es dato de gestión, sin nada personal. Pedir justificación para
        // verlo convertiría el control en un trámite que la gente aprende a saltarse
        // escribiendo cualquier cosa — y ahí el registro entero deja de valer.
        ReglasDeLaConsulta.ExigirNecesidadDeConocer(AlcanceDeLaConsulta.SoloRecuento, null);
    }

    [Fact]
    public void Con_necesidad_declarada_pasa()
    {
        ReglasDeLaConsulta.ExigirNecesidadDeConocer(
            AlcanceDeLaConsulta.ManifiestoCompleto, "despacho de la misión OM-2026-0451");
    }

    [Fact]
    public void El_registro_guarda_QUE_se_mostro_y_no_solo_que_se_abrio()
    {
        // Ver una lista de nombres y ver el manifiesto completo son dos accesos distintos al
        // mismo registro, y el titular tiene derecho a saber cuál de los dos ocurrió.
        var lista = Consulta(alcance: AlcanceDeLaConsulta.ListaDeNombres);
        var completo = Consulta(alcance: AlcanceDeLaConsulta.ManifiestoCompleto);

        Assert.NotEqual(lista.Alcance, completo.Alcance);
    }

    // ── El patrón de acceso ─────────────────────────────────────────────────

    [Fact]
    public void Quien_supera_el_umbral_queda_marcado_para_que_alguien_pregunte()
    {
        var consultas = Enumerable.Range(0, 25)
            .Select(i => Consulta(registro: $"OM-{i}", horas: i))
            .ToList();

        var patrones = ReglasDeLaConsulta.Patrones(consultas, Lunes.AddDays(-1), umbral: 20);

        var p = Assert.Single(patrones);
        Assert.True(p.Marcado);
        Assert.Equal(25, p.Consultas);
        Assert.Equal(25, p.RegistrosDistintos);
    }

    [Fact]
    public void Marcado_no_significa_que_hizo_algo_malo()
    {
        // Un despachador que abre veinte manifiestos un lunes está trabajando. El reporte no
        // acusa: pone el número delante de alguien para que pregunte. Un reporte que acusa se
        // deja de leer tan rápido como uno que calla.
        var pocos = Enumerable.Range(0, 5)
            .Select(i => Consulta(registro: $"OM-{i}", horas: i)).ToList();

        Assert.False(Assert.Single(
            ReglasDeLaConsulta.Patrones(pocos, Lunes.AddDays(-1), umbral: 20)).Marcado);
    }

    [Fact]
    public void Las_consultas_sin_necesidad_declarada_se_cuentan_aparte()
    {
        // Es la cifra que dice **cuánto del registro es inauditable**: quedó el rastro de quién
        // miró y ninguna forma de juzgar si debía.
        var consultas = new[]
        {
            Consulta(necesidad: "despacho de la misión"),
            Consulta(registro: "OM-2", necesidad: null),
            Consulta(registro: "OM-3", necesidad: "  "),
        };

        var p = Assert.Single(
            ReglasDeLaConsulta.Patrones(consultas, Lunes.AddDays(-1), umbral: 20));

        Assert.Equal(2, p.SinNecesidadDeclarada);
    }

    [Fact]
    public void El_recuento_no_entra_en_el_patron()
    {
        // No lleva datos personales: contarlo inflaría el número de quien sólo mira cuántos
        // pasajeros van, y taparía al que sí abre manifiestos.
        var consultas = new[]
        {
            Consulta(alcance: AlcanceDeLaConsulta.SoloRecuento, necesidad: null),
            Consulta(registro: "OM-2", alcance: AlcanceDeLaConsulta.ListaDeNombres),
        };

        Assert.Equal(1, Assert.Single(
            ReglasDeLaConsulta.Patrones(consultas, Lunes.AddDays(-1), umbral: 20)).Consultas);
    }

    [Fact]
    public void Lo_anterior_al_corte_no_se_cuenta()
    {
        var consultas = new[]
        {
            Consulta(horas: -100),
            Consulta(registro: "OM-2", horas: 1),
        };

        Assert.Equal(1, Assert.Single(
            ReglasDeLaConsulta.Patrones(consultas, Lunes, umbral: 20)).Consultas);
    }

    [Fact]
    public void Cada_consultante_se_cuenta_por_separado()
    {
        var consultas = new[]
        {
            Consulta("P-DESPACHO"),
            Consulta("P-DESPACHO", registro: "OM-2"),
            Consulta("P-AUDITORIA", registro: "OM-3"),
        };

        var patrones = ReglasDeLaConsulta.Patrones(consultas, Lunes.AddDays(-1), umbral: 20);

        Assert.Equal(2, patrones.Count);
        Assert.Equal(2, patrones[0].Consultas);
    }

    [Fact]
    public void Sin_consultas_no_hay_patrones()
    {
        Assert.Empty(ReglasDeLaConsulta.Patrones([], Lunes, umbral: 20));
    }
}
