using Azure.Core;
using System.Threading;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Models;
using LawyerProject.Application.Common.Utils;
using LawyerProject.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static System.String;

namespace LawyerProject.Infrastructure.Identity;
public class IdentityService : IIdentityService
{
    private readonly UserManager<User> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly IUserClaimsPrincipalFactory<User> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly RoleManager<Role> _roleManager;
    private readonly IUser _user;

    public IdentityService(
        UserManager<User> userManager,
        IUserClaimsPrincipalFactory<User> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService, IApplicationDbContext context, RoleManager<Role> roleManager, IUser user)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _context = context;
        _roleManager = roleManager;
        _user = user;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.UserName;
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);
        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(User user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<Result> DeleteUserAsync(string? userId)
    {
        if (!IsNullOrWhiteSpace(userId) && !IsNullOrEmpty(userId))
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return Result.Success();
            }

            var result = await _userManager.DeleteAsync(user);

            return result.ToApplicationResult();
        }
        return Result.Failure(["User Not Found"]);
    }

    public async Task<Result> ValidateCredentialsAsync(string userName, string password)
    {
        var user = await _userManager.FindByNameAsync(userName);

        if (user == null)
        {
            return Result.Failure(new[] { "User not found" });
        }

        var result = await _userManager.CheckPasswordAsync(user, password);

        return result ? Result.Success() : Result.Failure(new[] { "Invalid password" });
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<Result> UpdateUserAsync(User user)
    {
        var result = await _userManager.UpdateAsync(user);
        return result.ToApplicationResult();
    }

    public async Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Result.Failure(new[] { "User not found" });
        }

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.ToApplicationResult();
    }

    public async Task<Result> ResetPasswordAsync(User user, string token, string newPassword)
    {
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.ToApplicationResult();
    }

    public async Task<Result> AddToRoleAsync(string userId, string? roleId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var regionId = _user.RegionId;
        if (user == null)
        {
            return Result.Failure(new[] { "User not found" });
        }

        if (roleId == null)
        {
            return Result.Failure(new[] { "Role not found" });
        }

        // Check if the region exists
        var region = await _context.Regions.FindAsync(regionId);
        if (region == null)
        {
            return Result.Failure(new[] { "Region not found" });
        }

        // Check if the role already exists for this user in this region
        var existingUserRole = await _context.UsersRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId &&
                                       ur.RoleId == roleId &&
                                       ((UsersRole)ur).RegionId == regionId);

        if (existingUserRole != null)
        {
            return Result.Success(); // User already has this role in this region
        }

        // Create the UsersRole entity with the region
        var userRole = new UsersRole
        {
            UserId = userId,
            RoleId = roleId,
            RegionId = regionId
        };

        await _context.UsersRoles.AddAsync(userRole);
        await _context.SaveChangesAsync(CancellationToken.None);

        return Result.Success();
    }
    public async Task<IList<string>> GetUserRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return new List<string>();
        }

        return await _userManager.GetRolesAsync(user);
    }

    public async Task<bool> IsInRoleInRegionAsync(string userId, string role, int regionId)
    {
        if (!await IsInRoleAsync(userId, role))
        {
            return false;
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        // Check if the user is associated with the specified region
        // This assumes you have entity access through UserManager or need to inject a DbContext
        var regionUser = user.RegionsUsers.FirstOrDefault(ru =>
            ru.UserId == userId &&
            ru.RegionId == regionId);

        return regionUser != null;
    }

    public async Task<(bool exists, string? message)> CheckUserExistsAsync(string? userName, string? email, string? nationalCode, string? phoneNumber, string? excludeUserId = null)
    {
        var regionId = _user.RegionId; // Get current user's region

        // Check username in region
        if (!IsNullOrWhiteSpace(userName) && !IsNullOrEmpty(userName))
        {
            var userByName = await _userManager.Users
                .Where(u => u.UserName == userName && u.Id != excludeUserId)
                .Join(_context.RegionsUsers,
                    user => user.Id,
                    ru => ru.UserId,
                    (user, ru) => new { User = user, RegionUser = ru })
                .AnyAsync(x => x.RegionUser.RegionId == regionId);

            if (userByName)
            {
                return (true, $"کاربری با نام کاربری {userName} قبلاً در این ناحیه ثبت شده است");
            }
        }

        // Check email in region
        if (!IsNullOrWhiteSpace(email) && !IsNullOrEmpty(email))
        {
            var userByEmail = await _userManager.Users
                .Where(u => u.Email == email && u.Id != excludeUserId)
                .Join(_context.RegionsUsers,
                    user => user.Id,
                    ru => ru.UserId,
                    (user, ru) => new { User = user, RegionUser = ru })
                .AnyAsync(x => x.RegionUser.RegionId == regionId);

            if (userByEmail)
            {
                return (true, $"کاربری با ایمیل {email} قبلاً در این ناحیه ثبت شده است");
            }
        }

        // Check national code in region
        if (!IsNullOrWhiteSpace(nationalCode) && !IsNullOrEmpty(nationalCode))
        {
            var userByNationalCode = await _userManager.Users
                .Where(u => u.NationalCode == nationalCode && u.Id != excludeUserId)
                .Join(_context.RegionsUsers,
                    user => user.Id,
                    ru => ru.UserId,
                    (user, ru) => new { User = user, RegionUser = ru })
                .AnyAsync(x => x.RegionUser.RegionId == regionId);

            if (userByNationalCode)
            {
                return (true, $"کاربری با کد ملی {nationalCode} قبلاً در این ناحیه ثبت شده است");
            }
        }

        // Check phone number in region
        if (!IsNullOrWhiteSpace(phoneNumber) && !IsNullOrEmpty(phoneNumber))
        {
            var userByPhoneNumber = await _userManager.Users
                .Where(u => u.PhoneNumber == phoneNumber && u.Id != excludeUserId)
                .Join(_context.RegionsUsers,
                    user => user.Id,
                    ru => ru.UserId,
                    (user, ru) => new { User = user, RegionUser = ru })
                .AnyAsync(x => x.RegionUser.RegionId == regionId);

            if (userByPhoneNumber)
            {
                return (true, $"کاربری با تلفن همراه {phoneNumber} قبلاً در این ناحیه ثبت شده است");
            }
        }

        return (false, null);
    }

    public async Task<Result> AddToRegionAsync(string userId, int regionId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Result.Failure(new[] { "کاربر پیدا نشد" });
        }
        else
        {
            var region = await _context.Regions.FindAsync(regionId);
            if (region == null)
            {
                return Result.Failure(new[] { "منطقه پیدا نشد" });
            }
            else
            {
                var regionsUser = await _context.RegionsUsers
                    .FirstOrDefaultAsync(ru => ru.UserId == userId && ru.RegionId == regionId);
                if (regionsUser != null)
                {
                    return Result.Success();
                }
            }
        }
        var entity = new RegionsUser()
        {
            UserId = userId,
            RegionId = regionId

        };

        await _context.RegionsUsers.AddAsync(entity);

        await _context.SaveChangesAsync(CancellationToken.None);

        return Result.Success();
    }

    public async Task<IList<Role>> GetAllRolesAsync()
    {
        return await _roleManager.Roles.Where(ro => ro.IsSystemRole == false)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<string?> GetRoleNameById(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        return role?.Name;
    }

    public async Task<string?> GetRoleIdByName(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        return role?.Id;
    }

    public async Task<Result> RemoveFromRolesAsync(string userId, string? role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var regionId = _user.RegionId;

        if (user == null)
        {
            return Result.Failure(new[] { "User not found" });
        }

        if (role == null)
        {
            return Result.Failure(new[] { "Role not found" });
        }

        // Check if the region exists
        var region = await _context.Regions.FindAsync(regionId);
        if (region == null)
        {
            return Result.Failure(new[] { "Region not found" });
        }

        // Find and remove the user role for this specific region
        var userRole = await _context.UsersRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId &&
                                       ur.RoleId == role &&
                                       ((UsersRole)ur).RegionId == regionId);

        if (userRole == null)
        {
            return Result.Success(); // Role was not assigned in this region
        }

        _context.UsersRoles.Remove(userRole);
        await _context.SaveChangesAsync(CancellationToken.None);

        return Result.Success();
    }

    public async Task<Result> RemoveAllRolesFromRegionAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var regionId = _user.RegionId;

        if (user == null)
        {
            return Result.Failure(new[] { "User not found" });
        }

        // Check if the region exists
        var region = await _context.Regions.FindAsync(regionId);
        if (region == null)
        {
            return Result.Failure(new[] { "Region not found" });
        }

        // Find all user roles for this specific region
        var userRoles = await _context.UsersRoles
            .Where(ur => ur.UserId == userId &&
                         ((UsersRole)ur).RegionId == regionId)
            .ToListAsync();

        if (!userRoles.Any())
        {
            return Result.Success(); // No roles to remove in this region
        }

        // Remove all roles for this user in the current region
        _context.UsersRoles.RemoveRange(userRoles);
        await _context.SaveChangesAsync(CancellationToken.None);

        return Result.Success();
    }

    public async Task<IList<User>> GetUsersInRolesAsync(string roleName, int? regionId = null)
    {
        // If no specific region is provided, use the current user's region
        var targetRegionId = regionId ?? _user.RegionId;

        // Get users in the specified role for the given region
        var usersInRole = await _userManager.Users
            .Join(_context.UsersRoles,
                user => user.Id,
                ur => ur.UserId,
                (user, ur) => new { User = user, UserRole = ur })
            .Where(x => x.UserRole.RoleId == roleName &&
                        ((UsersRole)x.UserRole).RegionId == targetRegionId)
            .Select(x => x.User)
            .ToListAsync();

        return usersInRole;
    }

    public async Task<IList<User>> GetUsersInRolesAsync(IList<string> roleNames, int? regionId = null)
    {
        var roleIds = new List<string>();

        foreach (var roleName in roleNames)
        {
            var roleId = await GetRoleIdByName(roleName);
            if (roleId != null)
            {
                roleIds.Add(roleId);
            }
        }

        // اگر region مشخص نشده باشد از region کاربر فعلی استفاده می‌کنیم
        var targetRegionId = regionId ?? _user.RegionId;

        // حالا کاربران را در نقش‌های مشخص شده در region مورد نظر پیدا می‌کنیم
        var usersInRoles = await _userManager.Users
            .Join(_context.UsersRoles,
                user => user.Id,
                ur => ur.UserId,
                (user, ur) => new { User = user, UserRole = ur })
            .Where(x => roleIds.Contains(x.UserRole.RoleId) &&
                        ((UsersRole)x.UserRole).RegionId == targetRegionId)
            .Select(x => x.User)
            .Distinct()
            .ToListAsync();

        return usersInRoles;
    }

    public async Task<Dictionary<string, IList<string>>> GetUsersRolesAsync(IEnumerable<string> userIds)
    {
        var userRoles = await _context.UsersRoles
            .Where(ur => userIds.Contains(ur.UserId))
            .Select(ur => new { ur.UserId, ur.RoleId })
            .ToListAsync();

        var roleNames = await _roleManager.Roles
            .Where(r => userRoles.Select(ur => ur.RoleId).Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => r.Name);

        var result = userRoles
            .GroupBy(ur => ur.UserId)
            .ToDictionary(
                g => g.Key,
                g => (IList<string>)g.Select(ur => roleNames[ur.RoleId]).ToList()
            );

        return result;
    }
}
