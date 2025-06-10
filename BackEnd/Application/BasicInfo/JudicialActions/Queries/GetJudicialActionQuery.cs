using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Application.Common.Utils;

namespace LawyerProject.Application.BasicInfo.JudicialActions.Queries
{
    [Authorize]
    [RequiresCaseAccess]
    public record GetJudicialActionByCaseIdQuery : IRequest<JudicialActionVm>
    {
        public int CaseId { get; init; }
    }

    public class GetJudicialActionByCaseIdQueryHandler : IRequestHandler<GetJudicialActionByCaseIdQuery, JudicialActionVm>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetJudicialActionByCaseIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<JudicialActionVm> Handle(GetJudicialActionByCaseIdQuery request, CancellationToken cancellationToken)
        {
            var judicialActions = await _context.JudicialActions
            .Where(hs => hs.IsDeleted == false && hs.CaseId == request.CaseId)
            .AsNoTracking()
            .OrderBy(hs => hs.Id)
            .Select(c => new JudicialActionDto
            {
                Id = c.Id,
                CaseId = c.CaseId,
                Description = c.Description,
            })
            .ToListAsync(cancellationToken);
            return new JudicialActionVm { Actions = judicialActions };
        }
    }
}
