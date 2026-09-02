using Microsoft.AspNetCore.Identity;
using VsaTemplate.Domain.Constants;
using VsaTemplate.Infrastructure.Identity;

namespace VsaTemplate.Infrastructure.Database;

public sealed class DatabaseInitialiser
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public DatabaseInitialiser(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager
    )
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        await SeedAsync();
    }

    private async Task SeedAsync()
    {
        foreach (var role in Roles.All)
        {
            var result = await _roleManager.CreateAsync(new IdentityRole<Guid>(role));

            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to create role: {string.Join(", ", result.Errors)}"
                );
        }

        List<(ApplicationUser user, string role)> users =
        [
            (
                new ApplicationUser()
                {
                    Email = "user@localhost",
                    UserName = "user@localhost",
                    EmailConfirmed = true,
                },
                Roles.User
            ),
            (
                new ApplicationUser()
                {
                    Email = "admin@localhost",
                    UserName = "admin@localhost",
                    EmailConfirmed = true,
                },
                Roles.Administrator
            ),
        ];

        foreach (var userTuple in users)
        {
            var creationResult = await _userManager.CreateAsync(userTuple.user, "Password123!");

            if (!creationResult.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to create user: {string.Join(", ", creationResult.Errors)}"
                );

            var roleResult = await _userManager.AddToRoleAsync(userTuple.user, userTuple.role);

            if (!roleResult.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to add user to role: {string.Join(", ", roleResult.Errors)}"
                );
        }
    }
}
