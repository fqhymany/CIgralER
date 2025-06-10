using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Application.Common.Utils;
using LawyerProject.Domain.Entities;


namespace LawyerProject.Application.BasicInfo.JudicialNotices.Commands
{
    [Authorize]
    [RequiresCaseAccess("CaseId")]
    public record UpdateJudicialNoticeCommand : IRequest<int>
    {
        public required int CaseId { get; init; }
        public int Id { get; init; }
        public string? NoticeSubject { get; init; }
        public string? IssuedDate { get; init; }
        public string? NoticeType { get; init; }
        public string? NoticeNumber { get; init; }
        public string? IssuingAuthority { get; init; }
        //public string? NoticeText { get; init; }

    }

    public class UpdateJudicialNoticeCommandHandler : IRequestHandler<UpdateJudicialNoticeCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public UpdateJudicialNoticeCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(UpdateJudicialNoticeCommand request, CancellationToken cancellationToken)
        {
            var JudicialNotices = await _context.JudicialNotices.FindAsync(request.Id);

            if (JudicialNotices == null)
            {
                return 0;
            }

            JudicialNotices.NoticeSubject = request.NoticeSubject;
            JudicialNotices.NoticeType = request.NoticeType;
            JudicialNotices.IssuedDate = !string.IsNullOrEmpty(request.IssuedDate) ? DateUtils.ConvertPersianDateToGregorian(request.IssuedDate) ?? null : null;
            JudicialNotices.NoticeNumber = request.NoticeNumber;
            JudicialNotices.IssuingAuthority = request.IssuingAuthority;
            //JudicialNotices.NoticeText = request.NoticeText;
            await _context.SaveChangesAsync(cancellationToken);
            return JudicialNotices.Id;
        }
    }
}
