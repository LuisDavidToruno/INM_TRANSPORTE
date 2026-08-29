using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Dominio.M19_Seguimiento;

/// <summary>
/// Qué se acepta como reporte de campo — `RN-76`, `RN-43`, `RN-46`.
///
/// ── La precondición no se juzga contra el estado de hoy ─────────────────────
/// Un dispositivo sin cobertura acumula reportes cuatro días y los sube de golpe. Para entonces
/// la misión ya puede estar `RETORNADA` o `LIQUIDADA`, y rechazar por eso <b>perdería
/// exactamente los datos que el módulo existe para conservar</b>.
///
/// Lo que se exige es que la misión <b>hubiera estado en ruta al momento del hecho</b>, y eso se
/// contesta contra el diario, no contra el estado actual (`P-1`: el estado es una proyección del
/// diario). Es la misma disciplina de `P-4` aplicada a una precondición en vez de a una tarifa.
/// </summary>
public static class ReglasDelSeguimiento
{
    /// <summary>
    /// La ventana en que la misión estuvo en ruta: de `T-14` hasta el retorno, sea el normal
    /// (`T-18`) o el anticipado por interrupción (`T-16`).
    ///
    /// <b>El fin es nulo mientras siga afuera</b>, y nulo acá es «todavía no volvió», no
    /// «volvió en una fecha desconocida».
    /// </summary>
    public static (DateTimeOffset Inicio, DateTimeOffset? Fin)? VentanaEnRuta(
        IReadOnlyList<Transicion> diario)
    {
        var salida = diario.FirstOrDefault(t => t.Id == "T-14");
        if (salida is null) return null;

        var retorno = diario
            .Where(t => (t.Id is "T-18" or "T-16") && t.Momento >= salida.Momento)
            .OrderBy(t => t.Momento)
            .FirstOrDefault();

        return (salida.Momento, retorno?.Momento);
    }

    /// <summary>
    /// Exige que el hecho haya ocurrido mientras la misión estaba en ruta.
    ///
    /// El mensaje dice la ventana concreta porque el motivo del rechazo casi siempre es un
    /// <b>reloj de dispositivo mal puesto</b>, y «fuera de rango» no le sirve a nadie para
    /// arreglarlo.
    /// </summary>
    public static void ExigirQueEstuvieraEnRuta(
        IReadOnlyList<Transicion> diario, DateTimeOffset momentoDelHecho)
    {
        var ventana = VentanaEnRuta(diario);

        if (ventana is null)
            throw new BloqueoDuro("RN-76",
                "La misión nunca inició ruta: no hay asiento `T-14`. Un reporte de campo sobre " +
                "una misión que no salió no describe nada que haya pasado.");

        var (inicio, fin) = ventana.Value;

        if (momentoDelHecho < inicio)
            throw new BloqueoDuro("RN-76",
                $"El hecho está fechado {Distancia(inicio - momentoDelHecho)} antes de la salida " +
                $"({inicio:yyyy-MM-dd HH:mm}). Revise el reloj del dispositivo.");

        if (fin is not null && momentoDelHecho > fin)
            throw new BloqueoDuro("RN-76",
                $"El hecho está fechado {Distancia(momentoDelHecho - fin.Value)} después del " +
                $"retorno ({fin:yyyy-MM-dd HH:mm}). Revise el reloj del dispositivo.");
    }

    /// <summary>
    /// El estado tiene que venir del catálogo cerrado `estado_en_ruta`.
    ///
    /// Cerrado y no texto libre porque el catálogo es lo que hace posible <b>el toque único</b>
    /// de `RN-76`: un campo de texto obliga a escribir, y escribir con una mano en el volante no
    /// se hace — se omite.
    /// </summary>
    public static void ExigirEstadoDelCatalogo(string? estado, IReadOnlySet<string> catalogo)
    {
        if (string.IsNullOrWhiteSpace(estado))
            throw new BloqueoDuro("RN-76", "La declaración de estado exige un estado.");

        if (catalogo.Count == 0)
            throw new BloqueoDuro("RN-76",
                "El catálogo `estado_en_ruta` está vacío. Aceptar cualquier texto mientras " +
                "tanto llenaría el histórico de variantes que después nadie puede agrupar.");

        if (!catalogo.Contains(estado))
            throw new BloqueoDuro("RN-76",
                $"«{estado}» no está en el catálogo `estado_en_ruta`. Los del catálogo son: " +
                $"{string.Join(", ", catalogo.Order())}.");
    }

    /// <summary>Arribo y salida son a un destino; sin él, el tiempo en sitio no se puede atribuir.</summary>
    public static void ExigirDestino(TipoDeReporte tipo, string? destino)
    {
        if (tipo is TipoDeReporte.Arribo or TipoDeReporte.Salida &&
            string.IsNullOrWhiteSpace(destino))
            throw new BloqueoDuro("RN-76",
                $"Un {tipo.ToString().ToLowerInvariant()} exige el destino: sin él el tiempo en " +
                "sitio no se puede atribuir a nadie, que es para lo que se mide.");
    }

    /// <summary>
    /// La posición, cuando viene, tiene que ser una posición.
    ///
    /// ── Por qué (0, 0) se rechaza aparte ────────────────────────────────────
    /// Está dentro de los rangos válidos y no es una posición: es lo que devuelve un GPS que
    /// todavía no fijó, y cae en el Golfo de Guinea. Guardarlo pondría toda la flota en el
    /// Atlántico, y peor: la haría ver <b>localizada</b>. Nulo dice la verdad — no se supo.
    /// </summary>
    public static void ExigirPosicionUsable(Posicion? posicion)
    {
        if (posicion is not { } p) return;

        if (p.Latitud is < -90 or > 90 || p.Longitud is < -180 or > 180)
            throw new BloqueoDuro("RN-76",
                $"La posición ({p.Latitud}, {p.Longitud}) está fuera de los rangos posibles.");

        if (p is { Latitud: 0, Longitud: 0 })
            throw new BloqueoDuro("RN-76",
                "(0, 0) no es una posición: es lo que informa un GPS que todavía no fijó. " +
                "Sin posición se registra sin posición, y el tablero lo dice.");

        if (p.PrecisionMetros is < 0)
            throw new BloqueoDuro("RN-76", "La precisión no puede ser negativa.");
    }

    /// <summary>
    /// El desfase entre el hecho y la captura. <b>No es un error</b>: es la medida de cuánto
    /// estuvo el dispositivo sin cobertura, y `RN-43` lo espera.
    /// </summary>
    public static TimeSpan DesfaseDeCaptura(ReporteDeCampo reporte) =>
        reporte.MomentoDeCaptura - reporte.MomentoDelHecho;

    private static string Distancia(TimeSpan d) =>
        d.TotalDays >= 1 ? $"{(int)d.TotalDays} día(s)"
        : d.TotalHours >= 1 ? $"{(int)d.TotalHours} hora(s)"
        : $"{(int)d.TotalMinutes} minuto(s)";
}
