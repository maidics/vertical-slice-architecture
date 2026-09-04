using System.Data.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using VsaTemplate.Domain.Constants;
using VsaTemplate.Infrastructure.Database;

namespace VsaTemplate.Tests.TestInfrastructure;

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
        var respawner = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions { TablesToIgnore = ["AspNetRoles"] }
        );
        await connection.CloseAsync();
        return new TestDatabase(connection, respawner);
    }

    public async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        using var roleManager = serviceProvider.GetRequiredService<
            RoleManager<IdentityRole<Guid>>
        >();

        foreach (var role in Roles.All)
        {
            var result = await roleManager.CreateAsync(new IdentityRole<Guid>(role));

            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to seed role: {string.Join(", ", result.Errors.Select(e => e.Description))}."
                );
        }
    }

    public async Task ResetAsync()
    {
        await _connection.OpenAsync();
        await _respawner.ResetAsync(_connection);
        await _connection.CloseAsync();
    }

    public async ValueTask DisposeAsync() => await _connection.DisposeAsync();
}
