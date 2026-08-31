using Scalar.AspNetCore;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Pipeline;
using VsaTemplate.Features.Users;
using VsaTemplate.Infrastructure;
using VsaTemplate.Infrastructure.Database;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults().AddCommonServices().AddInfrastructureServices();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();

    var initialiser = scope.ServiceProvider.GetRequiredService<DatabaseInitialiser>();
    await initialiser.InitialiseAsync();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseExceptionHandler(options => { });

app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/api")
    .AddEndpointFilter<LoggingFilter>()
    .AddEndpointFilter<ValidationFilter>()
    .AddEndpointFilter<PerformanceFilter>()
    .MapEndpoints(typeof(Program).Assembly)
    .MapLogoutEndpoint();

app.MapDefaultEndpoints(); // ServiceDefaults observability
app.MapGroup("/api/identity").MapIdentityApi<ApplicationUser>().WithTags("Users");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.Run();

namespace VsaTemplate
{
    public partial class Program;
}
