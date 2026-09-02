using VsaTemplate.Common.Interfaces;

namespace VsaTemplate.TemplateTests.Infrastructure.Common;

public sealed record TestRequest(string Prop) : IRequest;
