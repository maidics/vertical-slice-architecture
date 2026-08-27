using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace VsaTemplate.Tests.TestInfrastructure.Fixtures;

public sealed class EntityConfigurationFixture<TConfiguration, TEntity>
    where TConfiguration : class, IEntityTypeConfiguration<TEntity>, new()
    where TEntity : class
{
    private readonly ModelBuilder _modelBuilder = new(new ConventionSet());

    public IMutableEntityType EntityType { get; }

    public EntityConfigurationFixture()
    {
        new TConfiguration().Configure(_modelBuilder.Entity<TEntity>());

        EntityType =
            _modelBuilder.Model.FindEntityType(typeof(TEntity))
            ?? throw new InvalidOperationException($"Entity type not found: {typeof(TEntity)}");
    }

    public IMutableProperty GetProperty<TProperty>(Expression<Func<TEntity, TProperty>> selector)
    {
        var member = selector.Body switch
        {
            MemberExpression m => m,
            UnaryExpression { Operand: MemberExpression m } => m,
            _ => throw new ArgumentException(
                "Selector must be a simple property access, e.g. x => x.Name",
                nameof(selector)
            ),
        };

        return EntityType.FindProperty(member.Member.Name)
            ?? throw new InvalidOperationException(
                $"Property not found: '{member.Member.Name}' on {typeof(TEntity).Name}"
            );
    }
}
