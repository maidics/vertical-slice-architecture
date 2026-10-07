namespace VsaTemplate.Tests.TestInfrastructure.TemplateTests;

public sealed class TemplateTestFixture : TestFixtureBase<TemplateTestWebApplicationFactory>
{
    protected override TemplateTestWebApplicationFactory CreateFactory(string connectionString)
    {
        return new TemplateTestWebApplicationFactory(connectionString);
    }
}
