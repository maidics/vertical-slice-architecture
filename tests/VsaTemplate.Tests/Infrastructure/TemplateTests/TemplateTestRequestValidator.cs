using FluentValidation;

namespace VsaTemplate.Tests.Infrastructure.TemplateTests;

public sealed class TemplateTestRequestValidator : AbstractValidator<TemplateTestRequest>
{
    public TemplateTestRequestValidator()
    {
        RuleFor(x => x.Prop).MaximumLength(5);
    }
}
