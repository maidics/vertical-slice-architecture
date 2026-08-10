using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace VsaTemplate.FunctionalTests.Infrastructure.Common;

public abstract class EntityConfigurationTestBase<TConfiguration, TEntity>
    where TConfiguration : class, IEntityTypeConfiguration<TEntity>, new()
    where TEntity : class
{
    protected ModelBuilder ModelBuilder { get; } = new ModelBuilder(new ConventionSet());
    protected TConfiguration Configuration { get; } = new TConfiguration();

    [SetUp]
    public void Configure()
    {
        Configuration.Configure(ModelBuilder.Entity<TEntity>());

        var entityType = ModelBuilder.Model.FindEntityType(typeof(TEntity));

        if (entityType is null)
            throw new InvalidOperationException($"Entity type not found: {typeof(TEntity)}");
    }

    protected IMutableEntityType GetEntityType()
    {
        return ModelBuilder.Model.FindEntityType(typeof(TEntity))!;
    }

    protected IMutableProperty GetProperty<TProp>(Expression<Func<TEntity, TProp>> selector)
    {
        var member = selector.Body switch
        {
            MemberExpression m => m,
            UnaryExpression { Operand: MemberExpression m } => m,
            _ => throw new ArgumentException(
                "Selector must be a simple property access, e.g. x => x.Name"
            ),
        };

        var name = member.Member.Name;

        return GetEntityType().FindProperty(name)
            ?? throw new InvalidOperationException(
                $"Property not found: '{name}'. Entity {typeof(TEntity).Name}"
            );
    }
}
