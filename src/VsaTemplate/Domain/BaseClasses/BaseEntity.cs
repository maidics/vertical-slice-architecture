using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.Domain.BaseClasses;

//credit: https://github.com/jasontaylordev/CleanArchitecture
public abstract class BaseEntity
{
    // This can easily be modified to BaseEntity<T> to support different types for Id
    // Using Guid for type safety
    public Guid Id { get; set; } = Guid.NewGuid();

    private readonly List<IDomainEvent> _domainEvents = [];

    [NotMapped]
    public ReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
