using VsaTemplate.Domain.BaseClasses;
using VsaTemplate.Tests.TestInfrastructure.TemplateTests;

namespace VsaTemplate.Tests.Domain.BaseClasses;

public sealed class ValueObjectTests
{
    private sealed class TestValueObject : ValueObject
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

    [Test]
    public void EqualsShouldReturnTrueWhenValueObjectIsComparedToItself()
    {
        var obj = new TestValueObject(2);

        obj.Equals(obj).ShouldBeTrue();
    }

    [Test]
    public void EqualsShouldReturnFalseIfComparedToNull()
    {
        var obj = new TestValueObject(2);

        obj.Equals(null).ShouldBeFalse();
    }

    [Test]
    public void EqualsShouldReturnTrueWhenValueObjectsAreEqual()
    {
        var obj = new TestValueObject(1);
        var objOther = new TestValueObject(1);

        obj.Equals(objOther).ShouldBeTrue();
    }

    [Test]
    public void EqualsShouldReturnFalseWhenValueObjectsAreNotEqual()
    {
        var obj = new TestValueObject(1);
        var objOther = new TestValueObject(2);

        obj.Equals(objOther).ShouldBeFalse();
    }

    [Test]
    public void GetHashCodeShouldReturnCorrectHashCode()
    {
        var hash = new HashCode();
        hash.Add(1);
        var code = hash.ToHashCode();

        var obj = new TestValueObject(1);
        obj.GetHashCode().ShouldBe(code);
    }

    [Test]
    public void EqualOperatorShouldReturnTrueWhenValueObjectIsComparedToItself()
    {
        var obj = new TestValueObject(2);

#pragma warning disable CS1718 // Comparison made to same variable
        (obj == obj).ShouldBeTrue();
#pragma warning restore CS1718
    }

    [Test]
    public void EqualOperatorShouldReturnFalseIfComparisonContainsNull()
    {
        var obj = new TestValueObject(1);

        (obj == null!).ShouldBeFalse();
        (null! == obj).ShouldBeFalse();
    }

    [Test]
    public void EqualOperatorShouldReturnTrueWhenValueObjectsAreEqual()
    {
        var obj = new TestValueObject(1);
        var objOther = new TestValueObject(1);

        (obj == objOther).ShouldBeTrue();
    }

    [Test]
    public void EqualOperatorShouldReturnFalseWhenValueObjectsAreNotEqual()
    {
        var obj = new TestValueObject(1);
        var objOther = new TestValueObject(2);

        (obj == objOther).ShouldBeFalse();
    }

    [Test]
    public void NotEqualOperatorShouldReturnFalseWhenValueObjectIsComparedToItself()
    {
        var obj = new TestValueObject(1);

#pragma warning disable CS1718 // Comparison made to same variable
        (obj != obj).ShouldBeFalse();
#pragma warning restore CS1718
    }

    [Test]
    public void NotEqualOperatorShouldReturnFalseWhenValueObjectsAreEqual()
    {
        var obj = new TestValueObject(1);
        var objOther = new TestValueObject(1);

        (obj != objOther).ShouldBeFalse();
    }

    [Test]
    public void NotEqualOperatorShouldReturnTrueWhenValueObjectsAreNotEqual()
    {
        var obj = new TestValueObject(1);
        var objOther = new TestValueObject(2);

        (obj != objOther).ShouldBeTrue();
    }
}
