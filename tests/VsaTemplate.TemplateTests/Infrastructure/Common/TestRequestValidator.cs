using FluentValidation;

namespace VsaTemplate.TemplateTests.Infrastructure.Common;

public sealed class TestRequestValidator : AbstractValidator<TestRequest>
{
    public TestRequestValidator()
    {
        RuleFor(x => x.Prop).MaximumLength(5);
    }
}
