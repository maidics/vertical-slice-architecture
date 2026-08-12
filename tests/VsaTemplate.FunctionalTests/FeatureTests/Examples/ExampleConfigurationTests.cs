using Shouldly;
using VsaTemplate.Features.Examples;
using VsaTemplate.FunctionalTests.Infrastructure.Common;

namespace VsaTemplate.FunctionalTests.FeatureTests.Examples;

public sealed class ExampleConfigurationTests
    : EntityConfigurationTestBase<ExampleConfiguration, Example>
{
    [Test]
    public void ShouldHaveUniqueIndex()
    {
        var contentProperty = GetProperty(x => x.Content);

        var index = GetEntityType().FindIndex(contentProperty);

        index.ShouldNotBeNull();
        index.IsUnique.ShouldBeTrue();
    }
}
