using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Datos.M03_Flota;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Aplicacion.M03_Flota;

/// <summary>
/// El préstamo de vehículo como expediente del bien — `RN-63`.
///
/// ── Nunca una Orden de Misión ───────────────────────────────────────────────
/// <i>«Cuando el vehículo se cede con motorista de la institución propietaria, sí es una Orden de
/// Misión con motivo apoyo institucional: ahí no se cedió la tenencia, se prestó un servicio»</i>.
/// La diferencia decide quién responde por la unidad mientras está afuera.
/// </summary>
public sealed class ServicioDePrestamos(SigtiDbContext contexto)
{
    /// <summary>
    /// `P-01` — abrir el expediente con acto, receptor, ventana y acta de entrega.
    /// </summary>
    /// <param name="conMotoristaPropio">
    /// Si el vehículo se cede con motorista de la institución propietaria. <b>Bloquea</b>: eso no
    /// es un préstamo, es una Orden de Misión.
    /// </param>
    public async Task<Ulid> PrestarAsync(
        Ulid id,
        Ulid vehiculo,
        ActoAutorizante acto,
        string autoriza,
        ResponsableReceptor receptor,
        string motivo,
        DateOnly desde,
        DateOnly devolucionComprometida,
        ActaDeTenencia entrega,
        RubrosPactados rubros,
        bool conMotoristaPropio = false,
        CancellationToken cancelacion = default)
    {
        ReglasDelPrestamo.ExigirCesionDeTenencia(conMotoristaPropio);
        ReglasDelPrestamo.ExigirElExpediente(acto, receptor, desde, devolucionComprometida, motivo);
        ReglasDelPrestamo.ExigirSegregacion(autoriza, receptor);

        // **Un vehículo no se presta dos veces a la vez.** Sin esto, dos expedientes vivos
        // dejarían sin decidir quién respondía por la unidad — que es el entregable de la regla.
        var yaPrestado = await contexto.Prestamos
            .AnyAsync(p => p.VehiculoId == vehiculo && p.DevolucionFecha == null, cancelacion);

        if (yaPrestado)
            throw new BloqueoDuro("RN-63",
                "Este vehículo ya está prestado y sin acta de devolución. Dos préstamos vivos " +
                "sobre la misma unidad dejarían sin poder decir quién respondía por ella, que " +
                "es justamente lo que el expediente existe para contestar.");

        contexto.Prestamos.Add(new FilaDePrestamo
        {
            Id = id,
            VehiculoId = vehiculo,
            ActoFolio = acto.Folio.Trim(),
            ActoFirmante = acto.Firmante.Trim(),
            ActoFecha = acto.Fecha,
            ActoAdjunto = acto.Adjunto,
            Autoriza = autoriza.Trim(),
            ReceptorPersona = receptor.Persona.Trim(),
            ReceptorCargo = receptor.Cargo.Trim(),
            ReceptorInstitucion = receptor.Institucion.Trim(),
            ReceptorConstancia = receptor.ConstanciaDeRecepcion.Trim(),
            Motivo = motivo.Trim(),
            Desde = desde,
            DevolucionComprometida = devolucionComprometida,
            EntregaFecha = entrega.Fecha,
            EntregaOdometro = entrega.Odometro,
            EntregaFirma = entrega.Firma,
            EntregaCombustible = entrega.NivelDeCombustible,
            EntregaAccesorios = entrega.InventarioDeAccesorios,
            EntregaDocumentos = entrega.DocumentosEntregados,
            EntregaRotulacion = entrega.RotulacionConstatada,
            EntregaNovedades = entrega.NovedadesODanios,
            RubroCombustible = rubros.Combustible,
            RubroPeajes = rubros.Peajes,
            RubroMantenimiento = rubros.Mantenimiento,
            RubroMultas = rubros.Multas,
            RubroDanios = rubros.Danios,
        });

        await contexto.SaveChangesAsync(cancelacion);
        return id;
    }

    /// <summary>
    /// `P-02` — el acta de devolución. <b>El vehículo no vuelve a `DISPONIBLE` sin ella.</b>
    /// </summary>
    public async Task DevolverAsync(
        Ulid id,
        ActaDeTenencia devolucion,
        string quienFirmaLaDevolucion,
        CancellationToken cancelacion = default)
    {
        var fila = await contexto.Prestamos.SingleOrDefaultAsync(p => p.Id == id, cancelacion)
            ?? throw new BloqueoDuro("RN-63", $"No hay expediente de préstamo con id {id}.");

        if (fila.DevolucionFecha is not null)
            throw new BloqueoDuro("RN-63",
                $"Este préstamo ya se devolvió el {fila.DevolucionFecha:dd/MM/yyyy}. Reescribir " +
                "el acta borraría la constatación que consta; una novedad descubierta después " +
                "es un hecho nuevo (`RN-93`).");

        ReglasDelPrestamo.ExigirQuienRecibeLaDevolucion(
            new ResponsableReceptor(
                fila.ReceptorPersona, fila.ReceptorCargo, fila.ReceptorInstitucion,
                fila.ReceptorConstancia),
            quienFirmaLaDevolucion);

        // El odómetro no retrocede. `RN-89` lo llama invariante del expediente, y acá es lo que
        // hace creíbles los kilómetros bajo tenencia ajena.
        if (devolucion.Odometro < fila.EntregaOdometro)
            throw new BloqueoDuro("RN-63",
                $"El odómetro de devolución ({devolucion.Odometro:N0}) es menor que el de " +
                $"entrega ({fila.EntregaOdometro:N0}). El kilometraje no retrocede: o la " +
                "lectura está mal tomada, o hay una sustitución de tablero que es su propio " +
                "expediente.");

        fila.DevolucionFecha = devolucion.Fecha;
        fila.DevolucionOdometro = devolucion.Odometro;
        fila.DevolucionFirma = devolucion.Firma;
        fila.DevolucionCombustible = devolucion.NivelDeCombustible;
        fila.DevolucionNovedades = devolucion.NovedadesODanios;
        fila.DevolucionRotulacion = devolucion.RotulacionConstatada;
        fila.QuienFirmaLaDevolucion = quienFirmaLaDevolucion.Trim();

        await contexto.SaveChangesAsync(cancelacion);
    }

    public async Task<IReadOnlyList<ExpedienteDePrestamo>> TodosAsync(
        CancellationToken cancelacion = default) =>
        [.. (await contexto.Prestamos.OrderByDescending(p => p.Desde).ToListAsync(cancelacion))
            .Select(A)];

    /// <summary>
    /// Los préstamos vencidos al corte — `RN-97` punto 4, la fuente que faltaba para que el
    /// bloqueo del cierre quedara completo.
    /// </summary>
    public async Task<IReadOnlyList<ExpedienteDePrestamo>> VencidosAsync(
        DateOnly corte, CancellationToken cancelacion = default)
    {
        var filas = await contexto.Prestamos
            .Where(p => p.DevolucionComprometida < corte
                && (p.DevolucionFecha == null || p.DevolucionFecha > corte))
            .ToListAsync(cancelacion);

        return [.. filas.Select(A)];
    }

    /// <summary>
    /// <b>El entregable de `RN-63`</b> punto 7: quién respondía por la unidad en una fecha.
    ///
    /// Se resuelve por la fecha y no por el estado de hoy: un vehículo que hoy está disponible
    /// pudo estar prestado el día que se cometió la infracción.
    /// </summary>
    public async Task<QuienRespondia> QuienRespondiaPorAsync(
        Ulid vehiculo, DateOnly fecha, CancellationToken cancelacion = default)
    {
        var filas = await contexto.Prestamos
            .Where(p => p.VehiculoId == vehiculo && p.Desde <= fecha)
            .ToListAsync(cancelacion);

        return ReglasDelPrestamo.QuienRespondiaPor([.. filas.Select(A)], fecha);
    }

    public async Task<IReadOnlyList<ExpedienteDePrestamo>> DelVehiculoAsync(
        Ulid vehiculo, CancellationToken cancelacion = default) =>
        [.. (await contexto.Prestamos
            .Where(p => p.VehiculoId == vehiculo)
            .OrderByDescending(p => p.Desde)
            .ToListAsync(cancelacion)).Select(A)];

    private static ExpedienteDePrestamo A(FilaDePrestamo f) => new(
        f.Id,
        f.VehiculoId,
        new ActoAutorizante(f.ActoFolio, f.ActoFirmante, f.ActoFecha, f.ActoAdjunto),
        f.Autoriza,
        new ResponsableReceptor(
            f.ReceptorPersona, f.ReceptorCargo, f.ReceptorInstitucion, f.ReceptorConstancia),
        f.Motivo,
        f.Desde,
        f.DevolucionComprometida,
        new ActaDeTenencia(
            f.EntregaFecha, f.EntregaOdometro, f.EntregaFirma, f.EntregaCombustible,
            f.EntregaAccesorios, f.EntregaDocumentos, f.EntregaRotulacion, f.EntregaNovedades),
        new RubrosPactados(
            f.RubroCombustible, f.RubroPeajes, f.RubroMantenimiento, f.RubroMultas, f.RubroDanios),
        f.DevolucionFecha is null
            ? null
            : new ActaDeTenencia(
                f.DevolucionFecha.Value,
                f.DevolucionOdometro ?? 0,
                f.DevolucionFirma ?? "",
                f.DevolucionCombustible,
                null,
                null,
                f.DevolucionRotulacion ?? false,
                f.DevolucionNovedades),
        f.QuienFirmaLaDevolucion);
}
