using Microsoft.EntityFrameworkCore;
using Sigti.Datos;

namespace Sigti.Pruebas.Datos;

/// <summary>
/// Base real de SQL Server para las pruebas de integración.
///
/// No se sustituye por una base en memoria: lo que se está probando aquí es
/// precisamente el comportamiento del motor —sp_getapplock, transacciones,
/// concurrencia—, y un proveedor en memoria no lo tiene. Una prueba de
/// concurrencia contra InMemory pasa siempre y no prueba nada.
///
/// La base se crea en COMPATIBILITY_LEVEL 120 para restaurar el estimador de
/// cardinalidad de SQL Server 2014 (ADR-002).
/// </summary>
public sealed class BaseDePruebas : IAsyncLifetime
{
    private const string Servidor = "Server=localhost;Trusted_Connection=True;TrustServerCertificate=True";
    public const string NombreDeBase = "SIGTI_Pruebas";

    public string CadenaDeConexion => $"{Servidor};Database={NombreDeBase}";

    public SigtiDbContext Contexto() =>
        new(new DbContextOptionsBuilder<SigtiDbContext>()
            .UseSqlServer(CadenaDeConexion, sql => sql.UseCompatibilityLevel(120))
            .Options);

    public async Task InitializeAsync()
    {
        await using var contexto = Contexto();
        await contexto.Database.EnsureDeletedAsync();
        await contexto.Database.EnsureCreatedAsync();

        await using var maestro = new SigtiDbContext(
            new DbContextOptionsBuilder<SigtiDbContext>()
                .UseSqlServer($"{Servidor};Database=master")
                .Options);

        await maestro.Database.ExecuteSqlRawAsync(
            $"ALTER DATABASE [{NombreDeBase}] SET COMPATIBILITY_LEVEL = 120;");
    }

    public async Task DisposeAsync()
    {
        await using var contexto = Contexto();
        await contexto.Database.EnsureDeletedAsync();
    }
}
