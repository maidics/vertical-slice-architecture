using VsaTemplate.Shared;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddSqlite(Services.Database);

builder.Build().Run();
