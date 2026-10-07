using Microsoft.Extensions.DependencyInjection;

namespace VsaTemplate.Tests.TestInfrastructure.TemplateTests;

[NotInParallel("TemplateTests")]
public abstract class TemplateTestBase
{
    [ClassDataSource<TemplateTestFixture>(Shared = SharedType.PerTestSession)]
    public required TemplateTestFixture Fixture { get; init; }

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
