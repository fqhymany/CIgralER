using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Domain.Common;

namespace LawyerProject.Infrastructure.Services;

public class RegionFilter : IRegionFilter
{
    private readonly IUser _currentUser;
    private readonly IRegionService _regionService;

    public RegionFilter(IUser currentUser, IRegionService regionService)
    {
        _currentUser = currentUser;
        _regionService = regionService;
    }

    public async Task<IQueryable<T>?> FilterByRegionAsync<T>(IQueryable<T> query, string? userId) where T : IHasRegion
    {
        // اگر کاربر مشخص نشده، کاربر فعلی را استفاده کن
        userId ??= _currentUser.Id;

        if (userId == null)
            return query.Take(0); // هیچ نتیجه‌ای نشان نده

        // دریافت ناحیه‌های کاربر
        var userRegions = await _regionService.GetUserRegionsAsync(userId);
        if (userRegions != null)
        {
            var regionIds = userRegions.Select(r => r.RegionId).ToList();
            return query.Where(e => regionIds.Contains(e.RegionId));
        }

        return null;
    }
}
