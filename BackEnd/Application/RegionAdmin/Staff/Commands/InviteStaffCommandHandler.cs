using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Auth.Queries.GetUser;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Models;
using LawyerProject.Application.Common.Results;
using LawyerProject.Application.RegionAdmin.Staff.Commands;
using LawyerProject.Domain.Entities;
using MediatR;

namespace LawyerProject.Application.RegionAdmin.Staff.Commands;
public class InviteStaffCommandHandler
    : IRequestHandler<InviteStaffCommand, AuthResult>
{
    private readonly IIdentityService _identity;
    private readonly ICredentialService _credentialGen;
    private readonly IUser _user;
    private readonly ISmsService _smsService;
    public InviteStaffCommandHandler(IIdentityService identity, ICredentialService credentialGen, IUser user, ISmsService smsService)
    {
        _identity = identity;
        _credentialGen = credentialGen;
        _user = user;
        this._smsService = smsService;
    }

    public async Task<AuthResult> Handle(
        InviteStaffCommand request,
        CancellationToken cancellationToken)
    {
        // چک می‌کنیم کاربر وجود نداشته باشد
        var (exists, message) = await _identity
            .CheckUserExistsAsync(null, null, null, request.PhoneNumber);
        if (exists)
            return new AuthResult { Succeeded = false, Error = message };

        var username = _credentialGen.GenerateTemporaryEmail();

        var password = _credentialGen.GenerateStrongPassword();

        var phoneNumber = request.PhoneNumber;

        var fullName = request.FirstName;

        // ایجاد کاربر
        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = username,
            UserName = username,
            PhoneNumber = request.PhoneNumber,
            NationalCode = "0",
            IsActive = false
        };
        var (createResult, userId) = await _identity
            .CreateUserAsync(user, password);
        if (!createResult.Succeeded)
            return new AuthResult { Succeeded = false, Error = "خطا در دعوت کاربر" };

        // نسبت دادن کاربر به ریجن و نقش
        await _identity.AddToRegionAsync(userId, _user.RegionId);

        var dto = new AuthResponseDto
        {
            UserId = userId,
        };

        _smsService.SendInviteSmsAsync(phoneNumber, fullName, username, password);
        return new AuthResult { Succeeded = true, Data = dto };
    }
}
