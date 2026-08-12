using FluentValidation.TestHelper;
using Shouldly;
using VsaTemplate.Common.Models;
using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.Commands;
using VsaTemplate.FunctionalTests.Infrastructure;
using VsaTemplate.FunctionalTests.Infrastructure.Common;

namespace VsaTemplate.FunctionalTests.FeatureTests.Examples.Commands;

public sealed class AppendExampleContentTests : ApplicationTestBase
{
    [Test]
    public async Task ShouldReturnValidationErrors()
    {
        var command = new AppendExampleContentCommand(Guid.Empty, "");
        var validator = GetService<AppendExampleContentCommandValidator>();

        var result = await validator.TestValidateAsync(command);
        result
            .ShouldHaveValidationErrorFor(x => x.AdditionalContent)
            .WithErrorMessage("Additional content is required.");
    }

    [Test]
    public async Task ShouldReturnNotFoundIfExampleDoesNotExists()
    {
        var command = new AppendExampleContentCommand(Guid.Empty, "test");

        var handler = GetService<AppendExampleContentCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeFailed(ResultType.NotFound, ["Example not found."]);
    }

    [Test]
    public async Task ShouldReturnConflictIfExampleExistsWithContent()
    {
        var example1 = new Example { Content = "test-content" };
        var example2 = new Example { Content = "test" };

        await Testing.AddAsync(example1);
        await Testing.AddAsync(example2);

        var command = new AppendExampleContentCommand(example2.Id, "-content");

        var handler = GetService<AppendExampleContentCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeFailed(
            ResultType.Conflict,
            [$"Example with '{example1.Content}' content already exists."]
        );
    }

    [Test]
    public async Task ShouldAppendContent()
    {
        var example = new Example { Content = "test" };

        await Testing.AddAsync(example);

        var command = new AppendExampleContentCommand(example.Id, "-content");

        var handler = GetService<AppendExampleContentCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeSuccessful();

        var updated = await Testing.FirstOrDefaultAsync<Example>(x => x.Id == example.Id);
        updated!.Content.ShouldBe(example.Content + command.AdditionalContent);
    }
}
