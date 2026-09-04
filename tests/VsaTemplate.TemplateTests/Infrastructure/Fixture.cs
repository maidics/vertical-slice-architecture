using VsaTemplate.Tests.TestInfrastructure;

namespace VsaTemplate.TemplateTests.Infrastructure;

public sealed class Fixture : TestFixtureBase<TemplateTestFactory>
{
    protected override TemplateTestFactory CreateFactory(string connectionString)
    {
        return new TemplateTestFactory(connectionString);
    }
}
