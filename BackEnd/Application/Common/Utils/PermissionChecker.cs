using LawyerProject.Application.Common.Interfaces;

public enum SectionPermissionType
{
    View,
    Create,
    Edit,
    Delete
}

public class PermissionChecker : IPermissionChecker
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    public PermissionChecker(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<bool> HasSectionPermissionAsync(string userId, string sectionName, SectionPermissionType permissionType)
    {
        // دریافت نقش‌های کاربر
        var userRoles = await _context.UsersRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .ToListAsync();

        if (!userRoles.Any())
            return false;

        // اگر کاربر نقش سیستمی دارد، همیشه دسترسی دارد
        if (userRoles.Any(ur => ur.Role != null && ur.Role.IsSystemRole))
            return true;

        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();

        // بررسی دسترسی نقش به بخش بر اساس نوع دسترسی
        return permissionType switch
        {
            SectionPermissionType.View =>
                await _context.RolePermissions.AnyAsync(rp => roleIds.Contains(rp.RoleId) && rp.Section == sectionName && rp.CanView == true && rp.RegionId == _user.RegionId),
            SectionPermissionType.Create =>
                await _context.RolePermissions.AnyAsync(rp => roleIds.Contains(rp.RoleId) && rp.Section == sectionName && rp.CanCreate == true && rp.RegionId == _user.RegionId),
            SectionPermissionType.Edit =>
                await _context.RolePermissions.AnyAsync(rp => roleIds.Contains(rp.RoleId) && rp.Section == sectionName && rp.CanEdit == true && rp.RegionId == _user.RegionId),
            SectionPermissionType.Delete =>
                await _context.RolePermissions.AnyAsync(rp => roleIds.Contains(rp.RoleId) && rp.Section == sectionName && rp.CanDelete == true && rp.RegionId == _user.RegionId),
            _ => false
        };
    }
}
