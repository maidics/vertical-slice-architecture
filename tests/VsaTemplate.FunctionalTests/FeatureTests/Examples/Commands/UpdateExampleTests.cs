using FluentValidation.TestHelper;
using Shouldly;
using VsaTemplate.Common.Models;
using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.Commands;
using VsaTemplate.FunctionalTests.Infrastructure;
using VsaTemplate.FunctionalTests.Infrastructure.Common.BaseClasses;
using VsaTemplate.FunctionalTests.Infrastructure.Common.Extensions;

namespace VsaTemplate.FunctionalTests.FeatureTests.Examples.Commands;

public sealed class UpdateExampleTests : ApplicationTestBase
{
    [Test]
    public async Task ShouldReturnValidationErrors()
    {
        var command = new UpdateExampleCommand(Guid.Empty, string.Empty);
        var validator = GetService<UpdateExampleCommandValidator>();

        var result = await validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Test]
    public async Task ShouldNotReturnValidationErrors()
    {
        var command = new UpdateExampleCommand(Guid.Empty, "test");
        var validator = GetService<UpdateExampleCommandValidator>();

        var result = await validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public async Task ShouldReturnNotFoundIfExampleDoesNotExists()
    {
        var command = new UpdateExampleCommand(Guid.Empty, "test");

        var handler = GetService<UpdateExampleCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeFailed(ResultType.NotFound, ["Example not found."]);
    }

    [Test]
    public async Task ShouldReturnConflictIfExampleWithContentAlreadyExists()
    {
        var example1 = new Example() { Content = "test" };
        var example2 = new Example() { Content = "" };

        await Testing.AddAsync(example1);
        await Testing.AddAsync(example2);

        var command = new UpdateExampleCommand(example2.Id, example1.Content);
        var handler = GetService<UpdateExampleCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeFailed(
            ResultType.Conflict,
            $"Example with '{example1.Content}' content already exists."
        );
    }

    [Test]
    public async Task ShouldUpdateExampleContent()
    {
        var example = new Example { Content = "test" };

        await Testing.AddAsync(example);

        var command = new UpdateExampleCommand(example.Id, "new-test-content");

        var handler = GetService<UpdateExampleCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeSuccessful();

        var updated = await Testing.FirstOrDefaultAsync<Example>(x => x.Id == example.Id);
        updated!.Content.ShouldBe(command.Content);
    }

    [Test]
    public async Task ShouldReturnSuccessIfNewContentIsTheSameAsCurrent()
    {
        var example = new Example() { Content = "test" };

        await Testing.AddAsync(example);

        var command = new UpdateExampleCommand(example.Id, example.Content);

        var handler = GetService<UpdateExampleCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeSuccessful();
    }
}
