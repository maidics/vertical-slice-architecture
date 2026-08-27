using Microsoft.Extensions.DependencyInjection;

namespace VsaTemplate.Tests.TestInfrastructure.FunctionalTests;

public abstract class FunctionalTestBase
{
    [ClassDataSource<FunctionalTestFixture>(Shared = SharedType.PerTestSession)]
    public required FunctionalTestFixture Fixture { get; init; }

    [Before(Test)]
    public async Task ResetAsync() => await Fixture.ResetAsync();

    protected TService GetRequiredService<TService>()
        where TService : notnull =>
        Fixture.ServiceScope.ServiceProvider.GetRequiredService<TService>();
}
