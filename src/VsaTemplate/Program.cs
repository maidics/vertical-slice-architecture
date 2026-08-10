using Scalar.AspNetCore;
using VsaTemplate;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Pipeline;
using VsaTemplate.Features.Users;
using VsaTemplate.Infrastructure.Database;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApplicationServices();

var app = builder.Build();

if (!app.Environment.IsProduction())
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
    .MapEndpoints();

//app.MapDefaultEndpoints(); // ServiceDefaults observability
app.MapGroup("/api/identity").MapIdentityApi<ApplicationUser>().WithTags("Users");

app.MapOpenApi();
app.MapScalarApiReference();

app.Run();

namespace VsaTemplate
{
    public partial class Program;
}
