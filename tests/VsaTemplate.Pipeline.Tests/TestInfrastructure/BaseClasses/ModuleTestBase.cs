using System.Reflection;
using ModularPipelines.Modules;
using Shouldly;
using DependsOnAttribute = ModularPipelines.Attributes.DependsOnAttribute;

namespace VsaTemplate.Pipeline.Tests.TestInfrastructure.BaseClasses;

public abstract class ModuleTestBase<TModule>(params Type[] dependencies)
    where TModule : class, IModule
{
    [Test]
    public void ShouldHaveCorrectDependencies()
    {
        typeof(TModule)
            .GetCustomAttributes<DependsOnAttribute>(inherit: false)
            .Select(a => a.Type)
            .ShouldBe(dependencies, ignoreOrder: true);
    }
}
