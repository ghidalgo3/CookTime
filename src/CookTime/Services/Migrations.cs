using Npgsql;

namespace babe_algorithms.Services;

public static class Migrations
{
    private static NpgsqlDataSource CreateNpgsqlDataSource(string connectionString)
    {
        Console.WriteLine("Creating new NpgsqlDataSource");
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        // dataSourceBuilder.MapEnum<Unit>();
        return dataSourceBuilder.Build();
    }

    public static void RunMigrations(ILogger<Program> logger, NpgsqlDataSource dataSource)
    {
        var scriptsPath = Path.Combine(AppContext.BaseDirectory, "Scripts");
        if (!Directory.Exists(scriptsPath))
        {
            logger.LogInformation("Scripts directory not found at {path}, skipping migrations", scriptsPath);
            return;
        }

        logger.LogInformation("Running database migrations from {path}", scriptsPath);

        try
        {
            using var connection = dataSource.OpenConnection();

            // First, unconditionally run the migration tracker setup
            var trackerScript = Path.Combine(scriptsPath, "000_migration_tracker.sql");
            if (File.Exists(trackerScript))
            {
                logger.LogInformation("→ Ensuring migration tracker exists...");
                var trackerSql = File.ReadAllText(trackerScript);
                using var cmd = new NpgsqlCommand(trackerSql, connection);
                cmd.ExecuteNonQuery();
                logger.LogInformation("✓ Migration tracker ready");
            }

            // Find and execute all numbered SQL files in order (excluding 000)
            var sqlFiles = Directory.GetFiles(scriptsPath, "*.sql")
                .Where(f => Path.GetFileName(f) != "000_migration_tracker.sql")
                .OrderBy(f => f)
                .ToList();

            foreach (var sqlFile in sqlFiles)
            {
                var filename = Path.GetFileName(sqlFile);

                // Check if migration already applied
                using var checkCmd = new NpgsqlCommand(
                    "SELECT COUNT(*) FROM cooktime.schema_migrations WHERE script_name = @name", connection);
                checkCmd.Parameters.AddWithValue("name", filename);
                var count = (long)checkCmd.ExecuteScalar()!;

                if (count > 0)
                {
                    logger.LogInformation("✓ Skipping {filename} (already applied)", filename);
                    continue;
                }

                logger.LogInformation("→ Applying {filename}...", filename);

                // Execute the migration - split into individual statements for better error reporting
                var sql = File.ReadAllText(sqlFile);
                var statements = SplitSqlStatements(sql);

                for (int i = 0; i < statements.Count; i++)
                {
                    var statement = statements[i];
                    if (string.IsNullOrWhiteSpace(statement)) continue;

                    try
                    {
                        using var cmd = new NpgsqlCommand(statement, connection);
                        cmd.ExecuteNonQuery();
                        logger.LogDebug("  [{index}/{total}] OK", i + 1, statements.Count);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("  [{index}/{total}] FAILED: {statement}", i + 1, statements.Count, statement.Trim());
                        throw new InvalidOperationException($"Statement {i + 1} failed: {statement.Trim()}", ex);
                    }
                }

                // Calculate checksum
                var checksum = CalculateMD5(sqlFile);

                // Record the migration
                using var recordCmd = new NpgsqlCommand(
                    "INSERT INTO cooktime.schema_migrations (script_name, checksum) VALUES (@name, @checksum)", connection);
                recordCmd.Parameters.AddWithValue("name", filename);
                recordCmd.Parameters.AddWithValue("checksum", checksum);
                recordCmd.ExecuteNonQuery();

                logger.LogInformation("✓ Applied {filename}", filename);
            }

            logger.LogInformation("All migrations complete!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to run migrations");
            throw;
        }
    }

    private static string CalculateMD5(string filePath)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        using var stream = File.OpenRead(filePath);
        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private static List<string> SplitSqlStatements(string sql)
    {
        // Split on semicolons, but be careful with function bodies ($$)
        var statements = new List<string>();
        var current = new System.Text.StringBuilder();
        var inDollarQuote = false;
        var lines = sql.Split('\n');

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Track $$ delimiters for function bodies
            var dollarCount = CountOccurrences(line, "$$");
            if (dollarCount % 2 == 1)
            {
                inDollarQuote = !inDollarQuote;
            }

            current.AppendLine(line);

            // If we're not in a dollar quote and line ends with semicolon, it's end of statement
            if (!inDollarQuote && trimmed.EndsWith(';'))
            {
                var stmt = current.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(stmt) && !IsCommentOnly(stmt))
                {
                    statements.Add(stmt);
                }
                current.Clear();
            }
        }

        // Add any remaining content
        var remaining = current.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(remaining) && !IsCommentOnly(remaining))
        {
            statements.Add(remaining);
        }

        return statements;
    }

    private static int CountOccurrences(string text, string pattern)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }

    private static bool IsCommentOnly(string sql)
    {
        var lines = sql.Split('\n');
        return lines.All(l => string.IsNullOrWhiteSpace(l) || l.Trim().StartsWith("--"));
    }
}