using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Domain.Entities;
using MediatR;

namespace LawyerProject.Application.BasicInfo.CourtTypes.Commands
{
    [Authorize]
    // Command to create a new CourtType with the name provided
    public record CreateCourtTypeCommand : IRequest<int>
    {
        public required string Title { get; init; }
    }

    public class CreateCourtTypeCommandHandler : IRequestHandler<CreateCourtTypeCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUser _user;

        public CreateCourtTypeCommandHandler(IApplicationDbContext context, IUser user)
        {
            _context = context;
            _user = user;
        }

        // Handle the CreateCourtTypeCommand, create the new CourtType, and save to the database
        public async Task<int> Handle(CreateCourtTypeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _user.Id;
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    throw new ArgumentException("Title cannot be empty or null.");
                }

                var entity = new LawyerProject.Domain.Entities.CourtType
                {
                    Title = request.Title,
                    RegionId = _user.RegionId
                };

                _context.CourtTypes.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);

                return entity.Id;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while creating the court type. Details: {ex.Message}", ex);
            }
        }
    }
}
