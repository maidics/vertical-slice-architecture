using Microsoft.Extensions.Hosting;
using VsaTemplate.Shared;

var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddSqlite(Services.Database);

if (builder.Environment.IsEnvironment(TestingEnvironments.Web))
    builder
        .AddProject<Projects.VsaTemplate>(Services.WebApi)
        .WithReference(database)
        .WaitFor(database)
        .WithExternalHttpEndpoints()
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
        .WithHttpHealthCheck("/health", endpointName: "http");

builder.Build().Run();
