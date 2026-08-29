using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Datos.M07_ProgramacionYDespacho;
using Sigti.Datos.M02_Parametros;
using Sigti.Datos.M14_Auditoria;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.M14_Auditoria;
using Sigti.Dominio.M20_Integraciones;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M14_Auditoria;

/// <summary>
/// El cierre de ejercicio como corte de imputación y de reporte — `RN-96`.
///
/// ── Lo que este servicio no tiene, y es su definición ───────────────────────
/// <b>Ningún método mueve un expediente.</b> `RN-96` es explícita: <i>«no ejecuta ni habilita
/// ninguna transición de la Orden de Misión. Ningún expediente cambia de estado por efecto de
/// una fecha»</i>.
///
/// La regla existe porque el riesgo tiene nombre: <i>«sin esta regla escrita la primera
/// implementación va a poner un cierre masivo por fecha, porque es lo que resuelve ese
/// problema»</i>. Lo que hace este servicio es <b>armar el acta y hacer visible el apuro</b> —
/// no resolverlo cerrando en bloque.
///
/// La única escritura sobre otro agregado es <see cref="AnularFoliosAsync"/>, que es un acto
/// aparte, con autor y motivo, y que sólo toca folios <b>no consumidos</b>.
/// </summary>
public sealed class ServicioDeCierreDeEjercicio(
    SigtiDbContext contexto, ServicioDeSaldoDeApertura saldos)
{
    /// <summary>
    /// El catálogo bitemporal, construido sobre el mismo contexto — igual que
    /// <c>ServicioDeParametros</c>. Es un repositorio con intención (`ADR-009`), no un servicio
    /// con estado: registrarlo en el contenedor solo para esto agregaría una pieza que nadie
    /// más pide.
    /// </summary>
    private readonly ParametrosNormativos _parametros = new(contexto);

    /// <summary>
    /// Las dos fechas de corte del ejercicio — `RN-96`, <b>parámetros con vigencia</b>.
    ///
    /// ── Resueltas al último día del ejercicio, no al corte ──────────────────
    /// Sería circular resolverlas a la fecha del corte: hace falta el corte para saberla. Se
    /// resuelven al 31 de diciembre del ejercicio, que se conoce sin depender de nada, y por eso
    /// una versión que empieza a regir a mitad de año ya alcanza a ese ejercicio.
    /// </summary>
    public async Task<(CortesDelEjercicio? Cortes, CortesSinResolver? Sin)> CortesAsync(
        string ejercicio, DateTimeOffset momento, CancellationToken cancelacion = default)
    {
        if (!int.TryParse(ejercicio, out var anio) || anio is < 1900 or > 9999)
            return ReglasDelCierreDeEjercicio.CortesDe(ejercicio, null, null, null, null);

        var alCierre = new DateOnly(anio, 12, 31);

        var legal = (await _parametros.CatalogoDeAsync(
                ReglasDelCierreDeEjercicio.ClaveDelCorteLegal, cancelacion))
            .ResolverSiHay(ReglasDelCierreDeEjercicio.ClaveDelCorteLegal, alCierre, momento);

        var operativo = (await _parametros.CatalogoDeAsync(
                ReglasDelCierreDeEjercicio.ClaveDelCorteOperativo, cancelacion))
            .ResolverSiHay(ReglasDelCierreDeEjercicio.ClaveDelCorteOperativo, alCierre, momento);

        return ReglasDelCierreDeEjercicio.CortesDe(
            ejercicio,
            legal?.Valor, legal?.VigenteDesde,
            operativo?.Valor, operativo?.VigenteDesde);
    }

    /// <summary>
    /// Arma el acta sin congelarla. Es lo que la pantalla muestra antes de producir.
    ///
    /// ── Los cortes salen del parámetro; imponerlos es explorar ──────────────
    /// `RN-96` declara las dos fechas configurables con vigencia. Quien no pase nada obtiene las
    /// de la institución; quien las imponga está preguntando <i>«qué pasaría si…»</i>, y el acta
    /// lo dice en su origen para que esa respuesta no se confunda con el cierre real.
    ///
    /// <b>Producir con cortes impuestos no se puede</b>, y por eso
    /// <see cref="ProducirAsync"/> ni siquiera los recibe.
    /// </summary>
    /// <param name="corteLegal">Nulo toma el del parámetro. Pasarlo es explorar.</param>
    /// <param name="corteOperativo">Ídem. Si se pasa uno hay que pasar los dos.</param>
    public async Task<ActaDeCierreDeEjercicio> ArmarAsync(
        string ejercicio,
        DateOnly? corteLegal,
        DateOnly? corteOperativo,
        Autoria ejecuta,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        string origenDeLosCortes;

        if (corteLegal is null || corteOperativo is null)
        {
            var (cortes, sin) = await CortesAsync(ejercicio, momento, cancelacion);

            if (cortes is null)
                throw new BloqueoDuro("RN-96",
                    $"Las fechas de corte del ejercicio no están parametrizadas: {sin!.PorQueNo} " +
                    "Sin ellas no se puede armar el acta: los cortes deciden qué expedientes " +
                    "entran al inventario y a qué ejercicio se imputa cada hecho, y suponerlos " +
                    "falsearía todo lo demás. Se cargan en " +
                    $"«{ReglasDelCierreDeEjercicio.ClaveDelCorteLegal}» y " +
                    $"«{ReglasDelCierreDeEjercicio.ClaveDelCorteOperativo}», con vigencia y " +
                    "doble control.");

            corteLegal = cortes.Legal;
            corteOperativo = cortes.Operativo;
            origenDeLosCortes = cortes.Origen;
        }
        else
        {
            // **Exploración, y el acta lo dice.** Una vista previa con cortes impuestos responde
            // «qué pasaría si», y esa respuesta no se puede confundir con el cierre real.
            origenDeLosCortes =
                "Cortes impuestos en la vista previa — NO son los parámetros de la institución.";
        }

        // El folio no se exige para armar: armar es mirar, no producir. `ExigirCortes` corre
        // igual con el folio de la vista previa para que la coherencia entre fechas se juzgue
        // antes de que alguien apriete producir.
        ReglasDelCierreDeEjercicio.ExigirCortes(
            "(vista previa)", ejercicio, corteLegal.Value, corteOperativo.Value);

        var legal = corteLegal.Value;
        var operativo = corteOperativo.Value;

        var (inventario, _) = await saldos.InventarioAsync(operativo, cancelacion);

        var saldo = await saldos.DelEjercicioAsync(ejercicio, cancelacion);

        // **Contra el saldo congelado, si existe.** Si no, la lista de diferencias va vacía y el
        // acta lo dice por el folio nulo: no es que cuadre, es que no hay contra qué cuadrar.
        var diferencias = saldo is null
            ? []
            : ReglasDelSaldoDeApertura.DiferenciasContraElInventario(saldo.Renglones, inventario);

        // ── La ventana se resuelve UNA vez y se pasa a lo que la usa ────────
        // Resolverla dentro de cada reporte dejaría que uno la leyera parametrizada y otro no,
        // y el acta mostraría dos secciones medidas contra ventanas distintas sin decirlo.
        var (ventana, sinVentana) = await VentanaAsync(legal, operativo, momento, cancelacion);

        return new ActaDeCierreDeEjercicio(
            Ulid.NewUlid(),
            "(vista previa)",
            ejercicio,
            legal,
            operativo,
            ejecuta,
            momento,
            inventario,
            await MisionesQueCruzanAsync(legal, operativo, cancelacion),
            await FoliosPorAnularAsync(operativo, cancelacion),

            // El registro de cambios de parámetros **sí necesita la ventana**, y por eso sale
            // vacío cuando no la hay: buscar cambios «en la ventana» sin ventana devolvería el
            // año entero, que es otra pregunta.
            ventana is null
                ? []
                : await CambiosDeParametrosAsync(ventana, cancelacion),

            ventana is null
                ? []
                : await MotivosCompartidosAsync(ventana, cancelacion),

            ventana is null
                ? null
                : await ApuroAsync(ejercicio, ventana, cancelacion),

            diferencias,
            saldo?.Folio,
            ventana,
            sinVentana,
            origenDeLosCortes);
    }

    /// <summary>
    /// La ventana de cierre — `RN-96`, <b>parámetro con vigencia</b>.
    ///
    /// ── Resuelta a la fecha del corte legal, no a hoy ───────────────────────
    /// `RN-40`: reevaluar el cierre de 2026 tiene que usar la ventana que regía entonces. Y el
    /// eje de transacción va al <c>momento</c> del acta, no a <c>UtcNow</c>: reabrir un acta
    /// producida en enero reproduce lo que se vio ese día, en vez de recalcularlo con lo que se
    /// cargó después.
    ///
    /// ── El parámetro que se mide a sí mismo ─────────────────────────────────
    /// La ventana es un parámetro como cualquier otro, así que <b>moverla dentro de la ventana
    /// aparece en el reporte de `RN-96` punto 6</b>. Es deseable: ensanchar la ventana en
    /// diciembre cambia qué cierres se miran, y eso tiene que quedar a la vista igual que
    /// aflojar un umbral.
    /// </summary>
    private async Task<(VentanaDeCierre? Ventana, VentanaSinResolver? Sin)> VentanaAsync(
        DateOnly corteLegal, DateOnly corteOperativo, DateTimeOffset momento,
        CancellationToken cancelacion)
    {
        var catalogo = await _parametros.CatalogoDeAsync(
            ReglasDelCierreDeEjercicio.ClaveDeLaVentana, cancelacion);

        var resuelto = catalogo.ResolverSiHay(
            ReglasDelCierreDeEjercicio.ClaveDeLaVentana, corteLegal, momento);

        return ReglasDelCierreDeEjercicio.VentanaDe(
            resuelto?.Valor, resuelto?.VigenteDesde, corteLegal, corteOperativo);
    }

    /// <summary>
    /// Produce el acta — `RN-96` punto 1: <b>documento con folio</b>.
    ///
    /// Congela el folio del saldo que cita, las diferencias vistas ese día y la lista de folios
    /// a anular. <b>No anula nada</b>: eso es <see cref="AnularFoliosAsync"/>.
    ///
    /// ── NO recibe las fechas de corte, y es deliberado ──────────────────────
    /// Salen de los parámetros de la institución. Recibirlas dejaría producir un acta contra un
    /// corte que alguien escribió en el momento, y los cortes deciden qué expedientes entran al
    /// inventario y a qué ejercicio se imputa cada hecho: sería el documento del cierre
    /// afirmando sobre todo lo demás contra un criterio que nadie autorizó.
    ///
    /// Explorar otras fechas se hace en <see cref="ArmarAsync"/>, que las admite y marca el acta
    /// como exploratoria.
    /// </summary>
    public async Task<ActaDeCierreDeEjercicio> ProducirAsync(
        string folio,
        string ejercicio,
        Autoria ejecuta,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        ReglasDelCierreDeEjercicio.ExigirFolioDelActa(folio, ejercicio);

        if (await contexto.ActasDeCierre.AnyAsync(a => a.Ejercicio == ejercicio, cancelacion))
            throw new BloqueoDuro("RN-96",
                $"Ya hay un acta de cierre para el ejercicio {ejercicio}. Producir una segunda " +
                "dejaría dos documentos del mismo cierre, y ni el saldo de apertura ni el acta " +
                "de anulación de folios podrían decir cuál citan. Lo que cambia después se " +
                "resuelve en el ejercicio corriente, con expediente de hallazgo posterior " +
                "(`RN-93`) — no reescribiendo el acta.");

        var vista = await ArmarAsync(ejercicio, null, null, ejecuta, momento, cancelacion);

        var saldo = await saldos.DelEjercicioAsync(ejercicio, cancelacion);

        var acta = vista with { Id = Ulid.NewUlid(), Folio = folio.Trim() };

        var fila = new FilaDeActaDeCierre
        {
            Id = acta.Id,
            Folio = acta.Folio,
            Ejercicio = acta.Ejercicio,
            CorteLegal = vista.CorteLegal,
            CorteOperativo = vista.CorteOperativo,
            Persona = ejecuta.Persona.ToString(),
            Puesto = ejecuta.Puesto.ToString(),
            MomentoUtc = momento.UtcDateTime,
            DesfaseMinutos = (int)momento.Offset.TotalMinutes,
            SaldoDeAperturaFolio = saldo?.Folio,
            DiferenciasConElSaldo = Truncar(
                string.Join(" · ", acta.DiferenciasConElSaldo), 4000),
            Observaciones = Truncar(string.Join(" · ", acta.Observaciones), 4000),
        };

        foreach (var f in acta.FoliosPorAnular)
            fila.Folios.Add(new FilaDeFolioDelActa
            {
                Id = Ulid.NewUlid(),
                ActaId = acta.Id,
                AsignacionId = f.Asignacion,
                Folio = f.Folio,
                Delegacion = f.Delegacion,
                Monto = f.Monto,
                Emitido = f.Emitido,
                Estado = f.Estado,
                SePuedeAnular = f.SePuedeAnular,
            });

        contexto.ActasDeCierre.Add(fila);
        await contexto.SaveChangesAsync(cancelacion);

        return acta;
    }

    /// <summary>
    /// `RN-96` punto 5 — el <b>acta de anulación de folios</b> no consumidos.
    ///
    /// ── Por qué es un acto y no un efecto de producir el acta ───────────────
    /// Porque anular decenas de folios al producirse un documento sería un cierre masivo por
    /// fecha con otro nombre. Acá hay autor, motivo y momento, y cada folio queda con su asiento
    /// `V-03` propio en el diario del vale.
    ///
    /// <b>Sólo toca los que se pueden anular.</b> El vale entregado no se anula: su camino es la
    /// devolución con acta o la obligación de reintegro (`RN-86`), y forzarlo acá metería por la
    /// puerta de atrás una transición que `V-03` prohíbe.
    /// </summary>
    /// <returns>Cuántos folios se anularon.</returns>
    public async Task<int> AnularFoliosAsync(
        string ejercicio,
        IdPersona ejecuta,
        string motivo,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new BloqueoDuro("RN-96",
                "La anulación de folios exige motivo: es lo que distingue el acta de anulación " +
                "de un borrado en bloque. `RN-27` lo pide para cada folio, y aquí van varios.");

        var fila = await contexto.ActasDeCierre
            .Include(a => a.Folios)
            .SingleOrDefaultAsync(a => a.Ejercicio == ejercicio, cancelacion)
            ?? throw new BloqueoDuro("RN-96",
                $"No hay acta de cierre del ejercicio {ejercicio}. Los folios se anulan " +
                "citando el acta que los listó: sin ella no consta que estos folios fueran los " +
                "que quedaron reservados y sin consumir al corte.");

        var pendientes = fila.Folios
            .Where(f => f.SePuedeAnular && f.AnuladoUtc is null)
            .ToList();

        if (pendientes.Count == 0) return 0;

        var ids = pendientes.Select(f => f.AsignacionId).ToHashSet();

        var vales = await contexto.AsignacionesDeCombustible
            .Include(a => a.Transiciones)
            .Where(a => ids.Contains(a.Id))
            .ToListAsync(cancelacion);

        var anulados = 0;

        foreach (var f in pendientes)
        {
            var vale = vales.SingleOrDefault(v => v.Id == f.AsignacionId);
            if (vale is null) continue;

            var actual = vale.Transiciones.OrderBy(t => t.Orden).Last();

            // **Pudo moverse entre el acta y la anulación**, y eso es normal: el acta es una
            // foto al corte y la operación siguió. Un vale que ya se entregó o se consumió
            // desde entonces no se anula — se deja constar que no procedía.
            if (actual.Destino is not EstadoDeAsignacion.Emitida) continue;

            vale.Transiciones.Add(new Datos.M09_Combustible.FilaDeTransicionDeAsignacion
            {
                Id = Ulid.NewUlid(),
                AsignacionId = vale.Id,
                Orden = actual.Orden + 1,
                Transicion = "V-03",
                Destino = EstadoDeAsignacion.Anulada,
                Ejecuta = ejecuta.ToString(),
                MomentoUtc = momento.UtcDateTime,
                DesfaseMinutos = (int)momento.Offset.TotalMinutes,
                Motivo = $"Acta de cierre {fila.Folio}, ejercicio {fila.Ejercicio}: {motivo}",
                Devuelto = vale.Monto,
            });

            f.AnuladoUtc = momento.UtcDateTime;
            f.AnuladoPor = ejecuta.ToString();
            anulados++;
        }

        await contexto.SaveChangesAsync(cancelacion);
        return anulados;
    }

    /// <summary>
    /// El reporte de reversión de compromisos — `RN-96` punto 5, para ARGOS y SIAFI (`RN-81`).
    ///
    /// ── Reporta lo que se revirtió, no lo que se listó ──────────────────────
    /// Sale de los folios del acta que <b>efectivamente se anularon</b>. Uno listado y todavía
    /// sin anular no liberó nada: su compromiso sigue vivo en SIGTI, y reportarlo haría que
    /// SIAFI revirtiera un dinero que acá sigue comprometido — el descuadre simétrico del que
    /// `RN-81` existe para impedir.
    ///
    /// ── El liberado es neto ─────────────────────────────────────────────────
    /// Se le resta lo que se hubiera consumido contra ese vale antes de anularlo. Para un vale
    /// que se anula desde <c>Emitida</c> eso es cero, pero se calcula en vez de suponerse: el
    /// día que un vale llegue a la anulación con consumo, el reporte lo va a decir en vez de
    /// devolver el bruto.
    /// </summary>
    /// <param name="corteDeConocimiento">
    /// `RN-94` — hasta qué momento se miran los registros. <b>Es lo que hace el reporte
    /// reproducible</b>: el mismo período con el mismo corte da el mismo resultado dentro de
    /// cinco años.
    /// </param>
    public async Task<ReporteDeReversion?> ReversionAsync(
        string ejercicio,
        DateTimeOffset corteDeConocimiento,
        CancellationToken cancelacion = default)
    {
        var acta = await contexto.ActasDeCierre
            .Include(a => a.Folios)
            .SingleOrDefaultAsync(a => a.Ejercicio == ejercicio, cancelacion);

        if (acta is null) return null;

        var hasta = corteDeConocimiento.UtcDateTime;

        var anulados = acta.Folios
            .Where(f => f.AnuladoUtc is not null && f.AnuladoUtc <= hasta)
            .ToList();

        var renglones = new List<CompromisoLiberado>();

        if (anulados.Count > 0)
        {
            var ids = anulados.Select(f => f.AsignacionId).ToHashSet();

            var vales = await contexto.AsignacionesDeCombustible
                .Where(a => ids.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id, cancelacion);

            var fondos = await contexto.Fondos.ToDictionaryAsync(f => f.Id, cancelacion);

            // Lo consumido contra estos vales **hasta el corte de conocimiento**. Un consumo
            // digitado después no cambia un reporte ya emitido: `RN-94` lo llama capa
            // identificada, no corrección del histórico.
            var consumido = await contexto.Abastecimientos
                .Where(a => a.AsignacionId != null
                    && ids.Contains(a.AsignacionId.Value)
                    && a.MomentoUtc <= hasta)
                .GroupBy(a => a.AsignacionId!.Value)
                .Select(g => new { Asignacion = g.Key, Monto = g.Sum(a => a.Monto ?? 0m) })
                .ToDictionaryAsync(x => x.Asignacion, x => x.Monto, cancelacion);

            foreach (var f in anulados)
            {
                if (!vales.TryGetValue(f.AsignacionId, out var vale)) continue;

                var anulado = f.AnuladoUtc!.Value;

                renglones.Add(new CompromisoLiberado(
                    // ⚠️ El ULID de la misión: **la clave de vinculación con ARGOS no existe**
                    // como campo. El modelo de datos la nombra, el código no la tiene.
                    vale.MisionId.ToString(),
                    vale.MisionId,
                    f.Folio,
                    f.Delegacion,

                    // Nula cuando el espejo de ARGOS no la traía — `RN-26` deja registrar el
                    // fondo igual, y ese renglón no se puede imputar en SIAFI.
                    fondos.TryGetValue(vale.FondoId, out var fondo)
                        ? fondo.PartidaPresupuestaria
                        : null,

                    DateOnly.FromDateTime(anulado),
                    DateOnly.FromDateTime(anulado),
                    f.Monto,
                    consumido.GetValueOrDefault(f.AsignacionId)));
            }
        }

        // El período del hecho es el del cierre: del corte legal al operativo. Los compromisos
        // se liberan por ese cierre, aunque la anulación se ejecute en enero.
        ReglasDeLaReversion.ExigirLasDosFechas(
            acta.CorteLegal, acta.CorteOperativo, corteDeConocimiento);

        return new ReporteDeReversion(
            acta.Ejercicio,
            acta.CorteLegal,
            acta.CorteOperativo,
            corteDeConocimiento,
            acta.Folio,
            [.. renglones.OrderBy(r => r.Delegacion).ThenBy(r => r.Folio)]);
    }

    public async Task<IReadOnlyList<(string Ejercicio, string Folio, DateOnly CorteLegal,
        DateOnly CorteOperativo, int Folios, int Anulados, decimal Monto,
        string? SaldoDeAperturaFolio)>> ProducidasAsync(CancellationToken cancelacion = default)
    {
        var filas = await contexto.ActasDeCierre
            .Include(a => a.Folios)
            .OrderByDescending(a => a.Ejercicio)
            .ToListAsync(cancelacion);

        return
        [
            .. filas.Select(a => (
                a.Ejercicio,
                a.Folio,
                a.CorteLegal,
                a.CorteOperativo,
                a.Folios.Count,
                a.Folios.Count(f => f.AnuladoUtc is not null),
                a.Folios.Where(f => f.SePuedeAnular).Sum(f => f.Monto),
                a.SaldoDeAperturaFolio)),
        ];
    }

    // ── Las piezas del acta ─────────────────────────────────────────────────

    /// <summary>
    /// `RN-96` punto 4 — las misiones que cruzaron el corte, con su desglose por ejercicio.
    ///
    /// ── La misión no se divide; sus hechos se imputan ───────────────────────
    /// Se buscan las que <b>salieron antes del corte legal y retornaron después</b>. Cada gasto
    /// —combustible y peaje— se imputa al ejercicio de <b>su propia fecha</b>, no a la de la
    /// misión ni a la de la liquidación.
    /// </summary>
    private async Task<IReadOnlyList<MisionQueCruza>> MisionesQueCruzanAsync(
        DateOnly corteLegal, DateOnly corteOperativo, CancellationToken cancelacion)
    {
        var filas = await contexto.Expedientes
            .Where(e => e.Salida <= corteLegal && e.Retorno > corteLegal)
            .ToListAsync(cancelacion);

        if (filas.Count == 0) return [];

        var ids = filas.Select(e => e.Id).ToHashSet();

        var abastecimientos = await contexto.Abastecimientos
            .Where(a => a.MisionId != null && ids.Contains(a.MisionId.Value))
            .ToListAsync(cancelacion);

        var pasos = await contexto.PasosPorCaseta
            .Where(p => p.MisionId != null && ids.Contains(p.MisionId.Value))
            .ToListAsync(cancelacion);

        var misiones = new List<MisionQueCruza>();

        foreach (var e in filas)
        {
            var hechos = new List<HechoImputado>();

            foreach (var a in abastecimientos.Where(a => a.MisionId == e.Id))
            {
                var fecha = DateOnly.FromDateTime(a.MomentoUtc.AddMinutes(a.DesfaseMinutos));

                hechos.Add(new HechoImputado(
                    ReglasDelCierreDeEjercicio.EjercicioDe(fecha),
                    fecha,
                    $"Combustible, {a.Galones:N2} gal en {a.Estacion ?? "tanque institucional"}",
                    a.Monto ?? 0m,

                    // El monto del combustible **no sale de una tabla paramétrica**: sale del
                    // comprobante. Declararlo así es lo que hace reproducible el cálculo — y el
                    // que no trae comprobante queda sin declarar, que es el hueco real.
                    a.Comprobante is null
                        ? null
                        : $"Comprobante {a.Comprobante}"));
            }

            foreach (var p in pasos.Where(p => p.MisionId == e.Id))
            {
                var fecha = DateOnly.FromDateTime(p.MomentoUtc.AddMinutes(p.DesfaseMinutos));

                hechos.Add(new HechoImputado(
                    ReglasDelCierreDeEjercicio.EjercicioDe(fecha),
                    fecha,
                    $"Peaje, categoría {p.CategoriaCobrada ?? "sin clasificar"}",
                    p.MontoPagado,

                    // Acá sí hay tabla: la tarifa vigente a la fecha del paso (`RN-40`). El que
                    // no la tiene es el que pasó por una caseta que el catálogo no conocía.
                    p.CategoriaEsperada is null
                        ? null
                        : $"Tarifa de peaje vigente al {fecha:dd/MM/yyyy}, " +
                          $"categoría {p.CategoriaEsperada}"));
            }

            if (hechos.Count == 0) continue;

            // Sólo interesa si sus hechos caen en más de un ejercicio. Una misión que cruzó la
            // fecha pero gastó todo antes no plantea el problema de imputación que `RN-96`
            // resuelve, y listarla diluiría las que sí.
            if (hechos.Select(h => h.Ejercicio).Distinct().Count() < 2) continue;

            misiones.Add(new MisionQueCruza(
                e.Id, e.Id.ToString(), e.Salida, e.Retorno, hechos));
        }

        _ = corteOperativo;
        return misiones;
    }

    /// <summary>
    /// `RN-96` — los folios reservados y no consumidos al corte.
    ///
    /// <b>Por el diario, no por una columna de estado</b> (P-1), y <b>al corte</b>: un vale que
    /// se consumió en enero no era un folio ocioso el 31 de diciembre.
    /// </summary>
    private async Task<IReadOnlyList<FolioPorAnular>> FoliosPorAnularAsync(
        DateOnly corte, CancellationToken cancelacion)
    {
        var hasta = corte.ToDateTime(TimeOnly.MaxValue);

        var vales = await contexto.AsignacionesDeCombustible
            .Include(a => a.Transiciones)
            .ToListAsync(cancelacion);

        var fondos = await contexto.Fondos.ToDictionaryAsync(f => f.Id, cancelacion);

        var folios = new List<FolioPorAnular>();

        foreach (var vale in vales)
        {
            var alCorte = vale.Transiciones
                .Where(t => t.MomentoUtc <= hasta)
                .OrderBy(t => t.Orden)
                .LastOrDefault();

            if (alCorte is null) continue;

            // Emitida: compromiso reservado y sin consumir — se anula.
            // Entregada: dinero fuera de la caja al cierre — se lista, no se anula.
            if (alCorte.Destino is not (EstadoDeAsignacion.Emitida or EstadoDeAsignacion.Entregada))
                continue;

            var primera = vale.Transiciones.OrderBy(t => t.Orden).First();

            folios.Add(new FolioPorAnular(
                vale.Id,
                vale.Folio,
                fondos.TryGetValue(vale.FondoId, out var fondo)
                    ? fondo.AmbitoDeclarado
                    : "(fondo no encontrado)",
                vale.Monto,
                DateOnly.FromDateTime(primera.MomentoUtc),
                alCorte.Destino.ToString(),
                SePuedeAnular: alCorte.Destino is EstadoDeAsignacion.Emitida));
        }

        return [.. folios.OrderBy(f => f.Delegacion).ThenBy(f => f.Folio)];
    }

    /// <summary>
    /// `RN-96` punto 6 — el registro de cambios de parámetros en la ventana de cierre.
    ///
    /// ── Se busca por el eje de TRANSACCIÓN, no por el de vigencia ───────────
    /// Lo que interesa es <b>cuándo se registró el cambio</b>, no desde cuándo rige. Un umbral
    /// cargado el 28 de diciembre con vigencia retroactiva a enero es precisamente el caso que
    /// la regla quiere ver, y buscar por `VigenteDesde` lo dejaría fuera. La bitemporalidad de
    /// `ADR-006` es lo que hace posible esta consulta.
    /// </summary>
    private async Task<IReadOnlyList<CambioDeParametro>> CambiosDeParametrosAsync(
        VentanaDeCierre ventana, CancellationToken cancelacion)
    {
        var desde = new DateTimeOffset(
            ventana.Desde.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        var hasta = new DateTimeOffset(
            ventana.Hasta.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        var enLaVentana = await contexto.Parametros
            .Where(p => p.RegistradoDesde >= desde && p.RegistradoDesde <= hasta)
            .OrderBy(p => p.RegistradoDesde)
            .ToListAsync(cancelacion);

        if (enLaVentana.Count == 0) return [];

        // El valor anterior sale de la versión que regía inmediatamente antes en la misma clave.
        // Sin él el reporte diría «se cargó 15%» sin decir que venía de 5%, que es la mitad que
        // importa.
        var claves = enLaVentana.Select(p => p.Clave).ToHashSet();

        var historia = await contexto.Parametros
            .Where(p => claves.Contains(p.Clave))
            .ToListAsync(cancelacion);

        return
        [
            .. enLaVentana.Select(p => new CambioDeParametro(
                p.Clave,
                historia
                    .Where(x => x.Clave == p.Clave && x.RegistradoDesde < p.RegistradoDesde)
                    .OrderByDescending(x => x.RegistradoDesde)
                    .FirstOrDefault()?.Valor,
                p.Valor,
                p.VigenteDesde,
                p.RegistradoDesde,
                p.CargadoPor.ToString(),
                p.AprobadoPor?.ToString())),
        ];
    }

    /// <summary>
    /// `RN-96` punto 3 — los motivos de cierre compartidos por varios expedientes.
    ///
    /// Se miran los cierres <b>de la ventana</b>: un motivo que se repite a lo largo del año
    /// puede ser una causa real que vuelve; el mismo motivo en decenas de expedientes en la
    /// semana del cierre es un cierre en bloque.
    /// </summary>
    private async Task<IReadOnlyList<MotivoCompartido>> MotivosCompartidosAsync(
        VentanaDeCierre ventana, CancellationToken cancelacion)
    {
        var desde = ventana.Desde.ToDateTime(TimeOnly.MinValue);
        var hasta = ventana.Hasta.ToDateTime(TimeOnly.MaxValue);

        var cierres = await contexto.Set<FilaDeTransicion>()
            .Where(t => t.MomentoUtc >= desde && t.MomentoUtc <= hasta
                && (t.Destino == EstadoDeMision.Cerrada
                    || t.Destino == EstadoDeMision.CerradaConHallazgo))
            .ToListAsync(cancelacion);

        return ReglasDelCierreDeEjercicio.DetectarMotivosCompartidos(
        [
            .. cierres.Select(t => (
                t.ExpedienteId,
                t.Motivo ?? "",
                new DateTimeOffset(t.MomentoUtc, TimeSpan.Zero))),
        ]);
    }

    /// <summary>
    /// El indicador de cierre apurado — `RN-96` casos límite: <i>«el sistema no la resuelve; la
    /// hace visible»</i>.
    /// </summary>
    private async Task<CierreApurado> ApuroAsync(
        string ejercicio, VentanaDeCierre ventana, CancellationToken cancelacion)
    {
        if (!int.TryParse(ejercicio, out var anio))
            return new CierreApurado(0, 0, 0, null);

        var inicio = new DateTime(anio, 1, 1);
        var fin = new DateTime(anio, 12, 31, 23, 59, 59);

        var cierres = await contexto.Set<FilaDeTransicion>()
            .Where(t => t.MomentoUtc >= inicio && t.MomentoUtc <= fin
                && (t.Destino == EstadoDeMision.Cerrada
                    || t.Destino == EstadoDeMision.CerradaConHallazgo))
            .Select(t => t.MomentoUtc)
            .ToListAsync(cancelacion);

        return ReglasDelCierreDeEjercicio.Apuro(
            [.. cierres.Select(DateOnly.FromDateTime)], ventana.Desde, ventana.Hasta);
    }

    private static string Truncar(string texto, int largo) =>
        texto.Length <= largo ? texto : texto[..largo];
}
