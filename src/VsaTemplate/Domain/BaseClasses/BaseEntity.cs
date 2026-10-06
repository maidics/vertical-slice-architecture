namespace VsaTemplate.Domain.BaseClasses;

//credit: https://github.com/jasontaylordev/CleanArchitecture
public abstract class BaseEntity
{
    // This can easily be modified to BaseEntity<T> to support different types for Id
    // Using Guid for type safety
    public Guid Id { get; set; } = Guid.NewGuid();
}
