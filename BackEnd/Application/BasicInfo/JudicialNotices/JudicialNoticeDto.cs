using LawyerProject.Domain.Entities;

namespace LawyerProject.Application.BasicInfo.JudicialNotices
{
    public class JudicialNoticeDto
    {
        public int Id { get; set; }
        public required int CaseId { get; set; }
        public string? NoticeNumber { get; set; }
        public string? IssuedDate { get; set; }
        public string? NoticeType { get; set; }
        public string? NoticeSubject { get; set; }
        public string? IssuingAuthority { get; set; }
        //public string? DecisionText { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<JudicialNotice, JudicialNoticeDto>();
            }
        }
    }
    public class JudicialNoticeVm
    {
        public IReadOnlyCollection<JudicialNoticeDto> Notices { get; init; } = Array.Empty<JudicialNoticeDto>();
    }
}

