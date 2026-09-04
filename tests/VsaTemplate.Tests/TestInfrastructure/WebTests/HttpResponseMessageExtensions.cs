using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace VsaTemplate.Tests.TestInfrastructure.WebTests;

public static class HttpResponseMessageExtensions
{
    extension(HttpResponseMessage response)
    {
        public async Task<string[]?> GetResultErrorsAsync()
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            if (
                problemDetails is null
                || !problemDetails.Extensions.TryGetValue("errors", out var errors)
            )
                return null;

            return ((JsonElement?)errors)?.Deserialize<string[]>();
        }

        public async Task<ValidationProblemDetails?> GetValidationProblemDetailsAsync()
        {
            return await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        }
    }
}
