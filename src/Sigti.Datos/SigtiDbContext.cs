using Microsoft.EntityFrameworkCore;
using Sigti.Dominio.Bitacora;

namespace Sigti.Datos;

/// <summary>
/// Los esquemas de SQL Server son espejo de los módulos: los permisos se otorgan
/// <b>por esquema</b>, que es lo que `RNF-14` necesita con 40 delegaciones.
/// </summary>
public sealed class SigtiDbContext(DbContextOptions<SigtiDbContext> opciones) : DbContext(opciones)
{
    public DbSet<Asiento> Asientos => Set<Asiento>();

    protected override void OnModelCreating(ModelBuilder modelo)
    {
        modelo.Entity<Asiento>(asiento =>
        {
            asiento.ToTable("Asiento", schema: "bitacora");

            // ADR-005: ULID como clave agrupada. Un GUID aleatorio fragmentaría el índice
            // en cada inserto, y 2014 Standard no tiene compresión de datos que lo amortigüe.
            // Se guarda como binary(16) y no como texto: 16 bytes contra 26, y conserva
            // la monotonía que motiva la elección.
            asiento.HasKey(a => a.Id);
            asiento.Property(a => a.Id)
                .HasConversion(id => id.ToByteArray(), bytes => new Ulid(bytes))
                .HasColumnType("binary(16)");

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
