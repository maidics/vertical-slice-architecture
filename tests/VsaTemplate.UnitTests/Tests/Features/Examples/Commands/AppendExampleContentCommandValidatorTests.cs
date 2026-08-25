using FluentValidation.TestHelper;
using VsaTemplate.Features.Examples.Commands;

namespace VsaTemplate.UnitTests.Tests.Features.Examples.Commands;

public sealed class AppendExampleContentCommandValidatorTests
{
    [Test]
    public async Task ShouldReturnValidationErrors()
    {
        var command = new AppendExampleContentCommand(Guid.Empty, "");
        var validator = new AppendExampleContentCommandValidator();

        var result = await validator.TestValidateAsync(command);
        result
            .ShouldHaveValidationErrorFor(x => x.AdditionalContent)
            .WithErrorMessage("Additional content is required.");
    }

    [Test]
    public async Task ShouldNotReturnValidationErrors()
    {
        var command = new AppendExampleContentCommand(Guid.Empty, "test");
        var validator = new AppendExampleContentCommandValidator();

        var result = await validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
