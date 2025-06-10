using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Auth.Queries.GetUser;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Models;
using LawyerProject.Application.RegionAdmin.Staff.Commands;
using MediatR;

namespace LawyerProject.Application.RegionAdmin.Staff.Commands;
public class UpdateStaffRolesCommandHandler
    : IRequestHandler<UpdateStaffRolesCommand, AuthResult>
{
    private readonly IIdentityService _identity;
    public UpdateStaffRolesCommandHandler(IIdentityService identity)
        => _identity = identity;

    public async Task<AuthResult> Handle(
        UpdateStaffRolesCommand request,
        CancellationToken cancellationToken)
    {
        // حذف تمام نقش‌های قبلی در آن ریجن
        await _identity.RemoveAllRolesFromRegionAsync(request.UserId);

        // افزودن نقش(ها) جدید
        foreach (var role in request.Roles)
        {
            string? roleId = _identity.GetRoleIdByName(role).ToString();
            var r = await _identity.AddToRoleAsync(request.UserId, roleId);
            if (!r.Succeeded)
                return new AuthResult() { Succeeded = false, Error = "خطا در ویرایش دسترسی های کاربر" };
        }

        return new AuthResult() { Succeeded = true };
    }
}
