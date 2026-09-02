using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using VsaTemplate.Common.Services;
using VsaTemplate.Domain.BaseClasses;

namespace VsaTemplate.Infrastructure.Database.Interceptors;

//credit: https://github.com/jasontaylordev/CleanArchitecture
public sealed class DispatchDomainEventInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        DispatchDomainEventsAsync(eventData.Context).GetAwaiter().GetResult();

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken()
    )
    {
        await DispatchDomainEventsAsync(eventData.Context);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public static async Task DispatchDomainEventsAsync(DbContext? context)
    {
        if (context is null)
            return;

        var entities = context
            .ChangeTracker.Entries<BaseEntity>()
            .Where(x => x.Entity.DomainEvents.Count != 0)
            .Select(x => x.Entity)
            .ToList();

        var events = entities.SelectMany(x => x.DomainEvents).ToList();

        entities.ForEach(x => x.ClearDomainEvents());

        var dispatcher = context.GetService<IDomainEventDispatcher>();

        foreach (var domainEvent in events)
        {
            await dispatcher.DispatchAsync(domainEvent, CancellationToken.None);
        }
    }
}
