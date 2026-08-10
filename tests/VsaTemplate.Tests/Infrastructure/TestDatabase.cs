using System.Data.Common;
using Microsoft.Data.Sqlite;
using Respawn;

namespace VsaTemplate.FunctionalTests.Infrastructure;

// credit: https://github.com/jasontaylordev/CleanArchitecture
public sealed class TestDatabase : IAsyncDisposable
{
    private readonly DbConnection _connection;
    private readonly Respawner _respawner;

    public TestDatabase(DbConnection connection, Respawner respawner)
    {
        _connection = connection;
        _respawner = respawner;
    }

    public static async Task<TestDatabase> CreateAsync(string connectionString)
    {
        var connection = new SqliteConnection(connectionString);

        await connection.OpenAsync();
        var respawner = await Respawner.CreateAsync(connection);
        await connection.CloseAsync();
        return new TestDatabase(connection, respawner);
    }

    public async Task ResetAsync()
    {
        await _connection.OpenAsync();
        await _respawner.ResetAsync(_connection);
        await _connection.CloseAsync();
    }

    public async ValueTask DisposeAsync() => await _connection.DisposeAsync();
}
