using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sigti.Datos.M07_ProgramacionYDespacho;
using Sigti.Dominio.Bitacora;
using Sigti.Dominio.M02_Parametros;
using Sigti.Dominio.Organizacion;

namespace Sigti.Datos;

/// <summary>
/// Los esquemas de SQL Server son espejo de los módulos: los permisos se otorgan
/// <b>por esquema</b>, que es lo que `RNF-14` necesita con 40 delegaciones.
/// </summary>
public sealed class SigtiDbContext(DbContextOptions<SigtiDbContext> opciones) : DbContext(opciones)
{
    public DbSet<Asiento> Asientos => Set<Asiento>();
    public DbSet<FilaDeExpediente> Expedientes => Set<FilaDeExpediente>();
    public DbSet<VersionDeParametro> Parametros => Set<VersionDeParametro>();
    public DbSet<FilaDeAdjunto> Adjuntos => Set<FilaDeAdjunto>();
    public DbSet<FilaDeVehiculo> Vehiculos => Set<FilaDeVehiculo>();

    /// <summary>
    /// ULID en binary(16) y no en texto: 16 bytes contra 26, y conserva la monotonía que
    /// motiva la elección de `ADR-005`. Un GUID aleatorio como clave agrupada fragmentaría
    /// el índice en cada inserto, y 2014 Standard no tiene compresión que lo amortigüe.
    /// </summary>
    private static readonly ValueConverter<Ulid, byte[]> UlidABinario =
        new(id => id.ToByteArray(), bytes => new Ulid(bytes));

    /// <summary>El mismo, para las columnas que admiten nulo — `IdDeCaptura`.</summary>
    private static readonly ValueConverter<Ulid?, byte[]?> UlidABinarioNulo =
        new(id => id == null ? null : id.Value.ToByteArray(),
            bytes => bytes == null ? null : new Ulid(bytes));

    protected override void OnModelCreating(ModelBuilder modelo)
    {
        modelo.Entity<VersionDeParametro>(parametro =>
        {
            parametro.ToTable("VersionDeParametro", schema: "catalogo");

            parametro.HasKey(p => p.Id);
            parametro.Property(p => p.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            parametro.Property(p => p.Clave).HasMaxLength(128).IsRequired();
            parametro.Property(p => p.Valor).HasMaxLength(512).IsRequired();

            // Los cuatro campos de la bitemporalidad. Ninguno es nativo del motor:
            // SQL Server 2014 no tiene temporal tables, y aunque las tuviera darían el
            // eje de transacción, no el de vigencia normativa (ADR-006).
            parametro.Property(p => p.VigenteDesde).IsRequired();
            parametro.Property(p => p.VigenteHasta);
            parametro.Property(p => p.RegistradoDesde).IsRequired();
            parametro.Property(p => p.RegistradoHasta);

            // El respaldo documental es obligatorio: «un parámetro sin respaldo no se
            // puede sostener ante el Tribunal Superior de Cuentas» (HU-144). El archivo
            // vive fuera de la base (ADR-004); acá va su referencia.
            parametro.ComplexProperty(p => p.Respaldo, respaldo =>
            {
                respaldo.Property(r => r.Adjunto)
                    .HasColumnName("RespaldoAdjunto")
                    .HasConversion(UlidABinario).HasColumnType("binary(16)").IsRequired();

                respaldo.Property(r => r.Fuente)
                    .HasColumnName("RespaldoFuente").HasMaxLength(512).IsRequired();

                respaldo.Property(r => r.FechaDeVerificacion)
                    .HasColumnName("RespaldoVerificadoEl").IsRequired();
            });

            parametro.Property(p => p.CargadoPor)
                .HasConversion(id => id.Valor, valor => new IdPersona(valor))
                .HasMaxLength(64).IsRequired();

            parametro.Property(p => p.AprobadoPor)
                .HasConversion(
                    id => id == null ? null : id.Value.Valor,
                    valor => valor == null ? null : new IdPersona(valor))
                .HasMaxLength(64);

            // Consultar el catálogo de una clave es la única lectura que hace el sistema
            // sobre esta tabla, y ocurre en cada cálculo.
            parametro.HasIndex(p => new { p.Clave, p.VigenteDesde });
        });

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

            transicion.Property(t => t.IdDeCaptura).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");

            // El diario de un expediente no admite dos transiciones en la misma posición.
            transicion.HasIndex(t => new { t.ExpedienteId, t.Orden }).IsUnique();

            // La idempotencia de la sincronización, garantizada por la base y no por una
            // comprobación previa: el mismo hecho reenviado choca acá. `IsUnique` con
            // filtro porque las transiciones de oficina no llevan identificador de captura.
            transicion.HasIndex(t => t.IdDeCaptura)
                .IsUnique()
                .HasFilter("[IdDeCaptura] IS NOT NULL");
        });

        modelo.Entity<FilaDeVehiculo>(vehiculo =>
        {
            vehiculo.ToTable("Vehiculo", schema: "flota");

            vehiculo.HasKey(v => v.Id);
            vehiculo.Property(v => v.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");

            // Las siglas son la identidad estable del bien y se citan en el descargo.
            vehiculo.Property(v => v.Siglas).HasMaxLength(32).IsRequired();
            vehiculo.HasIndex(v => v.Siglas).IsUnique();

            // La placa NO es unica ni obligatoria: "sin placa" es estado valido por el
            // desabastecimiento nacional, y un indice unico sobre nulos rompe la flota real.
            vehiculo.Property(v => v.Placa).HasMaxLength(16);

            vehiculo.Property(v => v.TipoDeVehiculo).HasMaxLength(80).IsRequired();
            vehiculo.Property(v => v.Clase).HasConversion<string>().HasMaxLength(32).IsRequired();

            // Encontrar lo que vence pronto es lo que sostiene RN-17, y sin indice eso
            // seria un recorrido completo de la flota cada vez que alguien abre la alerta.
            vehiculo.HasIndex(v => v.VenceMatricula);
        });

        modelo.Entity<FilaDeAdjunto>(adjunto =>
        {
            adjunto.ToTable("Adjunto", schema: "mision");

            adjunto.HasKey(a => a.Id);
            adjunto.Property(a => a.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            adjunto.Property(a => a.IdTransicion).HasConversion(UlidABinario).HasColumnType("binary(16)");
            adjunto.Property(a => a.Ruta).HasMaxLength(400).IsRequired();
            adjunto.Property(a => a.Hash).HasMaxLength(80).IsRequired();
            adjunto.Property(a => a.Tipo).HasMaxLength(120).IsRequired();
            adjunto.Property(a => a.Clasificacion).HasMaxLength(32).IsRequired();

            // Encontrar los adjuntos con dato personal es lo que hace atendible un
            // habeas data, y lo que permite depurar sin recorrer treinta mil filas.
            adjunto.HasIndex(a => a.Clasificacion);

            // Todo adjunto respalda un hecho: el paquete de evidencia se arma por ahi.
            adjunto.HasIndex(a => a.IdTransicion);
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
