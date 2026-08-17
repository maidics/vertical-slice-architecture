using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Common.Services;
using VsaTemplate.Features.Users;
using VsaTemplate.Infrastructure.Database;
using VsaTemplate.Infrastructure.Database.Interceptors;
using VsaTemplate.Shared;

namespace VsaTemplate.Infrastructure;

public static class DependencyInjection
{
    extension(WebApplicationBuilder builder)
    {
        public void AddInfrastructureServices()
        {
            // Db
            var connectionString = builder.Configuration.GetConnectionString(Services.Database);
            ArgumentException.ThrowIfNullOrEmpty(connectionString);

            builder.Services.AddDbContext<ApplicationDbContext>(
                (sp, options) =>
                {
                    options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                    options.UseSqlite(connectionString);
                    options.ConfigureWarnings(warnings =>
                        warnings.Ignore((RelationalEventId.PendingModelChangesWarning))
                    );
                }
            );

            builder.Services.AddScoped<DatabaseInitialiser>();

            builder.Services.AddAuthorizationBuilder();

            builder
                .Services.AddIdentityApiEndpoints<ApplicationUser>()
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
            builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventInterceptor>();

            // Other services
            builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            builder.Services.AddScoped<IUser, CurrentUser>();
            builder.Services.AddSingleton(TimeProvider.System);

            builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

            builder.Services.AddOpenApi();

            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                // violating these will throw BadHttpRequestException
                options.SerializerOptions.RespectRequiredConstructorParameters = true; // rejects payloads that pass no values for a required constructor parameter
                options.SerializerOptions.RespectNullableAnnotations = true; // rejects null on non-nullable properties
                options.SerializerOptions.UnmappedMemberHandling =
                    JsonUnmappedMemberHandling.Disallow; // rejects payloads with extra fields
            });

            builder.Services.AddProblemDetails();
        }
    }
}
