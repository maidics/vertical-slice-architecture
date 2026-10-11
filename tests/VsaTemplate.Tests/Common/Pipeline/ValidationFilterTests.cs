using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using TUnit.Mocks;
using TUnit.Mocks.Arguments;
using TUnit.Mocks.Generated;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Common.Pipeline;

namespace VsaTemplate.Tests.Common.Pipeline;

public sealed class ValidationFilterTests
{
    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task ValidationFilterShouldReturnCorrectResult(bool shouldPass)
    {
        var validator = IValidator<TestRequest>.Mock();
        validator
            .ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(
                shouldPass
                    ? new ValidationResult()
                    : new ValidationResult([new ValidationFailure("Prop", "Invalid")])
            );

        var services = new ServiceCollection();
        services.AddScoped<IValidator<TestRequest>>(_ => validator.Object);
        await using var provider = services.BuildServiceProvider();

        const string httpMethod = "POST";
        const string path = "/test";

        var httpContext = new DefaultHttpContext
        {
            Request = { Method = httpMethod, Path = new PathString(path) },
            RequestServices = provider,
        };
        var request = new TestRequest();
        var context = EndpointFilterInvocationContext.Create(httpContext, request);

        var expectedResult = TypedResults.Ok();
        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(expectedResult);

        var logger = new FakeLogger<ValidationFilter>();

        var user = IUser.Mock();
        var userId = Guid.NewGuid();
        user.Id.Returns(userId);

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
        logger.Collector.LatestRecord.Message.ShouldContain(
            $"Request validation failed: {httpMethod} {path}, {userId}, "
        );
    }

    private sealed record TestRequest : IRequest;
}
