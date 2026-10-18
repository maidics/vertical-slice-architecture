using System.ComponentModel.DataAnnotations;

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

    public bool IsPullRequest => EventName == _prEvent;

    public bool IsMainPush =>
        EventName == _pushEvent && RefType == _branchRefType && RefName == _mainRefName;

    public bool IsTagPush => RefType == _tagRefType;
}
