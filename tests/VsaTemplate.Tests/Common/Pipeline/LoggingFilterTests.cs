using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using TUnit.Mocks;
using TUnit.Mocks.Generated;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Common.Pipeline;

namespace VsaTemplate.Tests.Common.Pipeline;

public sealed class LoggingFilterTests
{
    [Test]
    public async Task LoggingFilterShouldLogRequestAndReturnNext()
    {
        const string httpMethod = "POST";
        const string path = "/test";

        var httpContext = new DefaultHttpContext
        {
            Request = { Method = httpMethod, Path = new PathString(path) },
        };
        var request = IRequest.Mock();
        var context = EndpointFilterInvocationContext.Create(httpContext, request.Object);

        var expectedResult = TypedResults.Ok();
        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(expectedResult);

        var logger = new FakeLogger<LoggingFilter>();

        var user = IUser.Mock();
        var userId = Guid.NewGuid();
        user.Id.Returns(userId);

        var filter = new LoggingFilter(logger, user.Object);

        var result = await filter.InvokeAsync(context, next);

        result.ShouldBe(expectedResult);

        logger.Collector.Count.ShouldBe(1);
        logger.Collector.LatestRecord.Level.ShouldBe(LogLevel.Information);
        logger.Collector.LatestRecord.Message.ShouldContain(
            $"Request: {httpMethod} {path}, {expectedResult.StatusCode}, {userId}, "
        );
    }
}
