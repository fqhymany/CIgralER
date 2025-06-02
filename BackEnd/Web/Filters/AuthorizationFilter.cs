using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;

namespace LawyerProject.Web.Filters;

public class AuthorizationFilter : IEndpointFilter
{
    private readonly IRegionService _regionService;
    private readonly IIdentityService _identityService;
    private readonly IUser _currentUser;

    public AuthorizationFilter(IRegionService regionService, IIdentityService identityService, IUser currentUser)
    {
        _regionService = regionService;
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var endpoint = context.HttpContext.GetEndpoint();
        var authorizeAttributes = endpoint?.Metadata.GetOrderedMetadata<AuthorizeAttribute>() ?? Array.Empty<AuthorizeAttribute>();

        if (!authorizeAttributes.Any())
        {
            return await next(context);
        }

        var user = context.HttpContext.RequestServices.GetRequiredService<IUser>();

        if (user.Id == null)
        {
            return Results.Unauthorized();
        }

        if (_currentUser.Id != null)
        {
            context.HttpContext.Items["UserRegions"] = await _regionService.GetUserRegionsAsync(_currentUser.Id);
            context.HttpContext.Items["UserRoles"] = await _identityService.GetUserRolesAsync(_currentUser.Id);
        }


        return await next(context);
    }
}
