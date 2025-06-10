using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Domain.Entities;
using MediatR;

namespace LawyerProject.Application.BasicInfo.CaseStatuses.Commands
{
    [Authorize]
    // Command to create a new CaseStatus with the name provided
    public record CreateCaseStatusCommand : IRequest<int>
    {
        public required string Name { get; init; }
    }

    public class CreateCaseStatusCommandHandler : IRequestHandler<CreateCaseStatusCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUser _user;

        public CreateCaseStatusCommandHandler(IApplicationDbContext context , IUser User)
        {
            _context = context;
            _user = User;
        }

        // Handle the CreateCaseStatusCommand, create the new CaseStatus, and save to the database
        public async Task<int> Handle(CreateCaseStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    throw new ArgumentException("Name cannot be empty or null.");
                }

                var entity = new LawyerProject.Domain.Entities.CaseStatus
                {
                    Name = request.Name,
                    RegionId = _user.RegionId,
                };

                _context.CaseStatuss.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);

                return entity.Id;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while creating the case Status. Details: {ex.Message}", ex);
            }
        }
    }
}
