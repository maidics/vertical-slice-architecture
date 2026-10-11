using VsaTemplate.Common.Interfaces;

namespace VsaTemplate.UnitTests.TestInfrastructure;

public abstract class EndpointTestBase<TEndpoint>
    where TEndpoint : IEndpoint
{
    protected static string Prefix => TEndpoint.Prefix;
    protected static string[] Tags => TEndpoint.Tags;

    public abstract void ShouldHaveCorrectPrefix();
    public abstract void ShouldHaveCorrectTags();

    // public abstract Task MapMethodShouldMapEndpointWithAttributes();
}
