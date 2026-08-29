using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Datos.M03_Flota;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Aplicacion.M03_Flota;

/// <summary>
/// El título de tenencia del vehículo — `RN-62`.
///
/// ── Lo que decide, más allá del dato ────────────────────────────────────────
/// Tres cosas: si el vehículo <b>se puede habilitar</b>, si una misión <b>se puede programar</b>
/// contra su vigencia, y cuál de los dos terminales corresponde cuando sale de la flota
/// (`HB3-17`). Sin él, esa última verificación siempre advierte en vez de juzgar.
/// </summary>
public sealed class ServicioDeTitulos(SigtiDbContext contexto)
{
    /// <summary>
    /// Registra un título. <b>Se agrega a la serie</b>: un vehículo que pasa de comodato a
    /// propiedad conserva el título anterior, porque las misiones de ese período se hicieron
    /// bajo comodato y sus rubros los cubría el cedente.
    /// </summary>
    public async Task<Ulid> RegistrarAsync(
        Ulid id,
        Ulid vehiculo,
        RegimenDeTenencia regimen,
        string titular,
        string documento,
        DateOnly desde,
        DateOnly? hasta,
        RubrosDelTitulo rubros,
        CancellationToken cancelacion = default)
    {
        ReglasDelTitulo.ExigirElTitulo(regimen, titular, documento, desde, hasta);

        // **Dos títulos vigentes a la vez no se puede.** El vehículo estaría en dos regímenes
        // al mismo tiempo, y la pregunta de si el bien es propio no tendría respuesta.
        var solapado = await contexto.TitulosDeTenencia
            .Where(t => t.VehiculoId == vehiculo)
            .Where(t => (t.Hasta == null || desde <= t.Hasta)
                && (hasta == null || t.Desde <= hasta))
            .AnyAsync(cancelacion);

        if (solapado)
            throw new BloqueoDuro("RN-62",
                "Este vehículo ya tiene un título de tenencia que se solapa con esa ventana. " +
                "Dos títulos vigentes a la vez dejarían al vehículo en dos regímenes al mismo " +
                "tiempo, y la pregunta de si el bien es del Estado no tendría respuesta. Lo que " +
                "cambia el régimen es cerrar el título anterior y abrir el nuevo.");

        contexto.TitulosDeTenencia.Add(new FilaDeTitulo
        {
            Id = id,
            VehiculoId = vehiculo,
            Regimen = regimen,
            Titular = titular.Trim(),
            Documento = documento.Trim(),
            Desde = desde,
            Hasta = hasta,
            Combustible = rubros.Combustible,
            Mantenimiento = rubros.Mantenimiento,
            Llantas = rubros.Llantas,
            Seguro = rubros.Seguro,
            Peajes = rubros.Peajes,
            Multas = rubros.Multas,
            Danios = rubros.Danios,
        });

        await contexto.SaveChangesAsync(cancelacion);
        return id;
    }

    /// <summary>
    /// El título que regía a una fecha. <b>Nulo cuando no hay ninguno</b> — y eso es distinto de
    /// «propiedad», que es la suposición cómoda que este sistema no hace.
    /// </summary>
    public async Task<TituloDeTenencia?> VigenteAlAsync(
        Ulid vehiculo, DateOnly fecha, CancellationToken cancelacion = default)
    {
        var filas = await contexto.TitulosDeTenencia
            .AsNoTracking()
            .Where(t => t.VehiculoId == vehiculo && t.Desde <= fecha)
            .ToListAsync(cancelacion);

        var fila = filas
            .Where(t => t.Hasta == null || fecha <= t.Hasta)

            // Si dos se solaparan —que el registro impide— manda el que empezó después.
            .OrderByDescending(t => t.Desde)
            .FirstOrDefault();

        return fila is null ? null : A(fila);
    }

    /// <summary>
    /// Lo que el despacho y la programación consultan. Devuelve
    /// <see cref="TituloAlProgramar.SinTitulo"/> cuando no hay, y nunca un nulo suelto: quien lo
    /// use está afirmando que consultó.
    /// </summary>
    public async Task<TituloAlProgramar> AlProgramarAsync(
        Ulid vehiculo, CancellationToken cancelacion = default) =>
        new(await DelVehiculoAsync(vehiculo, cancelacion));

    /// <summary>
    /// <b>Si el bien es del Estado a esta fecha</b> — lo que `HB3-17` necesita para juzgar cuál
    /// terminal corresponde.
    ///
    /// Nulo es <b>no se sabe</b>: el vehículo no tiene título registrado. Ahí la verificación
    /// advierte en vez de bloquear, porque frenar el descargo de toda la flota por un dato de
    /// alta que nadie llenó sería peor que el asiento que se quiere evitar.
    /// </summary>
    public async Task<bool?> EsBienPropioAsync(
        Ulid vehiculo, DateOnly fecha, CancellationToken cancelacion = default) =>
        (await VigenteAlAsync(vehiculo, fecha, cancelacion))?.EsBienPropio;

    public async Task<IReadOnlyList<TituloDeTenencia>> DelVehiculoAsync(
        Ulid vehiculo, CancellationToken cancelacion = default) =>
        [.. (await contexto.TitulosDeTenencia
            .AsNoTracking()
            .Where(t => t.VehiculoId == vehiculo)
            .OrderByDescending(t => t.Desde)
            .ToListAsync(cancelacion)).Select(A)];

    private static TituloDeTenencia A(FilaDeTitulo f) => new(
        f.Id,
        f.VehiculoId,
        f.Regimen,
        f.Titular,
        f.Documento,
        f.Desde,
        f.Hasta,
        new RubrosDelTitulo(
            f.Combustible, f.Mantenimiento, f.Llantas, f.Seguro, f.Peajes, f.Multas, f.Danios));
}
