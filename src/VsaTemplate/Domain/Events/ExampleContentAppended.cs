using VsaTemplate.Common.Interfaces;
using VsaTemplate.Domain.Entities;
using VsaTemplate.Infrastructure.Database;

namespace VsaTemplate.Domain.Events;

public sealed record ExampleContentAppendedEvent(Guid ExampleId) : IDomainEvent;

public sealed class ExampleContentAppendedEventHandler
    : IDomainEventHandler<ExampleContentAppendedEvent>
{
    private readonly ApplicationDbContext _context;

    public ExampleContentAppendedEventHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(
        ExampleContentAppendedEvent domainEvent,
        CancellationToken cancellationToken
    )
    {
        var example = await _context.Examples.FirstOrDefaultAsync(
            x => x.Id == domainEvent.ExampleId,
            cancellationToken
        );

        if (example is null)
            throw new InvalidOperationException(
                $"{nameof(Example)} not found: {domainEvent.ExampleId}"
            );

        example.HasAppendedContent = true;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
