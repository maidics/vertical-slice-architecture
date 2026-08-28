using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.Features.Examples.Update;

public sealed record UpdateExampleCommand(Guid Id, string Content) : IRequest;
