using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Results;
using LawyerProject.Application.Common.Utils;
using Microsoft.Extensions.Logging;

namespace LawyerProject.Application.Files.Queries;

public class GetCaseFilesQuery : IRequest<Result<List<FileDto>>>
{
    public Guid CaseId { get; set; }
    public string Area { get; set; } = string.Empty;
}

public class GetCaseFilesQueryHandler : IRequestHandler<GetCaseFilesQuery, Result<List<FileDto>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<GetCaseFilesQueryHandler> _logger;
    private readonly IUser _user;

    public GetCaseFilesQueryHandler(IApplicationDbContext dbContext, ILogger<GetCaseFilesQueryHandler> logger, IUser user)
    {
        _dbContext = dbContext;
        _logger = logger;
        _user = user;
    }

    public async Task<Result<List<FileDto>>> Handle(GetCaseFilesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var intCaseId = CastDataTypesUtils.ConvertGuidToInt(request.CaseId.ToString());
            var files = await _dbContext.EncryptedFileMetadata
                .Where(f => f.CaseId.ToString() == intCaseId.ToString() && f.RegionId == _user.RegionId && f.IsDeleted!=true)
                .OrderByDescending(f => f.UploadedDate)
                .Select(f => new FileDto
                {
                    FileId = f.Id,
                    FileName = f.FileName,
                    FileSize = f.FileSize,
                    UploadedDate = f.UploadedDate,
                    Area = f.RegionId.ToString(),
                    Category = null 
                })
                .ToListAsync(cancellationToken);

            return Result<List<FileDto>>.Success(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving case files for CaseId {CaseId} and Area {Area}", request.CaseId, request.Area);
            return Result<List<FileDto>>.Failure("خطا در بازیابی فایل‌ها: " + ex.Message);
        }
    }
}
