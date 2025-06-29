using System.Security.AccessControl;
using LawyerProject.Application.Auth.Queries.GetUser;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.UserRoles.Queries.GetUserNamesByRoleName;
using LawyerProject.Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LawyerProject.Application.Auth.Commands.Register;

public record RegisterCommand : IRequest<AuthResult>
{
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string UserName { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string PhoneNumber { get; init; } = null!;
    public string NationalCode { get; init; } = null!;
    public List<string> Roles { get; init; } = [];
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly IUser _user;
    private readonly ICredentialService _credentialGen;

    public RegisterCommandHandler(
        IIdentityService identityService,
        ITokenService tokenService,
        IUser user, ICredentialService credentialGen)
    {
        _identityService = identityService;
        _tokenService = tokenService;
        _user = user;
        _credentialGen = credentialGen;
    }

    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var (exists, message) = await _identityService.CheckUserExistsAsync(
            request.UserName,
            request.Email,
            request.NationalCode,
            request.PhoneNumber);

        if (!exists)
        {
            var username = request.UserName;
            var email = request.Email;
            var password = request.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrWhiteSpace(username))
            {
                username = _credentialGen.GenerateTemporaryEmail();
            }

            if (string.IsNullOrEmpty(email) || string.IsNullOrWhiteSpace(email))
            {
                email = username;
            }

            if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(password))
            {
                password = "Dadvik123.";
            }

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = email,
                UserName = username,
                PhoneNumber = request.PhoneNumber,
                NationalCode = request.NationalCode,
                IsActive = false
            };

            var result = await _identityService.CreateUserAsync(user, password);

            if (!result.Result.Succeeded)
                return new AuthResult { Succeeded = false, Error = result.Result.Errors[0] };


            foreach (var role in request.Roles)
            {
                await _identityService.AddToRoleAsync(result.UserId, await _identityService.GetRoleIdByName(role));
            }

            await _identityService.AddToRegionAsync(user.Id, _user.RegionId);

            var token = await _tokenService.GenerateTokenAsync(user);

            return new AuthResult
            {
                Succeeded = true,
                Data = new AuthResponseDto { AccessToken = token, UserId = user.Id, UserName = user.UserName }
            };
        }
        else
        {
            return new AuthResult { Succeeded = false, Error = message };
        }
    }
}
