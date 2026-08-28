using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.Features.Examples.Create;

public sealed record CreateExampleCommand(string Content) : IRequest;
