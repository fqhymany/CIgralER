using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Application.Common.Utils;

namespace LawyerProject.Application.BasicInfo.JudicialDecisions.Queries.GetAllJudges
{
    [Authorize]
    [RequiresCaseAccess]
    public record GetJudicialDecisionByCaseIdQuery : IRequest<JudicialDecisionVm>
    {
        public int CaseId { get; init; }
    }

    public class GetJudicialDecisionByCaseIdQueryHandler : IRequestHandler<GetJudicialDecisionByCaseIdQuery, JudicialDecisionVm>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetJudicialDecisionByCaseIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<JudicialDecisionVm> Handle(GetJudicialDecisionByCaseIdQuery request, CancellationToken cancellationToken)
        {
            var judicialDeadlines = await _context.JudicialDecisions
            .Where(hs => hs.IsDeleted == false && hs.CaseId == request.CaseId)
            .AsNoTracking()
            .OrderBy(hs => hs.Id)
            .Select(c => new JudicialDecisionDto
            {
                Id = c.Id,
                CaseId = c.CaseId,
                Title = c.Title,
                IssuedDate = c.IssuedDate.HasValue ? DateUtils.ToPersianDate(c.IssuedDate.Value): "نامشخص",
                DecisionType = c.DecisionType,
                DecisionNumber = c.DecisionNumber,
                DecisionOutcome = c.DecisionOutcome,
                IssuingAuthority = c.IssuingAuthority,
                //DecisionText = c.DecisionText
            })
            .ToListAsync(cancellationToken);
            return new JudicialDecisionVm { Decisions = judicialDeadlines };
        }
    }
}
