using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using TUnit.Mocks;
using TUnit.Mocks.Generated;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Common.Pipeline;

namespace VsaTemplate.Tests.Common.Pipeline;

public sealed class PerformanceFilterTests
{
    private readonly FakeLogger<PerformanceFilter> _logger = new();

    [Test]
    public async Task PerformanceFilterShouldNotLogIfRequestIsResolvedFasterThan500Ms()
    {
        var httpContext = new DefaultHttpContext
        {
            Request = { Method = "POST", Path = new PathString("/test") },
        };

        var request = IRequest.Mock();

        var context = EndpointFilterInvocationContext.Create(httpContext, request.Object);

        var expectedResult = TypedResults.Ok();
        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(expectedResult);

        var user = IUser.Mock();
        var filter = new PerformanceFilter(_logger, user);

        var result = await filter.InvokeAsync(context, next);

        result.ShouldBe(expectedResult);

        _logger.Collector.Count.ShouldBe(0);
    }

    [Test]
    public async Task PerformanceFilterShouldLogIfRequestIsResolvedSlowerThan500Ms()
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

        async ValueTask<object?> Next(EndpointFilterInvocationContext _)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(501));
            return expectedResult;
        }

        var user = IUser.Mock();
        var userId = Guid.NewGuid();
        user.Id.Returns(userId);

        var filter = new PerformanceFilter(_logger, user);

        var result = await filter.InvokeAsync(context, Next);

        result.ShouldBe(expectedResult);

        _logger.Collector.Count.ShouldBe(1);
        _logger.Collector.LatestRecord.Level.ShouldBe(LogLevel.Warning);
        _logger.Collector.LatestRecord.Message.ShouldContain(
            $"Long running request: {httpMethod} {path}, {userId}, "
        );
    }
}
