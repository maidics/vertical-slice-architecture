using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Infrastructure.Database;
using VsaTemplate.Infrastructure.Identity;

namespace VsaTemplate.Tests.TestInfrastructure.WebTests;

// should not be parallelized since the tests use a shared db instance
[NotInParallel("EndpointTests")]
public abstract class EndpointTestBase<TEndpoint>
    where TEndpoint : IEndpoint
{
    [ClassDataSource<WebTestFixture>(Shared = SharedType.PerTestSession)]
    public required WebTestFixture Fixture { get; init; }

    protected IServiceScope _scope = null!;

    protected EndpointRouteBuilderSpy CreateEndpointRouteBuilderSpy() =>
        new(_scope.ServiceProvider);

    protected HttpClient CreateHttpClient() => Fixture.CreateHttpClient();

    protected async Task<HttpClient> LogInAsync(params string[] roles)
    {
        string email = $"{Guid.NewGuid()}@test";
        const string password = "Passw0rd!";

        var user = new ApplicationUser { UserName = email, Email = email };

        var userManager = GetRequiredService<UserManager<ApplicationUser>>();

        var userResult = await userManager.CreateAsync(user, password);

        if (!userResult.Succeeded)
            throw new InvalidOperationException(
                $"Failed to create user: {string.Join(", ", userResult.Errors.Select(e => e.Description))}."
            );

        if (roles.Length > 0)
        {
            var roleResult = await userManager.AddToRolesAsync(user, roles);

            if (!roleResult.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to add user to roles: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}."
                );
        }

        var client = CreateHttpClient();

        var response = await client.PostAsJsonAsync(
            "api/identity/login?useCookies=true",
            new { user.Email, password }
        );

        if (response.StatusCode is not HttpStatusCode.OK)
            throw new InvalidOperationException("Failed to log user in.");

        return client;
    }

    protected async Task SeedAsync(params object[] entities)
    {
        var context = GetRequiredService<ApplicationDbContext>();

        await context.AddRangeAsync(entities);
        await context.SaveChangesAsync();
    }

    protected async Task<T> QueryAsync<T>(Func<ApplicationDbContext, Task<T>> query)
    {
        var context = GetRequiredService<ApplicationDbContext>();
        context.ChangeTracker.Clear(); // the operations on the db are happening through the HttpClient

        return await query(context);
    }

    protected TService GetRequiredService<TService>()
        where TService : notnull => _scope.ServiceProvider.GetRequiredService<TService>();

    protected static string Prefix => TEndpoint.Prefix;
    protected static string[] Tags => TEndpoint.Tags;

    protected abstract string Endpoint { get; }
    public abstract void ShouldHaveCorrectPrefix();
    public abstract void ShouldHaveCorrectTags();
    public abstract void MapMethodShouldMapEndpointWithAttributes();

    [Before(Test)]
    public async Task ResetAsync()
    {
        await Fixture.ResetAsync();

        _scope = Fixture.ScopeFactory.CreateScope();
    }

    [After(Test)]
    public void CleanUp()
    {
        _scope?.Dispose();
    }
}
