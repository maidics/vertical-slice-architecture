using VsaTemplate.Common.Interfaces;

namespace VsaTemplate.Domain.Events;

public sealed record ExampleContentAppendedEvent(Guid ExampleId) : IDomainEvent;
