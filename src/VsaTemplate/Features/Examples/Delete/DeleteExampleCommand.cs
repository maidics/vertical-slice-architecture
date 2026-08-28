using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.Features.Examples.Delete;

public sealed record DeleteExampleCommand(Guid Id) : IRequest;
