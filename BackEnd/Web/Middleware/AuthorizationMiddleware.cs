using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;

namespace LawyerProject.Web.Middleware;

public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUser currentUser, IIdentityService identityService)
    {
        // بررسی اگر مسیر نیاز به احراز هویت دارد
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            await _next(context);
            return;
        }

        var authorizeAttributes = endpoint.Metadata
            .OfType<AuthorizeAttribute>()
            .ToArray();

        if (!authorizeAttributes.Any())
        {
            await _next(context);
            return;
        }

        // بررسی احراز هویت کاربر
        if (currentUser.Id == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        try
        {
            // بررسی دسترسی منطقه‌ای
            var regionAttributes = authorizeAttributes.Where(a => a.Regions != null && a.Regions.Length > 0);
            if (regionAttributes.Any())
            {   
                bool hasRegionAccess = regionAttributes.Any(attr => attr.Regions.Contains(currentUser.RegionId));
                if (!hasRegionAccess)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }
            }

            // بررسی نقش‌ها
            var roleAttributes = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Roles));
            if (roleAttributes.Any())
            {
                bool hasRoleAccess = false;
                foreach (var attr in roleAttributes)
                {
                    foreach (var role in attr.Roles.Split(',').Select(r => r.Trim()))
                    {
                        if (await identityService.IsInRoleAsync(currentUser.Id, role))
                        {
                            hasRoleAccess = true;
                            break;
                        }
                    }
                    if (hasRoleAccess) break;
                }

                if (!hasRoleAccess)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }
            }

            // بررسی سیاست‌ها
            var policyAttributes = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Policy));
            foreach (var attr in policyAttributes)
            {
                if (!await identityService.AuthorizeAsync(currentUser.Id, attr.Policy))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            // لاگ خطا
            await context.Response.WriteAsync($"An error occurred: {ex.Message}");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}
