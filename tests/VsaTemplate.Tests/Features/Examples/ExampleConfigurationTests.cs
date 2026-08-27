using VsaTemplate.Features.Examples;
using VsaTemplate.Tests.TestInfrastructure.Fixtures;

namespace VsaTemplate.Tests.Features.Examples;

public sealed class ExampleConfigurationTests
{
    private readonly EntityConfigurationFixture<ExampleConfiguration, Example> _fixture = new();

    [Test]
    public void ShouldHaveUniqueIndex()
    {
        var contentProperty = _fixture.GetProperty(x => x.Content);

        var index = _fixture.EntityType.FindIndex(contentProperty);

        index.ShouldNotBeNull();
        index.IsUnique.ShouldBeTrue();
    }
}
