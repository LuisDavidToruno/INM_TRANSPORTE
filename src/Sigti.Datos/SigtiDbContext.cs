using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sigti.Datos.M07_ProgramacionYDespacho;
using Sigti.Dominio.Bitacora;

namespace Sigti.Datos;

/// <summary>
/// Los esquemas de SQL Server son espejo de los módulos: los permisos se otorgan
/// <b>por esquema</b>, que es lo que `RNF-14` necesita con 40 delegaciones.
/// </summary>
public sealed class SigtiDbContext(DbContextOptions<SigtiDbContext> opciones) : DbContext(opciones)
{
    public DbSet<Asiento> Asientos => Set<Asiento>();
    public DbSet<FilaDeExpediente> Expedientes => Set<FilaDeExpediente>();

    /// <summary>
    /// ULID en binary(16) y no en texto: 16 bytes contra 26, y conserva la monotonía que
    /// motiva la elección de `ADR-005`. Un GUID aleatorio como clave agrupada fragmentaría
    /// el índice en cada inserto, y 2014 Standard no tiene compresión que lo amortigüe.
    /// </summary>
    private static readonly ValueConverter<Ulid, byte[]> UlidABinario =
        new(id => id.ToByteArray(), bytes => new Ulid(bytes));

    protected override void OnModelCreating(ModelBuilder modelo)
    {
        modelo.Entity<FilaDeExpediente>(expediente =>
        {
            expediente.ToTable("Expediente", schema: "mision");

            expediente.HasKey(e => e.Id);
            expediente.Property(e => e.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            expediente.Property(e => e.CapturadaPor).HasMaxLength(64).IsRequired();
            expediente.Property(e => e.SolicitanteDeDerecho).HasMaxLength(64).IsRequired();

            expediente.HasMany(e => e.Transiciones)
                .WithOne()
                .HasForeignKey(t => t.ExpedienteId)
                // Cero DELETE en todo el sistema: RNF-02 lo pone como métrica,
                // «registros eliminados físicamente: 0». Toda anulación es asiento reverso.
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelo.Entity<FilaDeTransicion>(transicion =>
        {
            transicion.ToTable("Transicion", schema: "mision");

            transicion.HasKey(t => t.Id);
            transicion.Property(t => t.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            transicion.Property(t => t.ExpedienteId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            transicion.Property(t => t.Transicion).HasMaxLength(8).IsRequired();
            transicion.Property(t => t.Destino).HasConversion<string>().HasMaxLength(32).IsRequired();
            transicion.Property(t => t.Ejecuta).HasMaxLength(64).IsRequired();
            transicion.Property(t => t.Motivo).HasMaxLength(1000);

            // El diario de un expediente no admite dos transiciones en la misma posición.
            transicion.HasIndex(t => new { t.ExpedienteId, t.Orden }).IsUnique();
        });

        modelo.Entity<Asiento>(asiento =>
        {
            asiento.ToTable("Asiento", schema: "bitacora");

            // ADR-005: ULID como clave agrupada. Un GUID aleatorio fragmentaría el índice
            // en cada inserto, y 2014 Standard no tiene compresión de datos que lo amortigüe.
            // Se guarda como binary(16) y no como texto: 16 bytes contra 26, y conserva
            // la monotonía que motiva la elección.
            asiento.HasKey(a => a.Id);
            asiento.Property(a => a.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");

            asiento.Property(a => a.Cola).HasMaxLength(128).IsRequired();
            asiento.Property(a => a.Secuencia).IsRequired();
            asiento.Property(a => a.Contenido).HasMaxLength(4000).IsRequired();
            asiento.Property(a => a.Hash).HasMaxLength(64).IsFixedLength().IsRequired();
            asiento.Property(a => a.MomentoUtc).IsRequired();
            asiento.Property(a => a.DesfaseMinutos).IsRequired();
            asiento.Property(a => a.MomentoRecibidoUtc).IsRequired();

            // La unicidad de (cola, secuencia) es la última red: si el bloqueo de
            // aplicación fallara, el motor rechaza la bifurcación en vez de guardarla.
            asiento.HasIndex(a => new { a.Cola, a.Secuencia }).IsUnique();
        });
    }
}
