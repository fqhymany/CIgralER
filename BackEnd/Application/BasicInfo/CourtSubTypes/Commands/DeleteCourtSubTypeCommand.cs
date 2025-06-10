using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using MediatR;
using System;
using LawyerProject.Application.Common.Security;

namespace LawyerProject.Application.BasicInfo.CourtSubTypes.Commands
{
    [Authorize]
    // Command to soft delete an existing court subtype by its Id
    public record DeleteCourtSubTypeCommand : IRequest<bool>
    {
        public int Id { get; init; }
    }

    public class DeleteCourtSubTypeCommandHandler : IRequestHandler<DeleteCourtSubTypeCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public DeleteCourtSubTypeCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        // Handle the DeleteCourtSubTypeCommand, delete the CourtSubType from the database
        public async Task<bool> Handle(DeleteCourtSubTypeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var isCourtSubTypeDeleted = await EntityDeletionHelper.SoftDeleteEntityAsync(_context, "CourtSubtypes", request.Id, cancellationToken);

                if (!isCourtSubTypeDeleted)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while deleting the courtSubType. Details: {ex.Message}", ex);
            }
        }
    }
}
