using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Application.Common.Utils;
using LawyerProject.Domain.Entities;


namespace LawyerProject.Application.BasicInfo.JudicialDecisions.Commands
{
    [Authorize]
    [RequiresCaseAccess("CaseId")]
    public record UpdateJudicialDecisionCommand : IRequest<int>
    {
        public required int CaseId { get; init; }
        public int Id { get; init; }
        public string? Title { get; init; }
        public string? IssuedDate { get; init; }
        public string? DecisionType { get; init; }
        public string? DecisionNumber { get; init; }
        public string? DecisionOutcome { get; init; }
        public string? IssuingAuthority { get; init; }
        //public string? DecisionText { get; init; }

    }

    public class UpdateJudicialDecisionCommandHandler : IRequestHandler<UpdateJudicialDecisionCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public UpdateJudicialDecisionCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(UpdateJudicialDecisionCommand request, CancellationToken cancellationToken)
        {
            var JudicialDecisions = await _context.JudicialDecisions.FindAsync(request.Id);

            if (JudicialDecisions == null)
            {
                return 0;
            }

            JudicialDecisions.Title = request.Title;
            JudicialDecisions.DecisionType = request.DecisionType;
            JudicialDecisions.IssuedDate = !string.IsNullOrEmpty(request.IssuedDate) ? DateUtils.ConvertPersianDateToGregorian(request.IssuedDate) ?? null : null;
            JudicialDecisions.DecisionNumber = request.DecisionNumber;
            JudicialDecisions.DecisionOutcome = request.DecisionOutcome;
            JudicialDecisions.IssuingAuthority = request.IssuingAuthority;
            //JudicialDecisions.DecisionText = request.DecisionText;
            await _context.SaveChangesAsync(cancellationToken);
            return JudicialDecisions.Id;
        }
    }
}
