using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Domain.Entities;
using MediatR;

namespace LawyerProject.Application.BasicInfo.CaseTypes.Commands
{
    [Authorize]
    // Command to create a new CaseType with the name provided
    public record CreateCaseTypeCommand : IRequest<int>
    {
        public required string Name { get; init; }
    }

    public class CreateCaseTypeCommandHandler : IRequestHandler<CreateCaseTypeCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUser _user;

        public CreateCaseTypeCommandHandler(IApplicationDbContext context, IUser user)
        {
            _context = context;
            _user = user;
        }

        // Handle the CreateCaseTypeCommand, create the new CaseType, and save to the database
        public async Task<int> Handle(CreateCaseTypeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _user.Id;
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    throw new ArgumentException("Name cannot be empty or null.");
                }

                var entity = new LawyerProject.Domain.Entities.CaseType
                {
                    Name = request.Name,
                    RegionId = _user.RegionId
                };

                _context.CaseTypes.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);

                return entity.Id;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while creating the case type. Details: {ex.Message}", ex);
            }
        }
    }
}
