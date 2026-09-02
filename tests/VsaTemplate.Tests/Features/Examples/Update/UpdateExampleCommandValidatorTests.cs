using FluentValidation.TestHelper;
using VsaTemplate.Features.Examples;

namespace VsaTemplate.Tests.Features.Examples.Update;

public sealed class UpdateExampleCommandValidatorTests
{
    private readonly UpdateExampleCommandValidator _validator = new();

    [Test]
    public async Task ShouldReturnValidationErrors()
    {
        var command = new UpdateExampleCommand(Guid.Empty, string.Empty);

        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Test]
    public async Task ShouldNotReturnValidationErrors()
    {
        var command = new UpdateExampleCommand(Guid.Empty, "test");

        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
