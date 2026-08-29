using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Datos.M14_Auditoria;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.M14_Auditoria;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M14_Auditoria;

/// <summary>
/// La conciliación contra fuentes externas — `RN-95`.
///
/// ── Lo que `RN-30` no puede ver ─────────────────────────────────────────────
/// `RN-30` concilia hacia adentro: galones contra kilómetros, ambos registrados por nosotros.
/// `RN-95` es taxativa: <i>«una conciliación que solo compara nuestros datos con nuestros datos
/// verifica coherencia interna, no veracidad. <b>Un registro completo y coherente puede ser
/// completamente falso</b>, y solo la fuente externa lo revela»</i>.
///
/// De ahí salieron los tres casos de `CE-28`: el comprobante duplicado que apareció en el
/// estado de cuenta del proveedor, el paso por caseta de un domingo sin misión, y las multas
/// notificadas meses después.
/// </summary>
public sealed class ServicioDeConciliacionExterna(SigtiDbContext contexto)
{
    // ── El catálogo de fuentes ──────────────────────────────────────────────

    public async Task<IReadOnlyList<FuenteExterna>> FuentesAsync(
        CancellationToken cancelacion = default) =>
        [.. (await contexto.FuentesExternas.ToListAsync(cancelacion)).Select(A)];

    public async Task<Ulid> RegistrarFuenteAsync(
        Ulid id, TipoDeFuenteExterna tipo, string emisor, string formato,
        string responsableDeLaCarga, bool disponible, int? periodicidadEnDias,
        string? porQueNoEstaDisponible, CancellationToken cancelacion = default)
    {
        ReglasDeFuenteExterna.ExigirDatosDelCatalogo(
            emisor, responsableDeLaCarga, disponible, porQueNoEstaDisponible);

        contexto.FuentesExternas.Add(new FilaDeFuenteExterna
        {
            Id = id,
            Tipo = tipo,
            Emisor = emisor.Trim(),
            Formato = formato.Trim(),
            ResponsableDeLaCarga = responsableDeLaCarga.Trim(),
            Disponible = disponible,
            PeriodicidadEnDias = periodicidadEnDias,
            PorQueNoEstaDisponible = porQueNoEstaDisponible?.Trim(),
        });

        await contexto.SaveChangesAsync(cancelacion);
        return id;
    }

    // ── La conciliación ─────────────────────────────────────────────────────

    /// <summary>
    /// Ejecuta la conciliación y <b>persiste las diferencias como expedientes</b> — `RN-95`:
    /// cada una abre uno, en ambos sentidos.
    ///
    /// ── El resultado se guarda entero, no sólo el resumen ───────────────────
    /// Porque `RN-95` punto 6 exige que el reporte lleve la <b>fecha de corte de
    /// conocimiento</b> (`RN-94`) y el documento fuente. Sin eso, dos ejecuciones de la misma
    /// fuente con datos distintos se ven idénticas y una diferencia no se puede volver a
    /// comprobar contra el papel del que salió.
    /// </summary>
    /// <param name="responsableDeSeguimiento">
    /// Quién persigue las diferencias que no se resuelvan. `RN-66` lo exige junto con el plazo:
    /// sin ellos, «no resuelto» se vuelve un montón que crece y que nadie revisa.
    /// </param>
    /// <param name="hallazgos">
    /// <b>`RN-95` lo exige: cada diferencia abre expediente de hallazgo posterior «de forma
    /// automática»</b> (`RN-93`). Se recibe el servicio y no se construye acá porque el
    /// expediente es de `M-14` y esta conciliación es sólo una de sus fuentes.
    ///
    /// El expediente se abre <b>sólo para las diferencias que ya cayeron sobre un objeto
    /// cerrado</b>: una diferencia de una misión que sigue viva se resuelve en su liquidación,
    /// y `RN-93` no aplica al hallazgo detectado antes del cierre.
    /// </param>
    /// <param name="descubre">
    /// Quién y con qué competencia. Va al expediente: `RN-93` punto 1 pide quién lo descubrió,
    /// cómo, cuándo y contra qué fuente.
    /// </param>
    public async Task<ResultadoDeConciliacion> ConciliarAsync(
        Ulid fuenteId,
        DateOnly desde,
        DateOnly hasta,
        IReadOnlyList<LineaExterna> lineas,
        string documentoFuente,
        IdPersona ejecuta,
        string responsableDeSeguimiento,
        DateOnly plazo,
        DateTimeOffset momento,
        ServicioDeHallazgosPosteriores hallazgos,
        Autoria descubre,
        int toleranciaEnDias = 1,
        CancellationToken cancelacion = default)
    {
        var fila = await contexto.FuentesExternas
            .SingleOrDefaultAsync(f => f.Id == fuenteId, cancelacion)
            ?? throw new FuenteNoEncontrada(fuenteId);

        var fuente = A(fila);

        ReglasDeConciliacionExterna.ExigirFuenteDisponible(fuente);
        ReglasDeConciliacionExterna.ExigirRangoValido(desde, hasta);
        ReglasDeConciliacionExterna.ExigirDocumentoFuente(documentoFuente);
        ReglasDeImputacionExterna.ExigirResponsableYPlazoDeLoNoResuelto(
            responsableDeSeguimiento, plazo);

        var resultado = ReglasDeConciliacionExterna.Conciliar(
            fuenteId, desde, hasta, lineas,
            await AsientosAsync(fuente.Tipo, desde, hasta, cancelacion),
            await AnclasDeLaFlotaAsync(cancelacion),
            toleranciaEnDias, momento, documentoFuente.Trim());

        await GuardarAsync(
            resultado, ejecuta, responsableDeSeguimiento, plazo, momento, fila, cancelacion);

        // **`RN-95`: cada diferencia abre expediente de hallazgo posterior**, en ambos
        // sentidos. Hasta ahora la conciliación las dejaba como filas con responsable y plazo;
        // el expediente es lo que les da ciclo propio, asiento reverso y resolución que no se
        // borra — y lo que impide que se resuelvan reabriendo la misión.
        await AbrirExpedientesAsync(
            resultado, fuente, documentoFuente.Trim(), hallazgos, descubre, momento, cancelacion);

        return resultado;
    }

    /// <summary>
    /// Abre un expediente de hallazgo posterior por cada diferencia — `RN-95` y `RN-93`.
    ///
    /// ── La fecha del hecho y la del descubrimiento, separadas ───────────────
    /// `RN-93` las exige como campos distintos, y acá salen de dos lugares distintos: la del
    /// hecho es la de la línea o la del asiento; la del descubrimiento es la de esta
    /// conciliación. Contar la antigüedad desde el descubrimiento premiaría conciliar tarde.
    ///
    /// ── Sin misión vinculable, y eso es previsto ────────────────────────────
    /// La conciliación no sabe a qué misión pertenece una línea del proveedor. El expediente
    /// se abre con <b>cero misiones</b>, el vehículo cuando se resolvió, y el período — que es
    /// exactamente el caso que `RN-93` describe: <i>«el paso por caseta de un domingo, el
    /// consumo de un vehículo que ese día no tenía orden»</i>.
    /// </summary>
    private static async Task AbrirExpedientesAsync(
        ResultadoDeConciliacion resultado,
        FuenteExterna fuente,
        string documentoFuente,
        ServicioDeHallazgosPosteriores hallazgos,
        Autoria descubre,
        DateTimeOffset momento,
        CancellationToken cancelacion)
    {
        var descubrimiento = DateOnly.FromDateTime(momento.Date);
        var periodo = $"{resultado.Desde:yyyy-MM}";

        foreach (var d in resultado.SoloEnLaFuente)
        {
            await hallazgos.AbrirAsync(
                Ulid.NewUlid(),
                $"Cobro del emisor sin registro en SIGTI ({fuente.Tipo})",
                d.Linea.FechaDelHecho,
                descubrimiento,
                $"Conciliación contra {fuente.Emisor}, línea {d.Linea.Id}" +
                    (d.Linea.Referencia is { } r ? $", comprobante {r}" : ""),
                $"{fuente.Emisor} — {documentoFuente}",
                documentoFuente,
                misiones: [],
                d.Vehiculo.Vehiculo,
                motorista: null,
                periodo,
                descubre,
                momento,
                cancelacion);
        }

        foreach (var d in resultado.SoloEnSigti)
        {
            await hallazgos.AbrirAsync(
                Ulid.NewUlid(),
                $"Registro en SIGTI sin respaldo del emisor ({fuente.Tipo})",
                d.Asiento.FechaDelHecho,
                descubrimiento,
                $"Conciliación contra {fuente.Emisor}: {d.Asiento.Origen} registrado y no " +
                    "reportado por el emisor",
                $"{fuente.Emisor} — {documentoFuente}",
                documentoFuente,
                misiones: [],
                d.Asiento.Vehiculo,
                motorista: null,
                periodo,
                descubre,
                momento,
                cancelacion);
        }
    }

    /// <summary>
    /// Nuestros asientos, según lo que la fuente concilia — `RN-95` punto 1.
    ///
    /// ⚠️ <b>Las infracciones y las actas no tienen asiento propio contra el que conciliar.</b>
    /// La regla las manda cruzar contra la bitácora y los expedientes de M-12, que no existen
    /// como asientos comparables: toda línea suya cae en «solo en la fuente» y se resuelve al
    /// vehículo por la jerarquía de anclas. Es correcto —una multa <b>no</b> tiene contraparte
    /// nuestra— pero no es la conciliación completa que `RN-95` describe, y se dice acá.
    /// </summary>
    private async Task<IReadOnlyList<AsientoPropio>> AsientosAsync(
        TipoDeFuenteExterna tipo, DateOnly desde, DateOnly hasta, CancellationToken cancelacion)
    {
        var inicio = desde.ToDateTime(TimeOnly.MinValue);
        var fin = hasta.ToDateTime(TimeOnly.MaxValue);

        if (tipo is TipoDeFuenteExterna.EstadoDeCuentaDeCombustible)
        {
            // **Los abastecimientos, no los consumos del vale.** Son el mismo hecho visto desde
            // dos lados (`RN-83`), y el abastecimiento es el que cubre todas las fuentes —
            // incluida la del tanque, que el proveedor no factura pero que existe.
            var filas = await contexto.Abastecimientos
                .Where(a => a.MomentoUtc >= inicio && a.MomentoUtc <= fin)
                .Where(a => a.Fuente == FuenteDeAbastecimiento.FondoDeLaMision ||
                            a.Fuente == FuenteDeAbastecimiento.PeculioDelServidor)
                .Select(a => new
                {
                    a.Id, a.MomentoUtc, a.Monto, a.VehiculoId, a.Comprobante,
                })
                .ToListAsync(cancelacion);

            return
            [
                .. filas.Select(a => new AsientoPropio(
                    a.Id, "abastecimiento", DateOnly.FromDateTime(a.MomentoUtc),
                    a.Monto ?? 0m, a.VehiculoId, a.Comprobante)),
            ];
        }

        if (tipo is TipoDeFuenteExterna.EstadoDeCuentaDePeaje)
        {
            var filas = await contexto.PasosPorCaseta
                .Where(p => p.MomentoUtc >= inicio && p.MomentoUtc <= fin)
                .Select(p => new { p.Id, p.MomentoUtc, p.MontoPagado, p.VehiculoId, p.Ticket })
                .ToListAsync(cancelacion);

            return
            [
                .. filas.Select(p => new AsientoPropio(
                    p.Id, "paso por caseta", DateOnly.FromDateTime(p.MomentoUtc),
                    p.MontoPagado, p.VehiculoId, p.Ticket)),
            ];
        }

        return [];
    }

    /// <summary>
    /// Las anclas de toda la flota — `RN-66`.
    ///
    /// <b>Toda la flota, sin filtrar por alcance de datos</b>, y es de la regla: `RN-95` punto 3
    /// — <i>«la conciliación cruza el alcance de datos: dos delegaciones no se ven entre sí,
    /// pero un comprobante duplicado entre ellas sí se detecta»</i>.
    /// </summary>
    private async Task<IReadOnlyList<AnclasDelVehiculo>> AnclasDeLaFlotaAsync(
        CancellationToken cancelacion) =>
        [.. (await contexto.Vehiculos
                .Select(v => new
                {
                    v.Id, v.Siglas, v.BienDelInventario, v.Chasis, v.Motor,
                    v.CorrelativoInstitucional, v.Placa,
                })
                .ToListAsync(cancelacion))
            .Select(v => new AnclasDelVehiculo(
                v.Id, v.Siglas, v.BienDelInventario, v.Chasis, v.Motor,
                v.CorrelativoInstitucional, v.Placa))];

    private async Task GuardarAsync(
        ResultadoDeConciliacion resultado, IdPersona ejecuta, string responsable,
        DateOnly plazo, DateTimeOffset momento, FilaDeFuenteExterna fuente,
        CancellationToken cancelacion)
    {
        var ejecucion = new FilaDeEjecucion
        {
            Id = Ulid.NewUlid(),
            FuenteId = resultado.Fuente,
            Desde = resultado.Desde,
            Hasta = resultado.Hasta,
            DocumentoFuente = resultado.DocumentoFuente,
            FechaDeCorteUtc = resultado.FechaDeCorte.UtcDateTime,
            DesfaseMinutos = (int)resultado.FechaDeCorte.Offset.TotalMinutes,
            Ejecuta = ejecuta.Valor,
            Coincidentes = resultado.Coincidentes.Count,
            SoloEnLaFuente = resultado.SoloEnLaFuente.Count,
            SoloEnSigti = resultado.SoloEnSigti.Count,
        };

        foreach (var d in resultado.SoloEnLaFuente)
        {
            ejecucion.Diferencias.Add(new FilaDeDiferencia
            {
                Id = Ulid.NewUlid(),
                EjecucionId = ejecucion.Id,
                Lado = LadoDeLaDiferencia.SoloEnLaFuente,
                FechaDelHecho = d.Linea.FechaDelHecho,
                Monto = d.Linea.Monto,
                Referencia = d.Linea.Referencia,
                LineaExterna = d.Linea.Id,
                VehiculoId = d.Vehiculo.Vehiculo,
                Ancla = d.Vehiculo.Ancla,
                Explicacion = d.Vehiculo.Explicacion,
                ResponsableDeSeguimiento = responsable,
                Plazo = plazo,
            });
        }

        foreach (var d in resultado.SoloEnSigti)
        {
            ejecucion.Diferencias.Add(new FilaDeDiferencia
            {
                Id = Ulid.NewUlid(),
                EjecucionId = ejecucion.Id,
                Lado = LadoDeLaDiferencia.SoloEnSigti,
                FechaDelHecho = d.Asiento.FechaDelHecho,
                Monto = d.Asiento.Monto,
                Referencia = d.Asiento.Referencia,
                AsientoId = d.Asiento.Id,
                Origen = d.Asiento.Origen,
                VehiculoId = d.Asiento.Vehiculo,

                // No se presume qué es. `RN-95`: puede ser un comprobante falso, o una estación
                // que no reportó — y la conciliación no elige.
                Explicacion =
                    $"Registrado en SIGTI como {d.Asiento.Origen} y el emisor no lo reporta. " +
                    "Puede ser un comprobante falso, o una estación que no reportó: la " +
                    "conciliación no presume cuál.",

                ResponsableDeSeguimiento = responsable,
                Plazo = plazo,
            });
        }

        contexto.EjecucionesDeConciliacion.Add(ejecucion);

        // La fecha de la última conciliación es dato del catálogo — `RN-95` punto 1 —, y es lo
        // que después alimenta el retraso visible del punto 5.
        fuente.UltimaConciliacion = DateOnly.FromDateTime(momento.Date);

        await contexto.SaveChangesAsync(cancelacion);
    }

    // ── Los expedientes ─────────────────────────────────────────────────────

    /// <summary>Las diferencias abiertas, que son las que alguien tiene que resolver.</summary>
    public async Task<IReadOnlyList<FilaDeDiferencia>> DiferenciasAbiertasAsync(
        CancellationToken cancelacion = default) =>
        await contexto.DiferenciasDeConciliacion
            .Where(d => d.Resolucion == null)
            .OrderBy(d => d.Plazo)
            .ToListAsync(cancelacion);

    public async Task<IReadOnlyList<FilaDeEjecucion>> EjecucionesAsync(
        CancellationToken cancelacion = default) =>
        await contexto.EjecucionesDeConciliacion
            .Include(e => e.Diferencias)
            .OrderByDescending(e => e.FechaDeCorteUtc)
            .ToListAsync(cancelacion);

    /// <summary>
    /// Resuelve una diferencia. <b>No la borra</b>: que existió es parte del expediente, y el
    /// auditor pregunta por las resueltas tanto como por las abiertas.
    /// </summary>
    public async Task ResolverAsync(
        Ulid diferencia, string resolucion, DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        if (string.IsNullOrWhiteSpace(resolucion))
            throw new BloqueoDuro("RN-95",
                "Resolver una diferencia exige decir cómo se resolvió. Sin eso, la resolución " +
                "es indistinguible de haberla archivado sin mirar.");

        var fila = await contexto.DiferenciasDeConciliacion
            .SingleOrDefaultAsync(d => d.Id == diferencia, cancelacion)
            ?? throw new DiferenciaNoEncontrada(diferencia);

        if (fila.Resolucion is not null)
            throw new BloqueoDuro("RN-95",
                "Esta diferencia ya está resuelta. Reescribir su resolución borraría la que " +
                "constaba; lo que cambia después es un hallazgo nuevo, no una corrección de éste.");

        fila.Resolucion = resolucion.Trim();
        fila.ResueltaUtc = momento.UtcDateTime;

        await contexto.SaveChangesAsync(cancelacion);
    }

    private static FuenteExterna A(FilaDeFuenteExterna f) =>
        new(f.Id, f.Tipo, f.Emisor, f.Formato, f.ResponsableDeLaCarga, f.Disponible,
            f.PeriodicidadEnDias, f.UltimaConciliacion, f.PorQueNoEstaDisponible);
}

public sealed class FuenteNoEncontrada(Ulid id)
    : Exception($"No existe la fuente externa {id}.");

public sealed class DiferenciaNoEncontrada(Ulid id)
    : Exception($"No existe la diferencia de conciliación {id}.");
