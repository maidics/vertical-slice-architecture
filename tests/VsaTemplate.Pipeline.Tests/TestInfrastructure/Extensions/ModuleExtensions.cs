using System.Reflection;
using ModularPipelines.Configuration;
using ModularPipelines.Modules;
using Shouldly;

namespace VsaTemplate.Pipeline.Tests.TestInfrastructure.Extensions;

public static class ModuleExtensions
{
    extension<T>(Module<T> module)
    {
        public ModuleConfiguration GetConfiguration()
        {
            var configure = typeof(Module<T>).GetMethod(
                "Configure",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            configure.ShouldNotBeNull();

            return (ModuleConfiguration)
                configure.Invoke(
                    module,
                    BindingFlags.DoNotWrapExceptions,
                    binder: null,
                    [],
                    culture: null
                )!;
        }
    }
}
