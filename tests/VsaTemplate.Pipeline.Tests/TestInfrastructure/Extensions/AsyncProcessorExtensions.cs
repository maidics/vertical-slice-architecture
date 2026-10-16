using System.Reflection;
using EnumerableAsyncProcessor.Interfaces;
using Shouldly;

namespace VsaTemplate.Pipeline.Tests.TestInfrastructure.Extensions;

public static class AsyncProcessorExtensions
{
    extension<T>(IAsyncProcessor<T> processor)
    {
        public int? GetMaxConcurrency()
        {
            var field = processor
                .GetType()
                .GetField("_maxConcurrency", BindingFlags.Instance | BindingFlags.NonPublic);
            field.ShouldNotBeNull();

            return (int?)field.GetValue(processor);
        }
    }
}
