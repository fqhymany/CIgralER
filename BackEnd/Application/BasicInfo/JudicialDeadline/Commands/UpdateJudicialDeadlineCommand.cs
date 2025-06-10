using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Application.Common.Utils;
using LawyerProject.Domain.Entities;


namespace LawyerProject.Application.BasicInfo.JudicialDeadline.Commands
{
    [Authorize]
    [RequiresCaseAccess("CaseId")]
    public record UpdateJudicialDeadlineCommand : IRequest<int>
    {
        public required int CaseId { get; init; }
        public int Id { get; init; }
        public string? DeadlineType { get; init; }
        public string? StartDate { get; init; }
        public string? EndDate { get; init; }
        public string? Title { get; init; }

    }

    public class UpdateJudicialDeadlineCommandHandler : IRequestHandler<UpdateJudicialDeadlineCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public UpdateJudicialDeadlineCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(UpdateJudicialDeadlineCommand request, CancellationToken cancellationToken)
        {
            var JudicialDeadlines = await _context.JudicialDeadlines.FindAsync(request.Id);

            if (JudicialDeadlines == null)
            {   
                return 0;
            }
            
            JudicialDeadlines.Title = request.Title;
            JudicialDeadlines.DeadlineType = request.DeadlineType;
            JudicialDeadlines.StartDate = !string.IsNullOrEmpty(request.StartDate) ? DateUtils.ConvertPersianDateTimeToGregorian(request.StartDate) ?? null : null;
            JudicialDeadlines.EndDate = !string.IsNullOrEmpty(request.EndDate) ? DateUtils.ConvertPersianDateTimeToGregorian(request.EndDate) ?? null : null;
            await _context.SaveChangesAsync(cancellationToken);
            return JudicialDeadlines.Id;
        }
    }
}
