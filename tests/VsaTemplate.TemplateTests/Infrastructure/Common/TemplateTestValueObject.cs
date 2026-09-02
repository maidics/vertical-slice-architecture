using VsaTemplate.Domain.BaseClasses;

namespace VsaTemplate.TemplateTests.Infrastructure.Common;

public sealed class TestValueObject : ValueObject
{
    public int Number { get; }

    public TestValueObject(int number)
    {
        Number = number;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Number;
    }
}
