using FluentValidation.TestHelper;
using VsaTemplate.Features.Examples.Commands;

namespace VsaTemplate.UnitTests.Tests.Features.Examples.Commands;

public sealed class UpdateExampleCommandValidatorTests
{
    [Test]
    public async Task ShouldReturnValidationErrors()
    {
        var command = new UpdateExampleCommand(Guid.Empty, string.Empty);
        var validator = new UpdateExampleCommandValidator();

        var result = await validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Test]
    public async Task ShouldNotReturnValidationErrors()
    {
        var command = new UpdateExampleCommand(Guid.Empty, "test");
        var validator = new UpdateExampleCommandValidator();

        var result = await validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
