using VsaTemplate.Common.Interfaces;

namespace VsaTemplate.UnitTests.TestInfrastructure;

public abstract class EndpointUnitTestBase<TEndpoint>
    where TEndpoint : IEndpoint
{
    protected abstract string ExpectedPrefix { get; }
    protected abstract string[] ExpectedTags { get; }

    private static string _prefix => TEndpoint.Prefix;
    private static string[] _tags => TEndpoint.Tags;

    [Test]
    public void ShouldHaveCorrectPrefix()
    {
        _prefix.ShouldBe(ExpectedPrefix);
    }

    [Test]
    public void ShouldHaveCorrectTags()
    {
        _tags.ShouldBe(ExpectedTags);
    }

    public abstract Task MapMethodShouldMapEndpointWithAttributes();
}
