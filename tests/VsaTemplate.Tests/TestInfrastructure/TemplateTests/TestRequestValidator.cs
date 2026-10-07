using FluentValidation;

namespace VsaTemplate.Tests.TestInfrastructure.TemplateTests;

public sealed class TestRequestValidator : AbstractValidator<TestRequest>
{
    public TestRequestValidator()
    {
        RuleFor(x => x.Prop).MaximumLength(5);
    }
}
