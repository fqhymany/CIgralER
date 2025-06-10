using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Application.Common.Utils;
using LawyerProject.Domain.Entities;
using MediatR;

namespace LawyerProject.Application.BasicInfo.JudicialDecisions.Commands
{
    [Authorize]
    [RequiresCaseAccess("CaseId")]
    public record CreateJudicialDecisionCommand : IRequest<int>
    {
        public required int CaseId { get; init; }
        public string? Title { get; init; }
        public string? IssuedDate { get; init; }
        public string? DecisionType { get; init; }
        public string? DecisionNumber { get; init; }
        public string? DecisionOutcome { get; init; }
        public string? IssuingAuthority { get; init; }
        //public string? DecisionText { get; init; }
    }

    public class CreateJudicialDecisionCommandHandler : IRequestHandler<CreateJudicialDecisionCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateJudicialDecisionCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateJudicialDecisionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var entity = new JudicialDecision
                {
                    CaseId = request.CaseId,
                    DecisionType = request.DecisionType,
                    Title = request.Title,
                    DecisionNumber = request.DecisionNumber,
                    DecisionOutcome = request.DecisionOutcome,
                    IssuingAuthority = request.IssuingAuthority,
                    //DecisionText = request.DecisionText,
                };
                if (!string.IsNullOrEmpty(request.IssuedDate)) entity.IssuedDate = DateUtils.ConvertPersianDateToGregorian(request.IssuedDate) ?? null;
                _context.JudicialDecisions.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);

                return entity.Id;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while creating the JudicialDecision. Details: {ex.Message}", ex);
            }
        }
    }
}
