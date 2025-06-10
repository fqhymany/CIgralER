using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;

namespace LawyerProject.Application.BasicInfo.CasePredefinedSubjects.Queries
{
    [Authorize]
    // The query to fetch all CasePredefinedSubjects, requiring authorization to execute
    public record GetAllCasePredefinedSubjectsQuery : IRequest<List<PredefinedSubjectDto>>;

    public class GetAllCasePredefinedSubjectsQueryHandler : IRequestHandler<GetAllCasePredefinedSubjectsQuery, List<PredefinedSubjectDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUser _user;

        public GetAllCasePredefinedSubjectsQueryHandler(IApplicationDbContext context, IMapper mapper,IUser user)
        {
            _context = context;
            _mapper = mapper;
            _user = user;
        }

        // Handle the query and return the list of CasePredefinedSubjects
        public async Task<List<PredefinedSubjectDto>> Handle(GetAllCasePredefinedSubjectsQuery request, CancellationToken cancellationToken)
        {
            var casePredefinedSubjects = await _context.PredefinedSubjects
                .AsNoTracking()
                .Where(ps => ps.IsDeleted == false && _user.RegionId == ps.RegionId)
                .OrderBy(ps => ps.Id)
                .Select(ps => new PredefinedSubjectDto
                {
                    Id = ps.Id,
                    Title = ps.Title,
                    CaseTypeId = ps.CaseTypeId ?? 0,
                })
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<PredefinedSubjectDto>>(casePredefinedSubjects);
        }
    }
}
