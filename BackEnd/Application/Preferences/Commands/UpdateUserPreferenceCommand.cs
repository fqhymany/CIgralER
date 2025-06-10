using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Domain.Entities;
using LawyerProject.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace LawyerProject.Application.Preferences.Commands
{
    [Authorize]
    public record UpdateUserPreferenceCommand(string Key, string Value) : IRequest<int>;

    public class UpdateUserPreferenceCommandHandler : IRequestHandler<UpdateUserPreferenceCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUser _currentUser;

        public UpdateUserPreferenceCommandHandler(IApplicationDbContext context,IUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<int> Handle(UpdateUserPreferenceCommand request, CancellationToken cancellationToken)
        {
            var prefKey = await _context.PreferenceKeys.SingleOrDefaultAsync(k => k.Name == request.Key, cancellationToken);
            if (prefKey == null) throw new ValidationException("تنظیمات مورد نظر یافت نشد");
            var userId = _currentUser.Id;
            var regionId = _currentUser.RegionId;
            if (userId == null) throw new ValidationException("کاربر مورد نظر یافت نشد");
            var userPref = await _context.UserPreferences.SingleOrDefaultAsync(c => !c.IsDeleted && c.UserId == userId &&
            c.RegionId == regionId && c.Key == request.Key, cancellationToken);
            if (userPref != null)
            {
                userPref.Value = request.Value;
                _context.UserPreferences.Update(userPref);
            }
            else
            {
                userPref = new UserPreference
                {
                    UserId = userId,
                    RegionId = regionId,
                    Key = request.Key,
                    PreferenceKey = prefKey,
                    Value = request.Value
                };
                await _context.UserPreferences.AddAsync(userPref, cancellationToken);
            }
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
