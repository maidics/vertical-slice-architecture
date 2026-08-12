using Shouldly;
using VsaTemplate.FunctionalTests.Infrastructure.Common;
using VsaTemplate.FunctionalTests.Infrastructure.TemplateTests;
using VsaTemplate.Infrastructure.Database.Interceptors;

namespace VsaTemplate.FunctionalTests.TemplateTests;

public sealed class EntityEntryExtensionTests : TemplateTestBase
{
    [Test]
    public void ShouldReturnTrueIfOwnerEntityIsAdded()
    {
        using var context = GetContext();

        var entity = new TemplateTestEntity();
        context.Add(entity);

        context.Entry(entity).HasChangedOwnedEntities().ShouldBeTrue();
    }

    [Test]
    public void ShouldReturnFalseIfOwnerEntityExistsButNonOwnedPropertyIsChanged()
    {
        using var context = GetContext();

        var entity = new TemplateTestEntity();
        context.Add(entity);
        context.SaveChanges();

        entity.Prop = Guid.NewGuid().ToString();
        context.ChangeTracker.DetectChanges();

        context.Entry(entity).HasChangedOwnedEntities().ShouldBeFalse();
    }

    [Test]
    public void ShouldReturnTrueIfOwnerEntityExistsButOwnedEntityPropertyIsChanged()
    {
        using var context = GetContext();

        var entity = new TemplateTestEntity();
        context.Add(entity);
        context.SaveChanges();

        entity.OwnedEntity.Prop = Guid.NewGuid().ToString();
        context.ChangeTracker.DetectChanges();

        context.Entry(entity).HasChangedOwnedEntities().ShouldBeTrue();
    }

    [Test]
    public void ShouldReturnTrueIfOwnerEntityExistsButOwnedEntityIsReplaced()
    {
        using var context = GetContext();

        var entity = new TemplateTestEntity();
        context.Add(entity);
        context.SaveChanges();

        entity.OwnedEntity = new TemplateOwnedEntity() { Prop = Guid.NewGuid().ToString() };
        context.ChangeTracker.DetectChanges();

        context.Entry(entity).HasChangedOwnedEntities().ShouldBeTrue();
    }

    [Test]
    public void ShouldReturnFalseIfOwnerEntityIsRemoved()
    {
        using var context = GetContext();

        var entity = new TemplateTestEntity();
        context.Add(entity);
        context.SaveChanges();

        context.TemplateTestEntities.Remove(entity);
        context.ChangeTracker.DetectChanges();

        context.Entry(entity).HasChangedOwnedEntities().ShouldBeFalse();
    }
}
