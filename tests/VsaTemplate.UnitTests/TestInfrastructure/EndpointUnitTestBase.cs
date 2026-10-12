using VsaTemplate.Common.Interfaces;

namespace VsaTemplate.UnitTests.TestInfrastructure;

public abstract class EndpointUnitTestBase<TEndpoint>
    where TEndpoint : IEndpoint
{
    protected abstract string ExpectedPrefix { get; }
    protected abstract string[] ExpectedTags { get; }

    [Test]
    public void ShouldHaveCorrectPrefix()
    {
        TEndpoint.Prefix.ShouldBe(ExpectedPrefix);
    }

    [Test]
    public void ShouldHaveCorrectTags()
    {
        TEndpoint.Tags.ShouldBe(ExpectedTags);
    }

    public abstract Task MapMethodShouldMapEndpointWithAttributes();
}
