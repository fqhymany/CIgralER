using LawyerProject.Application.Common.Models;
using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.Common.Interfaces;
public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<bool> AuthorizeAsync(string userId, string policyName);
    Task<(Result Result, string UserId)> CreateUserAsync(User user, string password);
    Task<Result> DeleteUserAsync(string? userId);
    Task<Result> ValidateCredentialsAsync(string email, string password);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(string userId);
    Task<Result> UpdateUserAsync(User user);
    Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<Result> ResetPasswordAsync(User user, string token, string newPassword);
    Task<Result> AddToRoleAsync(string userId, string? roleId);
    Task<IList<string>> GetUserRolesAsync(string userId);
    Task<bool> IsInRoleInRegionAsync(string userId, string role, int regionId);
    Task<(bool exists, string? message)> CheckUserExistsAsync(string? userName, string? email, string? nationalCode, string? phoneNumber, string? excludeUserId = null);
    Task<Result> AddToRegionAsync(string userId, int regionId);
    Task<IList<Role>> GetAllRolesAsync();

    Task<string?> GetRoleNameById(string roleId);
    Task<string?> GetRoleIdByName(string roleName);
    Task<Result> RemoveFromRolesAsync(string userId, string? role);

    Task<Result> RemoveAllRolesFromRegionAsync(string userId);

    Task<IList<User>> GetUsersInRolesAsync(string roleName, int? regionId = null);

    Task<IList<User>> GetUsersInRolesAsync(IList<string> roleNames, int? regionId = null);

    Task<Dictionary<string, IList<string>>> GetUsersRolesAsync(IEnumerable<string> userIds);
}
