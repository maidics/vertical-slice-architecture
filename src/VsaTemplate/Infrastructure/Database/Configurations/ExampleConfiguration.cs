using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VsaTemplate.Domain.Entities;

namespace VsaTemplate.Infrastructure.Database.Configurations;

public sealed class ExampleConfiguration : IEntityTypeConfiguration<Example>
{
    public void Configure(EntityTypeBuilder<Example> builder)
    {
        builder.HasIndex(x => x.Content).IsUnique();
    }
}
