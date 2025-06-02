using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LawyerProject.Infrastructure.Services;

public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly IApplicationDbContext _dbContext;

    public RolePermissionRepository(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<RolePermission>> GetByRoleIdAsync(string roleId, CancellationToken cancellationToken)
    {
        return await _dbContext.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);
    }

    public async Task<RolePermission?> GetByRoleIdAndSectionAsync(string roleId, string section, CancellationToken cancellationToken)
    {
        return await _dbContext.RolePermissions
            .FirstOrDefaultAsync(rp =>
                    rp.RoleId == roleId &&
                    rp.Section == section,
                cancellationToken);
    }

    public async Task AddAsync(RolePermission rolePermission, CancellationToken cancellationToken)
    {
        await _dbContext.RolePermissions.AddAsync(rolePermission, cancellationToken);
    }

    public void Update(RolePermission rolePermission)
    {
        _dbContext.RolePermissions.Update(rolePermission);
    }

    public void Remove(RolePermission rolePermission)
    {
        _dbContext.RolePermissions.Remove(rolePermission);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
