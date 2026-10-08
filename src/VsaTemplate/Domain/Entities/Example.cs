using VsaTemplate.Domain.BaseClasses;

namespace VsaTemplate.Domain.Entities;

public sealed class Example : BaseAuditableEntity
{
    public required string Content { get; set; }
}
