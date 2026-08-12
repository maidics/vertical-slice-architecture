using Microsoft.EntityFrameworkCore;

namespace VsaTemplate.FunctionalTests.Infrastructure.TemplateTests;

public sealed class TemplateTestDbContext : DbContext
{
    public DbSet<TemplateTestEntity> TemplateTestEntities { get; set; }

    public TemplateTestDbContext(DbContextOptions options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TemplateTestEntity>().OwnsOne(t => t.OwnedEntity);
    }
}
