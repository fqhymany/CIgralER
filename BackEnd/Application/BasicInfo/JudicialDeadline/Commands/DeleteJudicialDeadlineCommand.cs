using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Domain.Entities;
using MediatR;

namespace LawyerProject.Application.BasicInfo.JudicialDeadline.Commands
{
    [Authorize]
    [RequiresCaseAccess("CaseId")]
    public record DeleteJudicialDeadlineCommand : IRequest<bool>
    {
        public required int CaseId { get; init; }
        public int Id { get; init; }
    }

    public class DeleteJudicialDeadlineCommandHandler : IRequestHandler<DeleteJudicialDeadlineCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public DeleteJudicialDeadlineCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteJudicialDeadlineCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var judicialDeadlineDeleted = await EntityDeletionHelper.SoftDeleteEntityAsync(_context, "JudicialDeadlines", request.Id, cancellationToken);

                if (!judicialDeadlineDeleted)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while deleting the JudicialDeadlines. Details: {ex.Message}", ex);
            }
        }
    }
}
