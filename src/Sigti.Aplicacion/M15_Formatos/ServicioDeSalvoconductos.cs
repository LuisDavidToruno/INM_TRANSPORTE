using Microsoft.EntityFrameworkCore;

using Sigti.Aplicacion.M06_Solicitudes;
using Sigti.Aplicacion.M07_ProgramacionYDespacho;
using Sigti.Datos;
using Sigti.Datos.M15_Formatos;
using Sigti.Dominio;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M15_Formatos;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M15_Formatos;

/// <summary>
/// `PT-023` — la emisión, la reimpresión y la verificación del salvoconducto.
///
/// ── El primer documento físico del sistema ──────────────────────────────────
/// Premisa rectora 4: híbrido digital-papel <b>por diseño, no por parche</b>. Hasta acá todo
/// vivía en pantalla; éste es el primer artefacto que sale por una impresora y se lleva en la
/// mano, y el único destinatario que importa —el agente en la carretera— no tiene usuario, no
/// se autentica y no verá nunca el expediente.
/// </summary>
public sealed class ServicioDeSalvoconductos(
    SigtiDbContext contexto,
    ServicioDeFolios folios)
{
    /// <summary>El tipo de documento en el control de rangos — `RN-44`.</summary>
    public const string TipoDeDocumento = "salvoconducto";

    /// <summary>
    /// Emite el documento y lo congela.
    ///
    /// <b>Exige permiso firmado.</b> El salvoconducto no autoriza nada: materializa una
    /// autorización que ya ocurrió. Emitir sobre un trámite sin firma produciría un papel con
    /// aspecto oficial que no ampara nada — y el motorista lo llevaría creyendo que sí.
    /// </summary>
    public async Task<Ulid> EmitirAsync(
        Ulid permisoDeCirculacion,
        IdPersona emite,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var permiso = await contexto.Permisos
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == permisoDeCirculacion, cancelacion)
            ?? throw new PermisoNoEncontrado(permisoDeCirculacion);

        var yaEmitido = await contexto.Salvoconductos
            .AnyAsync(s => s.PermisoId == permisoDeCirculacion, cancelacion);

        var firmado = permiso.Estado == EstadoDelPermiso.Firmado.ToString();

        if (ReglasDelSalvoconducto.PorQueNoSeEmite(firmado, yaEmitido) is { } porQue)
            throw new BloqueoDuro("RN-25", porQue);

        var expediente = await contexto.Expedientes
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.Id == permiso.ExpedienteId, cancelacion)
            ?? throw new ExpedienteNoEncontrado(permiso.ExpedienteId);

        // Los nombres se resuelven ACÁ y se congelan: el papel identifica al vehículo y al
        // motorista con las palabras que el agente va a comparar, no con identificadores.
        var nombres = await NombresAsync(permiso, cancelacion);

        var contenido = new ContenidoDelSalvoconducto(
            permiso.Folio,
            nombres.Vehiculo,
            nombres.Motorista,
            permiso.Destino,
            permiso.Desde,
            permiso.Hasta,
            Tramos(permiso.TramosInhabiles),
            permiso.Justificacion,
            new IdPersona(permiso.EmitidoPor!),
            new DateTimeOffset(permiso.FirmadoEnUtc!.Value, TimeSpan.Zero));

        var huella = ReglasDelSalvoconducto.Huella(contenido);

        // ⚠️ El folio sale del rango de la DELEGACIÓN (`RN-44`), no de un contador central: una
        // delegación sin conectividad tiene que poder emitir e imprimir antes de salir.
        //
        // Nulo es que no hay rango asignado — no un fallo. Se sigue con folio provisional **y el
        // documento lo declara**: un folio inventado que se ve oficial es peor que uno que dice
        // que es provisional.
        var folio = await folios.EmitirAsync(
            expediente.Dependencia, TipoDeDocumento, permiso.Desde, cancelacion);

        var id = Ulid.NewUlid();

        contexto.Salvoconductos.Add(new FilaDeSalvoconducto
        {
            Id = id,
            PermisoId = permisoDeCirculacion,
            ExpedienteId = permiso.ExpedienteId,

            Folio = folio?.Texto ?? $"SC-PROV-{id.ToString()[^8..]}",
            FolioNumero = folio?.Numero,
            FolioRangoId = folio?.RangoId,

            Huella = huella,
            CodigoCorto = ReglasDelSalvoconducto.CodigoCorto(huella),

            FolioDelPermiso = contenido.FolioDelPermiso,
            Vehiculo = contenido.Vehiculo,
            Motorista = contenido.Motorista,
            Destino = contenido.Destino,
            Desde = contenido.Desde,
            Hasta = contenido.Hasta,
            TramosInhabiles = string.Join(" · ", contenido.TramosInhabiles),
            Justificacion = contenido.Justificacion,
            FirmadoPor = contenido.FirmadoPor.Valor,
            FirmadoEnUtc = contenido.FirmadoEn.UtcDateTime,

            EmitidoPor = emite.Valor,
            EmitidoEnUtc = momento.UtcDateTime,

            // La emisión es la primera impresión, y por eso no lleva motivo: el motivo es
            // que se emitió. Contarla desde cero haría que «la segunda impresión» fuera la
            // tercera salida de papel.
            Impresiones =
            {
                new FilaDeImpresion
                {
                    Id = Ulid.NewUlid(),
                    SalvoconductoId = id,
                    Orden = 1,
                    Quien = emite.Valor,
                    MomentoUtc = momento.UtcDateTime,
                    Motivo = null,
                },
            },
        });

        await contexto.SaveChangesAsync(cancelacion);
        return id;
    }

    /// <summary>
    /// Vuelve a sacar el mismo papel. <b>Mismo folio, mismo contenido, misma huella.</b>
    ///
    /// `RN-04`: el folio no se recicla y dos folios para un mismo permiso rompen la
    /// conciliación. Lo único que se agrega es el asiento de quién, cuándo y por qué.
    /// </summary>
    public async Task<int> ReimprimirAsync(
        Ulid id, IdPersona quien, string motivo, DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        if (ReglasDelSalvoconducto.PorQueNoSeReimprime(motivo) is { } porQue)
            throw new BloqueoDuro("RN-25", porQue);

        var fila = await contexto.Salvoconductos
            .Include(s => s.Impresiones)
            .SingleOrDefaultAsync(s => s.Id == id, cancelacion)
            ?? throw new SalvoconductoNoEncontrado(id);

        var orden = fila.Impresiones.Count + 1;

        fila.Impresiones.Add(new FilaDeImpresion
        {
            Id = Ulid.NewUlid(),
            SalvoconductoId = fila.Id,
            Orden = orden,
            Quien = quien.Valor,
            MomentoUtc = momento.UtcDateTime,
            Motivo = motivo.Trim(),
        });

        await contexto.SaveChangesAsync(cancelacion);
        return orden;
    }

    /// <summary>Lo que se imprime, para una misión.</summary>
    public async Task<SalvoconductoImpreso?> DelExpedienteAsync(
        Ulid expediente, CancellationToken cancelacion = default)
    {
        var fila = await contexto.Salvoconductos
            .AsNoTracking()
            .Include(s => s.Impresiones)
            .SingleOrDefaultAsync(s => s.ExpedienteId == expediente, cancelacion);

        return fila is null ? null : await ArmarAsync(fila, cancelacion);
    }

    /// <summary>
    /// Lo que responde el punto de verificación — <b>por folio o por código corto</b>.
    ///
    /// ── Los dos caminos existen por la misma razón ──────────────────────────
    /// El QR resuelve al folio. En zona sin señal el agente no puede escanear: anota los ocho
    /// caracteres del código corto y consulta al volver. `RN-25` obliga a las dos vías porque
    /// la verificación en línea no puede ser la única en un país con la conectividad que
    /// documenta `NRM-09`.
    /// </summary>
    public async Task<SalvoconductoImpreso?> VerificarAsync(
        string folioOCodigo, CancellationToken cancelacion = default)
    {
        var buscado = folioOCodigo.Trim().ToUpperInvariant();

        var fila = await contexto.Salvoconductos
            .AsNoTracking()
            .Include(s => s.Impresiones)
            .SingleOrDefaultAsync(
                s => s.Folio == buscado || s.CodigoCorto == buscado, cancelacion);

        return fila is null ? null : await ArmarAsync(fila, cancelacion);
    }

    private async Task<SalvoconductoImpreso> ArmarAsync(
        FilaDeSalvoconducto fila, CancellationToken cancelacion)
    {
        var impreso = Contenido(fila);

        // ⚠️ **El estado se compara, no se lee.** «Desactualizado» no lo declara nadie: pasa
        // solo cuando la misión cambia debajo de un papel ya impreso, y no hay nadie ahí para
        // marcarlo. Por eso se resuelve lo que la misión tiene HOY y se contrasta.
        var permiso = await contexto.Permisos
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == fila.PermisoId, cancelacion);

        // ⚠️⚠️ Se contrasta contra la MISIÓN, no contra el permiso.
        //
        // El permiso guarda su propia copia congelada de los cuatro elementos —tiene vida
        // propia, igual que el papel—, así que compararse con ella daría «vigente» siempre:
        // las dos copias se congelaron en el mismo acto y nunca pueden diferir. El estado
        // `Desactualizado` sería inalcanzable, y un relevo de motorista dejaría un papel que
        // no ampara a nadie respondiendo «documento válido».
        //
        // Lo que cambia debajo del papel es la RESERVA de la misión — un hecho del diario
        // (`P-1`), no una columna—, y es contra eso que hay que comparar.
        var expediente = await contexto.Expedientes
            .AsNoTracking()
            .Include(e => e.Transiciones)
            .SingleOrDefaultAsync(e => e.Id == fila.ExpedienteId, cancelacion);

        ContenidoDelSalvoconducto? ahora = null;

        if (permiso is not null
            && permiso.Estado == EstadoDelPermiso.Firmado.ToString()
            && expediente is not null)
        {
            var reserva = ServicioDePermisos.Reserva(expediente);

            // Nulo es que la misión se desprogramó: no hay contra qué comparar, y eso
            // desactualiza. Dar por vigente lo que no se pudo verificar es la falla que este
            // estado existe para evitar.
            if (reserva.Vehiculo is not null && reserva.Motorista is not null)
            {
                var nombres = await NombresDeAsync(reserva.Vehiculo, reserva.Motorista, cancelacion);

                ahora = impreso with
                {
                    Vehiculo = nombres.Vehiculo,
                    Motorista = nombres.Motorista,

                    // El destino y la ventana salen del expediente por la misma razón: son
                    // de la misión, y el permiso también los tiene congelados.
                    //
                    // El fin lo calcula `VentanaDeMision`, no esta línea: la holgura entra en
                    // el rango y recalcularla acá crearía una segunda definición de «hasta
                    // cuándo dura la misión» que va a divergir de la del dominio.
                    Destino = expediente.Destino,
                    Desde = expediente.Salida,
                    Hasta = new VentanaDeMision(
                        expediente.Salida, expediente.Retorno, expediente.HolguraDias,
                        expediente.HoraDeSalida, expediente.HoraDeRetorno).FinDelRango,
                };
            }
        }

        var anulado = fila.Anulado
            || permiso is null
            || permiso.Estado == EstadoDelPermiso.Desistido.ToString();

        var estado = ReglasDelSalvoconducto.Estado(impreso, ahora, anulado);

        return new SalvoconductoImpreso(
            fila.Id,
            fila.Folio,
            fila.FolioNumero is null,
            fila.Huella,
            fila.CodigoCorto,
            impreso,
            new IdPersona(fila.EmitidoPor),
            new DateTimeOffset(fila.EmitidoEnUtc, TimeSpan.Zero),
            estado,
            ReglasDelSalvoconducto.Veredicto(estado),
            [
                .. fila.Impresiones
                    .OrderBy(i => i.Orden)
                    .Select(i => new Impresion(
                        i.Orden, new IdPersona(i.Quien),
                        new DateTimeOffset(i.MomentoUtc, TimeSpan.Zero), i.Motivo)),
            ]);
    }

    private static ContenidoDelSalvoconducto Contenido(FilaDeSalvoconducto f) => new(
        f.FolioDelPermiso, f.Vehiculo, f.Motorista, f.Destino, f.Desde, f.Hasta,
        Tramos(f.TramosInhabiles), f.Justificacion,
        new IdPersona(f.FirmadoPor), new DateTimeOffset(f.FirmadoEnUtc, TimeSpan.Zero));

    private static IReadOnlyList<string> Tramos(string texto) =>
        texto.Length == 0 ? [] : [.. texto.Split(" · ")];

    /// <summary>
    /// Cómo se nombran el vehículo y el motorista <b>en el papel</b>.
    ///
    /// Las siglas institucionales primero: `RN-15` y `CE-17` — hay vehículos del Estado
    /// circulando sin lámina metálica por el desabastecimiento nacional, y un documento que
    /// sólo diga la placa no identifica a ésos.
    /// </summary>
    private Task<(string Vehiculo, string Motorista)> NombresAsync(
        Datos.FilaDePermisoDeCirculacion permiso, CancellationToken cancelacion) =>
        NombresDeAsync(permiso.Vehiculo, permiso.Motorista, cancelacion);

    /// <inheritdoc cref="NombresAsync"/>
    private async Task<(string Vehiculo, string Motorista)> NombresDeAsync(
        Ulid? idVehiculo, Ulid? idMotorista, CancellationToken cancelacion)
    {
        var vehiculo = idVehiculo is null
            ? "(sin asignar)"
            : await contexto.Vehiculos
                .AsNoTracking()
                .Where(v => v.Id == idVehiculo.Value)
                .Select(v => v.Siglas + " · " + v.TipoDeVehiculo +
                             (v.Placa == null ? " · SIN LÁMINA METÁLICA" : " · placa " + v.Placa))
                .SingleOrDefaultAsync(cancelacion) ?? "(vehículo no encontrado)";

        var motorista = idMotorista is null
            ? "(sin asignar)"
            : await contexto.Conductores
                .AsNoTracking()
                .Where(c => c.Id == idMotorista.Value)
                .Select(c => c.Nombre)
                .SingleOrDefaultAsync(cancelacion) ?? "(motorista no encontrado)";

        return (vehiculo, motorista);
    }
}

/// <param name="FolioProvisional">
/// <b>Verdadero cuando no hay rango asignado a la delegación.</b> El documento lo declara: un
/// folio inventado que se ve oficial es peor que uno que dice que es provisional.
/// </param>
/// <param name="Veredicto">
/// Qué se le dice a quien verifica, en palabras que sirvan en la carretera. El estado por sí
/// solo no le dice a un agente si puede dejar pasar el vehículo.
/// </param>
public sealed record SalvoconductoImpreso(
    Ulid Id,
    string Folio,
    bool FolioProvisional,
    string Huella,
    string CodigoCorto,
    ContenidoDelSalvoconducto Contenido,
    IdPersona EmitidoPor,
    DateTimeOffset EmitidoEn,
    EstadoDelSalvoconducto Estado,
    string Veredicto,
    IReadOnlyList<Impresion> Impresiones);

/// <param name="Motivo">Nulo sólo en la primera, que es la emisión misma.</param>
public sealed record Impresion(
    int Orden, IdPersona Quien, DateTimeOffset Momento, string? Motivo);

public sealed class SalvoconductoNoEncontrado(Ulid id)
    : Exception($"No existe el salvoconducto {id}.");
