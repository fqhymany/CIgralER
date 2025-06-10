using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;

namespace LawyerProject.Application.BasicInfo.CourtSubTypes.Commands
{
    [Authorize]
    // Command to update an existing  Court Sub Type by its Id and new Title and CourtType
    public record UpdateCourtSubTypeCommand : IRequest<int>
    {
        public int Id { get; init; }
        public required string Title { get; init; }
        public int CourtTypeId { get; init; }

    }

    public class UpdateCourtSubTypeCommandHandler : IRequestHandler<UpdateCourtSubTypeCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public UpdateCourtSubTypeCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        // Handle the UpdateCourtSubTypeCommand, update the  Court Sub Type, and save to the database
        public async Task<int> Handle(UpdateCourtSubTypeCommand request, CancellationToken cancellationToken)
        {
            var courtSubType = await _context.CourtSubtypes.FindAsync(request.Id);

            if (courtSubType == null)
            {
                return 0;
            }
            var caseTypeExists = _context.CourtTypes.Any(c => c.Id == request.CourtTypeId);
            if (!caseTypeExists)
            {
                return 0;

            }
            courtSubType.Title = request.Title;
            courtSubType.CourtTypeId = request.CourtTypeId;

            await _context.SaveChangesAsync(cancellationToken);

            return courtSubType.Id;
        }
    }
}
