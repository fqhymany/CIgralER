using LawyerProject.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using LawyerProject.Application.Common.Models;

namespace LawyerProject.Infrastructure.Identity;

public class UserClaimsTransformation : IClaimsTransformation
{
    private readonly IIdentityService _identityService;
    private readonly IRegionService _regionService;

    public UserClaimsTransformation(IIdentityService identityService, IRegionService regionService)
    {
        _identityService = identityService;
        _regionService = regionService;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = principal.Identity as ClaimsIdentity;

        if (identity == null || !identity.IsAuthenticated)
            return principal;

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return principal;

        // اگر Region فعلی در claims وجود ندارد، اضافه کن
        ClaimsIdentity? claimsIdentity;
        if (!principal.HasClaim(c => c.Type == "RegionId"))
        {
            var userRegions = await _regionService.GetUserRegionsAsync(userId);
            var defaultRegion = userRegions!.FirstOrDefault();

            if (defaultRegion != null)
            {
                claimsIdentity = (ClaimsIdentity)principal.Identity!;
                claimsIdentity.AddClaim(new Claim("RegionId", defaultRegion.RegionId.ToString()));

                // می‌توان اطلاعات دیگر مرتبط با منطقه را نیز اضافه کرد
                claimsIdentity.AddClaim(new Claim("RegionName", defaultRegion.RegionName!));
            }
        }

        // کش کردن نقش‌های کاربر به عنوان claims
        var roles = await _identityService.GetUserRolesAsync(userId);
        claimsIdentity = principal.Identity as ClaimsIdentity;

        foreach (var role in roles)
        {
            if (!principal.IsInRole(role))
            {
                claimsIdentity!.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        }

        return principal;
    }
}
