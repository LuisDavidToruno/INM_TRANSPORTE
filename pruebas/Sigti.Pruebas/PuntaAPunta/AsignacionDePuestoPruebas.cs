using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Sigti.Datos;
using Sigti.Pruebas.Datos;

namespace Sigti.Pruebas.PuntaAPunta;

/// <summary>
/// Quién ocupa un puesto funcional de SIGTI — `HU-129`, `RN-100`, `RNF-14`.
///
/// ── ⚠️ La distinción que estas pruebas defienden ────────────────────────────
/// <b>La competencia vive en el puesto, nunca en la persona.</b> `RNF-14` es taxativo:
/// <i>«permisos asignados directamente a una persona: 0. El modelo no ofrece la operación»</i>.
/// Lo que sí se otorga es la <b>ocupación</b> — quién está en ese puesto y desde cuándo.
///
/// La diferencia no es formal: cuando esa persona rota, se cierra su ocupación y el siguiente
/// hereda las competencias sin tocarlas. `NRM-09` `[V]` describe por qué importa — con el
/// permiso colgando de la persona, cada rotación termina con alguien copiando los permisos del
/// saliente al entrante <i>«para que pueda trabajar»</i>, y arrastrando toda la acumulación
/// indebida que el saliente había juntado.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class AsignacionDePuestoPruebas(BaseDePruebas baseDePruebas)
{
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

    private static readonly DateOnly Desde = new(2026, 3, 12);

    /// <summary>
    /// El caso normal: alguien del padrón pasa a ocupar un puesto funcional, y <b>hereda sus
    /// competencias</b> sin que nadie le otorgue un permiso.
    /// </summary>
    [Fact]
    public async Task Quien_ocupa_el_puesto_hereda_las_competencias_del_puesto()
    {
        var persona = await EnElPadronAsync();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var respuesta = await cliente.PostComoAsync("/puesto/asignar", new
        {
            Ejecuta = "P-ADMIN",
            Persona = persona,
            Puesto = "PUE-DESPACHO-SEDE",
            Desde,
            Hasta = (DateOnly?)null,
            Momento,
        });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);

        var cuerpo = await respuesta.Content.ReadFromJsonAsync<JsonElement>();

        // Los roles salen del PUESTO. La petición no los mandó — no puede.
        Assert.Contains(
            cuerpo.GetProperty("roles").EnumerateArray().Select(r => r.GetString()),
            r => r == "EncargadoDeDespacho");

        // Y el sistema ya lo resuelve como competencia de esa persona.
        var suyas = await cliente.GetFromJsonAsync<JsonElement>($"/puesto/de/{persona}");

        Assert.Contains(
            suyas.GetProperty("puestos").EnumerateArray(),
            p => p.GetProperty("puesto").GetString() == "PUE-DESPACHO-SEDE");
    }

    /// <summary>
    /// ⚠️ <b>La acumulación de alcance «mismo expediente» NO se bloquea al asignar.</b>
    ///
    /// Lo verifiqué en vivo esperando lo contrario, y la regla tiene razón: `I-08` —quien
    /// despacha no entrega el dinero— es incompatible <b>dentro de un expediente</b>, no en
    /// abstracto. Prohibir la acumulación de entrada dejaría inoperante a una delegación de
    /// tres personas, que es el caso normal fuera de la sede.
    ///
    /// `RN-01` la <b>admite y la vigila</b>: el bloqueo llega al ejecutar (§5.3.B), cuando se
    /// sabe sobre qué expediente. Lo que esta prueba fija es que la acumulación <b>se
    /// devuelve visible</b> — una acumulación vigilada que nadie ve es una sin vigilar.
    /// </summary>
    [Fact]
    public async Task La_acumulacion_del_mismo_expediente_se_admite_y_se_declara()
    {
        var persona = await EnElPadronAsync();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        await cliente.PostComoAsync("/puesto/asignar", new
        {
            Ejecuta = "P-ADMIN",
            Persona = persona,
            Puesto = "PUE-DESPACHO-SEDE",
            Desde,
            Hasta = (DateOnly?)null,
            Momento,
        });

        var segunda = await cliente.PostComoAsync("/puesto/asignar", new
        {
            Ejecuta = "P-ADMIN",
            Persona = persona,
            Puesto = "PUE-COMBUSTIBLE",
            Desde,
            Hasta = (DateOnly?)null,
            Momento,
        });

        Assert.Equal(HttpStatusCode.Created, segunda.StatusCode);

        var cuerpo = await segunda.Content.ReadFromJsonAsync<JsonElement>();
        var vigilado = cuerpo.GetProperty("vigilados").EnumerateArray().Single();

        // Con el par nombrado y su porqué: «hay una acumulación vigilada» no le dice a nadie
        // qué vigilar.
        Assert.Equal("I-08", vigilado.GetProperty("par").GetString());
        Assert.Contains("no entrega el dinero", vigilado.GetProperty("porQue").GetString());
    }

    /// <summary>
    /// ⚠️ <b>La acumulación absoluta sí se bloquea al asignar.</b>
    ///
    /// `I-12` es del núcleo irreductible: el auditor no acumula con nada operativo, y eso no
    /// depende de ningún expediente. Es el control preventivo en el sentido que faltaba —
    /// otorgar un rol a un puesto ya lo evaluaba; <b>asignar a alguien un puesto que ya tiene
    /// roles</b> no lo miraba nadie, y es el camino por el que la incompatibilidad entra en la
    /// práctica: los puestos se crean una vez y la gente rota todo el tiempo.
    /// </summary>
    [Fact]
    public async Task La_acumulacion_absoluta_no_se_asigna()
    {
        var persona = await EnElPadronAsync();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        await cliente.PostComoAsync("/puesto/asignar", new
        {
            Ejecuta = "P-ADMIN",
            Persona = persona,
            Puesto = "PUE-DESPACHO-SEDE",
            Desde,
            Hasta = (DateOnly?)null,
            Momento,
        });

        // Y ahora, además, auditoría interna.
        var segunda = await cliente.PostComoAsync("/puesto/asignar", new
        {
            Ejecuta = "P-ADMIN",
            Persona = persona,
            Puesto = "PUE-AUDITORIA-INTERNA",
            Desde,
            Hasta = (DateOnly?)null,
            Momento,
        });

        Assert.Equal(HttpStatusCode.Conflict, segunda.StatusCode);

        var texto = await segunda.Content.ReadAsStringAsync();

        // Nombra el par, a la persona, y que no admite excepción: los tres son lo que dice
        // qué hacer.
        Assert.Contains("I-12", texto);
        Assert.Contains(persona, texto);
        Assert.Contains("no admite excepción", texto);
    }
    /// <summary>
    /// <b>No se asigna un puesto a quien el organigrama no conoce.</b> Produciría competencias
    /// a nombre de un identificador que nadie puede resolver — y el día de la auditoría, un
    /// acto sin persona detrás.
    /// </summary>
    [Fact]
    public async Task No_se_asigna_a_quien_el_padron_no_conoce()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var respuesta = await cliente.PostComoAsync("/puesto/asignar", new
        {
            Ejecuta = "P-ADMIN",
            Persona = "NO-EXISTE-EN-NINGUN-PADRON",
            Puesto = "PUE-DESPACHO-SEDE",
            Desde,
            Hasta = (DateOnly?)null,
            Momento,
        });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Contains("Sincronice el espejo", await respuesta.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// ⚠️ <b>Una asignación espejada no se cierra desde SIGTI</b> — `RN-48`.
    ///
    /// Quien dejó el cargo lo dejó en el sistema dueño, y la sincronización lo va a reflejar.
    /// Cerrarla acá produciría un espejo que contradice a su fuente, y la siguiente
    /// sincronización lo volvería a abrir.
    /// </summary>
    [Fact]
    public async Task La_asignacion_espejada_no_se_cierra_a_mano()
    {
        var id = await UnaEspejadaAsync();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var respuesta = await cliente.PostComoAsync($"/puesto/asignaciones/{id}/cerrar", new
        {
            Ejecuta = "P-ADMIN",
            Hasta = Desde,
            Motivo = "Se fue de la institución.",
            Momento,
        });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);

        var texto = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("RN-48", texto);
        Assert.Contains("Ciérrela allá", texto);
    }

    /// <summary>
    /// Cerrar exige motivo — `RN-04`. Sin él, dentro de un año nadie puede decir si fue una
    /// rotación, una sanción o un error de carga.
    /// </summary>
    [Fact]
    public async Task Cerrar_una_ocupacion_exige_motivo()
    {
        var persona = await EnElPadronAsync();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var creada = await cliente.PostComoAsync("/puesto/asignar", new
        {
            Ejecuta = "P-ADMIN",
            Persona = persona,
            Puesto = "PUE-CUSTODIO-FLOTA",
            Desde,
            Hasta = (DateOnly?)null,
            Momento,
        });

        var id = (await creada.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetString();

        var sinMotivo = await cliente.PostComoAsync($"/puesto/asignaciones/{id}/cerrar", new
        {
            Ejecuta = "P-ADMIN",
            Hasta = Desde,
            Motivo = "   ",
            Momento,
        });

        Assert.Equal(HttpStatusCode.Conflict, sinMotivo.StatusCode);

        // Con motivo sí cierra — y **no borra**: la fila sigue, con su fecha de fin.
        var conMotivo = await cliente.PostComoAsync($"/puesto/asignaciones/{id}/cerrar", new
        {
            Ejecuta = "P-ADMIN",
            Hasta = Desde,
            Motivo = "Rotación a la delegación de Choluteca.",
            Momento,
        });

        Assert.True(conMotivo.IsSuccessStatusCode, await conMotivo.Content.ReadAsStringAsync());

        await using var contexto = baseDePruebas.Contexto();
        var fila = await contexto.AsignacionesDePuesto.SingleAsync(a => a.Id == Ulid.Parse(id!));

        Assert.Equal(Desde, fila.Hasta);
        Assert.Equal(OrigenDeLaAsignacion.Propia, fila.Origen);
    }

    // ── Andamios ────────────────────────────────────────────────────────────

    private WebApplicationFactory<Program> Aplicacion() => FabricaDeSigti.Crear(baseDePruebas);

    /// <summary>
    /// Alguien que el padrón conoce, **espejado** — como llegaría del sistema dueño. Cada
    /// prueba usa el suyo: la base es compartida y dos pruebas asignando el mismo puesto a la
    /// misma persona chocarían entre sí.
    /// </summary>
    private async Task<string> EnElPadronAsync()
    {
        var persona = $"EMP-{Ulid.NewUlid().ToString()[^8..]}";

        await using var contexto = baseDePruebas.Contexto();

        contexto.AsignacionesDePuesto.Add(new FilaDeAsignacionDePuesto
        {
            Id = Ulid.NewUlid(),
            Persona = persona,
            Puesto = $"Cargo de prueba {persona}",
            Origen = OrigenDeLaAsignacion.Espejo,
            Desde = new DateOnly(2026, 1, 1),
            Hasta = null,
            // ⚠️ Vieja a proposito. La base es compartida y la antiguedad del espejo se mide
            // sobre el maximo: una fila recien confirmada aca haria que la prueba que verifica
            // «nueve dias sin confirmar» viera cero, y fallaria por vecindad.
            ConfirmadoAlUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        });

        await contexto.SaveChangesAsync();
        return persona;
    }

    private async Task<Ulid> UnaEspejadaAsync()
    {
        var id = Ulid.NewUlid();

        await using var contexto = baseDePruebas.Contexto();

        contexto.AsignacionesDePuesto.Add(new FilaDeAsignacionDePuesto
        {
            Id = id,
            Persona = $"EMP-{Ulid.NewUlid().ToString()[^8..]}",
            Puesto = "Inspector/a de Migración",
            Origen = OrigenDeLaAsignacion.Espejo,
            Desde = new DateOnly(2026, 1, 1),
            Hasta = null,
            // ⚠️ Vieja a proposito. La base es compartida y la antiguedad del espejo se mide
            // sobre el maximo: una fila recien confirmada aca haria que la prueba que verifica
            // «nueve dias sin confirmar» viera cero, y fallaria por vecindad.
            ConfirmadoAlUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        });

        await contexto.SaveChangesAsync();
        return id;
    }
}
