using Microsoft.Extensions.DependencyInjection;

namespace VsaTemplate.TemplateTests.Infrastructure.Common.BaseClasses;

public abstract class TestBase
{
    protected IServiceScope _scope = null!;
    protected IServiceProvider _serviceProvider = null!;

    [SetUp]
    public async Task SetUp()
    {
        await Testing.ResetState();

        _scope = TestSetUpFixture.ScopeFactory.CreateScope();
        _serviceProvider = _scope.ServiceProvider;
    }

    [TearDown]
    public void TearDown()
    {
        _scope.Dispose();
        _serviceProvider = null!;
    }
}
