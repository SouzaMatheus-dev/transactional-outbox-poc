using Microsoft.Data.SqlClient;

namespace POC.Infra.Persistence;

/// <summary>
/// Garante que o banco POC existe antes de aplicar migrations (evita "Cannot open database 'POC'").
/// </summary>
public static class SqlServerEnsureDatabase
{
    public static async Task EnsurePocDatabaseExistsAsync(string connectionString, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return;
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;
        if (string.IsNullOrWhiteSpace(databaseName))
            databaseName = "POC";
        builder.InitialCatalog = "master";
        builder.ConnectTimeout = 10;
        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync(ct);
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = @name)
BEGIN
    DECLARE @sql nvarchar(4000) = N'CREATE DATABASE [' + REPLACE(@name, N']', N']]') + N']';
    EXEC sp_executesql @sql;
END";
        cmd.Parameters.AddWithValue("@name", databaseName);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
