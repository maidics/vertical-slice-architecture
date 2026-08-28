using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.Tests.TestInfrastructure.WebTests;

//TODO: add E2E testing members - which confirms the method and its existence
public abstract class EndpointTestBase<TEndpoint> : IEndpointTests
    where TEndpoint : IEndpoint
{
    protected static string Prefix => TEndpoint.Prefix;
    protected static string[] Tags => TEndpoint.Tags;

    public abstract void ShouldHaveCorrectPrefix();
    public abstract void ShouldHaveCorrectTags();
}
