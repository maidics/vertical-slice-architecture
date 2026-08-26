using FluentValidation.TestHelper;
using VsaTemplate.Features.Examples.Commands;

namespace VsaTemplate.UnitTests.Tests.Features.Examples.Commands;

public sealed class CreateExampleCommandValidatorTests
{
    [Test]
    public async Task ShouldReturnValidationErrors()
    {
        var command = new CreateExampleCommand(string.Empty);
        var validator = new CreateExampleCommandValidator();

        var result = await validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Test]
    public async Task ShouldNotReturnValidationErrors()
    {
        var command = new CreateExampleCommand("test");
        var validator = new CreateExampleCommandValidator();

        var result = await validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
