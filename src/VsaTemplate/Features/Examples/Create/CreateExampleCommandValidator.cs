using FluentValidation;

namespace VsaTemplate.Features.Examples.Create;

public sealed class CreateExampleCommandValidator : AbstractValidator<CreateExampleCommand>
{
    public CreateExampleCommandValidator()
    {
        RuleFor(x => x.Content).NotEmpty();
    }
}
