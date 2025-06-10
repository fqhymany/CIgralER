using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Application.Cases.Queries.GetCases;
using LawyerProject.Domain.Entities;
using LawyerProject.Application.BasicInfo.Judges;

namespace LawyerProject.Application.BasicInfo.Judge.Queries.GetAllJudges
{
    [Authorize]
    // The query to fetch all Judges, requiring authorization to execute
    public record GetAllJudgesQuery : IRequest<List<JudgeDto>>;

    public class GetAllJudgesQueryHandler : IRequestHandler<GetAllJudgesQuery, List<JudgeDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUser _user;

        public GetAllJudgesQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user)
        {
            _context = context;
            _mapper = mapper;
            _user = user;
        }

        // Handle the query and return the list of Judges
        public async Task<List<JudgeDto>> Handle(GetAllJudgesQuery request, CancellationToken cancellationToken)
        {
            var judges = await _context.Judges
                .AsNoTracking()
                .Where(j => j.IsDeleted == false && _user.RegionId == j.RegionId)
                .OrderBy(j => j.Id)
            .ToListAsync(cancellationToken);

            return _mapper.Map<List<JudgeDto>>(judges);
        }
    }
}
