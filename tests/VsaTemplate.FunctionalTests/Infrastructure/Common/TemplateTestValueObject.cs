using VsaTemplate.Common.BaseClasses;

namespace VsaTemplate.FunctionalTests.Infrastructure.Common;

public sealed class TemplateTestValueObject : ValueObject
{
    public int Number { get; }

    public TemplateTestValueObject(int number)
    {
        Number = number;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Number;
    }
}
