using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.FunctionalTests.Infrastructure.TemplateTests;

public sealed record TemplateTestRequest(string Prop) : IRequest;
