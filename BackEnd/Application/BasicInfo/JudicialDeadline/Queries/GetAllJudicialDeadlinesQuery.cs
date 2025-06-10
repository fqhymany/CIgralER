using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Application.Cases.Queries.GetCases;
using LawyerProject.Domain.Entities;
using LawyerProject.Application.BasicInfo.Judges;
using LawyerProject.Application.BasicInfo.JudicialDeadline.Queries;
using LawyerProject.Application.BasicInfo.JudicialDeadline;
using LawyerProject.Application.Common.Utils;

namespace LawyerProject.Application.BasicInfo.Judge.Queries.GetAllJudges
{
    // The query to fetch all Deadlines, requiring authorization to execute
    [Authorize]
    public record GetAllJudicialDeadlinesQuery : IRequest<JudicialDeadlineWithCaseNoVm>;

    public class GetAllJudicialDeadlinesQueryHandler : IRequestHandler<GetAllJudicialDeadlinesQuery, JudicialDeadlineWithCaseNoVm>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUser _user;

        public GetAllJudicialDeadlinesQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user)
        {
            _context = context;
            _mapper = mapper;
            _user = user;
        }

        public async Task<JudicialDeadlineWithCaseNoVm> Handle(GetAllJudicialDeadlinesQuery request, CancellationToken cancellationToken)
        {
            var judicialDeadlines = await _context.JudicialDeadlines
                .AsNoTracking()
                .Where(jd => jd.IsDeleted == false && jd.EndDate.HasValue && jd.EndDate.Value >= DateTime.Now && jd.RegionId==_user.RegionId)
                .OrderBy(jd => jd.EndDate)
                .Join(_context.Cases,
                      jd => jd.CaseId,
                      cs => cs.Id,
                      (jd, cs) => new JudicialDeadlineWithCaseNoDto
                      {
                          Id = jd.Id,
                          CaseId = jd.CaseId,
                          Title = jd.Title,
                          DeadlineType = jd.DeadlineType,
                          StartDate = jd.StartDate.HasValue ? DateUtils.ToPersianDateTime(jd.StartDate.Value) : "نامشخص",
                          EndDate = jd.EndDate.HasValue ? DateUtils.ToPersianDateTime(jd.EndDate.Value) : "نامشخص",
                          CaseNumber = cs.CaseNumber
                      })
                .ToListAsync(cancellationToken);

            return new JudicialDeadlineWithCaseNoVm { Deadlines = judicialDeadlines };
        }
    }
}
