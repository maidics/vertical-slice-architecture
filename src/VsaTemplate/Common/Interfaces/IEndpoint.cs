namespace VsaTemplate.Common.Interfaces;

public interface IEndpoint
{
    static abstract string Prefix { get; }
    static virtual string[] Tags { get; } = [];

    static abstract void Map(IEndpointRouteBuilder builder);
}
