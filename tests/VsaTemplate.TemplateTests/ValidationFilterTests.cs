using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shouldly;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Common.Pipeline;
using VsaTemplate.TemplateTests.Infrastructure.Common;
using VsaTemplate.TemplateTests.Infrastructure.Common.BaseClasses;
using VsaTemplate.Tests.TestInfrastructure;

namespace VsaTemplate.TemplateTests;

public sealed class ValidationFilterTests : TestBase
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

        var logger = new LoggerSpy<ValidationFilter>();
        var user = GetRequiredService<IUser>();
        var filter = new ValidationFilter(logger, user);

        var result = await filter.InvokeAsync(context, next);

        if (shouldPass)
        {
            result.ShouldBe(expectedResult);
            logger.Entries.Count.ShouldBe(0);
            return;
        }

        result.ShouldNotBe(expectedResult);

        logger.Entries.Count.ShouldBe(1);
        logger.Entries[0].Level.ShouldBe(LogLevel.Warning);
        logger.Entries[0].Message.ShouldContain("Request validation failed");
    }
}
