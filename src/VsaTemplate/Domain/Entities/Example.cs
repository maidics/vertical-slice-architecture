using VsaTemplate.Domain.BaseClasses;

namespace VsaTemplate.Domain.Entities;

public sealed class Example : BaseEntity
{
    public required string Content { get; set; }
}
