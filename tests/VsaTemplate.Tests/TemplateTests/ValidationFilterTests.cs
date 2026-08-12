using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shouldly;
using VsaTemplate.Common.Pipeline;
using VsaTemplate.Tests.Infrastructure.TemplateTests;

namespace VsaTemplate.Tests.TemplateTests;

public sealed class ValidationFilterTests : TemplateTestBase
{
    [TestCase("valid", true)]
    [TestCase("invalid", false)]
    public async Task ValidationFilterShouldReturnCorrectResult(string prop, bool shouldPass)
    {
        _templateTesting.Services.AddValidatorsFromAssembly(typeof(ValidationFilterTests).Assembly);

        var httpContext = new DefaultHttpContext
        {
            Request = { Method = "POST", Path = new PathString("/test") },
            RequestServices = _templateTesting.ServiceProvider,
        };
        var request = new TemplateTestRequest(prop);
        var context = EndpointFilterInvocationContext.Create(httpContext, request);

        var expectedResult = TypedResults.Ok();
        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(expectedResult);

        var logger = new TemplateTestLogger<ValidationFilter>();
        var user = new TemplateTestUser(Guid.NewGuid(), []);
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
