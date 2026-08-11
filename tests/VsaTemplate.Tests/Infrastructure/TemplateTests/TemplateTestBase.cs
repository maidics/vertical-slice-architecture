using Microsoft.Extensions.DependencyInjection;

namespace VsaTemplate.Tests.Infrastructure.TemplateTests;

public abstract class TemplateTestBase
{
    protected IServiceScope _scope = null!;
    protected TemplateTesting _templateTesting = null!;

    [SetUp]
    public async Task SetUp()
    {
        await Testing.ResetState();

        _scope = TestSetUpFixture.ScopeFactory.CreateScope();
        _templateTesting = new TemplateTesting();
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
