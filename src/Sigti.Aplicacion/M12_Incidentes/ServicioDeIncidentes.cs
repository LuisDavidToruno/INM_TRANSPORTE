using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Datos.M12_Incidentes;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M12_Incidentes;

namespace Sigti.Aplicacion.M12_Incidentes;

/// <summary>
/// El expediente de incidente — M-12.
///
/// ── Lo que este servicio NO hace, y son dos cosas ───────────────────────────
/// <b>No captura responsabilidad</b> (`RN-74`): no hay un solo parámetro de culpa en ninguna
/// firma. La determinación entra por <see cref="AdjuntarDeterminacionAsync"/> como acto de otra
/// instancia, con su número y su emisor.
///
/// <b>No le cambia el estado a la misión</b> (`RN-70`): registrar una interrupción marca, y la
/// Orden de Misión sigue `EN_RUTA` — el vehículo salió y hubo consumo real de recursos públicos.
/// </summary>
public sealed class ServicioDeIncidentes(SigtiDbContext contexto)
{
    /// <summary>
    /// `I-01` — registrar el hecho. Abre el expediente con responsable y plazo (`RN-74` punto 4).
    ///
    /// ── Las dos fechas, siempre ─────────────────────────────────────────────
    /// `RN-70` admite captura sin ninguna conectividad, así que el momento del hecho y el de
    /// captura pueden estar a días de distancia. Guardar uno solo haría que un incidente
    /// digitado el lunes pareciera del lunes.
    /// </summary>
    public async Task<Ulid> RegistrarAsync(
        Ulid id,
        TipoDeIncidente tipo,
        string causa,
        DateTimeOffset momentoDelHecho,
        DateTimeOffset momentoDeCaptura,
        string descripcion,
        string registra,
        string responsableDeSeguimiento,
        DateOnly plazo,
        bool interrumpe,
        Ulid? mision = null,
        Ulid? vehiculo = null,
        string? ubicacion = null,
        int? odometro = null,
        IReadOnlyList<(string Descripcion, bool EsElVehiculo)>? bienes = null,
        CancellationToken cancelacion = default)
    {
        ReglasDelRegistroDeIncidente.ExigirElHecho(
            causa, descripcion, registra, responsableDeSeguimiento);

        var fechaDelHecho = DateOnly.FromDateTime(
            momentoDelHecho.UtcDateTime.AddMinutes(momentoDelHecho.Offset.TotalMinutes));

        var fila = new FilaDeIncidente
        {
            Id = id,
            Tipo = tipo,
            Causa = causa.Trim(),
            FechaDelHecho = fechaDelHecho,
            MomentoDelHechoUtc = momentoDelHecho.UtcDateTime,
            DesfaseDelHechoMinutos = (int)momentoDelHecho.Offset.TotalMinutes,
            MomentoDeCapturaUtc = momentoDeCaptura.UtcDateTime,
            Descripcion = descripcion.Trim(),
            Registra = registra.Trim(),
            MisionId = mision,
            VehiculoId = vehiculo,
            Ubicacion = ubicacion,
            Odometro = odometro,
            Interrumpe = interrumpe,
            ResponsableDeSeguimiento = responsableDeSeguimiento.Trim(),
            Plazo = plazo,
        };

        fila.Movimientos.Add(Asiento(id, 1, "I-01", registra, momentoDeCaptura,
            interrumpe
                ? $"{tipo}: {causa}. Marca la misión como interrumpida."
                : $"{tipo}: {causa}"));

        foreach (var (descripcionDelBien, esElVehiculo) in bienes ?? [])
            fila.Bienes.Add(new FilaDeBienAfectado
            {
                Id = Ulid.NewUlid(),
                IncidenteId = id,
                Descripcion = descripcionDelBien,
                EsElVehiculo = esElVehiculo,

                // Nace no recuperado: si estuviera en poder de la institución no habría bien
                // afectado que registrar.
                Estado = EstadoDelBien.NoRecuperado,
                FechaDelHecho = fechaDelHecho,
            });

        contexto.Incidentes.Add(fila);
        await contexto.SaveChangesAsync(cancelacion);

        return id;
    }

    /// <summary>
    /// `I-02` — adjuntar la constancia de denuncia o acta ante autoridad (`RN-75` punto 2).
    /// </summary>
    public async Task AdjuntarConstanciaAsync(
        Ulid id, ConstanciaAnteAutoridad constancia, string ejecuta, DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        if (string.IsNullOrWhiteSpace(constancia.Numero) ||
            string.IsNullOrWhiteSpace(constancia.AutoridadReceptora))
            throw new BloqueoDuro("RN-75",
                "La constancia exige número y autoridad receptora. Adjuntar una sin ellos " +
                "dejaría el expediente diciendo que se denunció sin poder decir dónde.");

        var fila = await BuscarFilaAsync(id, cancelacion);

        fila.ConstanciaNumero = constancia.Numero.Trim();
        fila.ConstanciaAutoridad = constancia.AutoridadReceptora.Trim();
        fila.ConstanciaFecha = constancia.Fecha;

        Anotar(fila, "I-02", ejecuta, momento,
            $"Constancia {constancia.Numero} ante {constancia.AutoridadReceptora}");

        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>
    /// `I-03` — registrar el desenlace de la interrupción (`RN-70`).
    ///
    /// <b>Y acá tampoco cambia el estado de la misión.</b> El desenlace dice cómo siguió; que la
    /// misión pase a `RETORNADA` o siga en ruta lo decide su propia máquina de estados, con su
    /// transición y su actor.
    /// </summary>
    public async Task RegistrarDesenlaceAsync(
        Ulid id,
        DesenlaceDeLaInterrupcion desenlace,
        string detalle,
        string ejecuta,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var fila = await BuscarFilaAsync(id, cancelacion);

        ReglasDeLaInterrupcion.ExigirDesenlaceRegistrable(A(fila), detalle);

        fila.Desenlace = desenlace;
        fila.DetalleDelDesenlace = detalle.Trim();

        Anotar(fila, "I-03", ejecuta, momento, $"{desenlace}: {detalle}");

        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>`I-04` — registrar una gestión de recuperación, con responsable y plazo.</summary>
    public async Task RegistrarGestionAsync(
        Ulid id, GestionDeRecuperacion gestion, string ejecuta, DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        if (string.IsNullOrWhiteSpace(gestion.Descripcion) ||
            string.IsNullOrWhiteSpace(gestion.Responsable))
            throw new BloqueoDuro("RN-75",
                "La gestión de recuperación exige decir qué se hizo y quién la tiene a cargo. " +
                "Un expediente que dice «se están haciendo gestiones» sin responsable ni plazo " +
                "no se puede seguir.");

        var fila = await BuscarFilaAsync(id, cancelacion);

        fila.Gestiones.Add(new FilaDeGestionDeRecuperacion
        {
            Id = Ulid.NewUlid(),
            IncidenteId = id,
            Fecha = gestion.Fecha,
            Descripcion = gestion.Descripcion.Trim(),
            Responsable = gestion.Responsable.Trim(),
            Plazo = gestion.Plazo,
        });

        Anotar(fila, "I-04", ejecuta, momento, gestion.Descripcion);

        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>
    /// `I-05` — el bien volvió. <b>No se borra del expediente</b>: cambia de estado, y el
    /// expediente conserva que estuvo afuera y cuánto tiempo.
    /// </summary>
    public async Task RecuperarBienAsync(
        Ulid id, Ulid bien, string ejecuta, DateTimeOffset momento, string donde,
        CancellationToken cancelacion = default)
    {
        var fila = await BuscarFilaAsync(id, cancelacion);
        var afectado = BuscarBien(fila, bien);

        if (afectado.Estado is not EstadoDelBien.NoRecuperado)
            throw new BloqueoDuro("RN-75",
                $"El bien «{afectado.Descripcion}» ya está en {afectado.Estado}.");

        afectado.Estado = EstadoDelBien.Recuperado;
        afectado.UbicacionConocida = donde;

        Anotar(fila, "I-05", ejecuta, momento, $"Recuperado: {afectado.Descripcion}. {donde}");

        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>
    /// `I-06` — descargo formal del bien (`RN-75`). La única salida que no es la recuperación.
    /// </summary>
    public async Task DescargarBienAsync(
        Ulid id, Ulid bien, ConstanciaDeDescargo descargo, string ejecuta, DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var fila = await BuscarFilaAsync(id, cancelacion);
        var afectado = BuscarBien(fila, bien);

        ReglasDelBienNoRecuperado.ExigirDescargoFormal(AlBien(afectado), descargo);

        afectado.Estado = EstadoDelBien.Descargado;
        afectado.DescargoNumero = descargo.Numero.Trim();
        afectado.DescargoAutoridad = descargo.Autoridad.Trim();
        afectado.DescargoFecha = descargo.Fecha;

        Anotar(fila, "I-06", ejecuta, momento,
            $"Descargado por acto {descargo.Numero} de {descargo.Autoridad}: " +
            afectado.Descripcion);

        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>
    /// `I-07` — adjuntar el acto de determinación de responsabilidad (`RN-74` punto 4).
    ///
    /// ── Es lo más cerca que SIGTI llega de la responsabilidad, y no es cerca ─
    /// Se adjunta un documento que otra instancia emitió. SIGTI no lo produce, no lo deduce y no
    /// lo propone: lo registra con su número, su emisor y lo que resolvió.
    /// </summary>
    public async Task AdjuntarDeterminacionAsync(
        Ulid id, DeterminacionDeResponsabilidad acto, string ejecuta, DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        ReglasDelRegistroDeIncidente.ExigirActoDeLaInstanciaCompetente(acto);

        var fila = await BuscarFilaAsync(id, cancelacion);

        if (fila.DeterminacionNumero is not null)
            throw new BloqueoDuro("RN-74",
                $"Este expediente ya tiene adjunto el acto {fila.DeterminacionNumero} de " +
                $"{fila.DeterminacionInstancia}. Reemplazarlo borraría el que constaba: un acto " +
                "posterior que lo revoque se adjunta como hecho nuevo con referencia al " +
                "anterior (`RN-42`).");

        fila.DeterminacionNumero = acto.Numero.Trim();
        fila.DeterminacionInstancia = acto.InstanciaQueLaEmite.Trim();
        fila.DeterminacionFecha = acto.Fecha;
        fila.DeterminacionResolucion = acto.Resolucion.Trim();

        Anotar(fila, "I-07", ejecuta, momento,
            $"Acto {acto.Numero} de {acto.InstanciaQueLaEmite}");

        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>
    /// `I-08` — resolver el expediente.
    ///
    /// No cierra con la interrupción sin desenlace ni con bienes afuera sin declararlo — ver
    /// <see cref="ReglasDelBienNoRecuperado.ExigirCierrePosible"/>.
    /// </summary>
    public async Task ResolverAsync(
        Ulid id,
        string comoSeResolvio,
        DateOnly fecha,
        string ejecuta,
        DateTimeOffset momento,
        string? declaracionDeBienes = null,
        CancellationToken cancelacion = default)
    {
        var fila = await BuscarFilaAsync(id, cancelacion);

        if (fila.ResueltoEn is not null)
            throw new BloqueoDuro("RN-75",
                "Este expediente ya está resuelto. Reescribir su resolución borraría la que " +
                "constaba; lo que se descubra después es un hecho nuevo (`RN-93`).");

        ReglasDelBienNoRecuperado.ExigirCierrePosible(
            A(fila), comoSeResolvio, declaracionDeBienes);

        fila.ResueltoEn = fecha;
        fila.ComoSeResolvio = comoSeResolvio.Trim();
        fila.DeclaracionDeBienes = declaracionDeBienes?.Trim();

        Anotar(fila, "I-08", ejecuta, momento, comoSeResolvio);

        await contexto.SaveChangesAsync(cancelacion);
    }

    // ── Consultas ───────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<ExpedienteDeIncidente>> TodosAsync(
        CancellationToken cancelacion = default) =>
        [.. (await Consulta().OrderByDescending(i => i.FechaDelHecho)
            .ToListAsync(cancelacion)).Select(A)];

    public async Task<ExpedienteDeIncidente?> BuscarAsync(
        Ulid id, CancellationToken cancelacion = default)
    {
        var fila = await Consulta().SingleOrDefaultAsync(i => i.Id == id, cancelacion);
        return fila is null ? null : A(fila);
    }

    /// <summary>
    /// Los expedientes abiertos al corte — la fuente que `RN-97` declaraba <b>no consultable</b>.
    ///
    /// ── Lo que esto desbloquea ──────────────────────────────────────────────
    /// El saldo de apertura enumera diez fuentes y cinco no se podían consultar. Dos de ellas
    /// —esta y las interrupciones sin desenlace— tienen <b>poder de bloqueo</b> sobre el cierre
    /// del período (`RN-97` punto 4), y por eso ese bloqueo no podía disparar.
    /// </summary>
    public async Task<IReadOnlyList<ExpedienteDeIncidente>> AbiertosAlCorteAsync(
        DateOnly corte, CancellationToken cancelacion = default)
    {
        var filas = await Consulta()
            .Where(i => i.FechaDelHecho <= corte
                && (i.ResueltoEn == null || i.ResueltoEn > corte))
            .ToListAsync(cancelacion);

        return [.. filas.Select(A)];
    }

    /// <summary>
    /// Las interrupciones sin desenlace abiertas al corte — `RN-70`, `RN-97` punto 4.
    /// </summary>
    public async Task<IReadOnlyList<ExpedienteDeIncidente>> InterrupcionesSinDesenlaceAsync(
        DateOnly corte, CancellationToken cancelacion = default)
    {
        var filas = await Consulta()
            .Where(i => i.Interrumpe
                && i.Desenlace == null
                && i.FechaDelHecho <= corte
                && (i.ResueltoEn == null || i.ResueltoEn > corte))
            .ToListAsync(cancelacion);

        return [.. filas.Select(A)];
    }

    /// <summary>
    /// Los bienes que siguen fuera del alcance de la institución — `RN-75`.
    ///
    /// <b>Atraviesa expedientes</b>: un bien no recuperado sigue en el registro patrimonial
    /// aunque su expediente se haya resuelto declarándolo, y esa lista es la que el control de
    /// bienes necesita ver.
    /// </summary>
    public async Task<IReadOnlyList<(ExpedienteDeIncidente Expediente, BienAfectado Bien)>>
        BienesNoRecuperadosAsync(CancellationToken cancelacion = default)
    {
        var filas = await Consulta()
            .Where(i => i.Bienes.Any(b => b.Estado == EstadoDelBien.NoRecuperado))
            .ToListAsync(cancelacion);

        return
        [
            .. filas.SelectMany(f =>
            {
                var expediente = A(f);
                return expediente.BienesNoRecuperados.Select(b => (expediente, b));
            }),
        ];
    }

    // ── Interna ─────────────────────────────────────────────────────────────

    private IQueryable<FilaDeIncidente> Consulta() =>
        contexto.Incidentes
            .Include(i => i.Movimientos)
            .Include(i => i.Bienes)
            .Include(i => i.Gestiones);

    private async Task<FilaDeIncidente> BuscarFilaAsync(Ulid id, CancellationToken cancelacion) =>
        await Consulta().SingleOrDefaultAsync(i => i.Id == id, cancelacion)
            ?? throw new BloqueoDuro("M-12", $"No hay expediente de incidente con id {id}.");

    private static FilaDeBienAfectado BuscarBien(FilaDeIncidente fila, Ulid bien) =>
        fila.Bienes.SingleOrDefault(b => b.Id == bien)
            ?? throw new BloqueoDuro("RN-75",
                $"El bien {bien} no pertenece a este expediente.");

    private static void Anotar(
        FilaDeIncidente fila, string movimiento, string ejecuta, DateTimeOffset momento,
        string? detalle)
    {
        var orden = fila.Movimientos.Count == 0 ? 1 : fila.Movimientos.Max(m => m.Orden) + 1;
        fila.Movimientos.Add(Asiento(fila.Id, orden, movimiento, ejecuta, momento, detalle));
    }

    private static FilaDeMovimientoDelIncidente Asiento(
        Ulid incidente, int orden, string movimiento, string ejecuta, DateTimeOffset momento,
        string? detalle) => new()
        {
            Id = Ulid.NewUlid(),
            IncidenteId = incidente,
            Orden = orden,
            Movimiento = movimiento,
            Ejecuta = ejecuta,
            MomentoUtc = momento.UtcDateTime,
            DesfaseMinutos = (int)momento.Offset.TotalMinutes,
            Detalle = detalle,
        };

    private static ExpedienteDeIncidente A(FilaDeIncidente f) => new(
        f.Id,
        f.Tipo,
        f.Causa,
        f.FechaDelHecho,
        new DateTimeOffset(f.MomentoDelHechoUtc, TimeSpan.Zero)
            .ToOffset(TimeSpan.FromMinutes(f.DesfaseDelHechoMinutos)),
        new DateTimeOffset(f.MomentoDeCapturaUtc, TimeSpan.Zero),
        f.Descripcion,
        f.Registra,
        f.MisionId,
        f.VehiculoId,
        f.Ubicacion,
        f.Odometro,
        f.Interrumpe,
        [.. f.Movimientos.OrderBy(m => m.Orden).Select(m => new MovimientoDelIncidente(
            m.Movimiento,
            new DateTimeOffset(m.MomentoUtc, TimeSpan.Zero)
                .ToOffset(TimeSpan.FromMinutes(m.DesfaseMinutos)),
            m.Ejecuta,
            m.Detalle))],
        [.. f.Bienes.Select(AlBien)],
        [.. f.Gestiones.Select(g => new GestionDeRecuperacion(
            g.Fecha, g.Descripcion, g.Responsable, g.Plazo))],
        f.ResponsableDeSeguimiento,
        f.Plazo,
        f.ConstanciaNumero is null
            ? null
            : new ConstanciaAnteAutoridad(
                f.ConstanciaNumero, f.ConstanciaAutoridad ?? "", f.ConstanciaFecha ?? default),
        f.Desenlace,
        f.DetalleDelDesenlace,
        f.DeterminacionNumero is null
            ? null
            : new DeterminacionDeResponsabilidad(
                f.DeterminacionNumero,
                f.DeterminacionInstancia ?? "",
                f.DeterminacionFecha ?? default,
                f.DeterminacionResolucion ?? ""),
        f.ResueltoEn,
        f.ComoSeResolvio);

    private static BienAfectado AlBien(FilaDeBienAfectado b) => new(
        b.Id,
        b.Descripcion,
        b.EsElVehiculo,
        b.Estado,
        b.FechaDelHecho,
        b.UbicacionConocida,
        b.AutoridadCustodia,
        b.NumeroDeExpedienteExterno,
        b.DescargoNumero is null
            ? null
            : new ConstanciaDeDescargo(
                b.DescargoNumero, b.DescargoAutoridad ?? "", b.DescargoFecha ?? default));
}
