using VsaTemplate.Domain.BaseClasses;

namespace VsaTemplate.TemplateTests.Infrastructure.Common;

public sealed class TestEntity : BaseAuditableEntity
{
    public string Prop { get; set; } = Guid.NewGuid().ToString();

    public TemplateOwnedEntity OwnedEntity { get; set; } =
        new() { Prop = Guid.NewGuid().ToString() };
}

public sealed class TemplateOwnedEntity
{
    public required string Prop { get; set; }
}
