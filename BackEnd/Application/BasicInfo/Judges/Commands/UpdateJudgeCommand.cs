using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Domain.Entities;


namespace LawyerProject.Application.BasicInfo.Judges.Commands
{
    [Authorize]
    // Command to update an existing Judge by its Id and new first name and last name
    public record UpdateJudgeCommand : IRequest<int>
    {
        public int Id { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }

    }

    public class UpdateJudgeCommandHandler : IRequestHandler<UpdateJudgeCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public UpdateJudgeCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        // Handle the UpdateJudgeCommand, update the Judge, and save to the database
        public async Task<int> Handle(UpdateJudgeCommand request, CancellationToken cancellationToken)
        {
            var judge = await _context.Judges.FindAsync(request.Id);

            if (judge == null)
            {
                return 0;
            }

            judge.FirstName = request.FirstName;
            judge.LastName = request.LastName;

            await _context.SaveChangesAsync(cancellationToken);

            return judge.Id;
        }
    }
}
