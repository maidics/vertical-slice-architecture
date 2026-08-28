using FluentValidation;

namespace VsaTemplate.Features.Examples.Update;

public sealed class UpdateExampleCommandValidator : AbstractValidator<UpdateExampleCommand>
{
    public UpdateExampleCommandValidator()
    {
        RuleFor(x => x.Content).NotEmpty();
    }
}
