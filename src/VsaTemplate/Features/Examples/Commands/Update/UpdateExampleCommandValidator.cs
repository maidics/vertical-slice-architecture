using FluentValidation;

namespace VsaTemplate.Features.Examples.Commands.Update;

public sealed class UpdateExampleCommandValidator : AbstractValidator<UpdateExampleCommand>
{
    public UpdateExampleCommandValidator()
    {
        RuleFor(x => x.Content).NotEmpty();
    }
}
