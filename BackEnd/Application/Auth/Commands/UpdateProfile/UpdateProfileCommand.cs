using LawyerProject.Application.Auth.Queries.GetUser;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Models;

namespace LawyerProject.Application.Auth.Commands.UpdateProfile;

public record UpdateProfileCommand : IRequest<AuthResult>
{
    public string UserId { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public string? NationalCode { get; init; }
    public List<string>? Roles { get; init; } = [];
}

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, AuthResult>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;

    public UpdateProfileCommandHandler(IIdentityService identityService, IApplicationDbContext context)
    {
        _identityService = identityService;
        _context = context;
    }

    public async Task<AuthResult> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var (exists, message) = await _identityService.CheckUserExistsAsync(
            null,
            null,
            request.NationalCode,
            request.PhoneNumber,
            request.UserId);

        if (!exists)
        {
            var user = await _identityService.GetUserByIdAsync(request.UserId);

            if (user == null)
                if (user == null)
                    return new AuthResult { Succeeded = false, Error ="کاربر پیدا نشد" };

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            user.NationalCode = request.NationalCode;

             await _identityService.UpdateUserAsync(user);

            if (request.Roles == null)
            {
                await _identityService.RemoveAllRolesFromRegionAsync(user.Id);
                return new AuthResult { Succeeded = true };
            }

            var currentRoles = await _identityService.GetUserRolesAsync(user.Id);

            var rolesToAdd = request.Roles.Except(currentRoles).ToList();
            var rolesToRemove = currentRoles.Except(request.Roles).ToList();

            foreach (var role in rolesToRemove)
            {
                string? roleId = await _identityService.GetRoleIdByName(role);
                await _identityService.RemoveFromRolesAsync(user.Id, roleId);
            }

            foreach (var role in rolesToAdd)
            {
                string? roleId = await _identityService.GetRoleIdByName(role);
                await _identityService.AddToRoleAsync(user.Id, roleId);
            }

            return new AuthResult { Succeeded = true };
        }
        else
        {
            return new AuthResult { Succeeded = false, Error = message };
        }
    }
}
