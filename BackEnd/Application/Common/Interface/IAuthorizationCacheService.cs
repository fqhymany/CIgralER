using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.Common.Interfaces;

public interface IAuthorizationCacheService
{
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<bool> IsInRoleInRegionAsync(string userId, string role, int regionId);
    Task<IEnumerable<string>?> GetUserRolesAsync(string userId);
    void InvalidateUserCache(string userId);
    Task<IList<Role>?> GetAllRolesAsync();
    void InvalidateRolesCache();

}
