using Shouldly;
using VsaTemplate.Common.Models;

namespace VsaTemplate.FunctionalTests.Infrastructure.Common;

public static class ResultExtensions
{
    extension(Result result)
    {
        public void ShouldBeFailed(ResultType type, string[] errors)
        {
            if (Result.SuccessTypes.Contains(type))
                throw new InvalidOperationException($"Failure type expected. Received: {type}");

            result.Succeeded.ShouldBeFalse();
            result.Type.ShouldBe(type);
            result.Errors.ShouldBeEquivalentTo(errors);
        }

        public void ShouldBeSuccessful(ResultType type = ResultType.Success)
        {
            if (!Result.SuccessTypes.Contains(type))
                throw new InvalidOperationException($"Success type expected. Received: {type}");

            result.Succeeded.ShouldBeTrue();
            result.Errors.ShouldBe([]);
            result.Type.ShouldBe(type);
        }
    }

    extension<T>(Result<T> result)
    {
        public void ShouldBeFailed(ResultType type, string[] errors)
        {
            if (Result.SuccessTypes.Contains(type))
                throw new InvalidOperationException($"Failure type expected. Received: {type}");

            result.Succeeded.ShouldBeFalse();
            result.Type.ShouldBe(type);
            result.Errors.ShouldBeEquivalentTo(errors);
        }

        public void ShouldBeSuccessful(ResultType type = ResultType.Success)
        {
            if (!Result.SuccessTypes.Contains(type))
                throw new InvalidOperationException($"Success type expected. Received: {type}");

            result.Succeeded.ShouldBeTrue();
            result.Type.ShouldBe(type);
            result.Errors.ShouldBe([]);
        }

        public void ShouldBeSuccessful(ResultType type, T value)
        {
            result.ShouldBeSuccessful(type);
            result.Value.ShouldBe(value);
        }
    }
}
