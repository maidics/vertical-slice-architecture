using System.ComponentModel.DataAnnotations;
using ModularPipelines.Attributes;

namespace VsaTemplate.Pipeline.Settings;

public sealed record NuGetSettings
{
    [Required]
    [SecretValue]
    public required string User { get; set; } = string.Empty;

    private const string _tokenEndpoint = "https://www.nuget.org/api/v2/token";

    public static Uri TokenEndpointUri => new(_tokenEndpoint);

    public const string ApiKeyTokenType = "ApiKey";

    public const string PushSource = "https://api.nuget.org/v3/index.json";
}
