using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Common.Pipeline;
using VsaTemplate.TemplateTests.Infrastructure;
using VsaTemplate.TemplateTests.Infrastructure.Common;
using VsaTemplate.TemplateTests.Infrastructure.Common.BaseClasses;

namespace VsaTemplate.TemplateTests.Tests;

public sealed class PerformanceFilterTests : TestBase
{
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

        var logger = new LoggerSpy<PerformanceFilter>();
        var user = GetRequiredService<IUser>();
        var filter = new PerformanceFilter(logger, user);

        var result = await filter.InvokeAsync(context, next);

        result.ShouldBe(expectedResult);

        logger.Entries.Count.ShouldBe(0);
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

        var logger = new LoggerSpy<PerformanceFilter>();
        var user = GetRequiredService<IUser>();
        var filter = new PerformanceFilter(logger, user);

        var result = await filter.InvokeAsync(context, Next);

        result.ShouldBe(expectedResult);

        logger.Entries.Count.ShouldBe(1);

        var log = logger.Entries[0];
        log.Level.ShouldBe(LogLevel.Warning);
        log.Message.ShouldContain("Long running request");
        log.Message.ShouldContain("performance-test");
    }
}
