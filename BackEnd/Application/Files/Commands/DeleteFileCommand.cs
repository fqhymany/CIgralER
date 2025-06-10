using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Results;
using Microsoft.Extensions.Logging;

namespace LawyerProject.Application.Files.Commands;

public class DeleteFileCommand : IRequest<Result<bool>>
{
    public Guid FileId { get; set; }
}

public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<DeleteFileCommandHandler> _logger;

    public DeleteFileCommandHandler(
        IApplicationDbContext dbContext,
        IEncryptionService encryptionService,
        ILogger<DeleteFileCommandHandler> logger)
    {
        _dbContext = dbContext;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var fileMetadata = await _dbContext.EncryptedFileMetadata
                .FirstOrDefaultAsync(f => f.Id == request.FileId, cancellationToken);

            if (fileMetadata == null)
            {
                return Result<bool>.Failure("فایل مورد نظر یافت نشد");
            }

            // حذف فایل فیزیکی از دیسک
            //if (File.Exists(fileMetadata.FilePath))
            //{
            //    File.Delete(fileMetadata.FilePath);
            //}

            // حذف منطقی رکورد از دیتابیس
            fileMetadata.IsDeleted = true;
            _dbContext.EncryptedFileMetadata.Update(fileMetadata);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileId}", request.FileId);
            return Result<bool>.Failure("خطا در حذف فایل: " + ex.Message);
        }
    }
}
