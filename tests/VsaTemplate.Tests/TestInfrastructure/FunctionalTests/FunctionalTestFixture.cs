namespace VsaTemplate.Tests.TestInfrastructure.FunctionalTests;

public sealed class FunctionalTestFixture : TestFixtureBase<FunctionalTestWebApplicationFactory>
{
    protected override FunctionalTestWebApplicationFactory CreateFactory(string connectionString)
    {
        return new FunctionalTestWebApplicationFactory(connectionString);
    }
}
