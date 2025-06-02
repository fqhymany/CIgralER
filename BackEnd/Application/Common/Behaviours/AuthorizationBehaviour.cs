using System.Reflection;
using LawyerProject.Application.Common.Exceptions;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;

namespace LawyerProject.Application.Common.Behaviours;
public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    private readonly IPermissionChecker _permissionChecker;

    public AuthorizationBehaviour(
        IUser user,
        IIdentityService identityService,
        IPermissionChecker permissionChecker)
    {
        _user = user;
        _identityService = identityService;
        _permissionChecker = permissionChecker;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>().ToArray();

        if (authorizeAttributes.Any())
        {
            // بررسی احراز هویت کاربر
            if (_user.Id == null)
            {
                throw new UnauthorizedAccessException();
            }

            // بررسی دسترسی منطقه‌ای
            var authorizeAttributesWithRegions = authorizeAttributes.Where(a => a.Regions != null && a.Regions.Length > 0);
            if (authorizeAttributesWithRegions.Any())
            {
                var authorized = false;
                foreach (var attribute in authorizeAttributesWithRegions)
                {
                    if (attribute.Regions.Contains(_user.RegionId))
                    {
                        authorized = true;
                        break;
                    }
                }

                if (!authorized)
                {
                    throw new ForbiddenAccessException("User doesn't have access to this region");
                }
            }

            // بررسی دسترسی نقش-محور
            var authorizeAttributesWithRoles = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Roles));
            if (authorizeAttributesWithRoles.Any())
            {
                var authorized = false;
                foreach (var roles in authorizeAttributesWithRoles.Select(a => a.Roles.Split(',')))
                {
                    foreach (var role in roles)
                    {
                        var isInRole = await _identityService.IsInRoleAsync(_user.Id, role.Trim());
                        if (isInRole)
                        {
                            authorized = true;
                            break;
                        }
                    }
                }

                if (!authorized)
                {
                    throw new ForbiddenAccessException("User is not authorized to access this resource");
                }
            }

            // بررسی دسترسی سیاست-محور
            var authorizeAttributesWithPolicies = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Policy));
            if (authorizeAttributesWithPolicies.Any())
            {
                foreach (var policy in authorizeAttributesWithPolicies.Select(a => a.Policy))
                {
                    var authorized = await _identityService.AuthorizeAsync(_user.Id, policy);
                    if (!authorized)
                    {
                        throw new ForbiddenAccessException("User does not meet policy requirements");
                    }
                }
            }

            // بررسی دسترسی بخش (Section)
            var authorizeAttributesWithSection = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Section));
            if (authorizeAttributesWithSection.Any())
            {
                var authorized = false;
                foreach (var attr in authorizeAttributesWithSection)
                {
                    var section = attr.Section;
                    var permissionType = attr.PermissionType;
                    if (await _permissionChecker.HasSectionPermissionAsync(_user.Id, section, permissionType))
                    {
                        authorized = true;
                        break;
                    }
                }
                if (!authorized)
                {
                    throw new ForbiddenAccessException("User is not authorized to access this section");
                }
            }
        }

        // اجرا درخواست
        return await next();
    }
}
