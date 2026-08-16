using Shouldly;
using VsaTemplate.Common.Models;

namespace VsaTemplate.FunctionalTests.Infrastructure.Common.Extensions;

public static class ResultExtensions
{
    extension(Result result)
    {
        public void ShouldBeFailed(ResultType type, params string[] errors)
        {
            if (type == ResultType.Success)
                throw new InvalidOperationException($"Failure type expected. Received: {type}");

            result.Succeeded.ShouldBeFalse();
            result.Type.ShouldBe(type);
            result.Errors.ShouldBeEquivalentTo(errors);
        }

        public void ShouldBeSuccessful()
        {
            result.Succeeded.ShouldBeTrue();
            result.Errors.ShouldBe([]);
            result.Type.ShouldBe(ResultType.Success);
        }
    }

    extension<T>(Result<T> result)
    {
        public void ShouldBeFailed(ResultType type, params string[] errors)
        {
            if (type == ResultType.Success)
                throw new InvalidOperationException($"Failure type expected. Received: {type}");

            result.Succeeded.ShouldBeFalse();
            result.Type.ShouldBe(type);
            result.Errors.ShouldBeEquivalentTo(errors);
        }

        public void ShouldBeSuccessful()
        {
            result.Succeeded.ShouldBeTrue();
            result.Type.ShouldBe(ResultType.Success);
            result.Errors.ShouldBe([]);
        }

        public void ShouldBeSuccessful(T value)
        {
            result.ShouldBeSuccessful();
            result.Value.ShouldBe(value);
        }
    }
}
