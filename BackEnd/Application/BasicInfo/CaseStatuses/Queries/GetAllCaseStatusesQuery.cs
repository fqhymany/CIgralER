using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Application.BasicInfo.CaseStatuses;

namespace LawyerProject.Application.BasicInfo.CaseType.Queries.GetAllCaseStatuses
{
    [Authorize]
    // The query to fetch all CaseStatuses, requiring authorization to execute
    public record GetAllCaseStatusesQuery : IRequest<List<CaseStatusDto>>;

    public class GetAllCaseStatusesQueryHandler : IRequestHandler<GetAllCaseStatusesQuery, List<CaseStatusDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUser _user;

        public GetAllCaseStatusesQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user)
        {
            _context = context;
            _mapper = mapper;
            _user = user;
        }

        // Handle the query and return the list of CaseStatuses
        public async Task<List<CaseStatusDto>> Handle(GetAllCaseStatusesQuery request, CancellationToken cancellationToken)
        {

            var caseStatuses = await _context.CaseStatuss
                .AsNoTracking()
                .Where(ct => ct.IsDeleted == false && (_user.RegionId == ct.RegionId))
                .OrderBy(cs => cs.Id)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<CaseStatusDto>>(caseStatuses);
        }
    }
}
