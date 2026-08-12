using Shouldly;
using VsaTemplate.FunctionalTests.Infrastructure.Common;

namespace VsaTemplate.FunctionalTests.TemplateTests;

public sealed class ValueObjectTests
{
    [Test]
    public void EqualsShouldReturnTrueWhenValueObjectIsComparedToItself()
    {
        var obj = new TemplateTestValueObject(2);

        obj.Equals(obj).ShouldBeTrue();
    }

    [Test]
    public void EqualsShouldReturnTrueWhenValueObjectsAreEqual()
    {
        var obj = new TemplateTestValueObject(1);
        var objOther = new TemplateTestValueObject(1);

        obj.Equals(objOther).ShouldBeTrue();
    }

    [Test]
    public void EqualsShouldReturnFalseWhenValueObjectsAreNotEqual()
    {
        var obj = new TemplateTestValueObject(1);
        var objOther = new TemplateTestValueObject(2);

        obj.Equals(objOther).ShouldBeFalse();
    }

    [Test]
    public void GetHashCodeShouldReturnCorrectHashCode()
    {
        var hash = new HashCode();
        hash.Add(1);
        var code = hash.ToHashCode();

        var obj = new TemplateTestValueObject(1);
        obj.GetHashCode().ShouldBe(code);
    }

    [Test]
    public void EqualOperatorShouldReturnTrueWhenValueObjectIsComparedToItself()
    {
        var obj = new TemplateTestValueObject(2);

#pragma warning disable CS1718 // Comparison made to same variable
        (obj == obj).ShouldBeTrue();
#pragma warning restore CS1718
    }

    [Test]
    public void EqualOperatorShouldReturnTrueWhenValueObjectsAreEqual()
    {
        var obj = new TemplateTestValueObject(1);
        var objOther = new TemplateTestValueObject(1);

        (obj == objOther).ShouldBeTrue();
    }

    [Test]
    public void EqualOperatorShouldReturnFalseWhenValueObjectsAreNotEqual()
    {
        var obj = new TemplateTestValueObject(1);
        var objOther = new TemplateTestValueObject(2);

        (obj == objOther).ShouldBeFalse();
    }

    [Test]
    public void NotEqualOperatorShouldReturnFalseWhenValueObjectIsComparedToItself()
    {
        var obj = new TemplateTestValueObject(1);

#pragma warning disable CS1718 // Comparison made to same variable
        (obj != obj).ShouldBeFalse();
#pragma warning restore CS1718
    }

    [Test]
    public void NotEqualOperatorShouldReturnFalseWhenValueObjectsAreEqual()
    {
        var obj = new TemplateTestValueObject(1);
        var objOther = new TemplateTestValueObject(1);

        (obj != objOther).ShouldBeFalse();
    }

    [Test]
    public void NotEqualOperatorShouldReturnTrueWhenValueObjectsAreNotEqual()
    {
        var obj = new TemplateTestValueObject(1);
        var objOther = new TemplateTestValueObject(2);

        (obj != objOther).ShouldBeTrue();
    }
}
