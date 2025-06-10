using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Application.Common.Utils;
using LawyerProject.Domain.Entities;
using MediatR;

namespace LawyerProject.Application.BasicInfo.JudicialDeadline.Commands
{
    [Authorize]
    [RequiresCaseAccess("CaseId")]
    public record CreateJudicialDeadlineCommand : IRequest<int>
    {
        public required int CaseId { get; init; }
        public string? DeadlineType { get; init; }
        public string? StartDate { get; init; }
        public string? EndDate { get; init; }
        public string? Title { get; init; }
    }

    public class CreateJudgeCommandHandler : IRequestHandler<CreateJudicialDeadlineCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUser _user;
        public CreateJudgeCommandHandler(IApplicationDbContext context, IUser user)
        {
            _context = context;
            _user = user;
        }

        public async Task<int> Handle(CreateJudicialDeadlineCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var entity = new Domain.Entities.JudicialDeadline
                {
                    CaseId = request.CaseId,
                    DeadlineType = request.DeadlineType,
                    Title = request.Title,
                    RegionId = _user.RegionId
                    
                };
                if (!string.IsNullOrEmpty(request.StartDate)) entity.StartDate = DateUtils.ConvertPersianDateTimeToGregorian(request.StartDate) ?? null;
                if (!string.IsNullOrEmpty(request.EndDate)) entity.EndDate = DateUtils.ConvertPersianDateTimeToGregorian(request.EndDate) ?? null;
                _context.JudicialDeadlines.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);

                return entity.Id;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while creating the JudicialDeadline. Details: {ex.Message}", ex);
            }
        }
    }
}
