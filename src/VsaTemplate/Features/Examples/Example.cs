using VsaTemplate.Common.BaseClasses;
using VsaTemplate.Features.Examples.AppendContent;

namespace VsaTemplate.Features.Examples;

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
