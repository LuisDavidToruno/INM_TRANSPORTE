IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826174227_Inicial'
)
BEGIN
    IF SCHEMA_ID(N'bitacora') IS NULL EXEC(N'CREATE SCHEMA [bitacora];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826174227_Inicial'
)
BEGIN
    IF SCHEMA_ID(N'mision') IS NULL EXEC(N'CREATE SCHEMA [mision];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826174227_Inicial'
)
BEGIN
    CREATE TABLE [bitacora].[Asiento] (
        [Id] binary(16) NOT NULL,
        [Cola] nvarchar(128) NOT NULL,
        [Secuencia] bigint NOT NULL,
        [Contenido] nvarchar(4000) NOT NULL,
        [Hash] nchar(64) NOT NULL,
        [MomentoUtc] datetime2 NOT NULL,
        [DesfaseMinutos] int NOT NULL,
        [MomentoRecibidoUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_Asiento] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826174227_Inicial'
)
BEGIN
    CREATE TABLE [mision].[Expediente] (
        [Id] binary(16) NOT NULL,
        [CapturadaPor] nvarchar(64) NOT NULL,
        [SolicitanteDeDerecho] nvarchar(64) NOT NULL,
        CONSTRAINT [PK_Expediente] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826174227_Inicial'
)
BEGIN
    CREATE TABLE [mision].[Transicion] (
        [Id] binary(16) NOT NULL,
        [ExpedienteId] binary(16) NOT NULL,
        [Orden] int NOT NULL,
        [Transicion] nvarchar(8) NOT NULL,
        [Destino] nvarchar(32) NOT NULL,
        [Ejecuta] nvarchar(64) NOT NULL,
        [MomentoUtc] datetime2 NOT NULL,
        [DesfaseMinutos] int NOT NULL,
        [Motivo] nvarchar(1000) NULL,
        CONSTRAINT [PK_Transicion] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Transicion_Expediente_ExpedienteId] FOREIGN KEY ([ExpedienteId]) REFERENCES [mision].[Expediente] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826174227_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Asiento_Cola_Secuencia] ON [bitacora].[Asiento] ([Cola], [Secuencia]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826174227_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Transicion_ExpedienteId_Orden] ON [mision].[Transicion] ([ExpedienteId], [Orden]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826174227_Inicial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260826174227_Inicial', N'10.0.11');
END;

COMMIT;
GO

