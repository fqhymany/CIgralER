using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Application.Common.Utils;
using LawyerProject.Domain.Entities;


namespace LawyerProject.Application.BasicInfo.JudicialActions.Commands
{
    [Authorize]
    [RequiresCaseAccess("CaseId")]
    public record UpdateJudicialActionCommand : IRequest<int>
    {
        public required int CaseId { get; init; }
        public int Id { get; init; }
        public string? Description { get; init; }

    }

    public class UpdateJudicialActionCommandHandler : IRequestHandler<UpdateJudicialActionCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public UpdateJudicialActionCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(UpdateJudicialActionCommand request, CancellationToken cancellationToken)
        {
            var JudicialActions = await _context.JudicialActions.FindAsync(request.Id);

            if (JudicialActions == null)
            {
                return 0;
            }

            JudicialActions.Description = request.Description;
            await _context.SaveChangesAsync(cancellationToken);
            return JudicialActions.Id;
        }
    }
}
