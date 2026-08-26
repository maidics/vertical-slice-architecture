using FluentValidation;

namespace VsaTemplate.Features.Examples.Commands.Create;

public sealed class CreateExampleCommandValidator : AbstractValidator<CreateExampleCommand>
{
    public CreateExampleCommandValidator()
    {
        RuleFor(x => x.Content).NotEmpty();
    }
}
