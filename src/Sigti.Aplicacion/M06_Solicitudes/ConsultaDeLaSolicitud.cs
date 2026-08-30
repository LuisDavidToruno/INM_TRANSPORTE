using Microsoft.EntityFrameworkCore;

using Sigti.Aplicacion.M02_Parametros;
using Sigti.Datos;
using Sigti.Dominio.M02_Parametros;
using Sigti.Dominio.M06_Solicitudes;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Aplicacion.M06_Solicitudes;

/// <summary>
/// Lo que hay que ver de una solicitud <b>antes de decidir sobre ella</b> — `PT-009`, `PT-010`.
///
/// ── Las dos cosas que el mapa pide dentro del expediente ────────────────────
/// El diagrama de `ACT-03` cuelga del expediente en decisión dos ramas: <i>«desglose de peajes
/// por punto»</i> y <i>«tramos inhábiles señalados»</i>. Las dos existen porque quien autoriza
/// necesita <b>ver el costo y el riesgo sin salir de la pantalla</b>: `R-8` dice que todo total
/// tiene su desglose a un toque, y una jefatura que tiene que navegar para verlo autoriza sin
/// verlo.
///
/// ── Y ninguna de las dos bloquea ────────────────────────────────────────────
/// `HU-006` es explícita: los tramos inhábiles <b>se señalan, no impiden</b> la solicitud. El
/// permiso de la máxima autoridad se gestiona después y `BD-04` lo exige al despachar. Bloquear
/// acá adelantaría un control que corresponde a otro momento, y dejaría al solicitante sin poder
/// ni pedir lo que sabe que necesita permiso.
/// </summary>
public sealed class ConsultaDeLaSolicitud(
    SigtiDbContext contexto, IParametrosDeLaInstitucion parametros)
{
    /// <summary>
    /// `PT-010` — los tramos de la ventana que caen en día u hora inhábil.
    ///
    /// El calendario se resuelve <b>a la fecha de salida</b>, no a la de hoy (`P-4`): una misión
    /// programada para diciembre se juzga con los feriados de diciembre.
    /// </summary>
    public async Task<TramosDeLaVentana?> TramosAsync(
        Ulid mision, CancellationToken cancelacion = default)
    {
        var fila = await contexto.Expedientes
            .AsNoTracking().SingleOrDefaultAsync(e => e.Id == mision, cancelacion);

        if (fila is null) return null;

        var ventana = new VentanaDeMision(
            fila.Salida, fila.Retorno, fila.HolguraDias, fila.HoraDeSalida, fila.HoraDeRetorno);

        var calendario = parametros.CalendarioVigenteAl(fila.Salida);

        return new TramosDeLaVentana(
            calendario.Version,
            ventana.FinDelRango,
            [.. calendario.InhabilesEn(ventana)],
            [.. calendario.HorasInhabilesEn(ventana)],

            // ⚠️ **Las dos mitades de `BD-04` se declaran por separado**, porque cada una puede
            // faltar por su cuenta y las consecuencias son distintas.
            //
            // Sin feriados cargados el calendario **subdeclara**: dirá que el 15 de septiembre
            // es hábil. Sin horario declarado, la hora sencillamente no se evalúa. Un reporte
            // que muestre «ningún tramo inhábil» sin decir cuál de las dos mitades no se miró
            // afirma algo que nadie comprobó.
            calendario.Feriados.Count > 0,
            calendario.Horario is not null,
            ventana.DeclaraHoras);
    }

    /// <summary>
    /// `PT-009` — el desglose de peajes congelado de la misión, punto por punto.
    ///
    /// ── Se lee lo congelado, no se recalcula ────────────────────────────────
    /// `RN-35` congela el paquete al programar. Recalcular al abrir mostraría un total distinto
    /// del que se autorizó cada vez que una tarifa cambie, y quien audite no podría explicar la
    /// diferencia. El desglose que se muestra es el que se usó.
    /// </summary>
    public async Task<DesgloseDePeajes?> PeajesAsync(
        Ulid mision, CancellationToken cancelacion = default)
    {
        var existe = await contexto.Expedientes
            .AsNoTracking().AnyAsync(e => e.Id == mision, cancelacion);

        if (!existe) return null;

        var lineas = await contexto.RutasAutorizadasDePeaje
            .AsNoTracking()
            .Where(r => r.MisionId == mision)
            .ToListAsync(cancelacion);

        var puntos = await contexto.PuntosDePeaje
            .AsNoTracking()
            .ToDictionaryAsync(p => p.Id, p => p.Nombre, cancelacion);

        var detalle = lineas
            .Select(l => new LineaDelDesglose(
                l.PuntoId.ToString(),
                puntos.GetValueOrDefault(l.PuntoId),
                l.Cruces,

                // Nulo es «no se pudo valorar»: sin tarifa cargada o sin categoría resuelta.
                // Nunca cero — un cero diría que este punto no cuesta peaje.
                l.Subtotal))
            .OrderBy(l => l.Nombre ?? l.Punto)
            .ToList();

        var sinValorar = detalle.Count(l => l.Subtotal is null);

        return new DesgloseDePeajes(
            // El total suma lo valorado. **Nulo cuando no hay ninguna línea**: no hay estimado
            // congelado todavía, que es distinto de un estimado de cero.
            detalle.Count == 0 ? null : detalle.Sum(l => l.Subtotal ?? 0m),
            detalle,
            sinValorar,

            // Un total parcial presentado como completo subestima el costo, y eso se paga en
            // efectivo faltante a mitad de camino.
            Parcial: sinValorar > 0);
    }
}

/// <param name="ConFeriadosCargados">
/// Falso significa que el calendario <b>subdeclara</b>: dirá que un feriado es día hábil. Va
/// aparte de <paramref name="ConHorarioDeclarado"/> porque cada mitad de `BD-04` puede faltar
/// por su cuenta.
/// </param>
/// <param name="LaMisionDeclaraHoras">
/// Para juzgar la hora hacen falta las dos cosas: que la institución declare su horario y que
/// la misión declare sus horas. Sin cualquiera de las dos, la hora no se evalúa.
/// </param>
public sealed record TramosDeLaVentana(
    string VersionDelCalendario,
    DateOnly FinDelRango,
    IReadOnlyList<DateOnly> DiasInhabiles,
    IReadOnlyList<string> HorasInhabiles,
    bool ConFeriadosCargados,
    bool ConHorarioDeclarado,
    bool LaMisionDeclaraHoras)
{
    /// <summary>
    /// Si hay algo que señalar. <b>Falso no es «no hay riesgo»</b> cuando falta alguna mitad:
    /// para eso están las dos banderas.
    /// </summary>
    public bool HayAlgoQueSeñalar => DiasInhabiles.Count > 0 || HorasInhabiles.Count > 0;
}

/// <param name="Total">
/// <b>Nulo cuando no hay estimado congelado</b> — la misión todavía no se programó. Distinto de
/// un estimado de cero, que diría que la ruta no tiene peajes.
/// </param>
public sealed record DesgloseDePeajes(
    decimal? Total,
    IReadOnlyList<LineaDelDesglose> Lineas,
    int SinValorar,
    bool Parcial);

/// <param name="Subtotal">Nulo es «no se pudo valorar». Nunca cero.</param>
public sealed record LineaDelDesglose(
    string Punto, string? Nombre, int Cruces, decimal? Subtotal);
