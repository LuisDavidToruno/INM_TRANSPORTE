using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sigti.Datos;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M05_Motoristas;
using Sigti.Pruebas.Datos;

namespace Sigti.Pruebas.PuntaAPunta;

/// <summary>
/// `M-03` y `M-04` — la flota deja de ser un catálogo en código.
///
/// ── Por qué esto importa más que «guardar una tabla» ─────────────────────────
/// Mientras la flota vivía en código, `BD-03` <b>no podía bloquear</b>: la
/// documentación provisional devolvía siempre vencimientos de 2030, y el propio
/// código lo decía en un comentario para no fingir que había verificado algo.
///
/// Con vencimientos reales, `BD-03` se convierte en el control que `RN-103`
/// describe — y ese control tiene consecuencia: un vehículo del Estado circulando
/// con matrícula vencida es un hallazgo de auditoría cómodo de levantar y difícil
/// de defender.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class FlotaRealPruebas(BaseDePruebas baseDePruebas)
{
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

    private WebApplicationFactory<Program> Aplicacion() => FabricaDeSigti.Crear(baseDePruebas);

    [Fact]
    public async Task Una_matricula_vencida_dentro_del_rango_bloquea_la_programacion()
    {
        // **Esto era imposible antes.** La documentación provisional devolvía 2030 para
        // todo, así que `BD-03` nunca podía bloquear por matrícula — y `RN-103` quedaba
        // escrita y sin ejecutarse.
        var idVehiculo = await SembrarVehiculo(
            siglas: "TR-0099",
            venceMatricula: new DateOnly(2026, 3, 21)); // La ventana llega al 23 con holgura.

        var idMision = Ulid.NewUlid().ToString();
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        await CrearYAprobar(cliente, idMision);

        var respuesta = await cliente.PostComoAsync($"/misiones/{idMision}/programar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            IdVehiculo = idVehiculo,
            IdConductor = _conductor,
        });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);

        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("BD-03", cuerpo);
        // El mensaje dice **cuándo vence**, no «documentación vencida» a secas: quien
        // programa necesita saber si le alcanza con esperar o tiene que cambiar de
        // vehículo.
        Assert.Contains("2026-03-21", cuerpo);
    }

    [Fact]
    public async Task Un_vehiculo_con_la_documentacion_al_dia_si_programa()
    {
        var idVehiculo = await SembrarVehiculo(
            siglas: "TR-0098",
            venceMatricula: new DateOnly(2027, 12, 31));

        var idMision = Ulid.NewUlid().ToString();
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        await CrearYAprobar(cliente, idMision);

        var respuesta = await cliente.PostComoAsync($"/misiones/{idMision}/programar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            IdVehiculo = idVehiculo,
            IdConductor = _conductor,
        });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    [Fact]
    public async Task La_flota_sale_de_la_base_y_no_de_un_catalogo_en_codigo()
    {
        var idVehiculo = await SembrarVehiculo("TR-0097", new DateOnly(2027, 6, 30));

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var flota = await cliente.GetStringAsync("/flota");

        Assert.Contains(idVehiculo, flota);
        Assert.Contains("TR-0097", flota);
    }

    /// <summary>
    /// Siembra un vehículo real. <b>Sin placa metálica</b>, que es estado válido: hay
    /// desabastecimiento nacional y un campo obligatorio rompería la flota real
    /// (`RN-15`).
    /// </summary>
    private async Task<string> SembrarVehiculo(string siglas, DateOnly venceMatricula)
    {
        var id = Ulid.NewUlid();

        await using var contexto = baseDePruebas.Contexto();
        await FlotaSembrada.SembrarAsync(contexto);
        _conductor ??= (await FlotaSembrada.NuevoConductorAsync(contexto, "Motorista de FlotaReal")).ToString();

        contexto.Vehiculos.Add(new FilaDeVehiculo
        {
            Id = id,
            Siglas = siglas,
            Placa = null,
            TieneConstanciaSustitutaDePlaca = true,
            TipoDeVehiculo = "Pick-up",
            Clase = ClaseNormativa.Automovil,
            PesoBrutoKg = 2_800,
            CapacidadPasajeros = 5,
            LlevaRemolque = false,
            VenceMatricula = venceMatricula,
            VencePoliza = null,
            VenceRevisionMecanica = null,
            IdentificacionInstitucionalVerificada = true,
        });

        await contexto.SaveChangesAsync();

        return id.ToString();
    }

    /// <summary>
    /// El motorista de ESTA clase de pruebas. Desde `BD-11`, compartir el del catálogo
    /// sembrado con las otras pruebas de punta a punta es una doble asignación real: todas
    /// programan sobre la misma franja de marzo.
    ///
    /// Se reusa dentro de la clase porque acá **una sola** misión llega a programarse; las
    /// otras dos se rechazan antes por `BD-03` o ni siquiera programan.
    /// </summary>
    private string? _conductor;

    private static async Task CrearYAprobar(HttpClient cliente, string id)
    {
        await cliente.PostComoAsync("/misiones", new
        {
            Id = id,
            CapturadaPor = "P-ASISTENTE",
            SolicitanteDeDerecho = "P-ASISTENTE",
            Dependencia = "Delegacion de Choluteca",
            ObjetoDelTraslado = "Traslado de personal",
            Destino = "Choluteca",
            Salida = new DateOnly(2026, 3, 20),
            Retorno = new DateOnly(2026, 3, 22),
            HoraDeSalida = new TimeOnly(8, 0),
            HoraDeRetorno = new TimeOnly(16, 0),
            HolguraDias = 1,
            Momento,
        });

        await cliente.PostComoAsync($"/misiones/{id}/enviar", new { Ejecuta = "P-ASISTENTE", Momento });
        await cliente.PostComoAsync($"/misiones/{id}/aprobar", new { Ejecuta = "P-JEFATURA", Momento });
    }
}
