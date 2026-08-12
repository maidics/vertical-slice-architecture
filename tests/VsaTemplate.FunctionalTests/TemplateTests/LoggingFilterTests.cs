using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shouldly;
using VsaTemplate.Common.Pipeline;
using VsaTemplate.FunctionalTests.Infrastructure.TemplateTests;

namespace VsaTemplate.FunctionalTests.TemplateTests;

public sealed class LoggingFilterTests : TemplateTestBase
{
    [Test]
    public async Task LoggingFilterShouldLogRequestAndReturnNext()
    {
        var httpContext = new DefaultHttpContext
        {
            Request = { Method = "POST", Path = new PathString("/test") },
        };
        var request = new TemplateTestRequest("logging-test");
        var context = EndpointFilterInvocationContext.Create(httpContext, request);

        var expectedResult = TypedResults.Ok();
        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(expectedResult);

        var logger = new TemplateTestLogger<LoggingFilter>();
        var user = new TemplateTestUser(Guid.NewGuid(), []);
        var filter = new LoggingFilter(logger, user);

        var result = await filter.InvokeAsync(context, next);

        result.ShouldBe(expectedResult);

        logger.Entries.Count.ShouldBe(1);
        logger.Entries[0].Level.ShouldBe(LogLevel.Information);
        logger.Entries[0].Message.ShouldContain("logging-test");
    }
}
