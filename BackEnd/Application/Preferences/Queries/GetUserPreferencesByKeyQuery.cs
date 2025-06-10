using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Application.Common.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Domain.Exceptions;
using LawyerProject.Application.BasicInfo.PaymentMethodOptions;
using LawyerProject.Application.CaseFinancials.CasePaymentAgreements;

namespace LawyerProject.Application.Preferences.Queries
{
    [Authorize]
    public record GetUserPreferencesByKeyQuery(string Key) : IRequest<string>;

    public class GetUserPreferencesByKeyQueryHandler : IRequestHandler<GetUserPreferencesByKeyQuery, string>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUser _currentUser;
        public GetUserPreferencesByKeyQueryHandler(IApplicationDbContext context,IUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<string> Handle(GetUserPreferencesByKeyQuery request,CancellationToken cancellationToken)
        {
            var userId = _currentUser.Id;     
            var regionId = _currentUser.RegionId;
            var result = await _context.PreferenceKeys.AsNoTracking()
            .Where(k => k.Name == request.Key).Select(k => new
            {
                Default = k.DefaultValue,
                UserValue = k.UserPreferences.Where(c => !c.IsDeleted && c.RegionId == regionId &&  c.UserId == userId )
                .Select(c => c.Value).FirstOrDefault()
            }).SingleOrDefaultAsync(cancellationToken);
            if (result == null) return string.Empty;
            return string.IsNullOrEmpty(result.UserValue) ? result.Default : result.UserValue;
        }
    }
}
