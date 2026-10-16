using Docker.DotNet;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Configurations;

namespace VsaTemplate.Pipeline.Benchmarks.Services;

public sealed class ContainerStorageUsage : IAsyncDisposable
{
    private readonly DockerClient _client;
    private readonly string _containerId;

    public ContainerStorageUsage(string containerId)
    {
        _client = TestcontainersSettings
            .OS.DockerEndpointAuthConfig.GetDockerClientBuilder()
            .Build();

        _containerId = containerId;
    }

    public async Task<long?> GetWritableLayerBytesAsync()
    {
        var list = await _client.Containers.ListContainersAsync(
            new ContainersListParameters
            {
                All = true,
                Size = true,
                Filters = new Dictionary<string, IDictionary<string, bool>>
                {
                    ["id"] = new Dictionary<string, bool> { [_containerId] = true },
                },
            }
        );

        return list.Single().SizeRw;
    }

    public ValueTask DisposeAsync()
    {
        _client.Dispose();

        return ValueTask.CompletedTask;
    }
}
