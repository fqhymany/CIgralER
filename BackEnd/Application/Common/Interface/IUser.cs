namespace LawyerProject.Application.Common.Interfaces;

public interface IUser
{
    string? Id { get; }
    int RegionId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
}
