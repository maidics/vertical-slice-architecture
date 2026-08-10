using System.Collections.Frozen;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VsaTemplate.Common.Interfaces.Features;
using VsaTemplate.Features.Users;
using VsaTemplate.Infrastructure;
using VsaTemplate.Infrastructure.Database;

namespace VsaTemplate.FunctionalTests.Infrastructure;

public static class Testing
{
    private static Guid? _userId;
    private static FrozenSet<string>? _roles;

    public static Guid? GetUserId() => _userId;

    public static FrozenSet<string>? GetUserRoles() => _roles;

    public static async Task ResetState()
    {
        if (TestSetUpFixture.Database is not null)
            await TestSetUpFixture.Database.ResetAsync();

        _userId = null;
        _roles = [];

        using var scope = TestSetUpFixture.ScopeFactory.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<
            RoleManager<IdentityRole<Guid>>
        >();

        foreach (var role in Roles.All)
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }
    }

    public static async Task<Guid> RunAsUserAsync(string email, string password, string[] roles)
    {
        if (roles.Any(role => !Roles.IsValid(role)))
            throw new ArgumentException(
                $"Invalid role: {string.Join(", ", roles.Where(role => !Roles.IsValid(role)))}"
            );

        using var scope = TestSetUpFixture.ScopeFactory.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser { UserName = email, Email = email };

        var creationResult = await userManager.CreateAsync(user, password);

        if (!creationResult.Succeeded)
            throw new InvalidOperationException($"Failed to create user: {email}");

        if (roles.Length > 1)
        {
            var roleResult = await userManager.AddToRolesAsync(user, roles);

            if (!roleResult.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to add user to roles: {email}. Roles: {string.Join(", ", roles)}."
                );
        }

        return user.Id;
    }

    public static async Task<TEntity?> FirstOrDefaultAsync<TEntity>(
        Expression<Func<TEntity, bool>> expression
    )
        where TEntity : class
    {
        using var scope = TestSetUpFixture.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().FirstOrDefaultAsync(expression);
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = TestSetUpFixture.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.AddAsync(entity);

        await context.SaveChangesAsync();
    }

    public static async Task<int> CountAsync<TEntity>()
        where TEntity : class
    {
        using var scope = TestSetUpFixture.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().CountAsync();
    }

    public static async Task DispatchDomainEventAsync(IDomainEvent @event)
    {
        using var scope = TestSetUpFixture.ScopeFactory.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

        await dispatcher.DispatchAsync(@event, CancellationToken.None);
    }
}
