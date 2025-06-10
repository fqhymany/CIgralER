using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Application.Common.Utils;

namespace LawyerProject.Application.BasicInfo.JudicialDeadline.Queries.GetAllJudges
{
    [Authorize]
    [RequiresCaseAccess]
    public record GetJudicialDeadlineByCaseIdQuery : IRequest<JudicialDeadlineVm>
    {
        public int CaseId { get; init; }
    }

    public class GetJudicialDeadlineByCaseIdQueryHandler : IRequestHandler<GetJudicialDeadlineByCaseIdQuery, JudicialDeadlineVm>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUser _user;

        public GetJudicialDeadlineByCaseIdQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user)
        {
            _context = context;
            _mapper = mapper;
            _user = user;
        }

        public async Task<JudicialDeadlineVm> Handle(GetJudicialDeadlineByCaseIdQuery request, CancellationToken cancellationToken)
        {
            var judicialDeadlines = await _context.JudicialDeadlines
            .Where(hs => hs.IsDeleted == false && hs.CaseId == request.CaseId && hs.RegionId==_user.RegionId)
            .AsNoTracking()
            .OrderBy(hs => hs.Id)
            .Select(c => new JudicialDeadlineDto
            {
                Id = c.Id,
                CaseId = c.CaseId,
                Title = c.Title,
                DeadlineType = c.DeadlineType,
                StartDate = c.StartDate.HasValue ? DateUtils.ToPersianDateTime(c.StartDate.Value) : "نامشخص",
                EndDate = c.EndDate.HasValue ? DateUtils.ToPersianDateTime(c.EndDate.Value) : "نامشخص"
            })
            .ToListAsync(cancellationToken);
            return new JudicialDeadlineVm { Deadlines = judicialDeadlines };
        }
    }
}
