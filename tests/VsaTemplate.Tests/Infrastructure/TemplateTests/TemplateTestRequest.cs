using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.Tests.Infrastructure.TemplateTests;

public sealed record TemplateTestRequest(string Prop) : IRequest;
