using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.Features.Examples.Commands.Create;

public sealed record CreateExampleCommand(string Content) : IRequest;
