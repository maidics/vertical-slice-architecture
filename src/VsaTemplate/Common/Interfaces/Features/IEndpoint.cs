namespace VsaTemplate.Common.Interfaces.Features;

public interface IEndpoint
{
    static abstract string Prefix { get; }
    static virtual string[] Tags { get; } = [];

    static abstract void Map(IEndpointRouteBuilder builder);
}
