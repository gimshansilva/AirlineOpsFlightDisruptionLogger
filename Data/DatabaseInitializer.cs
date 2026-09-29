using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AirlineOpsFlightDisruptionLogger.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'AirlineOpsDb' is missing.");
        }

        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("The database name is required in the connection string.");
        }

        var masterBuilder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master"
        };

        await using var masterConnection = new SqlConnection(masterBuilder.ConnectionString);
        await masterConnection.OpenAsync();

        var databaseExists = await masterConnection.ExecuteScalarAsync<int>(
            "SELECT CASE WHEN DB_ID(@DatabaseName) IS NOT NULL THEN 1 ELSE 0 END",
            new { DatabaseName = databaseName });

        if (databaseExists == 0)
        {
            await masterConnection.ExecuteAsync($"CREATE DATABASE [{databaseName}]");
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        var createDisruptionTypesSql = @"
            IF OBJECT_ID(N'dbo.DisruptionTypes', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.DisruptionTypes
                (
                    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DisruptionTypes PRIMARY KEY,
                    Code VARCHAR(10) NOT NULL CONSTRAINT UQ_DisruptionTypes_Code UNIQUE,
                    Description VARCHAR(100) NOT NULL
                );
            END;";

        var createDisruptionLogsSql = @"
            IF OBJECT_ID(N'dbo.DisruptionLogs', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.DisruptionLogs
                (
                    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DisruptionLogs PRIMARY KEY,
                    FlightNumber VARCHAR(10) NOT NULL,
                    DisruptionTypeId INT NOT NULL,
                    DelayMinutes INT NOT NULL,
                    RequiresPassengerHotel BIT NOT NULL,
                    RequiresMealVoucher BIT NOT NULL,
                    Remarks NVARCHAR(500) NULL,
                    LoggedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_DisruptionLogs_LoggedAtUtc DEFAULT GETUTCDATE(),
                    CONSTRAINT FK_DisruptionLogs_DisruptionTypes FOREIGN KEY (DisruptionTypeId)
                        REFERENCES dbo.DisruptionTypes(Id)
                );
            END;";

        await connection.ExecuteAsync(createDisruptionTypesSql);
        await connection.ExecuteAsync(createDisruptionLogsSql);

        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM dbo.DisruptionTypes");

        if (count == 0)
        {
            await connection.ExecuteAsync(@"
                INSERT INTO dbo.DisruptionTypes (Code, Description)
                VALUES (@Code, @Description)",
                new[]
                {
                    new { Code = "TECH", Description = "Technical Delay" },
                    new { Code = "WX", Description = "Weather" },
                    new { Code = "CREW", Description = "Crew Duty Limit" },
                    new { Code = "ATC", Description = "Air Traffic Control" },
                    new { Code = "OPS", Description = "Operational Issue" }
                });
        }
    }
}
