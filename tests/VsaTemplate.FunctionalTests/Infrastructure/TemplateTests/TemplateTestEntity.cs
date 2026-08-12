using VsaTemplate.Common.BaseClasses;

namespace VsaTemplate.FunctionalTests.Infrastructure.TemplateTests;

// Test entity for template testing
public sealed class TemplateTestEntity : BaseAuditableEntity
{
    public string Prop { get; set; } = Guid.NewGuid().ToString();

    public TemplateOwnedEntity OwnedEntity { get; set; } =
        new() { Prop = Guid.NewGuid().ToString() };
}

public sealed class TemplateOwnedEntity
{
    public required string Prop { get; set; }
}
