using FluentValidation;

namespace VsaTemplate.Features.Examples.AppendContent;

public sealed class AppendExampleContentCommandValidator
    : AbstractValidator<AppendExampleContentCommand>
{
    public AppendExampleContentCommandValidator()
    {
        RuleFor(x => x.AdditionalContent).NotEmpty().WithMessage("Additional content is required.");
    }
}
