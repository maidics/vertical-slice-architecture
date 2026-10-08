using VsaTemplate.Common.Interfaces;

namespace VsaTemplate.Tests.TestInfrastructure.TemplateTests;

public sealed record TestRequest(string Prop) : IRequest;
