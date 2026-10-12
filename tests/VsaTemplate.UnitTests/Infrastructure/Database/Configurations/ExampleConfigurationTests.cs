using VsaTemplate.Domain.Entities;
using VsaTemplate.Infrastructure.Database.Configurations;

namespace VsaTemplate.UnitTests.Infrastructure.Database.Configurations;

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
