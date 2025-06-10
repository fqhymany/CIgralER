using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Domain.Entities;
using MediatR;

namespace LawyerProject.Application.BasicInfo.JudicialDecisions.Commands
{
    [Authorize]
    [RequiresCaseAccess("CaseId")]
    public record DeleteJudicialDecisionCommand : IRequest<bool>
    {
        public required int CaseId { get; init; }
        public int Id { get; init; }
    }

    public class DeleteJudicialDecisionCommandHandler : IRequestHandler<DeleteJudicialDecisionCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public DeleteJudicialDecisionCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteJudicialDecisionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var judicialDecisionDeleted = await EntityDeletionHelper.SoftDeleteEntityAsync(_context, "JudicialDecisions", request.Id, cancellationToken);

                if (!judicialDecisionDeleted)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while deleting the JudicialDecisions. Details: {ex.Message}", ex);
            }
        }
    }
}
