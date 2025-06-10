using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Domain.Entities;
using MediatR;

namespace LawyerProject.Application.BasicInfo.JudicialNotices.Commands
{
    [Authorize]
    [RequiresCaseAccess("CaseId")]
    public record DeleteJudicialNoticeCommand : IRequest<bool>
    {
        public required int CaseId { get; init; }
        public int Id { get; init; }
    }

    public class DeleteJudicialNoticeCommandHandler : IRequestHandler<DeleteJudicialNoticeCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public DeleteJudicialNoticeCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteJudicialNoticeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var judicialNoticeDeleted = await EntityDeletionHelper.SoftDeleteEntityAsync(_context, "JudicialNotices", request.Id, cancellationToken);

                if (!judicialNoticeDeleted)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while deleting the JudicialNotices. Details: {ex.Message}", ex);
            }
        }
    }
}
