using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.Features.Examples.AppendContent;

public sealed record AppendExampleContentCommand(Guid ExampleId, string AdditionalContent)
    : IRequest;
