using Microsoft.EntityFrameworkCore;

using Sigti.Aplicacion.M18_Peajes;
using Sigti.Datos;
using Sigti.Datos.M18_Peajes;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M18_Peajes;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.Datos;

/// <summary>
/// `RN-61` — <b>la sustitución de vehículo recalcula y vuelve a congelar el estimado de peajes</b>,
/// con asiento de diferencia.
///
/// ── Por qué esto no puede faltar ────────────────────────────────────────────
/// El estimado congelado <b>es lo que el autorizador autorizó</b>. Sustituir un pick-up por un
/// camión de dos ejes puede duplicar el peaje de una ruta larga, y sin recalcular la misión
/// sigue liquidándose contra una cifra que ya no corresponde a ningún vehículo real: la
/// conciliación cuadraría contra un número inventado.
///
/// ── Y por qué el anterior se conserva ───────────────────────────────────────
/// `RN-04`: el asiento anterior no se sobrescribe. Un estimado que se pisa deja al auditor sin
/// poder contestar qué se autorizó originalmente ni cuánto cambió — y ver que <b>hubo</b> un
/// cambio, de cuánto y por qué es exactamente lo que un control interno quiere.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class RecongelamientoDePeajesPruebas(BaseDePruebas baseDePruebas)
{
    private static readonly DateOnly Salida = new(2026, 9, 4);

    private static readonly DateTimeOffset Momento =
        new(2026, 8, 1, 9, 0, 0, TimeSpan.FromHours(-6));

    /// <summary>
    /// Cuándo se REGISTRÓ el catálogo, en el eje de transacción (`ADR-006`).
    ///
    /// ⚠️ Tiene que estar en el pasado respecto de ahora. Sembrarlo con una fecha futura hace
    /// que la matriz de `RN-33` no exista todavía para el sistema, y el estimado sale sin
    /// valorar con un mensaje que habla del insumo #23 y no de la siembra.
    /// </summary>
    private static readonly DateTime Registrado = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly IdPersona Jefe = new("P-TRANSPORTE");

    [Fact]
    public async Task Sustituir_el_vehiculo_recalcula_el_estimado_y_deja_asiento_de_diferencia()
    {
        var s = await SembrarAsync();

        // ── El congelamiento original, con el liviano ───────────────────────
        await using (var contexto = baseDePruebas.Contexto())
        {
            var servicio = new ServicioDePeajes(contexto);

            // La categoria primero: si esto falla, el problema es la matriz de RN-33 y no la
            // tarifa, y el mensaje lo dice en vez de aparecer como un total nulo.
            var categoria = await servicio.CategoriaDelVehiculoAsync(s.Liviano, Salida);
            Assert.True(categoria.EstaResuelta, categoria.Explicacion);
            Assert.Equal("Liviano", categoria.Categoria!.Nombre);

            var estimacion = await servicio.EstimarAsync(
                [(s.Punto, 2)], s.Liviano, null, "", Salida);

            Assert.Equal(100m, estimacion.Total);   // 2 cruces × L 50.00

            await servicio.CongelarEstimadoAsync(s.Mision, estimacion, Jefe, Momento);
        }

        // ── La sustitución ──────────────────────────────────────────────────
        DiferenciaDelRecongelamiento? diferencia;

        await using (var contexto = baseDePruebas.Contexto())
        {
            diferencia = await new ServicioDePeajes(contexto).RecongelarPorSustitucionAsync(
                s.Mision, s.Pesado, Salida,
                "AVERIA_MECANICA · El liviano entró a taller la víspera.",
                Jefe, Momento);
        }

        Assert.NotNull(diferencia);

        // El camión de dos ejes paga el triple en la misma caseta.
        Assert.Equal(100m, diferencia.TotalAnterior);
        Assert.Equal(300m, diferencia.TotalNuevo);
        Assert.Equal(200m, diferencia.Diferencia);

        // Y el asiento nombra las dos categorías: decir que el total subió sin decir de qué
        // categoría a cuál deja la diferencia sin explicación.
        Assert.Equal("Liviano", diferencia.CategoriaAnterior);
        Assert.Equal("Camión 2 ejes", diferencia.CategoriaNueva);

        await using var lectura = baseDePruebas.Contexto();

        // ── El anterior se conserva, no se pisa (`RN-04`) ───────────────────
        var todas = await lectura.RutasAutorizadasDePeaje
            .Where(r => r.MisionId == s.Mision)
            .ToListAsync();

        Assert.Equal(2, todas.Count);
        Assert.Single(todas, r => r.SupersedidaPor == diferencia.Id);
        Assert.Single(todas, r => r.SupersedidaPor == null);

        // Y la línea superada conserva su subtotal original: es lo que se autorizó.
        Assert.Equal(100m, todas.Single(r => r.SupersedidaPor is not null).Subtotal);

        // ── Y la ruta VIGENTE es sólo una ───────────────────────────────────
        //
        // ⚠️ Si las superadas siguieran contando, `RN-37` —«¿esta caseta estaba en la ruta
        // aprobada?»— se contestaría contra dos rutas a la vez, y el desglose del expediente
        // mostraría el doble de líneas con un total que no es ninguno de los dos.
        var vigentes = await lectura.RutasAutorizadasDePeaje
            .Where(r => r.MisionId == s.Mision && r.SupersedidaPor == null)
            .ToListAsync();

        Assert.Single(vigentes);
        Assert.Equal(300m, vigentes[0].Subtotal);
    }

    /// <summary>
    /// Volver a congelar después de una sustitución <b>no está bloqueado</b>.
    ///
    /// El guardia de `RN-41` —«esta misión ya tiene el estimado congelado»— mira sólo lo
    /// vigente. Si mirara todas las filas, una misión sustituida quedaría marcada como
    /// congelada para siempre y una segunda sustitución fallaría por una razón falsa.
    /// </summary>
    [Fact]
    public async Task Una_segunda_sustitucion_tambien_recongela()
    {
        var s = await SembrarAsync();

        await using (var contexto = baseDePruebas.Contexto())
        {
            var servicio = new ServicioDePeajes(contexto);
            var estimacion = await servicio.EstimarAsync([(s.Punto, 1)], s.Liviano, null, "", Salida);
            await servicio.CongelarEstimadoAsync(s.Mision, estimacion, Jefe, Momento);
        }

        await using (var contexto = baseDePruebas.Contexto())
        {
            await new ServicioDePeajes(contexto).RecongelarPorSustitucionAsync(
                s.Mision, s.Pesado, Salida, "Primera sustitución.", Jefe, Momento);
        }

        await using (var contexto = baseDePruebas.Contexto())
        {
            var segunda = await new ServicioDePeajes(contexto).RecongelarPorSustitucionAsync(
                s.Mision, s.Liviano, Salida, "Volvió el liviano.", Jefe, Momento);

            Assert.NotNull(segunda);
            Assert.Equal(150m, segunda.TotalAnterior);
            Assert.Equal(50m, segunda.TotalNuevo);
            Assert.Equal(-100m, segunda.Diferencia);
        }

        await using var lectura = baseDePruebas.Contexto();

        // Tres congelamientos, uno vigente. La línea de tiempo entera queda.
        Assert.Equal(3, await lectura.RutasAutorizadasDePeaje.CountAsync(r => r.MisionId == s.Mision));
        Assert.Equal(1, await lectura.RutasAutorizadasDePeaje
            .CountAsync(r => r.MisionId == s.Mision && r.SupersedidaPor == null));

        // Y los dos asientos, en orden: el expediente cuenta la historia completa.
        var asientos = await new ServicioDePeajes(lectura).RecongelamientosDeAsync(s.Mision);
        Assert.Equal(2, asientos.Count);
        Assert.Equal("Primera sustitución.", asientos[0].Motivo);
        Assert.Equal("Volvió el liviano.", asientos[1].Motivo);
    }

    /// <summary>
    /// Una misión <b>sin estimado congelado</b> no falla: devuelve nulo.
    ///
    /// Reasignar antes de aprobar es normal, y ahí no hay nada que recongelar. Tratarlo como
    /// error bloquearía la reasignación por algo que no es un problema.
    /// </summary>
    [Fact]
    public async Task Sin_estimado_congelado_no_hay_nada_que_recongelar()
    {
        var s = await SembrarAsync();

        await using var contexto = baseDePruebas.Contexto();

        Assert.Null(await new ServicioDePeajes(contexto).RecongelarPorSustitucionAsync(
            s.Mision, s.Pesado, Salida, "Cambió el vehículo.", Jefe, Momento));
    }

    // ── Andamios ────────────────────────────────────────────────────────────

    private async Task<Sembrado> SembrarAsync()
    {
        var sufijo = Ulid.NewUlid().ToString()[^6..];

        var punto = Ulid.NewUlid();
        var liviano = Ulid.NewUlid();
        var pesado = Ulid.NewUlid();

        await using var contexto = baseDePruebas.Contexto();

        await SembrarElCatalogoAsync(contexto);

        contexto.PuntosDePeaje.Add(new FilaDePunto
        {
            Id = punto,
            Nombre = $"Caseta {sufijo}",
            Operador = "COVI-H",
            Carretera = "CA-5",
            Corredor = "CA-5",
            Kilometro = 60,
            Vigencias =
            {
                new FilaDeVigenciaDelPunto
                {
                    Id = Ulid.NewUlid(),
                    PuntoId = punto,
                    Estado = EstadoDelPunto.Activo,
                    Fundamento = "Caseta de prueba.",
                    VigenteDesde = new DateOnly(2020, 1, 1),
                    RegistradoDesdeUtc = Registrado,
                },
            },
        });

        // Dos categorías con tarifas distintas en la MISMA caseta: es lo que hace que
        // sustituir el vehículo cambie el monto sin que la ruta cambie.
        foreach (var (categoria, monto) in new[] { ("PRU-LIVIANO", 50m), ("PRU-C2", 150m) })
        {
            contexto.TarifasDePeaje.Add(new FilaDeTarifa
            {
                Id = Ulid.NewUlid(),
                PuntoId = punto,
                Categoria = categoria,
                Monto = monto,
                Fuente = "SAPP · tabla de prueba",
                FechaDeVerificacion = new DateOnly(2026, 1, 1),
                VigenteDesde = new DateOnly(2026, 1, 1),
                RegistradoDesdeUtc = Registrado,
            });
        }

        // ⚠️ **El catálogo de categorías y la matriz son GLOBALES**, no de esta prueba: la
        // categoría se identifica por código y la matriz de `RN-33` se evalúa entera. Sembrarlos
        // en cada prueba duplicaría las reglas, y con dos filas de la misma prioridad la
        // derivación deja de resolver — la categoría vuelve nula y todo estimado queda sin
        // valorar, con un error que no menciona la duplicación.
        //
        // Se siembran una sola vez, con identificadores fijos.

        // Dos vehículos de DOS EJES cada uno y pesos distintos: la categoría la decide el
        // peso, no los ejes, y así la sustitución cambia la tarifa sin cambiar la ruta.
        contexto.Vehiculos.AddRange(
            FlotaSembrada.Vehiculo(
                liviano, $"PRU-L-{sufijo}", null, "Pick-up de prueba RN-61",
                ClaseNormativa.Automovil, kg: 2800, pasajeros: 5, remolque: false, ejes: 2),
            FlotaSembrada.Vehiculo(
                pesado, $"PRU-C-{sufijo}", null, "Camión de prueba RN-61",
                ClaseNormativa.Camion, kg: 9000, pasajeros: 3, remolque: false, ejes: 2));

        await contexto.SaveChangesAsync();

        return new Sembrado(Ulid.NewUlid(), punto, liviano, pesado);
    }

    /// <summary>
    /// El catálogo compartido: las dos categorías y las dos reglas de derivación.
    ///
    /// Idempotente por identificador fijo. Es lo que permite que varias pruebas de esta clase
    /// —y de cualquier otra— convivan sobre la misma base sin pisarse.
    /// </summary>
    private static async Task SembrarElCatalogoAsync(Sigti.Datos.SigtiDbContext contexto)
    {
        if (await contexto.CategoriasDePeaje.AnyAsync(c => c.Codigo == "PRU-LIVIANO")) return;

        contexto.CategoriasDePeaje.AddRange(
            new FilaDeCategoriaDePeaje { Codigo = "PRU-LIVIANO", Nombre = "Liviano" },
            new FilaDeCategoriaDePeaje { Codigo = "PRU-C2", Nombre = "Camión 2 ejes" });

        // Las reglas que derivan la categoría desde la ficha técnica — `RN-33`. Los dos
        // vehículos tienen DOS ejes: lo que los separa es el peso, y por eso la sustitución
        // cambia la tarifa sin cambiar la ruta.
        contexto.ReglasDeCategoriaDePeaje.AddRange(
            new FilaDeReglaDeCategoria
            {
                Id = Ulid.Parse("01JQ8Z00000000000000PRUL01"),
                Categoria = "PRU-LIVIANO",
                Prioridad = 1,
                Fundamento = "Ficha de prueba",
                TipoDeVehiculo = "Pick-up de prueba RN-61",
                VigenteDesde = new DateOnly(2026, 1, 1),
                RegistradoDesdeUtc = Registrado,
            },
            new FilaDeReglaDeCategoria
            {
                Id = Ulid.Parse("01JQ8Z00000000000000PRUC02"),
                Categoria = "PRU-C2",
                Prioridad = 2,
                Fundamento = "Ficha de prueba",
                TipoDeVehiculo = "Camión de prueba RN-61",
                VigenteDesde = new DateOnly(2026, 1, 1),
                RegistradoDesdeUtc = Registrado,
            });

        await contexto.SaveChangesAsync();
    }

    private sealed record Sembrado(Ulid Mision, Ulid Punto, Ulid Liviano, Ulid Pesado);
}
