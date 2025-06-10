using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Application.Common.Utils;

namespace LawyerProject.Application.BasicInfo.JudicialNotices.Queries.GetAllJudges
{
    [Authorize]
    [RequiresCaseAccess]
    public record GetJudicialNoticeByCaseIdQuery : IRequest<JudicialNoticeVm>
    {
        public int CaseId { get; init; }
    }

    public class GetJudicialNoticeByCaseIdQueryHandler : IRequestHandler<GetJudicialNoticeByCaseIdQuery, JudicialNoticeVm>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetJudicialNoticeByCaseIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<JudicialNoticeVm> Handle(GetJudicialNoticeByCaseIdQuery request, CancellationToken cancellationToken)
        {
            var judicialNotices = await _context.JudicialNotices
            .Where(hs => hs.IsDeleted == false && hs.CaseId == request.CaseId)
            .AsNoTracking()
            .OrderBy(hs => hs.Id)
            .Select(c => new JudicialNoticeDto
            {
                Id = c.Id,
                CaseId = c.CaseId,
                NoticeSubject = c.NoticeSubject,
                IssuedDate = c.IssuedDate.HasValue ? DateUtils.ToPersianDate(c.IssuedDate.Value): "نامشخص",
                NoticeType = c.NoticeType,
                NoticeNumber = c.NoticeNumber,
                IssuingAuthority = c.IssuingAuthority,
                //NoticeText = c.NoticeText
            })
            .ToListAsync(cancellationToken);
            return new JudicialNoticeVm { Notices = judicialNotices };
        }
    }
}
