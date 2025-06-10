using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Domain.Entities;
using MediatR;

namespace LawyerProject.Application.BasicInfo.CourtSubTypes.Commands
{
    [Authorize]
    // Command to create a new  Court Sub Type with the title provided
    public record CreateCourtSubTypeCommand : IRequest<int>
    {
        public required string Title { get; init; }
        public int CourtTypeId { get; init; }
    }

    public class CreateCourtSubTypeCommandHandler : IRequestHandler<CreateCourtSubTypeCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUser _user;

        public CreateCourtSubTypeCommandHandler(IApplicationDbContext context, IUser user)
        {
            _context = context;
            _user = user;
        }

        // Handle the CreateCourtSubTypeCommand, create the new  Court Sub Type, and save to the database
        public async Task<int> Handle(CreateCourtSubTypeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    throw new ArgumentException("Title cannot be empty or null.");
                }

                // find court type by its id
                var courtType = await _context.CourtTypes
                    .FirstOrDefaultAsync(c => c.Id == request.CourtTypeId, cancellationToken);

                if (courtType == null)
                {
                    throw new ApplicationException("The specified CourtTypeId does not exist.");
                }

                var entity = new CourtSubtype
                {
                    Title = request.Title,
                    CourtType = courtType,
                    RegionId = _user.RegionId
                };

                _context.CourtSubtypes.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);

                return entity.Id;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while creating the court sub type. Details: {ex.Message}", ex);
            }
        }

    }
}
