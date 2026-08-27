using FluentValidation.TestHelper;
using VsaTemplate.Features.Examples.Commands.Create;

namespace VsaTemplate.Tests.Features.Examples.Commands.Create;

public sealed class CreateExampleCommandValidatorTests
{
    private readonly CreateExampleCommandValidator _validator = new();

    [Test]
    public async Task ShouldReturnValidationErrors()
    {
        var command = new CreateExampleCommand(string.Empty);

        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Test]
    public async Task ShouldNotReturnValidationErrors()
    {
        var command = new CreateExampleCommand("test");

        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
