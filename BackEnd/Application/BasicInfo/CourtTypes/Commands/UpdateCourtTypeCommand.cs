using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;

namespace LawyerProject.Application.BasicInfo.CourtTypes.Commands
{
    [Authorize]
    // Command to update an existing CourtType by its Id and new Title
    public record UpdateCourtTypeCommand : IRequest<int>
    {
        public int Id { get; init; }
        public required string Title { get; init; }
    }

    public class UpdateCourtTypeCommandHandler : IRequestHandler<UpdateCourtTypeCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public UpdateCourtTypeCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        // Handle the UpdateCourtTypeCommand, update the CourtType, and save to the database
        public async Task<int> Handle(UpdateCourtTypeCommand request, CancellationToken cancellationToken)
        {
            var courtType = await _context.CourtTypes.FindAsync(request.Id);

            if (courtType == null)
            {
                return 0;
            }

            courtType.Title = request.Title;

            await _context.SaveChangesAsync(cancellationToken);

            return courtType.Id;
        }
    }
}
