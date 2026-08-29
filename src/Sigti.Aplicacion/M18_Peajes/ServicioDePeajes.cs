using Microsoft.EntityFrameworkCore;
using Sigti.Aplicacion.M03_Flota;
using Sigti.Datos;
using Sigti.Datos.M18_Peajes;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M18_Peajes;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M18_Peajes;

/// <summary>
/// M-18 — peajes.
///
/// ── Lo que este módulo tiene que impedir ────────────────────────────────────
/// Dos cosas concretas, las dos documentadas con evidencia en `NRM-10`:
///
/// <b>Que el sistema le cobre mal a su propia flota.</b> Un pickup de dos ejes paga L 22 y un
/// «Vehículo de 2 Ejes» paga L 90; resolver por ejes multiplicaría por cuatro el estimado de
/// cada pickup. `RN-33` lo prohíbe y esta capa no tiene una sola línea de aritmética sobre
/// ejes.
///
/// <b>Que un cobro indebido en caseta se vuelva la verdad institucional.</b> COVI-H reclasificó
/// H-100, K2700 y Sprinter en 2025 y la SAPP tuvo que ordenarle suspenderlo `[V]`. `RN-36`:
/// el cobro se registra como hecho y la categoría del vehículo no se toca.
/// </summary>
public sealed class ServicioDePeajes(SigtiDbContext contexto)
{
    private readonly PeajesDelPais _peajes = new(contexto);
    private readonly ConsultaDeFlota _flota = new(contexto);

    // ── La categoría del vehículo ───────────────────────────────────────────

    /// <summary>
    /// `RN-33` — deriva la categoría de una unidad concreta desde su ficha técnica.
    ///
    /// Es la que vale para programar (`BD-07`) y la que se compara con lo cobrado en caseta
    /// (`RN-36`).
    /// </summary>
    /// <param name="fechaDelHecho">
    /// Contra qué versión de la matriz se juzga (P-4). La SAPP reclasifica por resolución y ya
    /// lo hizo: la reclasificación <b>no reescribe el pasado</b>, abre una nueva vigencia.
    /// </param>
    public async Task<CategoriaResuelta> CategoriaDelVehiculoAsync(
        Ulid vehiculo, DateOnly fechaDelHecho, CancellationToken cancelacion = default)
    {
        var fila = await _flota.PorIdAsync(vehiculo, cancelacion);

        if (fila is null)
            return new CategoriaResuelta(null, BaseDeLaCategoria.VehiculoAsignado,
                "El vehículo no está en el padrón de flota.");

        var matriz = await _peajes.MatrizAsync(cancelacion);
        var nombres = await _peajes.NombresDeCategoriaAsync(cancelacion);

        return ReglasDeCategoriaDePeaje.Derivar(
            fila.Ficha(), matriz, nombres, fechaDelHecho, DateTimeOffset.UtcNow,

            // ⚠️ **Siempre provisional**, y no es un descuido. `RN-33`: el criterio legal es el
            // Artículo 51 de la Ley de Tránsito, y el PDF oficial es un escaneo sin capa de
            // texto (`[C]`, insumo #23). La matriz no se puede fijar definitivamente hasta
            // obtenerlo, y decirlo en cada resultado es lo que impide que una categoría
            // provisional se cite después como firme.
            matrizProvisional: true);
    }

    // ── La estimación ───────────────────────────────────────────────────────

    /// <summary>
    /// El estimado de una ruta — `RN-35`.
    /// </summary>
    /// <param name="vehiculo">
    /// Nulo en la estimación previa de `T-02`, donde todavía no hay unidad asignada. Entonces la
    /// categoría sale del <b>tipo requerido</b> y el estimado lo dice.
    /// </param>
    /// <param name="cruces">
    /// Punto y <b>cuántas veces se cruza</b>. Se recibe resuelto porque el trazado de la ruta
    /// contra el mapa es de ARGOS (`DP-001`), no de SIGTI: acá se valora lo que la ruta
    /// atraviesa, no se calcula por dónde pasa.
    /// </param>
    public async Task<Estimacion> EstimarAsync(
        IReadOnlyList<(Ulid Punto, int Cruces)> cruces,
        Ulid? vehiculo,
        CategoriaDePeaje? categoriaDelTipo,
        string tipoDeVehiculo,
        DateOnly fechaPrevista,
        CancellationToken cancelacion = default)
    {
        var categoria = vehiculo is { } idVehiculo
            ? await CategoriaDelVehiculoAsync(idVehiculo, fechaPrevista, cancelacion)
            : ReglasDeCategoriaDePeaje.DelTipoRequerido(
                categoriaDelTipo, tipoDeVehiculo, provisional: true);

        var puntos = await _peajes.PuntosAsync(cancelacion);
        var porId = puntos.ToDictionary(p => p.Id);

        var ruta = new List<CruceDeRuta>();

        foreach (var (punto, veces) in cruces)
        {
            // Un punto que no está en el catálogo no se puede valorar y tampoco se descarta en
            // silencio: se omite de la estimación con el resto de la ruta intacta, y el paso
            // real lo registrará `RN-34` como punto no catalogado.
            if (porId.TryGetValue(punto, out var p)) ruta.Add(new CruceDeRuta(p, veces));
        }

        return ReglasDeEstimacionDePeajes.Armar(
            ruta,
            categoria,
            await _peajes.TarifasAsync(cancelacion),
            await _peajes.VigenciasAsync(cancelacion),
            await _peajes.ExoneracionesAsync(cancelacion),
            vehiculo,
            fechaPrevista,
            DateTimeOffset.UtcNow);
    }

    // ── El paso por caseta ──────────────────────────────────────────────────

    /// <summary>
    /// `RN-36` — registra un paso tal como ocurrió.
    ///
    /// ── La categoría esperada se resuelve acá, no se recibe ─────────────────
    /// Si quien llama pudiera declararla, la comparación de `RN-36` se haría contra lo que el
    /// cliente diga y no contra lo que la ficha técnica determina — y entonces el error de la
    /// caseta podría entrar por la puerta de atrás como «esperada».
    /// </summary>
    public async Task<Ulid> RegistrarPasoAsync(
        Ulid id,
        Ulid punto,
        Ulid vehiculo,
        Ulid? mision,
        DateTimeOffset ocurridoEn,
        int odometro,
        decimal montoPagado,
        MedioDePagoDelPeaje medio,
        IdPersona registra,
        string? categoriaCobrada = null,
        string? ticket = null,
        string? causaSinTicket = null,
        bool puntoNoCatalogado = false,
        string? ubicacionDeclarada = null,
        Ulid? idDeCaptura = null,
        CancellationToken cancelacion = default)
    {
        ReglasDelPasoPorCaseta.ExigirDatosDelHecho(odometro, montoPagado);
        ReglasDelPasoPorCaseta.ExigirUbicacionSiNoEstaCatalogado(
            puntoNoCatalogado, ubicacionDeclarada);

        var fecha = DateOnly.FromDateTime(ocurridoEn.Date);
        var esperada = await CategoriaDelVehiculoAsync(vehiculo, fecha, cancelacion);
        var nombres = await _peajes.NombresDeCategoriaAsync(cancelacion);

        var cobrada = categoriaCobrada is null
            ? null
            : new CategoriaDePeaje(
                categoriaCobrada, nombres.GetValueOrDefault(categoriaCobrada, categoriaCobrada));

        // Lo que se esperaba pagar, para que la diferencia de monto se pueda leer sin rehacer
        // el cálculo. Nulo cuando no se pudo resolver — y nulo no es cero: cero diría que se
        // esperaba no pagar nada.
        decimal? montoEsperado = null;

        if (!puntoNoCatalogado && esperada.Categoria is { } cat)
        {
            var tarifa = ReglasDeTarifaDePeaje.Resolver(
                await _peajes.TarifasAsync(cancelacion), punto, cat, fecha, DateTimeOffset.UtcNow);

            montoEsperado = tarifa?.Monto;
        }

        var paso = new PasoPorCaseta(
            id, punto, vehiculo, mision, ocurridoEn, odometro, montoPagado, medio, registra,
            esperada.Categoria, cobrada, montoEsperado, ticket,
            puntoNoCatalogado, ubicacionDeclarada);

        // La evidencia se exige **después** de saber si hay discrepancia, y sólo entonces. Un
        // paso normal sin ticket no tiene por qué justificarse: la caseta a veces no lo da.
        ReglasDelPasoPorCaseta.ExigirEvidenciaDeLaDiscrepancia(
            paso.HayDiscrepanciaDeClasificacion, ticket, causaSinTicket);

        return await _peajes.GuardarPasoAsync(paso, causaSinTicket, idDeCaptura, cancelacion);
    }

    public Task<IReadOnlyList<PasoPorCaseta>> PasosDeLaMisionAsync(
        Ulid mision, CancellationToken cancelacion = default) =>
        _peajes.PasosDeLaMisionAsync(mision, cancelacion);

    /// <summary>
    /// <b>Dónde nos están cobrando mal.</b> Es el insumo del expediente de reclamo ante la SAPP
    /// (`RN-36` punto 4), agrupable por punto y período.
    /// </summary>
    public Task<IReadOnlyList<PasoPorCaseta>> DiscrepanciasAsync(
        CancellationToken cancelacion = default) => _peajes.DiscrepanciasAsync(cancelacion);

    // ── La carga del catálogo ───────────────────────────────────────────────

    /// <summary>
    /// Da de alta un punto con su primera vigencia.
    ///
    /// <b>El estado va junto con el alta, no después.</b> Un punto sin vigencia declarada no
    /// se puede valorar —ni activo ni cerrado— y dejaría toda ruta que lo atraviese sin
    /// estimar por haber guardado el catálogo a medias.
    /// </summary>
    public async Task<Ulid> AbrirPuntoAsync(
        Ulid id, string nombre, string operador, string carretera, string? sentidoDeCobro,
        EstadoDelPunto estado, string fundamento, DateOnly vigenteDesde,
        CancellationToken cancelacion = default)
    {
        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(operador))
            throw new BloqueoDuro("RN-34",
                "El punto exige nombre y operador. El operador es contra lo que se resuelve " +
                "una exoneración otorgada por concesionario, que es como se otorgan.");

        if (string.IsNullOrWhiteSpace(fundamento))
            throw new BloqueoDuro("RN-34",
                "El estado del punto exige fundamento: es lo que después permite explicar por " +
                "qué una caseta dejó de cobrar, y distinguirlo de una exoneración del vehículo.");

        contexto.PuntosDePeaje.Add(new FilaDePunto
        {
            Id = id, Nombre = nombre.Trim(), Operador = operador.Trim(),
            Carretera = carretera.Trim(), SentidoDeCobro = sentidoDeCobro?.Trim(),
        });

        contexto.VigenciasDePunto.Add(new FilaDeVigenciaDelPunto
        {
            Id = Ulid.NewUlid(), PuntoId = id, Estado = estado,
            Fundamento = fundamento.Trim(), VigenteDesde = vigenteDesde,
            RegistradoDesdeUtc = DateTime.UtcNow,
        });

        await contexto.SaveChangesAsync(cancelacion);
        return id;
    }

    /// <summary>
    /// Cambia el estado de un punto <b>abriendo una vigencia nueva</b>, no editando la que
    /// hay: un viaje pasado por una caseta que ya cerró tiene que seguir valorándose con el
    /// estado que regía entonces.
    /// </summary>
    public async Task CambiarEstadoAsync(
        Ulid punto, EstadoDelPunto estado, string fundamento, DateOnly vigenteDesde,
        CancellationToken cancelacion = default)
    {
        if (string.IsNullOrWhiteSpace(fundamento))
            throw new BloqueoDuro("RN-34", "El cambio de estado exige fundamento.");

        // Se cierra la vigencia abierta el día anterior. Dejarlas solapadas haría que dos
        // estados rigieran a la vez y que cuál gana dependiera del orden de la consulta.
        var abierta = await contexto.VigenciasDePunto
            .Where(v => v.PuntoId == punto && v.VigenteHasta == null)
            .OrderByDescending(v => v.VigenteDesde)
            .FirstOrDefaultAsync(cancelacion);

        if (abierta is not null) abierta.VigenteHasta = vigenteDesde.AddDays(-1);

        contexto.VigenciasDePunto.Add(new FilaDeVigenciaDelPunto
        {
            Id = Ulid.NewUlid(), PuntoId = punto, Estado = estado,
            Fundamento = fundamento.Trim(), VigenteDesde = vigenteDesde,
            RegistradoDesdeUtc = DateTime.UtcNow,
        });

        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>Da de alta una categoría. <b>Tabla abierta</b> — `RN-33`.</summary>
    public async Task CargarCategoriaAsync(
        string codigo, string nombre, CancellationToken cancelacion = default)
    {
        if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nombre))
            throw new BloqueoDuro("RN-33", "La categoría exige código y nombre.");

        contexto.CategoriasDePeaje.Add(new FilaDeCategoriaDePeaje
        {
            Codigo = codigo.Trim(), Nombre = nombre.Trim(),
        });

        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>
    /// Carga una tarifa — `RN-34`. <b>Exige fuente y fecha de verificación</b>, y cierra la
    /// vigencia abierta anterior para esa combinación.
    ///
    /// El cierre anticipado no es un lujo: en 2026 la tarifa se anunció el 08/01 y se
    /// suspendió hacia el 15/01. Sin él, la vigencia de enero seguiría abierta para siempre y
    /// dos tarifas regirían el mismo día.
    /// </summary>
    public async Task<Ulid> CargarTarifaAsync(
        Ulid punto, string categoria, decimal monto, string fuente,
        DateOnly fechaDeVerificacion, DateOnly vigenteDesde,
        CancellationToken cancelacion = default)
    {
        ReglasDeTarifaDePeaje.ExigirFuenteYVerificacion(fuente, monto);

        var abierta = await contexto.TarifasDePeaje
            .Where(t => t.PuntoId == punto && t.Categoria == categoria && t.VigenteHasta == null)
            .OrderByDescending(t => t.VigenteDesde)
            .FirstOrDefaultAsync(cancelacion);

        if (abierta is not null) abierta.VigenteHasta = vigenteDesde.AddDays(-1);

        var id = Ulid.NewUlid();

        contexto.TarifasDePeaje.Add(new FilaDeTarifa
        {
            Id = id, PuntoId = punto, Categoria = categoria.Trim(), Monto = monto,
            Fuente = fuente.Trim(), FechaDeVerificacion = fechaDeVerificacion,
            VigenteDesde = vigenteDesde, RegistradoDesdeUtc = DateTime.UtcNow,
        });

        await contexto.SaveChangesAsync(cancelacion);
        return id;
    }

    /// <summary>
    /// Carga una fila de la matriz de derivación — `RN-33`.
    ///
    /// <b>Exige fundamento</b>: `RN-33` punto 2 — una categoría sin explicación no se puede
    /// defender ante la SAPP ni ante un auditor, y la explicación sale de acá.
    /// </summary>
    public async Task<Ulid> CargarReglaAsync(
        FilaDeReglaDeCategoria regla, CancellationToken cancelacion = default)
    {
        if (string.IsNullOrWhiteSpace(regla.Fundamento))
            throw new BloqueoDuro("RN-33",
                "La fila de la matriz exige fundamento. Es lo que después contesta por qué " +
                "este vehículo paga lo que paga.");

        contexto.ReglasDeCategoriaDePeaje.Add(regla);
        await contexto.SaveChangesAsync(cancelacion);
        return regla.Id;
    }

    /// <summary>
    /// Registra una exoneración — `RN-38`. <b>Exige fundamento</b> y no admite exoneración
    /// global: una excepción permanente al pago exige vigilancia proporcional, y sin
    /// fundamento por vehículo no hay qué vigilar.
    /// </summary>
    public async Task<Ulid> CargarExoneracionAsync(
        Ulid vehiculo, Ulid? punto, string? operador, string fundamento,
        DateOnly vigenteDesde, DateOnly? vigenteHasta,
        CancellationToken cancelacion = default)
    {
        if (string.IsNullOrWhiteSpace(fundamento))
            throw new BloqueoDuro("RN-38",
                "La exoneración exige fundamento documental. El sistema no asume que un " +
                "vehículo está exonerado por pertenecer al Estado: el valor por defecto es " +
                "que paga, y si la institución tiene un acuerdo, ese acuerdo es el fundamento.");

        if (punto is null && string.IsNullOrWhiteSpace(operador))
            throw new BloqueoDuro("RN-38",
                "Una exoneración exige punto u operador. Sin ninguno de los dos sería una " +
                "exoneración en todo el país, y eso exigiría un acuerdo con cada concesionario.");

        var id = Ulid.NewUlid();

        contexto.ExoneracionesDePeaje.Add(new FilaDeExoneracion
        {
            Id = id, VehiculoId = vehiculo, PuntoId = punto, Operador = operador?.Trim(),
            Fundamento = fundamento.Trim(), VigenteDesde = vigenteDesde,
            VigenteHasta = vigenteHasta, RegistradoDesdeUtc = DateTime.UtcNow,
        });

        await contexto.SaveChangesAsync(cancelacion);
        return id;
    }

    // ── El catálogo ─────────────────────────────────────────────────────────

    public Task<IReadOnlyList<PuntoDePeaje>> PuntosAsync(
        CancellationToken cancelacion = default) => _peajes.PuntosAsync(cancelacion);

    public Task<IReadOnlyList<TarifaDePeaje>> TarifasAsync(
        CancellationToken cancelacion = default) => _peajes.TarifasAsync(cancelacion);

    public Task<IReadOnlyList<VigenciaDelPunto>> VigenciasAsync(
        CancellationToken cancelacion = default) => _peajes.VigenciasAsync(cancelacion);
}
