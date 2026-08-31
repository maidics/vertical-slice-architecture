using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace VsaTemplate.Tests.TestInfrastructure;

public static class HttpResponseMessageExtensions
{
    extension(HttpResponseMessage response)
    {
        public async Task<string[]?> GetResultErrorsAsync()
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            var element = (JsonElement?)problemDetails?.Extensions["errors"];

            return element?.Deserialize<string[]>();
        }

        public async Task<ValidationProblemDetails?> GetValidationProblemDetailsAsync()
        {
            return await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        }
    }
}
