using Shouldly;
using VsaTemplate.Infrastructure.Database.Interceptors;
using VsaTemplate.TemplateTests.Infrastructure;
using VsaTemplate.TemplateTests.Infrastructure.Common;
using VsaTemplate.TemplateTests.Infrastructure.Common.BaseClasses;

namespace VsaTemplate.TemplateTests;

public sealed class EntityEntryExtensionTests : TestBase
{
    [Test]
    public void ShouldReturnTrueIfOwnerEntityIsAdded()
    {
        using var context = GetRequiredService<TestDbContext>();

        var entity = new TestEntity();
        context.Add(entity);

        context.Entry(entity).HasChangedOwnedEntities().ShouldBeTrue();
    }

    [Test]
    public void ShouldReturnFalseIfOwnerEntityExistsButNonOwnedPropertyIsChanged()
    {
        using var context = GetRequiredService<TestDbContext>();

        var entity = new TestEntity();
        context.Add(entity);
        context.SaveChanges();

        entity.Prop = Guid.NewGuid().ToString();
        context.ChangeTracker.DetectChanges();

        context.Entry(entity).HasChangedOwnedEntities().ShouldBeFalse();
    }

    [Test]
    public void ShouldReturnTrueIfOwnerEntityExistsButOwnedEntityPropertyIsChanged()
    {
        using var context = GetRequiredService<TestDbContext>();

        var entity = new TestEntity();
        context.Add(entity);
        context.SaveChanges();

        entity.OwnedEntity.Prop = Guid.NewGuid().ToString();
        context.ChangeTracker.DetectChanges();

        context.Entry(entity).HasChangedOwnedEntities().ShouldBeTrue();
    }

    [Test]
    public void ShouldReturnTrueIfOwnerEntityExistsButOwnedEntityIsReplaced()
    {
        using var context = GetRequiredService<TestDbContext>();

        var entity = new TestEntity();
        context.Add(entity);
        context.SaveChanges();

        entity.OwnedEntity = new TemplateOwnedEntity() { Prop = Guid.NewGuid().ToString() };
        context.ChangeTracker.DetectChanges();

        context.Entry(entity).HasChangedOwnedEntities().ShouldBeTrue();
    }

    [Test]
    public void ShouldReturnFalseIfOwnerEntityIsRemoved()
    {
        using var context = GetRequiredService<TestDbContext>();

        var entity = new TestEntity();
        context.Add(entity);
        context.SaveChanges();

        context.TestEntities.Remove(entity);
        context.ChangeTracker.DetectChanges();

        context.Entry(entity).HasChangedOwnedEntities().ShouldBeFalse();
    }
}
