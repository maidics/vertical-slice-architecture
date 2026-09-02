using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Domain.BaseClasses;

namespace VsaTemplate.Infrastructure.Database.Interceptors;

//credit: https://github.com/jasontaylordev/CleanArchitecture
public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public AuditableEntityInterceptor(IUser user, TimeProvider timeProvider)
    {
        _user = user;
        _timeProvider = timeProvider;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        UpdateAuditableProperties(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken()
    )
    {
        UpdateAuditableProperties(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public void UpdateAuditableProperties(DbContext? context)
    {
        if (context is null)
            return;

        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (
                entry.State == EntityState.Added
                || entry.State == EntityState.Modified
                || entry.HasChangedOwnedEntities()
            )
            {
                var now = _timeProvider.GetUtcNow();

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedOn = now;
                    entry.Entity.CreatedBy = _user.Id;
                }

                entry.Entity.LastModifiedOn = now;
                entry.Entity.LastModifiedBy = _user.Id;
            }
        }
    }
}

public static class EntityEntryExtensions
{
    extension(EntityEntry entry)
    {
        public bool HasChangedOwnedEntities() =>
            entry.References.Any(r =>
                r.TargetEntry is not null
                && r.TargetEntry.Metadata.IsOwned()
                && r.TargetEntry.State is EntityState.Added or EntityState.Modified
            );
    }
}
