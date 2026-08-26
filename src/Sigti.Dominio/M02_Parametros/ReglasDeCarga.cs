using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M02_Parametros;

/// <summary>
/// De dónde salió el dato y quién lo verificó.
///
/// <b>Un parámetro sin respaldo no se puede sostener ante el Tribunal Superior de
/// Cuentas</b> (`HU-144`). Por eso no es opcional ni se agrega después: sin él la carga
/// no entra.
/// </summary>
/// <param name="Adjunto">
/// El comunicado, acuerdo o tabla oficial. El archivo vive fuera de la base y acá va su
/// referencia (`ADR-004`).
/// </param>
public sealed record RespaldoDocumental(
    Ulid Adjunto,
    string Fuente,
    DateOnly FechaDeVerificacion);

/// <summary>Lo que alguien pide cargar. Todavía no es una versión: puede rechazarse.</summary>
public sealed record SolicitudDeCarga(
    string Clave,
    string Valor,
    DateOnly VigenteDesde,
    DateOnly? VigenteHasta,
    RespaldoDocumental? Respaldo,
    IdPersona CargadoPor);

public enum MotivoDeRechazoDeCarga
{
    Ninguno,
    SinRespaldoDocumental,
    SinFuenteDeclarada,
    SolapaConOtraVigencia,
    DejaHuecoSinVigencia,
    RangoInvertido
}

public sealed record ResultadoDeCarga(bool Aceptada, MotivoDeRechazoDeCarga Motivo, string Mensaje)
{
    public static ResultadoDeCarga Ok() => new(true, MotivoDeRechazoDeCarga.Ninguno, "");
}

/// <summary>
/// `HU-144` — Qué se admite cargar.
///
/// Pura: recibe lo que se quiere cargar y lo que ya existe. No consulta nada.
/// </summary>
public static class ReglasDeCarga
{
    public static ResultadoDeCarga Evaluar(
        SolicitudDeCarga solicitud, IReadOnlyList<VersionDeParametro> existentes)
    {
        if (solicitud.Respaldo is null)
            return Rechazo(MotivoDeRechazoDeCarga.SinRespaldoDocumental,
                "Adjunte el respaldo documental: comunicado, acuerdo o tabla oficial. " +
                "Un parámetro sin respaldo no se puede sostener ante el Tribunal Superior de Cuentas.");

        if (string.IsNullOrWhiteSpace(solicitud.Respaldo.Fuente))
            return Rechazo(MotivoDeRechazoDeCarga.SinFuenteDeclarada,
                "Declare la fuente del dato y la fecha en que se verificó.");

        if (solicitud.VigenteHasta is { } fin && fin < solicitud.VigenteDesde)
            return Rechazo(MotivoDeRechazoDeCarga.RangoInvertido,
                "La vigencia termina antes de empezar.");

        // Solo lo que el sistema cree hoy: una versión superada en el eje de transacción
        // ya no ocupa lugar en la línea de vigencias.
        var vigentes = existentes
            .Where(v => v.Clave == solicitud.Clave && v.RegistradoHasta is null)
            .ToList();

        if (vigentes.FirstOrDefault(v => SeSolapan(v, solicitud)) is { } solapada)
            return Rechazo(MotivoDeRechazoDeCarga.SolapaConOtraVigencia,
                $"La vigencia desde el {solicitud.VigenteDesde:dd/MM/yyyy} solapa con el valor " +
                $"{solapada.Valor} vigente del {solapada.VigenteDesde:dd/MM/yyyy} al " +
                $"{solapada.VigenteHasta:dd/MM/yyyy}. Cierre la anterior el " +
                $"{solicitud.VigenteDesde.AddDays(-1):dd/MM/yyyy} o inicie esta el " +
                $"{solapada.VigenteHasta?.AddDays(1):dd/MM/yyyy}.");

        // El hueco es el fallo silencioso: no rompe al cargarlo, rompe meses después
        // cuando alguien liquida una misión de esos días.
        var anterior = vigentes
            .Where(v => v.VigenteHasta is { } hasta && hasta < solicitud.VigenteDesde)
            .MaxBy(v => v.VigenteHasta);

        if (anterior?.VigenteHasta is { } cierre && solicitud.VigenteDesde > cierre.AddDays(1))
            return Rechazo(MotivoDeRechazoDeCarga.DejaHuecoSinVigencia,
                $"Quedaría un hueco del {cierre.AddDays(1):dd/MM/yyyy} al " +
                $"{solicitud.VigenteDesde.AddDays(-1):dd/MM/yyyy} sin vigencia para " +
                $"'{solicitud.Clave}'. Todo hecho de esos días quedaría sin poder calcularse.");

        return ResultadoDeCarga.Ok();
    }

    private static bool SeSolapan(VersionDeParametro existente, SolicitudDeCarga solicitud)
    {
        var finExistente = existente.VigenteHasta ?? DateOnly.MaxValue;
        var finNuevo = solicitud.VigenteHasta ?? DateOnly.MaxValue;

        return solicitud.VigenteDesde <= finExistente && existente.VigenteDesde <= finNuevo;
    }

    private static ResultadoDeCarga Rechazo(MotivoDeRechazoDeCarga motivo, string mensaje) =>
        new(false, motivo, mensaje);
}
