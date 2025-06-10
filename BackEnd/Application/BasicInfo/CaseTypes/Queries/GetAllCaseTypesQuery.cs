using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Application.BasicInfo.CaseTypes;
using LawyerProject.Application.BasicInfo.CaseStatuses;

namespace LawyerProject.Application.BasicInfo.CaseType.Queries.GetAllCaseTypes
{
    [Authorize]
    // The query to fetch all CaseTypes, requiring authorization to execute
    public record GetAllCaseTypesQuery(bool IncludeSystemRecords = true) : IRequest<List<CaseTypeDto>>;

    public class GetAllCaseTypesQueryHandler : IRequestHandler<GetAllCaseTypesQuery, List<CaseTypeDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUser _user;

        public GetAllCaseTypesQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user)
        {
            _context = context;
            _mapper = mapper;
            _user = user;
        }

        // Handle the query and return the list of CaseTypes
        public async Task<List<CaseTypeDto>> Handle(GetAllCaseTypesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.CaseTypes.AsNoTracking().Where(ct => ct.IsDeleted == false);
            if (request.IncludeSystemRecords)
            {
                query = query.Where(ct => ct.RegionId == 0 || ct.RegionId == _user.RegionId);
            }
            else
            {
                query = query.Where(ct => ct.RegionId == _user.RegionId);
            }

            var caseTypes = await query
                .OrderBy(cs => cs.Id)
                .ToListAsync(cancellationToken);
            return _mapper.Map<List<CaseTypeDto>>(caseTypes);
            //var caseTypes = await _context.CaseTypes
            //    .AsNoTracking()
            //    .Where(ct => ct.IsDeleted == false)
            //    .OrderBy(ct => ct.Id)
            //    .ToListAsync(cancellationToken);

            //return _mapper.Map<List<CaseTypeDto>>(caseTypes);
        }
    }
}
