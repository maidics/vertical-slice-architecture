using Microsoft.EntityFrameworkCore;

namespace VsaTemplate.TemplateTests.Infrastructure;

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
