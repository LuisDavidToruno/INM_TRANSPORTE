using Sigti.Dominio.M15_Formatos;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M15_Formatos;

/// <summary>
/// `HU-017` y `RN-25` — el salvoconducto impreso.
///
/// ── Lo que se prueba acá ────────────────────────────────────────────────────
/// El documento es lo que dice el papel. Estas pruebas cuidan que <b>el papel y el sistema no
/// puedan divergir en silencio</b>: la huella distingue contenidos, el código corto se puede
/// dictar por teléfono, y el estado dice «desactualizado» cuando la misión cambió debajo de una
/// impresión que ya está en la mano de alguien.
/// </summary>
public class ReglasDelSalvoconductoPruebas
{
    private static readonly IdPersona Doris = new("P-DORIS");

    private static readonly DateTimeOffset Firma =
        new(2026, 9, 3, 16, 40, 0, TimeSpan.FromHours(-6));

    // ── Emitir ──────────────────────────────────────────────────────────────

    [Fact]
    public void Con_permiso_firmado_y_sin_emitir_antes_se_emite()
    {
        Assert.Null(ReglasDelSalvoconducto.PorQueNoSeEmite(permisoFirmado: true, yaEmitido: false));
    }

    /// <summary>
    /// El salvoconducto <b>no autoriza, materializa</b>. Emitirlo sobre un trámite sin firma
    /// produciría un papel con aspecto oficial que no ampara nada, y el motorista lo llevaría
    /// creyendo que sí — que es peor que no llevar ninguno.
    /// </summary>
    [Fact]
    public void Sin_permiso_firmado_no_hay_documento_que_emitir()
    {
        var porQue = ReglasDelSalvoconducto.PorQueNoSeEmite(permisoFirmado: false, yaEmitido: false);

        Assert.NotNull(porQue);
        Assert.Contains("materializa", porQue);
    }

    /// <summary>`RN-04` — dos folios para un mismo permiso rompen la conciliación.</summary>
    [Fact]
    public void Un_permiso_ya_emitido_se_reimprime_en_vez_de_emitirse_otra_vez()
    {
        var porQue = ReglasDelSalvoconducto.PorQueNoSeEmite(permisoFirmado: true, yaEmitido: true);

        Assert.NotNull(porQue);

        // Y dice la salida: un bloqueo que no dice qué hacer deja a alguien sin su papel.
        Assert.Contains("reimprima", porQue);
    }

    // ── La huella ───────────────────────────────────────────────────────────

    [Fact]
    public void El_mismo_contenido_produce_la_misma_huella()
    {
        Assert.Equal(
            ReglasDelSalvoconducto.Huella(Contenido()),
            ReglasDelSalvoconducto.Huella(Contenido()));
    }

    [Fact]
    public void Cambiar_el_motorista_cambia_la_huella()
    {
        var otro = Contenido() with { Motorista = "Marlon Pineda" };

        Assert.NotEqual(
            ReglasDelSalvoconducto.Huella(Contenido()),
            ReglasDelSalvoconducto.Huella(otro));
    }

    /// <summary>
    /// ⚠️ <b>El separador tiene que ser no imprimible.</b>
    ///
    /// Con uno tecleable, dos contenidos distintos producen la misma cadena canónica y la
    /// huella deja de distinguir dos documentos que no son el mismo. Acá se mueve el corte
    /// entre dos campos: si el separador fuera <c>|</c>, las dos huellas coincidirían.
    /// </summary>
    [Fact]
    public void Mover_el_corte_entre_dos_campos_no_produce_la_misma_huella()
    {
        var uno = Contenido() with { Destino = "Choluteca|Sur", Justificacion = "Operativo" };
        var otro = Contenido() with { Destino = "Choluteca", Justificacion = "Sur|Operativo" };

        Assert.NotEqual(
            ReglasDelSalvoconducto.Huella(uno),
            ReglasDelSalvoconducto.Huella(otro));
    }

    // ── El código corto ─────────────────────────────────────────────────────

    /// <summary>
    /// Se dicta por teléfono desde una carretera sin señal. Las letras que se confunden al
    /// dictar —<c>I</c> con <c>1</c>, <c>O</c> con <c>0</c>— no pueden aparecer.
    /// </summary>
    [Fact]
    public void El_codigo_corto_no_usa_caracteres_que_se_confunden_al_dictar()
    {
        var codigo = ReglasDelSalvoconducto.CodigoCorto(ReglasDelSalvoconducto.Huella(Contenido()));

        Assert.DoesNotContain('I', codigo);
        Assert.DoesNotContain('O', codigo);
        Assert.DoesNotContain('0', codigo);
        Assert.DoesNotContain('1', codigo);
    }

    [Fact]
    public void El_codigo_corto_se_puede_copiar_a_mano()
    {
        var codigo = ReglasDelSalvoconducto.CodigoCorto(ReglasDelSalvoconducto.Huella(Contenido()));

        // Ocho caracteres y un guion en medio: una huella de 64 hexadecimales no se copia
        // a mano sin equivocarse.
        Assert.Equal(9, codigo.Length);
        Assert.Equal('-', codigo[4]);
    }

    [Fact]
    public void Dos_documentos_distintos_dan_codigos_distintos()
    {
        var uno = ReglasDelSalvoconducto.CodigoCorto(ReglasDelSalvoconducto.Huella(Contenido()));

        var otro = ReglasDelSalvoconducto.CodigoCorto(
            ReglasDelSalvoconducto.Huella(Contenido() with { Motorista = "Marlon Pineda" }));

        Assert.NotEqual(uno, otro);
    }

    // ── El estado, que es lo que responde el punto de verificación ──────────

    [Fact]
    public void Si_nada_cambio_el_documento_esta_vigente()
    {
        var estado = ReglasDelSalvoconducto.Estado(Contenido(), Contenido(), anulado: false);

        Assert.Equal(EstadoDelSalvoconducto.Vigente, estado);
        Assert.Contains("Compare los cuatro", ReglasDelSalvoconducto.Veredicto(estado));
    }

    /// <summary>
    /// ⚠️ <b>El caso que «vigente o anulado» no sabe contestar.</b>
    ///
    /// El documento se imprime antes de salir —una delegación sin cobertura lo emite por
    /// anticipado— y después hay un relevo de motorista. <b>Nadie anuló nada</b>, y el papel
    /// que el motorista lleva en la mano ya no corresponde.
    ///
    /// Un verificador que lea «vigente» sobre ese papel recibe una respuesta correcta a la
    /// pregunta equivocada.
    /// </summary>
    [Fact]
    public void Un_relevo_de_motorista_desactualiza_el_papel_sin_anularlo()
    {
        var ahora = Contenido() with { Motorista = "Marlon Pineda" };

        var estado = ReglasDelSalvoconducto.Estado(Contenido(), ahora, anulado: false);

        Assert.Equal(EstadoDelSalvoconducto.Desactualizado, estado);

        // Y el veredicto distingue las dos cosas, porque en carretera significan distinto.
        var veredicto = ReglasDelSalvoconducto.Veredicto(estado);
        Assert.Contains("YA NO CORRESPONDE", veredicto);
        Assert.DoesNotContain("anulado", veredicto);
    }

    [Fact]
    public void Una_ventana_corrida_tambien_desactualiza()
    {
        var ahora = Contenido() with { Hasta = new DateOnly(2026, 9, 8) };

        Assert.Equal(
            EstadoDelSalvoconducto.Desactualizado,
            ReglasDelSalvoconducto.Estado(Contenido(), ahora, anulado: false));
    }

    /// <summary>
    /// Si la misión se desprogramó no hay contra qué comparar, <b>y eso desactualiza</b>. Dar
    /// por vigente lo que no se pudo verificar es la falla que este estado existe para evitar.
    /// </summary>
    [Fact]
    public void Sin_asignacion_actual_el_papel_queda_desactualizado()
    {
        Assert.Equal(
            EstadoDelSalvoconducto.Desactualizado,
            ReglasDelSalvoconducto.Estado(Contenido(), ahora: null, anulado: false));
    }

    /// <summary>
    /// Corregir la justificación no toca lo amparado. Si lo desactualizara, una corrección de
    /// redacción invalidaría un papel que sigue describiendo el mismo viaje.
    /// </summary>
    [Fact]
    public void Corregir_la_justificacion_no_desactualiza_el_papel()
    {
        var ahora = Contenido() with { Justificacion = "Operativo migratorio, redacción corregida." };

        Assert.Equal(
            EstadoDelSalvoconducto.Vigente,
            ReglasDelSalvoconducto.Estado(Contenido(), ahora, anulado: false));
    }

    [Fact]
    public void Lo_anulado_manda_sobre_todo_lo_demas()
    {
        Assert.Equal(
            EstadoDelSalvoconducto.Anulado,
            ReglasDelSalvoconducto.Estado(Contenido(), Contenido(), anulado: true));
    }

    // ── Reimprimir ──────────────────────────────────────────────────────────

    [Fact]
    public void No_se_reimprime_sin_motivo()
    {
        var porQue = ReglasDelSalvoconducto.PorQueNoSeReimprime("   ");

        Assert.NotNull(porQue);
        Assert.Contains("copia de más", porQue);
    }

    [Fact]
    public void Con_motivo_se_reimprime()
    {
        Assert.Null(ReglasDelSalvoconducto.PorQueNoSeReimprime("Extraviado en ruta."));
    }

    private static ContenidoDelSalvoconducto Contenido() => new(
        "PC-PROV-ABCD1234",
        "INS-P-014 · Pick-up doble cabina · SIN LÁMINA METÁLICA",
        "José Ramón Cruz",
        "Choluteca",
        new DateOnly(2026, 9, 4),
        new DateOnly(2026, 9, 7),
        ["05/09/2026", "06/09/2026"],
        "Operativo migratorio coordinado con la Policía Nacional.",
        Doris,
        Firma);
}
