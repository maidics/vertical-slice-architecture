using Shouldly;
using VsaTemplate.Features.Examples;
using VsaTemplate.Tests.Infrastructure.Common;

namespace VsaTemplate.Tests.FeatureTests.Examples;

public sealed class ExampleEndpointGroupTests : IEndpointGroupTests
{
    [Test]
    public void PrefixShouldBeCorrect()
    {
        ExampleEndpoints.Prefix.ShouldBe("Examples");
    }

    [Test]
    public void TagsShouldBeCorrect()
    {
        var tags = ExampleEndpoints.Tags;

        tags.Length.ShouldBe(1);

        var first = tags.FirstOrDefault();
        first.ShouldNotBeNullOrEmpty();
        first.ShouldBe(ExampleEndpoints.Prefix);
    }
}
