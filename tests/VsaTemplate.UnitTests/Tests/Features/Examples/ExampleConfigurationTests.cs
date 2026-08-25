using Shouldly;
using VsaTemplate.Features.Examples;
using VsaTemplate.UnitTests.Infrastructure;

namespace VsaTemplate.UnitTests.Tests.Features.Examples;

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
