using Shouldly;
using VsaTemplate.Common.Extensions;

namespace VsaTemplate.FunctionalTests.TemplateTests;

public sealed class MethodInfoExtensionTests
{
    [Test]
    public void IsAnonymousShouldReturnTrueWhenMethodIsAnonymous()
    {
        new Action(() => { }).Method.IsAnonymous().ShouldBeTrue();
    }

    [Test]
    public void IsAnonymousShouldReturnFalseWhenMethodIsNotAnonymous()
    {
        ((Delegate)GetDelegate).Method.IsAnonymous().ShouldBeFalse();
    }

    private void GetDelegate() { }
}
