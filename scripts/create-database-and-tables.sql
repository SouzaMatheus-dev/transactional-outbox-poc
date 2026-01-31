-- ============================================================
-- Script completo: cria o banco POC e todas as tabelas do dominio.
-- Execute contra o SQL Server (ex.: sqlcmd, SSMS, Azure Data Studio).
-- Exemplo: sqlcmd -S localhost,1433 -U sa -P YourStrong!Passw0rd -i scripts/create-database-and-tables.sql -C
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'POC')
BEGIN
    CREATE DATABASE [POC];
    PRINT 'Database POC criado.';
END
ELSE
    PRINT 'Database POC ja existe.';
GO

USE [POC];
GO

-- Tabela de migrations do EF Core (para o EF nao tentar recriar as tabelas)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'__EFMigrationsHistory')
BEGIN
    CREATE TABLE [dbo].[__EFMigrationsHistory] (
        [MigrationId]    nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32)  NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
    PRINT 'Tabela __EFMigrationsHistory criada.';
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20250130000000_Initial')
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250130000000_Initial', N'8.0.11');
GO

-- Tabela Initializations (dominio)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Initializations')
BEGIN
    CREATE TABLE [dbo].[Initializations] (
        [Id]         uniqueidentifier NOT NULL,
        [ExternalId] nvarchar(50)     NOT NULL,
        [Status]     int              NOT NULL,
        [CreatedAt]  datetime2        NOT NULL,
        CONSTRAINT [PK_Initializations] PRIMARY KEY ([Id])
    );
    PRINT 'Tabela Initializations criada.';
END
GO

-- Tabela Outbox
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Outbox')
BEGIN
    CREATE TABLE [dbo].[Outbox] (
        [Id]          uniqueidentifier NOT NULL,
        [AggregateId] uniqueidentifier NOT NULL,
        [Type]        nvarchar(200)    NOT NULL,
        [Payload]     nvarchar(max)    NOT NULL,
        [OccurredAt]  datetime2        NOT NULL,
        [ProcessedAt] datetime2        NULL,
        [Attempts]    int              NOT NULL,
        [LockedUntil] datetime2        NULL,
        [LockId]      uniqueidentifier NULL,
        [LastError]   nvarchar(max)    NULL,
        CONSTRAINT [PK_Outbox] PRIMARY KEY ([Id])
    );
    CREATE INDEX [IX_Outbox_ProcessedAt_LockedUntil] ON [dbo].[Outbox] ([ProcessedAt], [LockedUntil]);
    PRINT 'Tabela Outbox criada.';
END
GO

-- Tabela ReceivedEvents
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'ReceivedEvents')
BEGIN
    CREATE TABLE [dbo].[ReceivedEvents] (
        [Id]         uniqueidentifier NOT NULL,
        [MessageKey] nvarchar(100)    NOT NULL,
        [Topic]      nvarchar(200)    NOT NULL,
        [ReceivedAt] datetime2        NOT NULL,
        CONSTRAINT [PK_ReceivedEvents] PRIMARY KEY ([Id])
    );
    PRINT 'Tabela ReceivedEvents criada.';
END
GO

PRINT 'Script concluido. Banco POC com Initializations, Outbox e ReceivedEvents.';
