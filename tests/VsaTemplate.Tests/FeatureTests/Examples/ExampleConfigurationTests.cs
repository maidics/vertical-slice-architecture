using Shouldly;
using VsaTemplate.Features.Examples;
using VsaTemplate.Tests.Infrastructure.Common;

namespace VsaTemplate.Tests.FeatureTests.Examples;

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
