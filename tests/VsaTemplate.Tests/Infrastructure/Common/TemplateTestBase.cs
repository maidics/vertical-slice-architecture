using Microsoft.Extensions.DependencyInjection;
using VsaTemplate.Tests.Infrastructure.TemplateTests;

namespace VsaTemplate.Tests.Infrastructure.Common;

public abstract class TemplateTestBase
{
    protected IServiceScope _scope = null!;

    [SetUp]
    public async Task SetUp()
    {
        await Testing.ResetState();

        _scope = TestSetUpFixture.ScopeFactory.CreateScope();
    }

    [TearDown]
    public void TearDown()
    {
        _scope.Dispose();
    }

    protected TemplateTestDbContext GetContext() =>
        _scope.ServiceProvider.GetRequiredService<TemplateTestDbContext>();

    public IEnumerable<TService> GetServices<TService>()
    {
        return TestSetUpFixture.ScopeFactory.CreateScope().ServiceProvider.GetServices<TService>();
    }

    public TService GetRequiredService<TService>()
        where TService : notnull
    {
        return TestSetUpFixture
            .ScopeFactory.CreateScope()
            .ServiceProvider.GetRequiredService<TService>();
    }
}
