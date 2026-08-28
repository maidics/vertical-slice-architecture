using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using VsaTemplate.Features.Users;
#if (AddExamples)
using VsaTemplate.Features.Examples;
#endif

namespace VsaTemplate.Infrastructure.Database;

public sealed class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

#if (AddExamples)
    public DbSet<Example> Examples => Set<Example>();
#endif

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
