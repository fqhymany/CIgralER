using MediatR;
using LawyerProject.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.RolePermissions.Commands.SetRolePermissions;

public class SetRolePermissionsCommandHandler : IRequestHandler<SetRolePermissionsCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IIdentityService _identityService;
    private readonly IUser _user;

    public SetRolePermissionsCommandHandler(IApplicationDbContext dbContext, IIdentityService identityService, IUser user)
    {
        _dbContext = dbContext;
        _identityService = identityService;
        _user = user;
    }

    public async Task<Unit> Handle(SetRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        string roleId = await _identityService.GetRoleIdByName(request.RoleId) ?? request.RoleId;

        // برای هر بخش در درخواست
        foreach (var permissionDto in request.Permissions)
        {
            // بررسی وجود رکورد مشابه در دیتابیس
            var existingPermission = await _dbContext.RolePermissions
                .FirstOrDefaultAsync(rp =>
                    rp.RoleId == roleId &&
                    rp.Section == permissionDto.Section &&
                    rp.RegionId==_user.RegionId,
                    cancellationToken);

            if (existingPermission != null)
            {
                // بروزرسانی رکورد موجود
                existingPermission.CanView = permissionDto.CanView;
                existingPermission.CanCreate = permissionDto.CanCreate;
                existingPermission.CanEdit = permissionDto.CanEdit;
                existingPermission.CanDelete = permissionDto.CanDelete;
            }
            else
            {
                // اضافه کردن رکورد جدید
                _dbContext.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    Section = permissionDto.Section,
                    CanView = permissionDto.CanView,
                    CanCreate = permissionDto.CanCreate,
                    CanDelete = permissionDto.CanDelete,
                    CanEdit = permissionDto.CanEdit,
                    RegionId = _user.RegionId
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
