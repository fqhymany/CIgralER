using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Domain.Entities;
using MediatR;

namespace LawyerProject.Application.BasicInfo.ClientRolesInCase.Commands
{
    [Authorize]
    // Command to create a new Client Role In Case with the title provided
    public record CreateClientRoleInCaseCommand : IRequest<int>
    {
        public required string Title { get; init; }
        public int CaseTypeId { get; init; }
    }

    public class CreateClientRoleInCaseCommandHandler : IRequestHandler<CreateClientRoleInCaseCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUser _user;

        public CreateClientRoleInCaseCommandHandler(IApplicationDbContext context, IUser user)
        {
            _context = context;
            _user = user;
        }

        // Handle the CreateClientRoleInCaseCommand, create the new Client Role In Case, and save to the database
        public async Task<int> Handle(CreateClientRoleInCaseCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    throw new ArgumentException("Title cannot be empty or null.");
                }

                var caseTypeExists = _context.CaseTypes.Any(c => c.Id == request.CaseTypeId && (c.RegionId == 0 || c.RegionId == _user.RegionId));
                if (!caseTypeExists)
                {
                    throw new ApplicationException("The specified CaseTypeId does not exist.");
                }

                var entity = new LawyerProject.Domain.Entities.ClientRoleInCase
                {
                    Title = request.Title,
                    CaseTypeId = request.CaseTypeId,
                    RegionId = _user.RegionId
                };

                _context.ClientRolesInCase.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);

                return entity.Id;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while creating the client role in case. Details: {ex.Message}", ex);
            }
        }
    }
}
