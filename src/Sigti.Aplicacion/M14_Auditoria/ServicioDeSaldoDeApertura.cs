using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M12_Incidentes;
using Sigti.Dominio.M14_Auditoria;

namespace Sigti.Aplicacion.M14_Auditoria;

/// <summary>
/// El saldo de apertura de control interno — `RN-97`.
///
/// ── La regla que impide el abandono ─────────────────────────────────────────
/// <i>«Sin saldo de apertura, el mecanismo de olvido es automático y no requiere mala fe: llega
/// enero, el sistema arranca con reportes en cero, y una misión interrumpida en noviembre, un
/// préstamo vencido en agosto y una obligación de reintegro de mayo simplemente dejan de
/// aparecer en ninguna pantalla. <b>Nadie decidió abandonarlos: se abandonaron solos</b>»</i>.
///
/// ── Y lo que este servicio NO puede contar todavía ──────────────────────────
/// Dos de las diez fuentes que `RN-97` enumera no existen como registro: los reclamos de peaje
/// (`RN-92`) y las bitácoras pendientes de digitación. <b>Se declaran igual, como fuentes no
/// consultadas</b>, porque un saldo que las omite en silencio es el mismo abandono con formato de
/// reporte.
///
/// ── El bloqueo del cierre ya dispara, entero ────────────────────────────────
/// `RN-97` punto 4 le da poder de bloqueo a dos fuentes: préstamos vencidos e interrupciones sin
/// desenlace. Las dos estuvieron declaradas y vacías durante varios turnos —el bloqueo escrito y
/// sin poder disparar—. <b>`RN-63` trajo la primera y M-12 la segunda.</b>
/// </summary>
public sealed class ServicioDeSaldoDeApertura(SigtiDbContext contexto)
{
    /// <summary>
    /// El inventario de lo que sigue vivo al corte.
    ///
    /// ── Sin filtrar por alcance de datos ────────────────────────────────────
    /// El saldo de apertura es de la institución, no de una delegación: un pendiente que dos
    /// delegaciones no se ven entre sí sigue siendo un pendiente de la institución, y es la
    /// Gerencia Administrativa y Auditoría Interna quienes lo reciben (`RN-97` punto 5).
    /// </summary>
    public async Task<(IReadOnlyList<RenglonDelSaldo> Renglones, IReadOnlyList<FuenteDelSaldo> Fuentes)>
        InventarioAsync(DateOnly corte, CancellationToken cancelacion = default)
    {
        var renglones = new List<RenglonDelSaldo>();
        var fuentes = new List<FuenteDelSaldo>();

        renglones.AddRange(await MisionesSinCerrarAsync(corte, cancelacion));
        fuentes.Add(new FuenteDelSaldo(TipoDeRenglon.MisionSinCerrar, true,
            renglones.Count(r => r.Tipo is TipoDeRenglon.MisionSinCerrar)));

        var vales = await ValesSinLiquidarAsync(corte, cancelacion);
        renglones.AddRange(vales);
        fuentes.Add(new FuenteDelSaldo(TipoDeRenglon.ValeSinLiquidar, true, vales.Count));

        var obligaciones = await ObligacionesAbiertasAsync(corte, cancelacion);
        renglones.AddRange(obligaciones);
        fuentes.Add(new FuenteDelSaldo(
            TipoDeRenglon.ObligacionDeReintegro, true, obligaciones.Count));

        var hallazgos = await HallazgosAbiertosAsync(corte, cancelacion);
        renglones.AddRange(hallazgos);
        fuentes.Add(new FuenteDelSaldo(
            TipoDeRenglon.HallazgoPosteriorAbierto, true, hallazgos.Count));

        var diferencias = await DiferenciasAbiertasAsync(corte, cancelacion);
        renglones.AddRange(diferencias);
        fuentes.Add(new FuenteDelSaldo(
            TipoDeRenglon.ImputacionExternaNoResuelta, true, diferencias.Count));

        // ── Las dos fuentes con poder de bloqueo, ya completas ──────────────
        // `RN-97` punto 4 se lo da a los préstamos vencidos y a las interrupciones sin
        // desenlace. Las dos estuvieron declaradas y vacías: el bloqueo escrito y sin poder
        // disparar. `RN-63` trajo la primera y M-12 la segunda.
        var prestamos = await PrestamosVencidosAsync(corte, cancelacion);
        renglones.AddRange(prestamos);
        fuentes.Add(new FuenteDelSaldo(TipoDeRenglon.PrestamoVencido, true, prestamos.Count));
        var interrupciones = await InterrupcionesSinDesenlaceAsync(corte, cancelacion);
        renglones.AddRange(interrupciones);
        fuentes.Add(new FuenteDelSaldo(
            TipoDeRenglon.InterrupcionSinDesenlace, true, interrupciones.Count));

        var incidentes = await IncidentesAbiertosAsync(corte, cancelacion);
        renglones.AddRange(incidentes);
        fuentes.Add(new FuenteDelSaldo(
            TipoDeRenglon.ExpedienteDeIncidente, true, incidentes.Count));

        fuentes.Add(new FuenteDelSaldo(TipoDeRenglon.ReclamoDePeaje, false, 0,
            "`RN-92` no está construida: las discrepancias de clasificación se detectan " +
            "(`RN-36`) pero el expediente de reclamo ante la SAPP no existe."));

        fuentes.Add(new FuenteDelSaldo(TipoDeRenglon.BitacoraPendienteDeDigitacion, false, 0,
            "No hay forma de distinguir una bitácora que nunca se digitó de una misión que " +
            "todavía no ha salido. Contarlas juntas inflaría el saldo con misiones normales."));

        return (renglones, fuentes);
    }

    /// <summary>
    /// Produce el saldo de apertura — `RN-97` punto 1: <b>documento con folio</b>.
    ///
    /// ── El arrastre es lo que hace que la regla sirva ───────────────────────
    /// Cada renglón que ya venía del saldo anterior conserva <b>su fecha del hecho original</b> y
    /// suma uno a su contador. Un renglón que aparece en tres saldos consecutivos es visible como
    /// tal, y eso es lo que impide presentarlo como pendiente reciente cada enero.
    /// </summary>
    /// <param name="declaracionDeBloqueantes">
    /// El motivo por el que se produce el saldo con préstamos vencidos o interrupciones sin
    /// desenlace vivos. `RN-97` punto 4: <i>«hay que resolverlos o declararlos explícitamente»</i>.
    /// </param>
    public async Task<SaldoDeApertura> ProducirAsync(
        Ulid id,
        string folio,
        string ejercicio,
        DateOnly corte,
        Autoria produce,
        DateTimeOffset momento,
        string? declaracionDeBloqueantes = null,
        CancellationToken cancelacion = default)
    {
        ReglasDelSaldoDeApertura.ExigirFolioYEjercicio(folio, ejercicio);

        if (await contexto.SaldosDeApertura.AnyAsync(s => s.Ejercicio == ejercicio, cancelacion))
            throw new BloqueoDuro("RN-97",
                $"Ya hay un saldo de apertura para el ejercicio {ejercicio}. Producir un segundo " +
                "dejaría dos inventarios del mismo corte, y el acta de cierre no podría citar " +
                "cuál de los dos es. Lo que cambia después se resuelve marcando renglones como " +
                "resueltos, no rehaciendo el documento.");

        var (renglones, fuentes) = await InventarioAsync(corte, cancelacion);

        foreach (var r in renglones)
            ReglasDelSaldoDeApertura.ExigirResponsable(r.Tipo, r.Referencia, r.Responsable);

        ReglasDelSaldoDeApertura.ExigirCierrePosible(renglones, declaracionDeBloqueantes);

        var anterior = await UltimoAnteriorAsync(ejercicio, cancelacion);
        var conArrastre = ReglasDelSaldoDeApertura.ArrastrarDesde(renglones, anterior);

        var saldo = new SaldoDeApertura(
            id, folio.Trim(), ejercicio.Trim(), corte, conArrastre, fuentes, produce, momento,

            // **El primero es el inicial de implantación.** `RN-97`: «es esperable — es la
            // primera vez que la institución ve todo junto». Se declara para que no se compare
            // contra los siguientes como si fueran la misma medición.
            EsInicialDeImplantacion: anterior.Count == 0 &&
                !await contexto.SaldosDeApertura.AnyAsync(cancelacion));

        await GuardarAsync(saldo, declaracionDeBloqueantes, cancelacion);
        return saldo;
    }

    // ── Las fuentes que sí se pueden consultar ──────────────────────────────

    /// <summary>
    /// Las órdenes que no llegaron a un estado terminal al corte.
    ///
    /// <b>Se juzga por el último asiento del diario</b>, no por una columna de estado: es la
    /// misma proyección que usa todo el sistema (P-1), y una columna se desincroniza.
    /// </summary>
    private async Task<IReadOnlyList<RenglonDelSaldo>> MisionesSinCerrarAsync(
        DateOnly corte, CancellationToken cancelacion)
    {
        var hasta = corte.ToDateTime(TimeOnly.MaxValue);

        var filas = await contexto.Expedientes
            .Include(e => e.Transiciones)
            .Where(e => e.Transiciones.Any(t => t.MomentoUtc <= hasta))
            .ToListAsync(cancelacion);

        var renglones = new List<RenglonDelSaldo>();

        foreach (var fila in filas)
        {
            // El estado **al corte**, no el de hoy: una misión que cerró en febrero no era un
            // pendiente del 31 de diciembre anterior.
            var alCorte = fila.Transiciones
                .Where(t => t.MomentoUtc <= hasta)
                .OrderBy(t => t.Orden)
                .LastOrDefault();

            if (alCorte is null) continue;
            if (EsTerminal(alCorte.Destino)) continue;

            var primera = fila.Transiciones.OrderBy(t => t.Orden).First();

            renglones.Add(new RenglonDelSaldo(
                TipoDeRenglon.MisionSinCerrar,
                // ⚠️ El ULID, porque **la orden de misión todavía no tiene folio**. `RN-44`
                // reserva rangos por delegación para eso, y sin él un renglón del saldo se cita
                // con un identificador que nadie reconoce en un acta.
                fila.Id.ToString(),
                $"Orden de misión en {alCorte.Destino}: {fila.Destino}",
                DateOnly.FromDateTime(primera.MomentoUtc),
                CausaDelRenglon.PendienteDeGestionInterna,

                // El responsable es quien ejecutó el último acto: es quien tiene el expediente
                // en la mano. `RN-97` exige nominarlo, no dejarlo abierto.
                alCorte.Ejecuta,
                alCorte.Destino.ToString()));
        }

        return renglones;
    }

    private static bool EsTerminal(EstadoDeMision estado) => estado
        is EstadoDeMision.Cerrada
        or EstadoDeMision.CerradaConHallazgo
        or EstadoDeMision.Anulada
        or EstadoDeMision.Rechazada;

    private async Task<IReadOnlyList<RenglonDelSaldo>> ValesSinLiquidarAsync(
        DateOnly corte, CancellationToken cancelacion)
    {
        var hasta = corte.ToDateTime(TimeOnly.MaxValue);

        var filas = await contexto.AsignacionesDeCombustible
            .Include(a => a.Transiciones)
            .ToListAsync(cancelacion);

        var renglones = new List<RenglonDelSaldo>();

        foreach (var fila in filas)
        {
            var alCorte = fila.Transiciones
                .Where(t => t.MomentoUtc <= hasta)
                .OrderBy(t => t.Orden)
                .LastOrDefault();

            if (alCorte is null) continue;

            if (alCorte.Destino is EstadoDeAsignacion.Liquidada
                or EstadoDeAsignacion.Conciliada
                or EstadoDeAsignacion.ConciliadaConDesviacion
                or EstadoDeAsignacion.Anulada
                or EstadoDeAsignacion.Devuelta)
                continue;

            var primera = fila.Transiciones.OrderBy(t => t.Orden).First();

            renglones.Add(new RenglonDelSaldo(
                TipoDeRenglon.ValeSinLiquidar,
                fila.Folio,
                $"Vale en {alCorte.Destino} por {fila.Monto:N2}",
                DateOnly.FromDateTime(primera.MomentoUtc),
                CausaDelRenglon.PendienteDeGestionInterna,
                alCorte.Ejecuta,
                alCorte.Destino.ToString(),
                Monto: fila.Monto));
        }

        return renglones;
    }

    private async Task<IReadOnlyList<RenglonDelSaldo>> ObligacionesAbiertasAsync(
        DateOnly corte, CancellationToken cancelacion)
    {
        var filas = await contexto.ObligacionesDeReintegro
            .Include(o => o.Movimientos)
            .Where(o => o.FechaDelHecho <= corte)
            .ToListAsync(cancelacion);

        var hasta = corte.ToDateTime(TimeOnly.MaxValue);
        var renglones = new List<RenglonDelSaldo>();

        foreach (var fila in filas)
        {
            var alCorte = fila.Movimientos
                .Where(m => m.MomentoUtc <= hasta)
                .OrderBy(m => m.Orden)
                .LastOrDefault();

            if (alCorte is null) continue;
            if (alCorte.Destino is EstadoDeObligacion.Saldada
                or EstadoDeObligacion.DejadaSinEfecto) continue;

            var pagado = fila.Movimientos
                .Where(m => m.MomentoUtc <= hasta)
                .Sum(m => m.Pagado ?? 0m);

            renglones.Add(new RenglonDelSaldo(
                TipoDeRenglon.ObligacionDeReintegro,
                fila.Id.ToString(),
                $"Obligación de reintegro por {fila.Causa}, a cargo de {fila.Responsable}",

                // **La fecha del hecho original**, que es la que `RN-86` congela y la que
                // `RN-97` manda arrastrar sin reiniciar.
                fila.FechaDelHecho,
                CausaDelRenglon.PendienteDeGestionInterna,
                alCorte.Persona,
                alCorte.Destino.ToString(),
                Monto: Math.Max(0m, fila.Monto - pagado)));
        }

        return renglones;
    }

    /// <summary>
    /// `RN-63` — los préstamos vencidos al corte.
    ///
    /// ── La otra mitad del bloqueo del cierre ────────────────────────────────
    /// <i>«Vencida la fecha de devolución comprometida, el préstamo alerta con escalamiento
    /// diario y entra al reporte de auditoría con los días de mora. No puede cerrarse el período
    /// con préstamos vencidos»</i>.
    ///
    /// La antigüedad se cuenta desde <b>la fecha comprometida</b>, no desde el inicio del
    /// préstamo: lo que está vencido es la devolución, y un préstamo de tres años en plazo no
    /// tiene mora.
    /// </summary>
    private async Task<IReadOnlyList<RenglonDelSaldo>> PrestamosVencidosAsync(
        DateOnly corte, CancellationToken cancelacion)
    {
        var filas = await contexto.Prestamos
            .Where(p => p.DevolucionComprometida < corte
                && (p.DevolucionFecha == null || p.DevolucionFecha > corte))
            .ToListAsync(cancelacion);

        return
        [
            .. filas.Select(p => new RenglonDelSaldo(
                TipoDeRenglon.PrestamoVencido,
                p.Id.ToString(),
                $"Vehículo prestado a {p.ReceptorPersona} ({p.ReceptorInstitucion}) por " +
                $"{p.Motivo}, con acto {p.ActoFolio}",

                // Desde la fecha comprometida: es la que venció.
                p.DevolucionComprometida,

                // El bien está fuera del alcance de la institución y depende de que otra parte
                // lo devuelva — la misma causa que `RN-75` da a lo sustraído o retenido.
                CausaDelRenglon.BienNoRecuperado,

                p.Autoriza,
                $"{(corte.DayNumber - p.DevolucionComprometida.DayNumber)} días de mora")),
        ];
    }

    /// <summary>
    /// `RN-70` — las interrupciones en ruta sin desenlace, abiertas al corte.
    ///
    /// ── La fuente que le da poder de bloqueo al cierre ──────────────────────
    /// `RN-70`: <i>«ninguna misión con marca de interrupción sin desenlace puede quedar viva al
    /// cierre del período»</i>. `RN-97` punto 4 la usa para bloquear, y hasta que M-12 existió
    /// esta consulta devolvía la nada — con el bloqueo declarado y sin poder disparar.
    /// </summary>
    private async Task<IReadOnlyList<RenglonDelSaldo>> InterrupcionesSinDesenlaceAsync(
        DateOnly corte, CancellationToken cancelacion)
    {
        var filas = await contexto.Incidentes
            .Where(i => i.Interrumpe
                && i.Desenlace == null
                && i.FechaDelHecho <= corte
                && (i.ResueltoEn == null || i.ResueltoEn > corte))
            .ToListAsync(cancelacion);

        return
        [
            .. filas.Select(i => new RenglonDelSaldo(
                TipoDeRenglon.InterrupcionSinDesenlace,
                i.Id.ToString(),
                $"{i.Tipo} en ruta sin desenlace: {i.Causa}",

                // Desde el hecho, no desde la captura ni desde el corte. Es la disciplina de
                // `RN-97` punto 3: la antigüedad no se reinicia.
                i.FechaDelHecho,
                CausaDelRenglon.PendienteDeGestionInterna,
                i.ResponsableDeSeguimiento,
                "Sin desenlace")),
        ];
    }

    /// <summary>
    /// M-12 — los expedientes de incidente abiertos al corte.
    ///
    /// <b>Sin las interrupciones sin desenlace</b>, que van por su propia fuente: contarlas dos
    /// veces inflaría el inventario que `RN-96` punto 2 manda cuadrar renglón por renglón.
    /// </summary>
    private async Task<IReadOnlyList<RenglonDelSaldo>> IncidentesAbiertosAsync(
        DateOnly corte, CancellationToken cancelacion)
    {
        var filas = await contexto.Incidentes
            .Include(i => i.Bienes)
            .Where(i => i.ResueltoEn == null
                && i.FechaDelHecho <= corte
                && !(i.Interrumpe && i.Desenlace == null))
            .ToListAsync(cancelacion);

        return
        [
            .. filas.Select(i => new RenglonDelSaldo(
                TipoDeRenglon.ExpedienteDeIncidente,
                i.Id.ToString(),
                $"{i.Tipo}: {i.Causa}" +
                    (i.Bienes.Any(b => b.Estado == EstadoDelBien.NoRecuperado)
                        ? $" · {i.Bienes.Count(b => b.Estado == EstadoDelBien.NoRecuperado)} " +
                          "bien(es) sin recuperar"
                        : ""),
                i.FechaDelHecho,

                // `RN-75` — el bien sustraído o retenido tiene su propia causa en el saldo, y
                // no es «pendiente de gestión»: permanece hasta su recuperación o su descargo.
                i.Bienes.Any(b => b.Estado == EstadoDelBien.NoRecuperado)
                    ? CausaDelRenglon.BienNoRecuperado
                    : CausaDelRenglon.PendienteDeGestionInterna,

                i.ResponsableDeSeguimiento,
                "Abierto")),
        ];
    }

    private async Task<IReadOnlyList<RenglonDelSaldo>> HallazgosAbiertosAsync(
        DateOnly corte, CancellationToken cancelacion)
    {
        var filas = await contexto.HallazgosPosteriores
            .Include(h => h.Movimientos)
            .Where(h => h.FechaDelDescubrimiento <= corte && h.Resolucion == null)
            .ToListAsync(cancelacion);

        return
        [
            .. filas.Select(h => new RenglonDelSaldo(
                TipoDeRenglon.HallazgoPosteriorAbierto,
                h.Id.ToString(),
                h.Tipo,

                // Desde el hecho, no desde el descubrimiento: es la misma disciplina de `RN-93`.
                h.FechaDelHecho,
                CausaDelRenglon.PendienteDeGestionInterna,
                h.Movimientos.OrderBy(m => m.Orden).Last().Persona,
                "Abierto")),
        ];
    }

    private async Task<IReadOnlyList<RenglonDelSaldo>> DiferenciasAbiertasAsync(
        DateOnly corte, CancellationToken cancelacion)
    {
        var filas = await contexto.DiferenciasDeConciliacion
            .Where(d => d.Resolucion == null && d.FechaDelHecho <= corte)
            .ToListAsync(cancelacion);

        return
        [
            .. filas.Select(d => new RenglonDelSaldo(
                TipoDeRenglon.ImputacionExternaNoResuelta,
                d.Id.ToString(),
                $"{d.Lado}: {d.Explicacion}",
                d.FechaDelHecho,

                // La que no se pudo atribuir a un vehículo depende del emisor, no de nosotros —
                // y `RN-97` tiene causa para eso: «que no dependa de nosotros no lo hace
                // inexistente».
                d.VehiculoId is null
                    ? CausaDelRenglon.FueraDelControlInstitucional
                    : CausaDelRenglon.PendienteDeGestionInterna,
                d.ResponsableDeSeguimiento ?? "",
                "Sin resolver",
                Monto: d.Monto)),
        ];
    }

    // ── La serie histórica ──────────────────────────────────────────────────

    public async Task<IReadOnlyList<SaldoDeApertura>> TodosAsync(
        CancellationToken cancelacion = default)
    {
        var filas = await contexto.SaldosDeApertura
            .Include(s => s.Renglones)
            .OrderByDescending(s => s.Ejercicio)
            .ToListAsync(cancelacion);

        return [.. filas.Select(A)];
    }

    public async Task<SaldoDeApertura?> DelEjercicioAsync(
        string ejercicio, CancellationToken cancelacion = default)
    {
        var fila = await contexto.SaldosDeApertura
            .Include(s => s.Renglones)
            .SingleOrDefaultAsync(s => s.Ejercicio == ejercicio, cancelacion);

        return fila is null ? null : A(fila);
    }

    /// <summary>
    /// `RN-97` punto 6 — el renglón resuelto se marca con su fecha, y <b>el residuo al cierre
    /// siguiente es el nuevo saldo</b>.
    ///
    /// No se borra: que estuvo en el saldo es parte de la serie histórica, y borrarlo haría que
    /// el arrastre entre ejercicios dejara de verse.
    /// </summary>
    public async Task ResolverRenglonAsync(
        Ulid renglon, string comoSeResolvio, DateOnly fecha,
        CancellationToken cancelacion = default)
    {
        if (string.IsNullOrWhiteSpace(comoSeResolvio))
            throw new BloqueoDuro("RN-97",
                "Marcar un renglón como resuelto exige decir cómo. Sin eso, resolver es " +
                "indistinguible de vaciar el saldo — que es la presión que `RN-97` nombra.");

        var fila = await contexto.Set<Datos.M14_Auditoria.FilaDeRenglon>()
            .SingleOrDefaultAsync(r => r.Id == renglon, cancelacion)
            ?? throw new RenglonNoEncontrado(renglon);

        if (fila.ResueltoEn is not null)
            throw new BloqueoDuro("RN-97",
                "Este renglón ya está marcado como resuelto. Reescribir su resolución borraría " +
                "la que constaba, y el saldo dejaría de ser el documento que fue.");

        fila.ResueltoEn = fecha;
        fila.ComoSeResolvio = comoSeResolvio.Trim();

        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>
    /// El saldo del ejercicio inmediatamente anterior, de donde sale el arrastre.
    ///
    /// <b>Sólo sus renglones no resueltos</b>: un renglón que se resolvió durante el ejercicio no
    /// arrastra —`RN-97` punto 6, el residuo es el nuevo saldo—, pero su marca de resolución
    /// queda en el documento anterior.
    /// </summary>
    private async Task<IReadOnlyList<RenglonDelSaldo>> UltimoAnteriorAsync(
        string ejercicio, CancellationToken cancelacion)
    {
        var fila = await contexto.SaldosDeApertura
            .Include(s => s.Renglones)
            .Where(s => string.Compare(s.Ejercicio, ejercicio) < 0)
            .OrderByDescending(s => s.Ejercicio)
            .FirstOrDefaultAsync(cancelacion);

        return fila is null ? [] : [.. A(fila).Renglones];
    }

    private async Task GuardarAsync(
        SaldoDeApertura saldo, string? declaracion, CancellationToken cancelacion)
    {
        var fila = new Datos.M14_Auditoria.FilaDeSaldo
        {
            Id = saldo.Id,
            Folio = saldo.Folio,
            Ejercicio = saldo.Ejercicio,
            Corte = saldo.Corte,
            Persona = saldo.Produce.Persona.Valor,
            Puesto = saldo.Produce.Puesto.Valor,
            MomentoUtc = saldo.Momento.UtcDateTime,
            DesfaseMinutos = (int)saldo.Momento.Offset.TotalMinutes,
            EsInicialDeImplantacion = saldo.EsInicialDeImplantacion,
            DeclaracionDeBloqueantes = declaracion?.Trim(),

            // Las fuentes no consultadas van al documento, no a una nota. Sin ellas el saldo se
            // ve completo estando incompleto.
            FuentesNoConsultadas = string.Join(" · ",
                saldo.SinConsultar.Select(f => $"{f.Tipo}: {f.PorQueNo}")),
        };

        foreach (var r in saldo.Renglones)
        {
            fila.Renglones.Add(new Datos.M14_Auditoria.FilaDeRenglon
            {
                Id = Ulid.NewUlid(),
                SaldoId = saldo.Id,
                Tipo = r.Tipo,
                Referencia = r.Referencia,
                Descripcion = r.Descripcion,
                FechaDelHecho = r.FechaDelHecho,
                Causa = r.Causa,
                Responsable = r.Responsable,
                Estado = r.Estado,
                SaldosAnteriores = r.SaldosAnteriores,
                Monto = r.Monto,
            });
        }

        contexto.SaldosDeApertura.Add(fila);
        await contexto.SaveChangesAsync(cancelacion);
    }

    private static SaldoDeApertura A(Datos.M14_Auditoria.FilaDeSaldo f) =>
        new(f.Id, f.Folio, f.Ejercicio, f.Corte,
            [.. f.Renglones
                .Where(r => r.ResueltoEn is null)
                .Select(r => new RenglonDelSaldo(
                    r.Tipo, r.Referencia, r.Descripcion, r.FechaDelHecho, r.Causa,
                    r.Responsable, r.Estado, r.SaldosAnteriores, r.Monto))],
            [],
            Autoria.De(new Dominio.Organizacion.IdPersona(f.Persona), new IdPuesto(f.Puesto),
                f.Corte),
            new DateTimeOffset(f.MomentoUtc, TimeSpan.Zero)
                .ToOffset(TimeSpan.FromMinutes(f.DesfaseMinutos)),
            f.EsInicialDeImplantacion);
}

public sealed class RenglonNoEncontrado(Ulid id)
    : Exception($"No existe el renglón {id} en ningún saldo de apertura.");
