using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Sigti.Datos;

/// <summary>
/// Construye el contexto para las herramientas de EF —migraciones y generación de
/// script—, no en tiempo de ejecución.
///
/// La cadena sale de <c>SIGTI_CADENA</c> cuando existe. El valor por omisión apunta a
/// una base de desarrollo local y <b>no</b> es configuración de despliegue: en la
/// institución la cadena la provee el entorno.
/// </summary>
public sealed class FabricaEnTiempoDeDiseno : IDesignTimeDbContextFactory<SigtiDbContext>
{
    private const string CadenaDeDesarrollo =
        "Server=localhost;Database=SIGTI_Desarrollo;Trusted_Connection=True;TrustServerCertificate=True";

    public SigtiDbContext CreateDbContext(string[] argumentos)
    {
        var cadena = Environment.GetEnvironmentVariable("SIGTI_CADENA") ?? CadenaDeDesarrollo;

        return new SigtiDbContext(
            new DbContextOptionsBuilder<SigtiDbContext>()
                // ADR-002: EF Core vale 150 por omisión, y sin esto emite SQL que
                // SQL Server 2014 no entiende.
                .UseSqlServer(cadena, sql => sql.UseCompatibilityLevel(120))
                .Options);
    }
}
