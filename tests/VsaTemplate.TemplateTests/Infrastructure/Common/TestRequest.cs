using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.TemplateTests.Infrastructure.Common;

public sealed record TestRequest(string Prop) : IRequest;
