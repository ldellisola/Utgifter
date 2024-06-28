using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using Utgifter.Api.Configuration;

namespace Utgifter.Api.DataBase;

public class Migration
{
    private static ILogger<Migration>? _logger;
    public static async Task Run(IServiceProvider services)
    {
        var connectionString = services.GetRequiredService<IOptions<DataBaseOptions>>().Value.ConnectionString;
        _logger = services.GetRequiredService<ILogger<Migration>>();

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        var runAllMigrations = await connection.QueryFirstAsync<bool>("""
                                                                      SELECT NOT EXISTS (
                                                                      SELECT FROM information_schema.tables
                                                                      WHERE  table_schema = 'public'
                                                                      AND    table_name   = 'migrations'
                                                                      )
                                                                      """);

        if (runAllMigrations)
        {
            _logger.LogInformation("Creating migrations table.");
            await connection.ExecuteAsync(
                """
                create table migrations
                (
                    migrationfile text not null
                );
                """
            );
        }

        var migrationsToRun = runAllMigrations ? GetMigrations() : GetMigrations(await GetLastMigrationRan(connection));
        
        await foreach (var (name, content) in migrationsToRun)
        {
            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Running migration: {Migration}",name);
                await connection.ExecuteAsync(content, transaction: transaction);
                await connection.ExecuteAsync("""
                                              INSERT INTO migrations (migrationfile) 
                                              VALUES (@migrationfile)
                                              """, 
                    new { migrationfile = name }, transaction: transaction);

                await transaction.CommitAsync();
                _logger.LogInformation("Migration completed: {Migration}",name);
            }
            catch (Exception e)
            {
                _logger.LogError(e,"Migration failed:");
                await transaction.RollbackAsync();
                Environment.Exit(-1);
            }
        }
    }
    
    private static async Task<string?> GetLastMigrationRan(NpgsqlConnection connection)
    {
        return await connection.QueryFirstOrDefaultAsync<string>("""
                                                                 SELECT migrationfile
                                                                 FROM migrations
                                                                 ORDER BY migrationfile DESC
                                                                 LIMIT 1
                                                                 """) ?? null;
    }
    
    private static async IAsyncEnumerable<(string name, string content)> GetMigrations(string? lastMigrationRan = null)
    {
        var foundFirstMigration = false;
        var migrationFiles = Directory.GetFiles("DataBase/Migrations", "*.sql");
        foreach (var migrationFile in migrationFiles.Order())
        {
            if (foundFirstMigration || lastMigrationRan is null)
            {
                var name = new FileInfo(migrationFile).Name;
                _logger?.LogInformation("Selecting migration: {Migration}",name);
                yield return (name, await File.ReadAllTextAsync(migrationFile));
                continue;
            }
            _logger?.LogInformation("Skipping migration: {Migration}",new FileInfo(migrationFile).Name);
            foundFirstMigration |= new FileInfo(migrationFile).Name == lastMigrationRan;
        }
    }
}