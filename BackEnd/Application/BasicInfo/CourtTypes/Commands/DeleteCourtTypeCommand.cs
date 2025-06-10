using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using MediatR;
using System;
using LawyerProject.Application.Common.Security;

namespace LawyerProject.Application.BasicInfo.CourtTypes.Commands
{
    [Authorize]
    // Command to soft delete an existing court type by its Id
    public record DeleteCourtTypeCommand : IRequest<bool>
    {
        public int Id { get; init; }
    }

    public class DeleteCourtTypeCommandHandler : IRequestHandler<DeleteCourtTypeCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public DeleteCourtTypeCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        // Handle the DeleteCourtTypeCommand, delete the CourtType from the database
        public async Task<bool> Handle(DeleteCourtTypeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var isCourtTypeDeleted = await EntityDeletionHelper.SoftDeleteEntityAsync(_context, "CourtTypes", request.Id, cancellationToken);

                if (!isCourtTypeDeleted)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while deleting the courtType. Details: {ex.Message}", ex);
            }
        }
    }
}
