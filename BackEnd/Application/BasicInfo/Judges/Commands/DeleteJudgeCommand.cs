using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using MediatR;
using System;
using LawyerProject.Application.Common.Security;

namespace LawyerProject.Application.BasicInfo.Judges.Commands
{
    [Authorize]
    // Command to soft delete an existing judge by its Id
    public record DeleteJudgeCommand : IRequest<bool>
    {
        public int Id { get; init; }
    }

    public class DeleteJudgeCommandHandler : IRequestHandler<DeleteJudgeCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public DeleteJudgeCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        // Handle the DeleteJudgeCommand, delete the Judge from the database
        public async Task<bool> Handle(DeleteJudgeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var isJudgeDeleted = await EntityDeletionHelper.SoftDeleteEntityAsync(_context, "Judges", request.Id, cancellationToken);

                if (!isJudgeDeleted)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while deleting the judge. Details: {ex.Message}", ex);
            }
        }
    }
}
