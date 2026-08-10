using Microsoft.EntityFrameworkCore.Diagnostics;
using Shouldly;
using VsaTemplate.Infrastructure.Database.Interceptors;
using VsaTemplate.Tests.Infrastructure;
using VsaTemplate.Tests.Infrastructure.Common;
using VsaTemplate.Tests.Infrastructure.TemplateTests;

namespace VsaTemplate.Tests.TemplateTests;

public sealed class AuditableEntityInterceptorTests : TemplateTestBase
{
    /*
     * TODO: update README.md so that it is clear for the consumer that the tests are inside of this because they
     * can share the same SetUpFixture
     * TODO: add ADR so consumers understand why there's a different db context for template testing: so that template testing does not depend on examples which will in the future be optional
     * TODO: rename all FunctionalTests references to Tests
     * TODO: verify if this project can act as a hybrid test project for both application and web testing
     * TODO: clarify that this project can be used as an "Application" and "Web" layer test project -> "Infrastructure" layer should be tested inside another project
     * TODO: make in memory db only available for AddTemplateTests
     * TODO: ensure no TemplateTest reference, file or folder gets generated when opting out of it
     */

    [Test]
    public void AuditableEntityInterceptorShouldBeRegisteredInServiceCollection()
    {
        var interceptors = GetServices<ISaveChangesInterceptor>();

        var auditableInterceptors = interceptors.OfType<AuditableEntityInterceptor>().ToList();
        auditableInterceptors.Count.ShouldBe(1);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void UpdateAuditablePropertiesShouldUpdateCreatedPropertiesWhenEntityIsCreated(
        bool logUserIn
    )
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
    public void UpdateAuditablePropertiesShouldUpdateModifiedPropertiesWhenEntityIsModified(
        bool logUserIn
    )
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
}
