using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Common.Pipeline;
using VsaTemplate.Tests.TestInfrastructure;
using VsaTemplate.Tests.TestInfrastructure.TemplateTests;

namespace VsaTemplate.Tests.Common.Pipeline;

public sealed class ValidationFilterTests : TemplateTestBase
{
    [Test]
    [Arguments("valid", true)]
    [Arguments("invalid", false)]
    public async Task ValidationFilterShouldReturnCorrectResult(string prop, bool shouldPass)
    {
        var httpContext = new DefaultHttpContext
        {
            Request = { Method = "POST", Path = new PathString("/test") },
            RequestServices = _scope.ServiceProvider,
        };
        var request = new TestRequest(prop);
        var context = EndpointFilterInvocationContext.Create(httpContext, request);

        var expectedResult = TypedResults.Ok();
        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(expectedResult);

        var logger = new FakeLogger<ValidationFilter>();
        var user = GetRequiredService<IUser>();
        var filter = new ValidationFilter(logger, user);

        var result = await filter.InvokeAsync(context, next);

        if (shouldPass)
        {
            result.ShouldBe(expectedResult);
            logger.Collector.Count.ShouldBe(0);
            return;
        }

        result.ShouldNotBe(expectedResult);

        logger.Collector.Count.ShouldBe(1);
        logger.Collector.LatestRecord.Level.ShouldBe(LogLevel.Warning);
        logger.Collector.LatestRecord.Message.ShouldContain("Request validation failed");
    }
}
