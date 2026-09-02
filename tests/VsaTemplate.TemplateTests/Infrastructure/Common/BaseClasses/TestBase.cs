using Microsoft.Extensions.DependencyInjection;

namespace VsaTemplate.TemplateTests.Infrastructure.Common.BaseClasses;

[NotInParallel("TemplateTests")]
public abstract class TestBase
{
    [ClassDataSource<Fixture>(Shared = SharedType.PerTestSession)]
    public required Fixture Fixture { get; init; }

    [Before(Test)]
    public async Task ResetAsync() => await Fixture.ResetAsync();

    public TService GetRequiredService<TService>()
        where TService : notnull =>
        Fixture.ServiceScope.ServiceProvider.GetRequiredService<TService>();
}
