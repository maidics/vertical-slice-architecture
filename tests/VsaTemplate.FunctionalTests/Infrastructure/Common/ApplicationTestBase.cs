using Microsoft.Extensions.DependencyInjection;

namespace VsaTemplate.FunctionalTests.Infrastructure.Common;

public abstract class ApplicationTestBase
{
    private IServiceScope Scope = null!;

    [SetUp]
    public async Task Setup()
    {
        await Testing.ResetState();

        Scope = TestSetUpFixture.ScopeFactory.CreateScope();

        var dispatcherSpy = Scope.ServiceProvider.GetRequiredService<DomainEventDispatcherSpy>();
        dispatcherSpy.ClearDomainEvents();
    }

    [TearDown]
    public void TearDown()
    {
        Scope.Dispose();
    }

    protected TService GetService<TService>()
        where TService : notnull
    {
        return Scope.ServiceProvider.GetRequiredService<TService>();
    }
}
