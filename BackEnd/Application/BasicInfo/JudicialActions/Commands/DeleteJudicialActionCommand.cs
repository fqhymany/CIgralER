using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Domain.Entities;
using MediatR;

namespace LawyerProject.Application.BasicInfo.JudicialActions.Commands
{
    [Authorize]
    [RequiresCaseAccess("CaseId")]
    public record DeleteJudicialActionCommand : IRequest<bool>
    {
        public required int CaseId { get; init; }
        public int Id { get; init; }
    }

    public class DeleteJudicialActionCommandHandler : IRequestHandler<DeleteJudicialActionCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public DeleteJudicialActionCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteJudicialActionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var judicialActionDeleted = await EntityDeletionHelper.SoftDeleteEntityAsync(_context, "JudicialActions", request.Id, cancellationToken);

                if (!judicialActionDeleted)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while deleting the JudicialActions. Details: {ex.Message}", ex);
            }
        }
    }
}
