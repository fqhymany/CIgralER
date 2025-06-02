using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LawyerProject.Application.Common.Security
{
    public class RegionAuthorizationFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var regionIdClaim = context.HttpContext.User.FindFirst("RegionId");
            if (regionIdClaim == null)
            {
                return Task.FromResult(new Results.TokenValidationResult { Succeeded = false, Error = "دسترسی غیر مجاز" });
            }

            var regionId = int.Parse(regionIdClaim.Value);
            var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // بررسی دسترسی کاربر به ناحیه
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<IApplicationDbContext>();
            var hasAccess = await dbContext.RegionsUsers
                .AnyAsync(ur => ur.UserId == userId && ur.RegionId == regionId);

            if (!hasAccess)
            {
                return Task.FromResult(new Results.TokenValidationResult { Succeeded = false, Error = "دسترسی غیر مجاز" });
            }

            return await next(context);
        }
    }
}
