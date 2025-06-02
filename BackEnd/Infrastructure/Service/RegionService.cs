using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Models;
using LawyerProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LawyerProject.Infrastructure.Services;

public class RegionService : IRegionService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMemoryCache _cache;
    private readonly IApplicationDbContext _context;

    public RegionService(IApplicationDbContext dbContext, IMemoryCache cache, IApplicationDbContext context)
    {
        _dbContext = dbContext;
        _cache = cache;
        _context = context;
    }

    public async Task<IEnumerable<UserRegionDto>?> GetUserRegionsAsync(string userId)
    {
        // کش کردن مناطق کاربر برای بهبود عملکرد
        string cacheKey = $"UserRegions_{userId}";

        if (!_cache.TryGetValue(cacheKey, out IEnumerable<UserRegionDto>? regions))
        {
            regions = await _dbContext.RegionsUsers
                .Where(ru => ru.UserId == userId)
                .Select(ru => new UserRegionDto
                {
                    RegionId = ru.RegionId ?? 0,
                    RegionName = ru.Region!.Name
                })
                .ToListAsync();

            // ذخیره در کش با زمان انقضای 15 دقیقه
            _cache.Set(cacheKey, regions, TimeSpan.FromMinutes(15));
        }

        return regions;
    }

    public async Task<UserRegionDto?> GetUserDefaultRegionAsync(string userId)
    {
        var regions = await GetUserRegionsAsync(userId);
        return regions!.FirstOrDefault();
    }

    public async Task<bool> IsUserInRegionAsync(string userId, int regionId)
    {
        var regions = await GetUserRegionsAsync(userId);
        return regions!.Any(r => r.RegionId == regionId);
    }

    public async Task<List<int>?> GetUserAccessibleRegionIdsAsync(string userId, string? role = null)
    {
        // کش کردن نتایج
        string cacheKey = $"UserAccessibleRegions_{userId}_{role ?? "all"}";

        if (!_cache.TryGetValue(cacheKey, out List<int>? regionIds))
        {
            if (string.IsNullOrEmpty(role))
            {
                // همه مناطقی که کاربر به آنها دسترسی دارد
                regionIds = await _dbContext.RegionsUsers
                    .Where(ru => ru.UserId == userId)
                    .Select(ru => ru.RegionId ?? 0)
                    .ToListAsync();
            }
            else
            {
                // مناطقی که کاربر با نقش خاصی به آنها دسترسی دارد
                regionIds = await _dbContext.UsersRoles
                    .Where(ur => ur.UserId == userId && ur.Role.Name == role)
                    .Select(ur => ur.RegionId)
                    .ToListAsync();
            }

            _cache.Set(cacheKey, regionIds, TimeSpan.FromMinutes(15));
        }

        return regionIds;
    }

    public async Task<string?> GetRegionNameById(int regionId, CancellationToken cancellationToken)
    {
        Region? region = await _context.Regions
            .FirstOrDefaultAsync(r => r.Id == regionId, cancellationToken);
        if (region == null)
        {
            return "نامشخص";
        }

        return region.Name;
    }
}
