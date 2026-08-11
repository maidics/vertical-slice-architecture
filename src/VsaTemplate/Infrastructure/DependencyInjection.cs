using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VsaTemplate.Common.Interfaces;
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

            builder.Services.Configure<ApiBehaviorOptions>(options =>
                options.SuppressModelStateInvalidFilter = true // framework will not check model state: use FluentValidation instead (check VsaTemplate.Common.Pipeline.ValidationFilter.cs)
            );

            builder.Services.AddOpenApi();
        }
    }
}
