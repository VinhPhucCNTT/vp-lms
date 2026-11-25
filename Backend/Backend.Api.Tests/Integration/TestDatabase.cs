using Npgsql;

namespace Backend.Api.Tests.Integration;

internal static class TestDatabase
{
    internal const string Variable = "VP_LMS_TEST_DB";
    internal const string Name = "vp_lms_integration_tests";
    internal const string Role = "vp_lms_integration_runner";

    internal static NpgsqlConnectionStringBuilder Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Set {Variable} to the dedicated PostgreSQL test connection string.");

        NpgsqlConnectionStringBuilder target;
        try
        {
            target = new NpgsqlConnectionStringBuilder(value);
        }
        catch (Exception error) when (error is ArgumentException or FormatException or OverflowException)
        {
            // Avoid including credentials from a malformed setting in test output.
            throw new InvalidOperationException($"{Variable} is not a valid PostgreSQL connection string.");
        }

        if (target.Database != Name || target.Username != Role ||
            target.Host is not ("localhost" or "127.0.0.1" or "::1") ||
            !target.ShouldSerialize("Host") || !target.ShouldSerialize("Database") ||
            !target.ShouldSerialize("Username") || !string.IsNullOrWhiteSpace(target.Options))
        {
            throw new InvalidOperationException(
                $"{Variable} must explicitly target local database '{Name}' using role '{Role}', without connection options.");
        }

        return target;
    }

    internal static void VerifyEffectiveTarget(NpgsqlConnectionStringBuilder expected, string? effective)
    {
        var actual = Parse(effective);
        if (!expected.EquivalentTo(actual))
            throw new InvalidOperationException($"The API runtime connection differs from {Variable}.");
    }

    internal static async Task VerifyServerAsync(NpgsqlConnectionStringBuilder target)
    {
        await using var connection = new NpgsqlConnection(target.ConnectionString);
        try
        {
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand("""
                SELECT current_database(), current_user, session_user, rolsuper, rolcreatedb, rolcreaterole, rolreplication, rolbypassrls
                FROM pg_roles WHERE rolname = current_user
                """, connection);
            await using var row = await command.ExecuteReaderAsync();
            if (!await row.ReadAsync() || row.GetString(0) != Name ||
                row.GetString(1) != Role || row.GetString(2) != Role ||
                row.GetBoolean(3) || row.GetBoolean(4) || row.GetBoolean(5) ||
                row.GetBoolean(6) || row.GetBoolean(7))
            {
                throw new InvalidOperationException("PostgreSQL did not confirm the dedicated test database and restricted test role.");
            }
        }
        catch (NpgsqlException error)
        {
            throw new InvalidOperationException(
                $"Cannot access the dedicated PostgreSQL test database via {Variable}. Check the server, port, database and role credentials ({error.GetType().Name}).");
        }
    }
}
