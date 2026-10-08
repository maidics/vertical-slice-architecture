using VsaTemplate.Domain.BaseClasses;

namespace VsaTemplate.Tests.TestInfrastructure.TemplateTests;

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
