using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Common.Pipeline;
using VsaTemplate.Tests.TestInfrastructure;
using VsaTemplate.Tests.TestInfrastructure.TemplateTests;

namespace VsaTemplate.Tests.Common.Pipeline;

public sealed class PerformanceFilterTests : TemplateTestBase
{
    private readonly FakeLogger<PerformanceFilter> _logger = new();

    [Test]
    public async Task PerformanceFilterShouldNotLogIfRequestIsResolvedFasterThan500Ms()
    {
        var httpContext = new DefaultHttpContext
        {
            Request = { Method = "POST", Path = new PathString("/test") },
        };
        var request = new TestRequest(string.Empty);
        var context = EndpointFilterInvocationContext.Create(httpContext, request);

        var expectedResult = TypedResults.Ok();
        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(expectedResult);

        var user = GetRequiredService<IUser>();
        var filter = new PerformanceFilter(_logger, user);

        var result = await filter.InvokeAsync(context, next);

        result.ShouldBe(expectedResult);

        _logger.Collector.Count.ShouldBe(0);
    }

    [Test]
    public async Task PerformanceFilterShouldLogIfRequestIsResolvedSlowerThan500Ms()
    {
        var httpContext = new DefaultHttpContext
        {
            Request = { Method = "POST", Path = new PathString("/test") },
        };
        var request = new TestRequest("performance-test");
        var context = EndpointFilterInvocationContext.Create(httpContext, request);

        var expectedResult = TypedResults.Ok();

        async ValueTask<object?> Next(EndpointFilterInvocationContext _)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(501));
            return expectedResult;
        }

        var user = GetRequiredService<IUser>();
        var filter = new PerformanceFilter(_logger, user);

        var result = await filter.InvokeAsync(context, Next);

        result.ShouldBe(expectedResult);

        _logger.Collector.Count.ShouldBe(1);
        _logger.Collector.LatestRecord.Level.ShouldBe(LogLevel.Warning);
        _logger.Collector.LatestRecord.Message.ShouldContain("Long running request");
        _logger.Collector.LatestRecord.Message.ShouldContain("performance-test");
    }
}
