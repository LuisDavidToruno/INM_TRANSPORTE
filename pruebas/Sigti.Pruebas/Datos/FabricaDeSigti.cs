using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Sigti.Datos;

namespace Sigti.Pruebas.Datos;

/// <summary>
/// La aplicación de SIGTI apuntada a la base de pruebas, con la identidad resuelta por cabecera.
///
/// ── Por qué existe una sola y no veintiséis ─────────────────────────────────
/// Cada clase de punta a punta tenía su propia copia de este armado. Veintiséis copias de la
/// misma configuración significan veintiséis lugares donde acordarse de agregar el esquema de
/// autenticación — y la que se olvide no falla: <b>corre sin identidad y pasa</b>, que es
/// exactamente el silencio que este cambio viene a eliminar.
/// </summary>
public static class FabricaDeSigti
{
    /// <param name="ademas">
    /// Lo que una prueba concreta necesite encima. Existe para la que fija la raiz del almacen de
    /// archivos: sin esto tendria que volver a copiar el armado entero, y volveriamos a tener
    /// una copia que se puede olvidar del esquema de autenticacion.
    /// </param>
    public static WebApplicationFactory<Program> Crear(
        BaseDePruebas baseDePruebas, Action<IWebHostBuilder>? ademas = null) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(constructor =>
        {
            ademas?.Invoke(constructor);

            // La clave de firma. **Las pruebas declaran la suya** y no heredan la del
            // ambiente: heredarla ataria la suite a un archivo que no se versiona, y en una
            // maquina sin el la suite fallaria por configuracion en vez de por codigo.
            //
            // Ningun caso de esta suite llega a validar una firma —la identidad entra por
            // cabecera—, pero el arranque exige la clave, y con razon: sin ella el sistema no
            // puede saber quien ejecuta nada.
            constructor.UseSetting("Jwt:Clave", "clave-de-pruebas-de-al-menos-32-caracteres");

            constructor.ConfigureServices(servicios =>
            {
                servicios.RemoveAll(typeof(DbContextOptions<SigtiDbContext>));
                servicios.AddDbContext<SigtiDbContext>(opciones =>
                    opciones.UseSqlServer(
                        baseDePruebas.CadenaDeConexion,
                        sql => sql.UseCompatibilityLevel(120)));

                // La identidad sale de una cabecera en vez de un token firmado. Ver
                // `AutenticacionDePrueba`: es una puerta trasera, y por eso vive acá y no
                // detrás de una condición de ambiente en el API.
                servicios.AddAuthentication(AutenticacionDePrueba.Esquema)
                    .AddScheme<AuthenticationSchemeOptions, AutenticacionDePrueba>(
                        AutenticacionDePrueba.Esquema, _ => { });
            });
        });
}
