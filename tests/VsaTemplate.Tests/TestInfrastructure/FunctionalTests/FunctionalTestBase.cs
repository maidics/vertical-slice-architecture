using Microsoft.Extensions.DependencyInjection;

namespace VsaTemplate.Tests.TestInfrastructure.FunctionalTests;

// should not be parallelized since the tests use a shared db instance
[NotInParallel("FunctionalTests")]
public abstract class FunctionalTestBase
{
    [ClassDataSource<FunctionalTestFixture>(Shared = SharedType.PerTestSession)]
    public required FunctionalTestFixture Fixture { get; init; }

    protected IServiceScope _scope = null!;

    [Before(Test)]
    public async Task ResetAsync()
    {
        await Fixture.ResetAsync();

        _scope = Fixture.ScopeFactory.CreateScope();
    }

    [After(Test)]
    public void CleanUp()
    {
        _scope?.Dispose();
    }

    protected TService GetRequiredService<TService>()
        where TService : notnull => _scope.ServiceProvider.GetRequiredService<TService>();
}
