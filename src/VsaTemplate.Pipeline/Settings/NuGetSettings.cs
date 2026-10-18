using System.ComponentModel.DataAnnotations;
using ModularPipelines.Attributes;

namespace VsaTemplate.Pipeline.Settings;

public sealed record NuGetSettings
{
    [Required]
    [SecretValue]
    public required string User { get; set; } = string.Empty;
}
