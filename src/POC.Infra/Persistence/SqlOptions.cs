namespace POC.Infra.Persistence;

/// <summary>
/// Connection string e opções do SQL Server.
/// </summary>
public class SqlOptions
{
    public const string SectionName = "Sql";
    public string ConnectionString { get; set; } = "Server=localhost,1433;Database=POC;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";
}
