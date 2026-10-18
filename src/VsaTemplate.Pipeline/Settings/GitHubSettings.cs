using System.ComponentModel.DataAnnotations;
using ModularPipelines.Attributes;

namespace VsaTemplate.Pipeline.Settings;

public sealed record GitHubSettings
{
    private const string _prEvent = "pull_request";
    private const string _pushEvent = "push";

    private const string _branchRefType = "branch";
    private const string _tagRefType = "tag";

    private const string _mainRefName = "main";

    [Required]
    public required string RefType { get; set; }

    [Required]
    public required string RefName { get; set; }

    [Required]
    public required string EventName { get; set; }

    public required string OidcRequestUrl { get; set; } // TODO: validate with custom validator on startup only, test the validator

    [SecretValue] // TODO: validate with custom validator on tag push only, test the validator
    public required string OidcRequestToken { get; set; } // TODO: add test via reflection that this has the secrets attribute

    public bool IsPullRequest => EventName == _prEvent;

    public bool IsMainPush =>
        EventName == _pushEvent && RefType == _branchRefType && RefName == _mainRefName;

    public bool IsTagPush => RefType == _tagRefType;
}
