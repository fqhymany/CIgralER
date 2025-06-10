using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Models;
using LawyerProject.Application.RegionAdmin.Staff.Commands;
using MediatR;

namespace LawyerProject.Application.RegionAdmin.Staff.Commands;
public class RemoveStaffCommandHandler
    : IRequestHandler<RemoveStaffCommand, Result>
{
    private readonly IIdentityService _identity;
    private readonly IApplicationDbContext _context;
    public RemoveStaffCommandHandler(IIdentityService identity, IApplicationDbContext context)
    {
        _identity = identity;
        _context = context;
    }

    public async Task<Result> Handle(
        RemoveStaffCommand request,
        CancellationToken cancellationToken)
    {
        // حذف تمام نقش‌های کاربر در آن ریجن
        

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw new NotFoundException("User  not found.", "User  not found.");

        try
        {
            user.IsDelete = true;
            await _context.SaveChangesAsync(cancellationToken);
            var result = await _identity.RemoveAllRolesFromRegionAsync(request.UserId);
            return result;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"An error occurred while deleting user. Details: {ex.Message}", ex);
        }
    }
}
