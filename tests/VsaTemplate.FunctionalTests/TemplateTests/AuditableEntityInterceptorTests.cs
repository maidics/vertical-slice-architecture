using Microsoft.EntityFrameworkCore.Diagnostics;
using Shouldly;
using VsaTemplate.FunctionalTests.Infrastructure;
using VsaTemplate.FunctionalTests.Infrastructure.Common;
using VsaTemplate.FunctionalTests.Infrastructure.TemplateTests;
using VsaTemplate.Infrastructure.Database.Interceptors;

namespace VsaTemplate.FunctionalTests.TemplateTests;

public sealed class AuditableEntityInterceptorTests : TemplateTestBase
{
    [Test]
    public void AuditableEntityInterceptorShouldBeRegisteredInServiceCollection()
    {
        var interceptors = GetServices<ISaveChangesInterceptor>();

        var auditableInterceptors = interceptors.OfType<AuditableEntityInterceptor>().ToList();
        auditableInterceptors.Count.ShouldBe(1);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ShouldUpdateCreatedPropertiesWhenEntityIsCreated(bool logUserIn)
    {
        using var context = GetContext();

        Guid? userId = logUserIn ? Testing.RunAsUserAsync(Guid.NewGuid(), []) : null;
        var time = GetRequiredService<TimeProvider>();

        var entity = new TemplateTestEntity();
        context.Add(entity);
        context.SaveChanges();

        var created = context.TemplateTestEntities.FirstOrDefault(x => x.Id == entity.Id);
        created.ShouldNotBeNull();
        created.CreatedBy.ShouldBe(userId);
        created.CreatedOn.Date.ShouldBe(time.GetUtcNow().Date);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ShouldUpdateModifiedPropertiesWhenEntityIsModified(bool logUserIn)
    {
        using var context = GetContext();

        var entity = new TemplateTestEntity();
        context.Add(entity);
        context.SaveChanges();

        var createdOn = context.TemplateTestEntities.First(x => x.Id == entity.Id).CreatedOn;

        Guid? userId = logUserIn ? Testing.RunAsUserAsync(Guid.NewGuid(), []) : null;

        entity.Prop = Guid.NewGuid().ToString();
        context.ChangeTracker.DetectChanges();
        context.SaveChanges();

        var updated = context.TemplateTestEntities.FirstOrDefault(x => x.Id == entity.Id);
        updated.ShouldNotBeNull();
        updated.LastModifiedBy.ShouldBe(userId);
        updated.LastModifiedOn.ShouldNotBe(createdOn);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ShouldUpdateModifiedPropertiesWhenOwnedEntityIsModified(bool logUserIn)
    {
        using var context = GetContext();

        var entity = new TemplateTestEntity();
        context.Add(entity);
        context.SaveChanges();

        var createdOn = context.TemplateTestEntities.First(x => x.Id == entity.Id).CreatedOn;

        Guid? userId = logUserIn ? Testing.RunAsUserAsync(Guid.NewGuid(), []) : null;

        entity.OwnedEntity.Prop = Guid.NewGuid().ToString();
        context.ChangeTracker.DetectChanges();
        context.SaveChanges();

        var updated = context.TemplateTestEntities.FirstOrDefault(x => x.Id == entity.Id);
        updated.ShouldNotBeNull();
        updated.LastModifiedBy.ShouldBe(userId);
        updated.LastModifiedOn.ShouldNotBe(createdOn);
    }
}
