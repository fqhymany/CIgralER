using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Domain.Entities;
using LawyerProject.Infrastructure.Extensions;
using Microsoft.Extensions.Caching.Memory;

namespace LawyerProject.Infrastructure.Services;

public class AuthorizationCacheService : IAuthorizationCacheService
{
    private readonly IIdentityService _identityService;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

    public AuthorizationCacheService(IIdentityService identityService, IMemoryCache cache)
    {
        _identityService = identityService;
        _cache = cache;
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        string cacheKey = $"UserRole_{userId}_{role}";

        if (!_cache.TryGetValue(cacheKey, out bool isInRole))
        {
            isInRole = await _identityService.IsInRoleAsync(userId, role);
            _cache.Set(cacheKey, isInRole, _cacheDuration);
        }

        return isInRole;
    }

    public async Task<bool> IsInRoleInRegionAsync(string userId, string role, int regionId)
    {
        string cacheKey = $"UserRoleRegion_{userId}_{role}_{regionId}";

        if (!_cache.TryGetValue(cacheKey, out bool isInRoleInRegion))
        {
            // اینجا باید متد مناسب در IIdentityService پیاده‌سازی شده باشد
            isInRoleInRegion = await _identityService.IsInRoleInRegionAsync(userId, role, regionId);
            _cache.Set(cacheKey, isInRoleInRegion, _cacheDuration);
        }

        return isInRoleInRegion;
    }

    public async Task<IEnumerable<string>?> GetUserRolesAsync(string userId)
    {
        string cacheKey = $"UserRoles_{userId}";

        if (!_cache.TryGetValue(cacheKey, out IEnumerable<string>? roles))
        {
            roles = await _identityService.GetUserRolesAsync(userId);
            _cache.Set(cacheKey, roles, _cacheDuration);
        }

        return roles;
    }

    public void InvalidateUserCache(string userId)
    {
        // روش ساده برای حذف کش کاربر - در پروژه‌های بزرگتر می‌توان از الگوی نشرکننده-مشترک استفاده کرد
        var cacheKeys = _cache.GetKeys().Where(k => k.StartsWith($"User_{userId}") ||
                                                   k.StartsWith($"UserRole_{userId}") ||
                                                   k.StartsWith($"UserRoleRegion_{userId}") ||
                                                   k.StartsWith($"UserRoles_{userId}"));

        foreach (var key in cacheKeys)
        {
            _cache.Remove(key);
        }
    }

    public async Task<IList<Role>?> GetAllRolesAsync()
    {
        string cacheKey = "AllRoles";

        if (!_cache.TryGetValue(cacheKey, out IList<Role>? roles))
        {
            roles = await _identityService.GetAllRolesAsync();
            _cache.Set(cacheKey, roles, _cacheDuration);
        }

        return roles;
    }

    public void InvalidateRolesCache()
    {
        _cache.Remove("AllRoles");
    }
}
