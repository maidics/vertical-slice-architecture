using VsaTemplate.AppHost;
using VsaTemplate.Shared;

var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddSqlite(Services.Database);

builder
    .AddProject<Projects.VsaTemplate>(Services.WebApi)
    .WithReference(database)
    .WaitFor(database)
    .WithExternalHttpEndpoints()
    .WithAspNetCoreEnvironment()
    .WithUrlForEndpoint(
        "http",
        url =>
        {
            url.DisplayText = "Scalar API";
            url.Url = "/scalar";
        }
    )
    .WithHttpHealthCheck("/health", endpointName: "http");

builder.Build().Run();
