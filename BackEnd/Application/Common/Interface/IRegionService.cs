using LawyerProject.Application.Common.Models;

namespace LawyerProject.Application.Common.Interfaces;

public interface IRegionService
{
    Task<IEnumerable<UserRegionDto>?> GetUserRegionsAsync(string userId);
    Task<UserRegionDto?> GetUserDefaultRegionAsync(string userId);
    Task<bool> IsUserInRegionAsync(string userId, int regionId);
    Task<List<int>?> GetUserAccessibleRegionIdsAsync(string userId, string? role = null);
    Task<string?> GetRegionNameById(int regionId, CancellationToken cancellationToken);
}
