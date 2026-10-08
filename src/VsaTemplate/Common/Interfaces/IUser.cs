namespace VsaTemplate.Common.Interfaces;

public interface IUser
{
    Guid? Id { get; }
    IReadOnlyList<string>? Roles { get; }
}
