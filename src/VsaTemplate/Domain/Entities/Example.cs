using VsaTemplate.Domain.BaseClasses;
using VsaTemplate.Domain.Events;

namespace VsaTemplate.Domain.Entities;

public sealed class Example : BaseEntity
{
    public required string Content { get; set; }
    public bool HasAppendedContent { get; set; }

    public void AppendContent(string additionalContent)
    {
        Content += additionalContent;

        AddDomainEvent(new ExampleContentAppendedEvent(Id));
    }
}
