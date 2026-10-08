using Microsoft.EntityFrameworkCore;

namespace VsaTemplate.Tests.TestInfrastructure.TemplateTests;

public sealed class TestDbContext : DbContext
{
    public DbSet<TestEntity> TestEntities { get; set; }

    public TestDbContext(DbContextOptions options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestEntity>().OwnsOne(t => t.OwnedEntity);
    }
}
