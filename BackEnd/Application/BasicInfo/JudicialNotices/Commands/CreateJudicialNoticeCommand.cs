using System.Threading;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Security;
using LawyerProject.Application.Common.Utils;
using LawyerProject.Domain.Entities;
using MediatR;

namespace LawyerProject.Application.BasicInfo.JudicialNotices.Commands
{
    [Authorize]
    [RequiresCaseAccess]
    public record CreateJudicialNoticeCommand : IRequest<int>
    {
        public required int CaseId { get; init; }
        public string? IssuedDate { get; init; }
        public string? NoticeType { get; init; }
        public string? NoticeNumber { get; init; }
        public string? NoticeSubject { get; init; }
        public string? IssuingAuthority { get; init; }
        //public string? NoticeText { get; init; }
    }

    public class CreateJudicialNoticeCommandHandler : IRequestHandler<CreateJudicialNoticeCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateJudicialNoticeCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateJudicialNoticeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var entity = new JudicialNotice
                {
                    CaseId = request.CaseId,
                    NoticeType = request.NoticeType,
                    NoticeSubject = request.NoticeSubject,
                    NoticeNumber = request.NoticeNumber,
                    IssuingAuthority = request.IssuingAuthority,
                    //NoticeText = request.NoticeText,
                };
                if (!string.IsNullOrEmpty(request.IssuedDate)) entity.IssuedDate = DateUtils.ConvertPersianDateToGregorian(request.IssuedDate) ?? null;
                _context.JudicialNotices.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);

                return entity.Id;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while creating the JudicialNotice. Details: {ex.Message}", ex);
            }
        }
    }
}
