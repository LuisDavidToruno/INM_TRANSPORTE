using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sigti.Datos.M07_ProgramacionYDespacho;
using Sigti.Datos.M09_Combustible;
using Sigti.Datos.M14_Auditoria;
using Sigti.Datos.M18_Peajes;
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
    public DbSet<FilaDeConductor> Conductores => Set<FilaDeConductor>();
    public DbSet<FilaDeAsignacionDePuesto> AsignacionesDePuesto => Set<FilaDeAsignacionDePuesto>();
    public DbSet<FilaDeCustodia> Custodias => Set<FilaDeCustodia>();
    public DbSet<FilaDePermisoDeCirculacion> Permisos => Set<FilaDePermisoDeCirculacion>();
    public DbSet<FilaDeCambioDeEstado> CambiosDeEstado => Set<FilaDeCambioDeEstado>();
    public DbSet<FilaDeFondo> Fondos => Set<FilaDeFondo>();
    public DbSet<FilaDeAsignacion> AsignacionesDeCombustible => Set<FilaDeAsignacion>();

    public DbSet<FilaDeObligacion> ObligacionesDeReintegro => Set<FilaDeObligacion>();

    public DbSet<FilaDeLevantamiento> LevantamientosDeBloqueo => Set<FilaDeLevantamiento>();

    public DbSet<FilaDeTanque> Tanques => Set<FilaDeTanque>();

    // ── M-18 Peajes ─────────────────────────────────────────────────────────

    public DbSet<FilaDePunto> PuntosDePeaje => Set<FilaDePunto>();

    public DbSet<FilaDeVigenciaDelPunto> VigenciasDePunto => Set<FilaDeVigenciaDelPunto>();

    public DbSet<FilaDeCategoriaDePeaje> CategoriasDePeaje => Set<FilaDeCategoriaDePeaje>();

    public DbSet<FilaDeTarifa> TarifasDePeaje => Set<FilaDeTarifa>();

    public DbSet<FilaDeReglaDeCategoria> ReglasDeCategoriaDePeaje =>
        Set<FilaDeReglaDeCategoria>();

    public DbSet<FilaDeExoneracion> ExoneracionesDePeaje => Set<FilaDeExoneracion>();

    public DbSet<FilaDePaso> PasosPorCaseta => Set<FilaDePaso>();

    public DbSet<FilaDeRutaAutorizada> RutasAutorizadasDePeaje =>
        Set<FilaDeRutaAutorizada>();

    public DbSet<FilaDeDesvio> DesviosDeclarados => Set<FilaDeDesvio>();

    // ── M-14 Auditoría ──────────────────────────────────────────────────────

    public DbSet<FilaDeFuenteExterna> FuentesExternas => Set<FilaDeFuenteExterna>();

    public DbSet<FilaDeEjecucion> EjecucionesDeConciliacion => Set<FilaDeEjecucion>();

    public DbSet<FilaDeDiferencia> DiferenciasDeConciliacion => Set<FilaDeDiferencia>();

    public DbSet<FilaDeHallazgo> HallazgosPosteriores => Set<FilaDeHallazgo>();

    public DbSet<FilaDeReverso> AsientosReversos => Set<FilaDeReverso>();

    public DbSet<FilaDeSaldo> SaldosDeApertura => Set<FilaDeSaldo>();

    public DbSet<FilaDeMovimientoDeExistencias> MovimientosDeExistencias =>
        Set<FilaDeMovimientoDeExistencias>();
    public DbSet<FilaDeAbastecimiento> Abastecimientos => Set<FilaDeAbastecimiento>();

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

            // `time(0)`: la misión declara a que hora sale, no en que segundo. La precision
            // por omision de SQL Server --time(7)-- guardaria centesimas que nadie escribio y
            // que harian que dos horas iguales se comparen distinto.
            expediente.Property(e => e.HoraDeSalida).HasColumnType("time(0)");
            expediente.Property(e => e.HoraDeRetorno).HasColumnType("time(0)");

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

            // El odometro de `T-14` y `T-18`, como DATO: `BD-05` lo vuelve a leer, y sacarlo
            // de una cadena seria el mismo error que tenia la reserva antes de existir.
            transicion.Property(t => t.Odometro);

            // El nivel del tanque, como DATO. `RN-30` lo vuelve a leer para saber si el
            // calculo es concluyente, y sacarlo de una cadena seria el mismo error que ya se
            // corrigio con la reserva y con el odometro.
            transicion.Property(t => t.NivelDeTanque).HasColumnType("decimal(9,4)");
            transicion.Property(t => t.EscalaDelNivel).HasConversion<string>().HasMaxLength(24);

            // La reserva de `T-08`, como DATO y no como prosa dentro del motivo.
            transicion.Property(t => t.VehiculoTomado).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");
            transicion.Property(t => t.ConductorTomado).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");

            // La ocupacion se consulta por vehiculo y ventana. Sin este indice, saber si el
            // pick-up esta libre el jueves recorre el diario entero de la institucion.
            // Filtrado porque casi ninguna transicion reserva.
            transicion.HasIndex(t => t.VehiculoTomado).HasFilter("[VehiculoTomado] IS NOT NULL");

            // El diario de un expediente no admite dos transiciones en la misma posición.
            transicion.HasIndex(t => new { t.ExpedienteId, t.Orden }).IsUnique();

            // La idempotencia de la sincronización, garantizada por la base y no por una
            // comprobación previa: el mismo hecho reenviado choca acá. `IsUnique` con
            // filtro porque las transiciones de oficina no llevan identificador de captura.
            transicion.HasIndex(t => t.IdDeCaptura)
                .IsUnique()
                .HasFilter("[IdDeCaptura] IS NOT NULL");
        });

        modelo.Entity<FilaDeAsignacionDePuesto>(asignacion =>
        {
            // Esquema propio y nombre que lo dice: es ESPEJO, no maestro (RN-48, DP-001).
            // La estructura de puestos es de ARGOS; aqui no hay endpoint de escritura.
            asignacion.ToTable("AsignacionDePuestoEspejo", schema: "organizacion");

            asignacion.HasKey(a => a.Id);
            asignacion.Property(a => a.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            asignacion.Property(a => a.Persona).HasMaxLength(64).IsRequired();
            asignacion.Property(a => a.Puesto).HasMaxLength(64).IsRequired();

            // Las dos preguntas que se hacen: quien ocupa un puesto, y que puestos tiene
            // una persona. RN-100 resuelve las dos a la fecha del hecho.
            asignacion.HasIndex(a => a.Puesto);
            asignacion.HasIndex(a => a.Persona);
        });

        modelo.Entity<FilaDeConductor>(conductor =>
        {
            conductor.ToTable("Conductor", schema: "motoristas");

            conductor.HasKey(c => c.Id);
            conductor.Property(c => c.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            conductor.Property(c => c.Nombre).HasMaxLength(160).IsRequired();

            // La licencia es unica por persona y es lo que se cita ante un reten.
            conductor.Property(c => c.NumeroDeLicencia).HasMaxLength(40).IsRequired();
            conductor.HasIndex(c => c.NumeroDeLicencia).IsUnique();

            conductor.Property(c => c.Categoria).HasConversion<string>().HasMaxLength(4).IsRequired();
            conductor.Property(c => c.Restricciones).HasMaxLength(400);

            // Lo que vence pronto sostiene RN-17, igual que en la flota.
            conductor.HasIndex(c => c.VenceLicencia);
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

            // Las anclas de `RN-66`. Indexadas las tres estables: la imputacion externa se
            // resuelve por ellas antes que por placa, y se resuelve en cada conciliacion.
            vehiculo.Property(v => v.BienDelInventario).HasMaxLength(64);
            vehiculo.Property(v => v.Chasis).HasMaxLength(64);
            vehiculo.Property(v => v.Motor).HasMaxLength(64);
            vehiculo.Property(v => v.CorrelativoInstitucional).HasMaxLength(64);

            vehiculo.HasIndex(v => v.BienDelInventario);
            vehiculo.HasIndex(v => v.Chasis);
            vehiculo.Property(v => v.CapacidadDeTanqueGalones).HasColumnType("decimal(9,2)");
            vehiculo.Property(v => v.Clase).HasConversion<string>().HasMaxLength(32).IsRequired();

            // Encontrar lo que vence pronto es lo que sostiene RN-17, y sin indice eso
            // seria un recorrido completo de la flota cada vez que alguien abre la alerta.
            vehiculo.HasIndex(v => v.VenceMatricula);
        });

        modelo.Entity<FilaDeCustodia>(custodia =>
        {
            custodia.ToTable("Custodia", schema: "flota");

            custodia.HasKey(c => c.Id);
            custodia.Property(c => c.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            custodia.Property(c => c.VehiculoId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            custodia.Property(c => c.Custodio).HasMaxLength(64).IsRequired();
            custodia.Property(c => c.Acta).HasMaxLength(200);

            // BD-13 pregunta SIEMPRE por vehiculo, y lo hace en el camino critico del
            // despacho. Sin indice, cada despacho recorreria el historial de custodias de
            // toda la institucion -- que crece para siempre, porque nada se borra.
            custodia.HasIndex(c => c.VehiculoId);

            // NO hay indice unico sobre (vehiculo, vigente): dos custodias solapadas son un
            // error de datos, pero impedirlo en la base impediria tambien registrar el
            // traspaso el mismo dia -- que es como ocurre, con acta y sin hueco entre las
            // dos. El solape se detecta al consultar, no se prohibe al escribir.
        });

        modelo.Entity<FilaDeCambioDeEstado>(cambio =>
        {
            cambio.ToTable("CambioDeEstado", schema: "flota");

            cambio.HasKey(c => c.Id);
            cambio.Property(c => c.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            cambio.Property(c => c.VehiculoId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            cambio.Property(c => c.Estado).HasConversion<string>().HasMaxLength(32).IsRequired();
            cambio.Property(c => c.Ejecuta).HasMaxLength(64).IsRequired();
            cambio.Property(c => c.Motivo).HasMaxLength(500);

            // El diario de un vehiculo no admite dos cambios en la misma posicion. Es el
            // mismo invariante que el de la mision, y hace mas falta aca: dos cambios
            // pueden compartir marca de tiempo cuando uno lo fija el sistema por una
            // transicion y otro una persona en el mismo instante.
            cambio.HasIndex(c => new { c.VehiculoId, c.Orden }).IsUnique();
        });

        modelo.Entity<FilaDeFondo>(fondo =>
        {
            fondo.ToTable("Fondo", schema: "combustible");

            fondo.HasKey(f => f.Id);
            fondo.Property(f => f.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            fondo.Property(f => f.Ambito).HasConversion<string>().HasMaxLength(16).IsRequired();
            fondo.Property(f => f.AmbitoDeclarado).HasMaxLength(120).IsRequired();
            fondo.Property(f => f.Solicita).HasMaxLength(64).IsRequired();
            fondo.Property(f => f.Aprueba).HasMaxLength(64);
            fondo.Property(f => f.PartidaPresupuestaria).HasMaxLength(64);

            fondo.HasMany(f => f.Movimientos)
                .WithOne()
                .HasForeignKey(m => m.FondoId)
                // Nada se borra fisicamente. Es la misma restriccion del expediente, y aca
                // pesa mas: lo que colgaria de un fondo borrado es el rastro del dinero.
                .OnDelete(DeleteBehavior.Restrict);

            // El saldo se pregunta por ambito y periodo: cual es el fondo vigente de esta
            // delegacion hoy. Sin indice, esa pregunta recorre todos los fondos de la
            // institucion en el camino critico de emitir un vale.
            fondo.HasIndex(f => new { f.AmbitoDeclarado, f.Desde, f.Hasta });
        });

        modelo.Entity<FilaDeMovimientoDelFondo>(movimiento =>
        {
            movimiento.ToTable("MovimientoDelFondo", schema: "combustible");

            movimiento.HasKey(m => m.Id);
            movimiento.Property(m => m.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            movimiento.Property(m => m.FondoId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            movimiento.Property(m => m.Movimiento).HasMaxLength(8).IsRequired();
            movimiento.Property(m => m.Destino).HasConversion<string>().HasMaxLength(32).IsRequired();
            movimiento.Property(m => m.Ejecuta).HasMaxLength(64).IsRequired();
            movimiento.Property(m => m.Motivo).HasMaxLength(1000);

            // `decimal(18,2)` explicito: el defecto de EF Core en SQL Server trunca a
            // decimal(18,2) igual, pero declararlo es lo que impide que una migracion
            // futura lo cambie sin que nadie lo note. Es dinero publico.
            movimiento.Property(m => m.Monto).HasColumnType("decimal(18,2)");

            movimiento.HasIndex(m => new { m.FondoId, m.Orden }).IsUnique();
        });

        modelo.Entity<FilaDeAsignacion>(asignacion =>
        {
            asignacion.ToTable("Asignacion", schema: "combustible");

            asignacion.HasKey(a => a.Id);
            asignacion.Property(a => a.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            asignacion.Property(a => a.FondoId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            asignacion.Property(a => a.MisionId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            asignacion.Property(a => a.VehiculoId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            asignacion.Property(a => a.Folio).HasMaxLength(32).IsRequired();
            asignacion.Property(a => a.Receptor).HasConversion(UlidABinario).HasColumnType("binary(16)");
            asignacion.Property(a => a.Instrumento).HasMaxLength(32).IsRequired();
            asignacion.Property(a => a.TipoDeCombustible).HasMaxLength(32).IsRequired();
            asignacion.Property(a => a.Monto).HasColumnType("decimal(18,2)");
            asignacion.Property(a => a.Galones).HasColumnType("decimal(18,3)");

            // **`RN-27` requisito 1, impuesto por la base.** El folio es unico en la
            // institucion y no se recicla. Dejarlo a una comprobacion en codigo significa
            // que el proximo endpoint que emita vales puede olvidarla -- y dos vales con el
            // mismo folio destruyen la unica pregunta que el folio contesta.
            asignacion.HasIndex(a => a.Folio).IsUnique();

            // El recuento que `T-15`, `T-19` y `T-21` piden es siempre por mision.
            asignacion.HasIndex(a => a.MisionId);

            // Y el saldo del fondo se calcula sumando las asignaciones de ese fondo.
            asignacion.HasIndex(a => a.FondoId);

            asignacion.HasMany(a => a.Transiciones)
                .WithOne()
                .HasForeignKey(t => t.AsignacionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelo.Entity<FilaDeTransicionDeAsignacion>(transicion =>
        {
            transicion.ToTable("TransicionDeAsignacion", schema: "combustible");

            transicion.HasKey(t => t.Id);
            transicion.Property(t => t.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            transicion.Property(t => t.AsignacionId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            transicion.Property(t => t.Transicion).HasMaxLength(8).IsRequired();
            transicion.Property(t => t.Destino).HasConversion<string>().HasMaxLength(32).IsRequired();
            transicion.Property(t => t.Ejecuta).HasMaxLength(64).IsRequired();
            transicion.Property(t => t.Motivo).HasMaxLength(1000);
            transicion.Property(t => t.IdDeCaptura).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");

            transicion.Property(t => t.ConsumoGalones).HasColumnType("decimal(18,3)");
            transicion.Property(t => t.ConsumoMonto).HasColumnType("decimal(18,2)");
            transicion.Property(t => t.ConsumoEstacion).HasMaxLength(120);
            transicion.Property(t => t.ConsumoComprobante).HasMaxLength(64);
            transicion.Property(t => t.ConsumoCausaSinComprobante).HasMaxLength(300);
            transicion.Property(t => t.Devuelto).HasColumnType("decimal(18,2)");

            transicion.HasIndex(t => new { t.AsignacionId, t.Orden }).IsUnique();

            // **La idempotencia de `V-04`, garantizada por la base.** El dispositivo que no
            // supo si el servidor recibio va a reintentar, y un consumo duplicado inventa
            // una desviacion de conciliacion que nadie va a poder explicar.
            transicion.HasIndex(t => t.IdDeCaptura)
                .IsUnique()
                .HasFilter("[IdDeCaptura] IS NOT NULL");
        });

        modelo.Entity<FilaDeObligacion>(obligacion =>
        {
            obligacion.ToTable("ObligacionDeReintegro", schema: "combustible");

            obligacion.HasKey(o => o.Id);
            obligacion.Property(o => o.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            obligacion.Property(o => o.Responsable).HasConversion(UlidABinario).HasColumnType("binary(16)");
            obligacion.Property(o => o.MisionId).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");
            obligacion.Property(o => o.AsignacionId).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");
            obligacion.Property(o => o.Direccion).HasConversion<string>().HasMaxLength(32).IsRequired();
            obligacion.Property(o => o.Causa).HasConversion<string>().HasMaxLength(32).IsRequired();
            obligacion.Property(o => o.Monto).HasColumnType("decimal(18,2)");

            // **La pregunta del bloqueo se hace en cada emision de vale.** Sin este indice,
            // `RN-86` cuesta un recorrido de tabla por cada V-01 -- y un control que se
            // vuelve caro es un control que alguien termina proponiendo saltarse.
            obligacion.HasIndex(o => o.Responsable);

            // El arqueo y el saldo de apertura de `RN-97` ordenan por antiguedad del hecho.
            obligacion.HasIndex(o => o.FechaDelHecho);

            obligacion.HasMany(o => o.Movimientos)
                .WithOne()
                .HasForeignKey(m => m.ObligacionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelo.Entity<FilaDeMovimientoDeObligacion>(movimiento =>
        {
            movimiento.ToTable("MovimientoDeObligacion", schema: "combustible");

            movimiento.HasKey(m => m.Id);
            movimiento.Property(m => m.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            movimiento.Property(m => m.ObligacionId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            movimiento.Property(m => m.Movimiento).HasMaxLength(8).IsRequired();
            movimiento.Property(m => m.Destino).HasConversion<string>().HasMaxLength(32).IsRequired();
            movimiento.Property(m => m.Persona).HasMaxLength(64).IsRequired();
            movimiento.Property(m => m.Puesto).HasMaxLength(64).IsRequired();
            movimiento.Property(m => m.Motivo).HasMaxLength(2000).IsRequired();
            movimiento.Property(m => m.Pagado).HasColumnType("decimal(18,2)");

            movimiento.HasIndex(m => new { m.ObligacionId, m.Orden }).IsUnique();
        });

        modelo.Entity<FilaDeLevantamiento>(levantamiento =>
        {
            levantamiento.ToTable("LevantamientoDeBloqueo", schema: "combustible");

            levantamiento.HasKey(l => l.Id);
            levantamiento.Property(l => l.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            levantamiento.Property(l => l.MisionId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            levantamiento.Property(l => l.Responsable).HasConversion(UlidABinario).HasColumnType("binary(16)");
            levantamiento.Property(l => l.Persona).HasMaxLength(64).IsRequired();
            levantamiento.Property(l => l.Puesto).HasMaxLength(64).IsRequired();
            levantamiento.Property(l => l.Motivo).HasMaxLength(2000).IsRequired();

            // **Uno por mision y persona.** Levantar dos veces el mismo bloqueo no agrega
            // nada y ensucia el indicador que `RN-86` pide: la excepcion es una, y si hace
            // falta otra razon se escribe en el motivo de la que ya esta.
            levantamiento.HasIndex(l => new { l.MisionId, l.Responsable }).IsUnique();

            levantamiento.HasIndex(l => l.Responsable);
        });

        // ── M-14 Auditoria ──────────────────────────────────────────────────

        modelo.Entity<FilaDeFuenteExterna>(fuente =>
        {
            fuente.ToTable("FuenteExterna", schema: "auditoria");

            fuente.HasKey(f => f.Id);
            fuente.Property(f => f.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            fuente.Property(f => f.Tipo).HasConversion<string>().HasMaxLength(40).IsRequired();
            fuente.Property(f => f.Emisor).HasMaxLength(160).IsRequired();
            fuente.Property(f => f.Formato).HasMaxLength(60).IsRequired();
            fuente.Property(f => f.ResponsableDeLaCarga).HasMaxLength(64).IsRequired();
            fuente.Property(f => f.PorQueNoEstaDisponible).HasMaxLength(500);
        });

        modelo.Entity<FilaDeEjecucion>(ejecucion =>
        {
            ejecucion.ToTable("EjecucionDeConciliacion", schema: "auditoria");

            ejecucion.HasKey(e => e.Id);
            ejecucion.Property(e => e.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            ejecucion.Property(e => e.FuenteId).HasConversion(UlidABinario).HasColumnType("binary(16)");

            // **Obligatorio.** `RN-95` punto 6: sin el documento fuente, una diferencia no se
            // puede volver a comprobar contra el papel del que salio.
            ejecucion.Property(e => e.DocumentoFuente).HasMaxLength(300).IsRequired();

            ejecucion.Property(e => e.Ejecuta).HasMaxLength(64).IsRequired();

            ejecucion.HasIndex(e => new { e.FuenteId, e.FechaDeCorteUtc });

            ejecucion.HasMany(e => e.Diferencias)
                .WithOne()
                .HasForeignKey(d => d.EjecucionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelo.Entity<FilaDeDiferencia>(diferencia =>
        {
            diferencia.ToTable("DiferenciaDeConciliacion", schema: "auditoria");

            diferencia.HasKey(d => d.Id);
            diferencia.Property(d => d.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            diferencia.Property(d => d.EjecucionId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            diferencia.Property(d => d.AsientoId).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");
            diferencia.Property(d => d.VehiculoId).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");
            diferencia.Property(d => d.Lado).HasConversion<string>().HasMaxLength(20).IsRequired();
            diferencia.Property(d => d.Ancla).HasConversion<string>().HasMaxLength(40);
            diferencia.Property(d => d.Monto).HasColumnType("decimal(18,2)");
            diferencia.Property(d => d.Referencia).HasMaxLength(64);
            diferencia.Property(d => d.LineaExterna).HasMaxLength(64);
            diferencia.Property(d => d.Origen).HasMaxLength(60);
            diferencia.Property(d => d.Explicacion).HasMaxLength(1000).IsRequired();
            diferencia.Property(d => d.ResponsableDeSeguimiento).HasMaxLength(64);
            diferencia.Property(d => d.Resolucion).HasMaxLength(1000);

            // Lo abierto se consulta por vehiculo: es la pregunta de quien tiene que
            // explicar una diferencia sobre su unidad.
            diferencia.HasIndex(d => d.VehiculoId);

            // Y el comprobante duplicado se busca por referencia entre delegaciones
            // (`RN-84`, `RN-95` punto 3: la conciliacion cruza el alcance de datos).
            diferencia.HasIndex(d => d.Referencia);
        });

        modelo.Entity<FilaDeHallazgo>(hallazgo =>
        {
            hallazgo.ToTable("HallazgoPosterior", schema: "auditoria");

            hallazgo.HasKey(h => h.Id);
            hallazgo.Property(h => h.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            hallazgo.Property(h => h.VehiculoId).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");
            hallazgo.Property(h => h.MotoristaId).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");
            hallazgo.Property(h => h.Tipo).HasMaxLength(160).IsRequired();
            hallazgo.Property(h => h.ComoSeDescubrio).HasMaxLength(500).IsRequired();
            hallazgo.Property(h => h.Fuente).HasMaxLength(300).IsRequired();
            hallazgo.Property(h => h.DocumentoAdjunto).HasMaxLength(300);
            hallazgo.Property(h => h.Periodo).HasMaxLength(40);
            hallazgo.Property(h => h.Resolucion).HasConversion<string>().HasMaxLength(40);
            hallazgo.Property(h => h.Fundamento).HasMaxLength(2000);

            // La antiguedad se cuenta desde el HECHO, y `RN-97` arrastra los abiertos al
            // ejercicio siguiente por ese orden.
            hallazgo.HasIndex(h => h.FechaDelHecho);
            hallazgo.HasIndex(h => h.VehiculoId);

            hallazgo.HasMany(h => h.Misiones)
                .WithOne()
                .HasForeignKey(m => m.HallazgoId)
                .OnDelete(DeleteBehavior.Restrict);

            hallazgo.HasMany(h => h.Movimientos)
                .WithOne()
                .HasForeignKey(m => m.HallazgoId)
                .OnDelete(DeleteBehavior.Restrict);

            hallazgo.HasMany(h => h.Reversos)
                .WithOne()
                .HasForeignKey(r => r.HallazgoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelo.Entity<FilaDeMisionDelHallazgo>(vinculo =>
        {
            vinculo.ToTable("MisionDelHallazgo", schema: "auditoria");

            vinculo.HasKey(v => new { v.HallazgoId, v.MisionId });
            vinculo.Property(v => v.HallazgoId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            vinculo.Property(v => v.MisionId).HasConversion(UlidABinario).HasColumnType("binary(16)");

            // **La pregunta desde la mision.** Una `CERRADA` no se reabre, pero tiene que
            // poder mostrar que tiene hallazgos posteriores vinculados (§7.5).
            vinculo.HasIndex(v => v.MisionId);
        });

        modelo.Entity<FilaDeMovimientoDelHallazgo>(movimiento =>
        {
            movimiento.ToTable("MovimientoDelHallazgo", schema: "auditoria");

            movimiento.HasKey(m => m.Id);
            movimiento.Property(m => m.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            movimiento.Property(m => m.HallazgoId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            movimiento.Property(m => m.ReversoId).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");
            movimiento.Property(m => m.Movimiento).HasMaxLength(8).IsRequired();
            movimiento.Property(m => m.Persona).HasMaxLength(64).IsRequired();
            movimiento.Property(m => m.Puesto).HasMaxLength(64).IsRequired();
            movimiento.Property(m => m.Motivo).HasMaxLength(2000).IsRequired();

            movimiento.HasIndex(m => new { m.HallazgoId, m.Orden }).IsUnique();
        });

        modelo.Entity<FilaDeReverso>(reverso =>
        {
            reverso.ToTable("AsientoReverso", schema: "auditoria");

            reverso.HasKey(r => r.Id);
            reverso.Property(r => r.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            reverso.Property(r => r.HallazgoId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            reverso.Property(r => r.Naturaleza).HasConversion<string>().HasMaxLength(40).IsRequired();
            reverso.Property(r => r.TipoDeAsiento).HasMaxLength(60).IsRequired();
            reverso.Property(r => r.IdentificadorDelAsiento).HasMaxLength(64).IsRequired();
            reverso.Property(r => r.DescripcionDelAsiento).HasMaxLength(300).IsRequired();
            reverso.Property(r => r.ValorAnterior).HasMaxLength(300).IsRequired();
            reverso.Property(r => r.ValorNuevo).HasMaxLength(300);
            reverso.Property(r => r.Persona).HasMaxLength(64).IsRequired();
            reverso.Property(r => r.Puesto).HasMaxLength(64).IsRequired();
            reverso.Property(r => r.Autoriza).HasMaxLength(64).IsRequired();
            reverso.Property(r => r.AutorDelAsientoOriginal).HasMaxLength(64).IsRequired();
            reverso.Property(r => r.MotivoTipificado).HasMaxLength(160).IsRequired();
            reverso.Property(r => r.Fundamento).HasMaxLength(2000).IsRequired();
            reverso.Property(r => r.Adjunto).HasMaxLength(300);
            reverso.Property(r => r.PeriodoAfectado).HasMaxLength(40).IsRequired();
            reverso.Property(r => r.PeriodoDeImputacion).HasMaxLength(40).IsRequired();
            reverso.Property(r => r.TablasParametricas).HasMaxLength(500);
            reverso.Property(r => r.EfectoEconomico).HasColumnType("decimal(18,2)");

            // **Un asiento se revierte una sola vez.** Un segundo reverso duplicaria el
            // efecto economico sobre el periodo corriente, y esa correccion de mas no la va
            // a poder rastrear nadie. El indice lo impone la base, no una comprobacion que
            // el proximo endpoint pueda olvidar.
            reverso.HasIndex(r => new { r.TipoDeAsiento, r.IdentificadorDelAsiento }).IsUnique();

            // Y el acumulado del periodo corriente se arma sumando por aca.
            reverso.HasIndex(r => r.PeriodoDeImputacion);
        });

        modelo.Entity<FilaDeSaldo>(saldo =>
        {
            saldo.ToTable("SaldoDeApertura", schema: "auditoria");

            saldo.HasKey(s => s.Id);
            saldo.Property(s => s.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            saldo.Property(s => s.Folio).HasMaxLength(32).IsRequired();
            saldo.Property(s => s.Ejercicio).HasMaxLength(16).IsRequired();
            saldo.Property(s => s.Persona).HasMaxLength(64).IsRequired();
            saldo.Property(s => s.Puesto).HasMaxLength(64).IsRequired();
            saldo.Property(s => s.DeclaracionDeBloqueantes).HasMaxLength(2000);
            saldo.Property(s => s.FuentesNoConsultadas).HasMaxLength(4000).IsRequired();

            // **Uno por ejercicio.** Dos inventarios del mismo corte dejarian al acta de
            // cierre sin poder decir cual es el que cita.
            saldo.HasIndex(s => s.Ejercicio).IsUnique();

            // El folio es unico en la institucion, como todo folio (`RN-44`).
            saldo.HasIndex(s => s.Folio).IsUnique();

            saldo.HasMany(s => s.Renglones)
                .WithOne()
                .HasForeignKey(r => r.SaldoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelo.Entity<FilaDeRenglon>(renglon =>
        {
            renglon.ToTable("RenglonDelSaldo", schema: "auditoria");

            renglon.HasKey(r => r.Id);
            renglon.Property(r => r.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            renglon.Property(r => r.SaldoId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            renglon.Property(r => r.Tipo).HasConversion<string>().HasMaxLength(40).IsRequired();
            renglon.Property(r => r.Causa).HasConversion<string>().HasMaxLength(40).IsRequired();
            renglon.Property(r => r.Referencia).HasMaxLength(64).IsRequired();
            renglon.Property(r => r.Descripcion).HasMaxLength(500).IsRequired();
            renglon.Property(r => r.Responsable).HasMaxLength(64).IsRequired();
            renglon.Property(r => r.Estado).HasMaxLength(40).IsRequired();
            renglon.Property(r => r.ComoSeResolvio).HasMaxLength(1000);
            renglon.Property(r => r.Monto).HasColumnType("decimal(18,2)");

            // El arrastre entre ejercicios se casa por (tipo, referencia): es lo que
            // identifica al mismo pendiente entre dos cortes.
            renglon.HasIndex(r => new { r.Tipo, r.Referencia });

            // **Un pendiente una vez por saldo.** Contarlo dos veces en el mismo documento
            // inflaria el inventario que `RN-97` manda cuadrar renglon por renglon.
            renglon.HasIndex(r => new { r.SaldoId, r.Tipo, r.Referencia }).IsUnique();
        });

        // ── M-18 Peajes ─────────────────────────────────────────────────────

        modelo.Entity<FilaDePunto>(punto =>
        {
            punto.ToTable("Punto", schema: "peajes");

            punto.HasKey(p => p.Id);
            punto.Property(p => p.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            punto.Property(p => p.Nombre).HasMaxLength(120).IsRequired();
            punto.Property(p => p.Operador).HasMaxLength(120).IsRequired();
            punto.Property(p => p.Carretera).HasMaxLength(120).IsRequired();
            punto.Property(p => p.SentidoDeCobro).HasMaxLength(60);
            punto.Property(p => p.Corredor).HasMaxLength(60);

            // El orden geografico de `RN-37` se resuelve por (corredor, kilometro): las
            // casetas intermedias de un tramo se buscan por ahi en cada liquidacion.
            punto.HasIndex(p => new { p.Corredor, p.Kilometro });

            // La exoneracion por operador se resuelve contra esta columna: es como se
            // otorgan, un acuerdo con un concesionario y no caseta por caseta.
            punto.HasIndex(p => p.Operador);

            punto.HasMany(p => p.Vigencias)
                .WithOne()
                .HasForeignKey(v => v.PuntoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelo.Entity<FilaDeVigenciaDelPunto>(vigencia =>
        {
            vigencia.ToTable("VigenciaDelPunto", schema: "peajes");

            vigencia.HasKey(v => v.Id);
            vigencia.Property(v => v.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            vigencia.Property(v => v.PuntoId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            vigencia.Property(v => v.Estado).HasConversion<string>().HasMaxLength(20).IsRequired();
            vigencia.Property(v => v.Fundamento).HasMaxLength(1000).IsRequired();

            vigencia.HasIndex(v => new { v.PuntoId, v.VigenteDesde });
        });

        modelo.Entity<FilaDeCategoriaDePeaje>(categoria =>
        {
            categoria.ToTable("Categoria", schema: "peajes");

            // **La llave es el codigo, no un ULID.** `RN-33` exige tabla abierta: las filas
            // se cargan en produccion y se citan por su codigo publicado por la SAPP.
            categoria.HasKey(c => c.Codigo);
            categoria.Property(c => c.Codigo).HasMaxLength(32);
            categoria.Property(c => c.Nombre).HasMaxLength(120).IsRequired();
        });

        modelo.Entity<FilaDeTarifa>(tarifa =>
        {
            tarifa.ToTable("Tarifa", schema: "peajes");

            tarifa.HasKey(t => t.Id);
            tarifa.Property(t => t.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            tarifa.Property(t => t.PuntoId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            tarifa.Property(t => t.Categoria).HasMaxLength(32).IsRequired();
            tarifa.Property(t => t.Monto).HasColumnType("decimal(12,2)");

            // **Obligatoria.** `RN-34` punto 3: una tarifa sin fuente no se guarda. La tarifa
            // que ve el usuario es politica y no contractual, y sin saber quien la publico no
            // se puede defender un cobro ante nadie.
            tarifa.Property(t => t.Fuente).HasMaxLength(120).IsRequired();

            tarifa.HasIndex(t => new { t.PuntoId, t.Categoria, t.VigenteDesde });
        });

        modelo.Entity<FilaDeReglaDeCategoria>(regla =>
        {
            regla.ToTable("ReglaDeCategoria", schema: "peajes");

            regla.HasKey(r => r.Id);
            regla.Property(r => r.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            regla.Property(r => r.Categoria).HasMaxLength(32).IsRequired();
            regla.Property(r => r.Clase).HasConversion<string>().HasMaxLength(32);
            regla.Property(r => r.TipoDeVehiculo).HasMaxLength(80);
            regla.Property(r => r.Fundamento).HasMaxLength(1000).IsRequired();

            regla.HasIndex(r => r.Prioridad);
        });

        modelo.Entity<FilaDeExoneracion>(exoneracion =>
        {
            exoneracion.ToTable("Exoneracion", schema: "peajes");

            exoneracion.HasKey(e => e.Id);
            exoneracion.Property(e => e.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            exoneracion.Property(e => e.VehiculoId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            exoneracion.Property(e => e.PuntoId).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");
            exoneracion.Property(e => e.Operador).HasMaxLength(120);

            // **Obligatorio.** Una exoneracion es una excepcion permanente al pago: exige
            // vigilancia proporcional, y sin fundamento no hay que vigilar.
            exoneracion.Property(e => e.Fundamento).HasMaxLength(1000).IsRequired();

            exoneracion.HasIndex(e => e.VehiculoId);
        });

        modelo.Entity<FilaDePaso>(paso =>
        {
            paso.ToTable("PasoPorCaseta", schema: "peajes");

            paso.HasKey(p => p.Id);
            paso.Property(p => p.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            paso.Property(p => p.PuntoId).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");
            paso.Property(p => p.VehiculoId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            paso.Property(p => p.MisionId).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");
            paso.Property(p => p.IdDeCaptura).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");
            paso.Property(p => p.Medio).HasConversion<string>().HasMaxLength(20).IsRequired();
            paso.Property(p => p.Registra).HasMaxLength(64).IsRequired();
            paso.Property(p => p.MontoPagado).HasColumnType("decimal(12,2)");
            paso.Property(p => p.MontoEsperado).HasColumnType("decimal(12,2)");

            // **Las dos categorias, en columnas separadas.** Guardar solo la cobrada haria
            // que el error de la caseta se volviera la verdad institucional y el reclamo
            // nunca ocurriria -- `RN-36`.
            paso.Property(p => p.CategoriaEsperada).HasMaxLength(32);
            paso.Property(p => p.CategoriaCobrada).HasMaxLength(32);

            paso.Property(p => p.Ticket).HasMaxLength(300);
            paso.Property(p => p.CausaSinTicket).HasMaxLength(500);
            paso.Property(p => p.UbicacionDeclarada).HasMaxLength(300);

            paso.HasIndex(p => p.MisionId);
            paso.HasIndex(p => p.VehiculoId);

            // El paso se captura sin conectividad (`RN-43`) y el dispositivo reintenta hasta
            // que le contesten. Un paso duplicado infla el gasto de la mision y produce una
            // discrepancia inventada por el propio sistema.
            paso.HasIndex(p => p.IdDeCaptura)
                .IsUnique()
                .HasFilter("[IdDeCaptura] IS NOT NULL");
        });

        modelo.Entity<FilaDeRutaAutorizada>(ruta =>
        {
            ruta.ToTable("RutaAutorizada", schema: "peajes");

            ruta.HasKey(r => r.Id);
            ruta.Property(r => r.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            ruta.Property(r => r.MisionId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            ruta.Property(r => r.PuntoId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            ruta.Property(r => r.TarifaId).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");
            ruta.Property(r => r.Subtotal).HasColumnType("decimal(12,2)");
            ruta.Property(r => r.Congela).HasMaxLength(64).IsRequired();

            // **Un punto por mision.** El estimado congelado es uno: congelarlo dos veces
            // dejaria dos rutas autorizadas y la pregunta de `RN-37` sin respuesta unica.
            ruta.HasIndex(r => new { r.MisionId, r.PuntoId }).IsUnique();
        });

        modelo.Entity<FilaDeDesvio>(desvio =>
        {
            desvio.ToTable("DesvioDeclarado", schema: "peajes");

            desvio.HasKey(d => d.Id);
            desvio.Property(d => d.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            desvio.Property(d => d.MisionId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            desvio.Property(d => d.VehiculoId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            desvio.Property(d => d.IdDeCaptura).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");
            desvio.Property(d => d.Motivo).HasMaxLength(1000).IsRequired();
            desvio.Property(d => d.Declara).HasMaxLength(64).IsRequired();

            desvio.HasIndex(d => d.MisionId);

            // El desvio se declara desde el campo sin conectividad, y el reintento duplicaria
            // la justificacion -- que es peor que duplicar un dato: justificaria de mas.
            desvio.HasIndex(d => d.IdDeCaptura)
                .IsUnique()
                .HasFilter("[IdDeCaptura] IS NOT NULL");
        });

        modelo.Entity<FilaDeTanque>(tanque =>
        {
            tanque.ToTable("Tanque", schema: "combustible");

            tanque.HasKey(t => t.Id);
            tanque.Property(t => t.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            tanque.Property(t => t.Nombre).HasMaxLength(120).IsRequired();
            tanque.Property(t => t.AmbitoDeclarado).HasMaxLength(120).IsRequired();
            tanque.Property(t => t.TipoDeCombustible).HasMaxLength(32).IsRequired();
            tanque.Property(t => t.CapacidadGalones).HasColumnType("decimal(12,2)");

            tanque.HasIndex(t => t.AmbitoDeclarado);

            tanque.HasMany(t => t.Movimientos)
                .WithOne()
                .HasForeignKey(m => m.TanqueId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelo.Entity<FilaDeMovimientoDeExistencias>(movimiento =>
        {
            movimiento.ToTable("MovimientoDeExistencias", schema: "combustible");

            movimiento.HasKey(m => m.Id);
            movimiento.Property(m => m.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            movimiento.Property(m => m.TanqueId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            movimiento.Property(m => m.VehiculoId).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");
            movimiento.Property(m => m.MisionId).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");
            movimiento.Property(m => m.AbastecimientoId).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");
            movimiento.Property(m => m.ContraparteId).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");
            movimiento.Property(m => m.Movimiento).HasMaxLength(8).IsRequired();
            movimiento.Property(m => m.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();
            movimiento.Property(m => m.MotivoDelAjuste).HasConversion<string>().HasMaxLength(40);
            movimiento.Property(m => m.Persona).HasMaxLength(64).IsRequired();
            movimiento.Property(m => m.Puesto).HasMaxLength(64).IsRequired();
            movimiento.Property(m => m.Motivo).HasMaxLength(1000).IsRequired();
            movimiento.Property(m => m.Comprobante).HasMaxLength(64);
            movimiento.Property(m => m.Galones).HasColumnType("decimal(12,3)");
            movimiento.Property(m => m.ExistenciaMedida).HasColumnType("decimal(12,3)");

            movimiento.HasIndex(m => new { m.TanqueId, m.Orden }).IsUnique();

            // **Un despacho por abastecimiento.** El galon del tanque y el galon que el
            // vehiculo declara son el MISMO hecho visto desde dos lados. Dos despachos
            // contra un abastecimiento descontarian dos veces del tanque, y el faltante
            // resultante lo pagaria alguien en el proximo arqueo.
            movimiento.HasIndex(m => m.AbastecimientoId)
                .IsUnique()
                .HasFilter("[AbastecimientoId] IS NOT NULL");

            // El consumo por vehiculo sale de aca: es el descargo del tanque contra placas.
            movimiento.HasIndex(m => m.VehiculoId);
        });

        modelo.Entity<FilaDeAbastecimiento>(abastecimiento =>
        {
            abastecimiento.ToTable("Abastecimiento", schema: "combustible");

            abastecimiento.HasKey(a => a.Id);
            abastecimiento.Property(a => a.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            abastecimiento.Property(a => a.VehiculoId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            abastecimiento.Property(a => a.MisionId).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");
            abastecimiento.Property(a => a.AsignacionId).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");
            abastecimiento.Property(a => a.TransicionDelValeId).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");
            abastecimiento.Property(a => a.IdDeCaptura).HasConversion(UlidABinarioNulo).HasColumnType("binary(16)");
            abastecimiento.Property(a => a.Fuente).HasConversion<string>().HasMaxLength(32).IsRequired();
            abastecimiento.Property(a => a.Registra).HasMaxLength(64).IsRequired();
            abastecimiento.Property(a => a.Galones).HasColumnType("decimal(18,3)");
            abastecimiento.Property(a => a.Monto).HasColumnType("decimal(18,2)");
            abastecimiento.Property(a => a.Estacion).HasMaxLength(120);
            abastecimiento.Property(a => a.Comprobante).HasMaxLength(64);
            abastecimiento.Property(a => a.CausaSinComprobante).HasMaxLength(300);

            // **El galon no se cuenta dos veces.** El asiento `V-04` del vale y esta fila son
            // el mismo hecho visto desde dos lados; dos filas apuntando al mismo asiento
            // inflarian el denominador de `RN-30` y producirian una desviacion inventada por
            // el propio sistema. Lo impone la BASE, no una comprobacion en codigo.
            abastecimiento.HasIndex(a => a.TransicionDelValeId)
                .IsUnique()
                .HasFilter("[TransicionDelValeId] IS NOT NULL");

            // `RN-30` pregunta por VEHICULO y periodo: cuantos galones entraron a este tanque,
            // vengan de donde vengan. Es la consulta del camino critico de la conciliacion.
            abastecimiento.HasIndex(a => new { a.VehiculoId, a.MomentoUtc });

            // Y por mision, para el desglose que `RN-30` manda mostrar junto a la desviacion.
            abastecimiento.HasIndex(a => a.MisionId).HasFilter("[MisionId] IS NOT NULL");

            // La idempotencia del cliente de campo, garantizada por la BASE. Un `SELECT`
            // previo parece mas limpio y es una condicion de carrera: dos lotes del mismo
            // dispositivo en vuelo pasarian los dos la comprobacion.
            abastecimiento.HasIndex(a => a.IdDeCaptura)
                .IsUnique()
                .HasFilter("[IdDeCaptura] IS NOT NULL");
        });

        modelo.Entity<FilaDePermisoDeCirculacion>(permiso =>
        {
            permiso.ToTable("PermisoDeCirculacion", schema: "mision");

            permiso.HasKey(p => p.Id);
            permiso.Property(p => p.Id).HasConversion(UlidABinario).HasColumnType("binary(16)");
            permiso.Property(p => p.ExpedienteId).HasConversion(UlidABinario).HasColumnType("binary(16)");
            permiso.Property(p => p.Vehiculo).HasConversion(UlidABinario).HasColumnType("binary(16)");
            permiso.Property(p => p.Motorista).HasConversion(UlidABinario).HasColumnType("binary(16)");
            permiso.Property(p => p.Folio).HasMaxLength(32).IsRequired();
            permiso.Property(p => p.EmitidoPor).HasMaxLength(64).IsRequired();
            permiso.Property(p => p.Destino).HasMaxLength(120).IsRequired();

            // BD-04 pregunta por expediente en el camino critico del despacho.
            permiso.HasIndex(p => p.ExpedienteId);
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
