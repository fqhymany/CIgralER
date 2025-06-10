using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Domain.Entities;
using MediatR;

namespace LawyerProject.Application.BasicInfo.Judges.Commands
{
    [Authorize]
    // Command to create a new Judge with the name provided
    public record CreateJudgeCommand : IRequest<int>
    {
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
    }

    public class CreateJudgeCommandHandler : IRequestHandler<CreateJudgeCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUser _user;
        public CreateJudgeCommandHandler(IApplicationDbContext context, IUser user)
        {
            _context = context;
            _user = user;
        }

        // Handle the CreateJudgeCommand, create the new Judge, and save to the database
        public async Task<int> Handle(CreateJudgeCommand request, CancellationToken cancellationToken)
        {
           
            try
            {
                var userId = _user.Id;
                if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
                {
                    throw new ArgumentException("First name and last name cannot be empty or null.");
                }

                var entity = new LawyerProject.Domain.Entities.Judge
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    RegionId = _user.RegionId,
                };

                _context.Judges.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);

                return entity.Id;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while creating the judge. Details: {ex.Message}", ex);
            }
        }
    }
}
