using Microsoft.EntityFrameworkCore.Infrastructure;
using VsaTemplate.Infrastructure.Database;
using VsaTemplate.Infrastructure.Database.Interceptors;
using VsaTemplate.Tests.TestInfrastructure.TemplateTests;

namespace VsaTemplate.Tests.Infrastructure.Database.Interceptors;

public sealed class AuditableEntityInterceptorTests : TemplateTestBase
{
    [Test]
    public void InterceptorShouldBeRegisteredToDbContext()
    {
        using var context = GetRequiredService<ApplicationDbContext>();

        var coreOptions = context
            .GetService<IDbContextOptions>()
            .FindExtension<CoreOptionsExtension>();
        coreOptions.ShouldNotBeNull();

        var interceptors = coreOptions.Interceptors?.ToList();
        interceptors.ShouldNotBeNull();

        var auditable = interceptors.OfType<AuditableEntityInterceptor>();
        auditable.Count().ShouldBe(1);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public void ShouldUpdateCreatedAndModifiedPropertiesWhenEntityIsCreated(bool logUserIn)
    {
        using var context = GetRequiredService<TestDbContext>();

        Guid? userId = LogUserIn(logUserIn);

        var time = GetRequiredService<TimeProvider>();

        var entity = new TestEntity();
        context.Add(entity);
        context.SaveChanges();

        var created = context.TestEntities.FirstOrDefault(x => x.Id == entity.Id);
        created.ShouldNotBeNull();

        var nowDate = time.GetUtcNow().Date;

        created.CreatedBy.ShouldBe(userId);
        created.CreatedOn.Date.ShouldBe(nowDate);
        created.LastModifiedBy.ShouldBe(userId);
        created.LastModifiedOn.Date.ShouldBe(nowDate);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public void ShouldUpdateModifiedPropertiesWhenEntityIsModified(bool logUserIn)
    {
        using var context = GetRequiredService<TestDbContext>();

        Guid? userId = LogUserIn(logUserIn);

        var entity = new TestEntity();
        context.Add(entity);
        context.SaveChanges();

        var created = context.TestEntities.FirstOrDefault(x => x.Id == entity.Id);
        created.ShouldNotBeNull();
        var createdOn = created.CreatedOn;

        entity.Prop = Guid.NewGuid().ToString();
        context.ChangeTracker.DetectChanges();
        context.SaveChanges();

        var updated = context.TestEntities.FirstOrDefault(x => x.Id == entity.Id);
        updated.ShouldNotBeNull();
        updated.LastModifiedBy.ShouldBe(userId);
        updated.LastModifiedOn.ShouldNotBe(createdOn);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public void ShouldUpdateModifiedPropertiesWhenOwnedEntityIsModified(bool logUserIn)
    {
        using var context = GetRequiredService<TestDbContext>();

        Guid? userId = LogUserIn(logUserIn);

        var entity = new TestEntity();
        context.Add(entity);
        context.SaveChanges();

        var created = context.TestEntities.FirstOrDefault(x => x.Id == entity.Id);
        created.ShouldNotBeNull();
        var createdOn = created.CreatedOn;

        entity.OwnedEntity.Prop = Guid.NewGuid().ToString();
        context.ChangeTracker.DetectChanges();
        context.SaveChanges();

        var updated = context.TestEntities.FirstOrDefault(x => x.Id == entity.Id);
        updated.ShouldNotBeNull();
        updated.LastModifiedBy.ShouldBe(userId);
        updated.LastModifiedOn.ShouldNotBe(createdOn);
    }

    private Guid? LogUserIn(bool logUserIn)
    {
        Guid? userId = null;

        if (!logUserIn)
        {
            return userId;
        }

        var user = GetRequiredService<TemplateTestUser>();
        return user.LogIn(Guid.NewGuid(), null);
    }
}
