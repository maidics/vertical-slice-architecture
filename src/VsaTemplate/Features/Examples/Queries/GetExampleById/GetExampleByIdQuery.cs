using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.Features.Examples.Queries.GetExampleById;

public sealed record GetExampleByIdQuery(Guid Id) : IRequest;
