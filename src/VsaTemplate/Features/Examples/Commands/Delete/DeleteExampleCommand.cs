using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.Features.Examples.Commands.Delete;

public sealed record DeleteExampleCommand(Guid Id) : IRequest;
