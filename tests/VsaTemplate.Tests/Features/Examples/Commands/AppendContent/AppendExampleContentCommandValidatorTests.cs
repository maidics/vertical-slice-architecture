using FluentValidation.TestHelper;
using VsaTemplate.Features.Examples.AppendContent;

namespace VsaTemplate.Tests.Features.Examples.Commands.AppendContent;

public sealed class AppendExampleContentCommandValidatorTests
{
    private readonly AppendExampleContentCommandValidator _validator = new();

    [Test]
    public async Task ShouldReturnValidationErrors()
    {
        var command = new AppendExampleContentCommand(Guid.Empty, "");

        var result = await _validator.TestValidateAsync(command);
        result
            .ShouldHaveValidationErrorFor(x => x.AdditionalContent)
            .WithErrorMessage("Additional content is required.");
    }

    [Test]
    public async Task ShouldNotReturnValidationErrors()
    {
        var command = new AppendExampleContentCommand(Guid.Empty, "test");

        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
